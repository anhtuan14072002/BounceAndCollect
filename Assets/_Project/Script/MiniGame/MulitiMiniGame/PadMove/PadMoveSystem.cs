using LitMotion;
using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace Wizard
{
    [UpdateInGroup(typeof(AfterPhysicsSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(KinematicCharacterPhysicsUpdateGroup))]
    public partial class PadMoveSystem : SystemBase
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

            foreach (var tweenPad in SystemAPI.Query<RefRW<PadMoveComponent>>())
            {
                ref PadMoveComponent padMove = ref tweenPad.ValueRW;
                if (padMove.isStop)
                {
                    padMove.Motion.TryCancel();
                    continue;
                }

                if (padMove.isInitialized) continue;

                padMove.PadTweenInitialTransform = SystemAPI.GetComponent<LocalTransform>(padMove.PadTween);
                float3 position = padMove.PadTweenInitialTransform.Position;
                Vector3 startPosition = new Vector3(position.x, position.y, position.z);

                padMove.Motion = LMotion.Create(
                        startPosition,
                        startPosition + Vector3.right * padMove.targetPosition,
                        3f)
                    .WithEase(Ease.InOutQuad)
                    .WithLoops(-1, LoopType.Yoyo)
                    .WithScheduler(_dispatcher.Scheduler)
                    .Bind(padMove.PadTween, SetPosition);
                padMove.isInitialized = true;
            }
        }

        private void SetPosition(Vector3 position, Entity entity)
        {
            if (!EntityManager.Exists(entity)) return;

            LocalTransform localTransform = EntityManager.GetComponentData<LocalTransform>(entity);
            localTransform.Position = new float3(position.x, position.y, position.z);
            EntityManager.SetComponentData(entity, localTransform);
        }
    }
}
