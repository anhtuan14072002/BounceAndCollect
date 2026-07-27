using LitMotion;
using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Stateful;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace Wizard
{
    [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    [UpdateBefore(typeof(KinematicCharacterPhysicsUpdateGroup))]
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

            foreach (var (_, triggerEventsBuffer) in
                     SystemAPI.Query<PadRandomComponent, DynamicBuffer<StatefulTriggerEvent>>())
            {
                for (int i = 0; i < triggerEventsBuffer.Length; i++)
                {
                    if (triggerEventsBuffer[i].State != StatefulEventState.Enter) continue;

                    foreach (var tweenRandom in SystemAPI.Query<RefRW<PadRandomComponent>>())
                    {
                        ref PadRandomComponent padRandom = ref tweenRandom.ValueRW;
                        if (padRandom.isTriggered) continue;

                        StartTweens(ref padRandom);
                    }
                }
            }
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
