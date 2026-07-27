using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;

namespace Wizard
{
    public struct PadPhysicsInitialized : IComponentData
    {
    }

    [BurstCompile]
    [UpdateInGroup(typeof(BeforePhysicsSystemGroup))]
    public partial struct PadPhysicsSetupSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            foreach (var (collider, entity) in SystemAPI
                         .Query<RefRO<PhysicsCollider>>()
                         .WithAny<PadJumpComponent, PadMultiplierComponent, PadRandomComponent>()
                         .WithNone<PadPhysicsInitialized>()
                         .WithEntityAccess())
            {
                PhysicsCollider uniqueCollider = collider.ValueRO;
                uniqueCollider.MakeUnique(entity, ecb);
                uniqueCollider.Value.Value.SetCollisionResponse(
                    CollisionResponsePolicy.RaiseTriggerEvents);
                ecb.SetComponent(entity, uniqueCollider);
                ecb.AddComponent<PadPhysicsInitialized>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
