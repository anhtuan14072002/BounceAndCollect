using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Stateful;

namespace Wizard
{
    [BurstCompile]
    public partial struct MysticBagCollisionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MysticBagComponent>();
            state.RequireForUpdate<CollectedBallCountComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);
            foreach (var (collectedBallCount, triggerEventsBuffer, bagEntity) in SystemAPI
                         .Query<RefRW<CollectedBallCountComponent>, DynamicBuffer<StatefulTriggerEvent>>()
                         .WithAll<MysticBagComponent>()
                         .WithEntityAccess())
            {
                foreach (var triggerEvent in triggerEventsBuffer)
                {
                    var otherEntity = triggerEvent.GetOtherEntity(bagEntity);

                    if (triggerEvent.State == StatefulEventState.Enter &&
                        SystemAPI.HasComponent<BallTag>(otherEntity))
                    {
                        collectedBallCount.ValueRW.Value++;

                        if (SystemAPI.HasComponent<BallJumpStateComponent>(otherEntity))
                        {
                            BallJumpStateComponent jumpState =
                                SystemAPI.GetComponent<BallJumpStateComponent>(otherEntity);
                            PhysicsCollider collider = SystemAPI.GetComponent<PhysicsCollider>(otherEntity);
                            collider.Value.Value.SetCollisionFilter(jumpState.OriginalFilter);
                            ecb.SetComponent(otherEntity, collider);
                            ecb.RemoveComponent<BallJumpStateComponent>(otherEntity);
                        }

                        ecb.AddComponent<Disabled>(otherEntity);
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
