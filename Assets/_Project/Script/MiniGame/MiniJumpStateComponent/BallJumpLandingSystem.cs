using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Wizard
{
    [BurstCompile]
    [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    [UpdateAfter(typeof(PadJumpSystem))]
    public partial struct BallJumpLandingSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            foreach (var (jumpState, velocity, collider, entity) in SystemAPI
                         .Query<RefRO<BallJumpStateComponent>, RefRO<PhysicsVelocity>, RefRW<PhysicsCollider>>()
                         .WithEntityAccess())
            {
                if (velocity.ValueRO.Linear.y >= 0f) continue;

                collider.ValueRW.Value.Value.SetCollisionFilter(jumpState.ValueRO.OriginalFilter);
                ecb.RemoveComponent<BallJumpStateComponent>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
