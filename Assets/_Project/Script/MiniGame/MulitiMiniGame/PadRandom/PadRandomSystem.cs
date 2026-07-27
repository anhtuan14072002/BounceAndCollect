using LitMotion;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Stateful;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace Wizard
{
    [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    [UpdateBefore(typeof(PadJumpSystem))]
    [UpdateBefore(typeof(PadMultiplierSystem))]
    public partial class PadRandomSystem : SystemBase
    {
        private ManualMotionDispatcher _dispatcher;

        protected override void OnCreate()
        {
            _dispatcher = new ManualMotionDispatcher();
        }

        protected override void OnDestroy()
        {
            _dispatcher.Reset();
        }

        protected override void OnUpdate()
        {
            _dispatcher.Update(SystemAPI.Time.DeltaTime);

            foreach (var padTween in SystemAPI.Query<RefRW<PadRandomComponent>>())
            {
                ref PadRandomComponent padRandom = ref padTween.ValueRW;
                if (padRandom.isInitialized) continue;

                padRandom.PadTweenInitialTransform =
                    SystemAPI.GetComponent<LocalTransform>(padRandom.PadTween);
                padRandom.isInitialized = true;
            }

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (padRandomRef, triggerEventsBuffer, entity) in
                     SystemAPI.Query<RefRW<PadRandomComponent>, DynamicBuffer<StatefulTriggerEvent>>()
                          .WithEntityAccess())
            {
                ref PadRandomComponent padRandom = ref padRandomRef.ValueRW;
                if (padRandom.isTriggered) continue;

                for (int i = 0; i < triggerEventsBuffer.Length; i++)
                {
                    StatefulTriggerEvent triggerEvent = triggerEventsBuffer[i];
                    if (triggerEvent.State != StatefulEventState.Enter ||
                        !SystemAPI.HasComponent<BallTag>(triggerEvent.GetOtherEntity(entity))) continue;

                    ResolvePad(ref padRandom, entity, ref ecb);
                    StartTweens(ref padRandom);
                    break;
                }
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        private void ResolvePad(ref PadRandomComponent padRandom, Entity entity, ref EntityCommandBuffer ecb)
        {
            uint seed = (uint)(SystemAPI.Time.ElapsedTime * 100000d) + (uint)entity.Index + 1u;
            var random = new Unity.Mathematics.Random(seed);
            if (random.NextInt(0, 2) == 0)
            {
                ecb.AddComponent(entity, new PadJumpComponent
                {
                    PadId = padRandom.PadId,
                    JumpForce = padRandom.JumpForce,
                    JumpForceXMin = padRandom.JumpForceXMin,
                    JumpForceXMax = padRandom.JumpForceXMax
                });
                return;
            }

            ecb.AddComponent(entity, new PadMultiplierComponent
            {
                PadId = padRandom.PadId,
                MultiNumber = random.NextInt(padRandom.MultiplierMin, padRandom.MultiplierMax + 1), 
                Radius = padRandom.MultiplierRadius,
                PadIdMove = -1
            });
        }

        private void StartTweens(ref PadRandomComponent padRandom)
        {
            padRandom.isTriggered = true;

            LMotion.Create(padRandom.PadTweenInitialTransform.Scale, padRandom.targetScale, 0.5f)
                .WithEase(Ease.InSine)
                .WithScheduler(_dispatcher.Scheduler)
                .Bind(padRandom.PadTween, SetUniformScale);

            Vector3 startScale = new Vector3(
                padRandom.initializedUniformScale.x,
                padRandom.initializedUniformScale.y,
                padRandom.initializedUniformScale.z);
            Vector3 targetScale = new Vector3(
                padRandom.targetUniformScale.x,
                padRandom.targetUniformScale.y,
                padRandom.targetUniformScale.z);

            LMotion.Create(startScale, targetScale, 0.75f)
                .WithEase(Ease.InOutBack)
                .WithScheduler(_dispatcher.Scheduler)
                .Bind(padRandom.PadTween, SetNonUniformScale);
        }

        private void SetUniformScale(float scale, Entity entity)
        {
            if (!EntityManager.Exists(entity)) return;

            LocalTransform localTransform = EntityManager.GetComponentData<LocalTransform>(entity);
            localTransform.Scale = scale;
            EntityManager.SetComponentData(entity, localTransform);
        }

        private void SetNonUniformScale(Vector3 scale, Entity entity)
        {
            if (!EntityManager.Exists(entity) ||
                !EntityManager.HasComponent<PostTransformMatrix>(entity)) return;

            EntityManager.SetComponentData(entity, new PostTransformMatrix
            {
                Value = float4x4.Scale(new float3(scale.x, scale.y, scale.z))
            });
        }
    }
}
