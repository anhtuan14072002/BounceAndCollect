using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Stateful;
using Unity.Physics.Systems;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace Wizard
{
    [BurstCompile]
    [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    public partial struct PadJumpSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            if (!SystemAPI.TryGetSingleton(out PhysicsStep physicsStep))
            {
                physicsStep = PhysicsStep.Default;
            }

            float gravity = math.abs(physicsStep.Gravity.y);
            uint timeSeed = (uint)(SystemAPI.Time.ElapsedTime * 100000d);

            foreach (var (jumpPad, triggerEventsBuffer, entity) in SystemAPI
                         .Query<PadJumpComponent, DynamicBuffer<StatefulTriggerEvent>>().WithEntityAccess())
            {
                for (int i = 0; i < triggerEventsBuffer.Length; i++)
                {
                    StatefulTriggerEvent triggerEvent = triggerEventsBuffer[i];
                    Entity otherEntity = triggerEvent.GetOtherEntity(entity);

                    if (triggerEvent.State != StatefulEventState.Enter) continue;
                    if (!SystemAPI.HasComponent<BallTag>(otherEntity) ||
                        !SystemAPI.HasComponent<PhysicsVelocity>(otherEntity) ||
                        !SystemAPI.HasComponent<LocalTransform>(otherEntity) ||
                        !SystemAPI.HasBuffer<PadJumpHistoryBufferElement>(otherEntity)) continue;

                    var jumpBuffer = SystemAPI.GetBuffer<PadJumpHistoryBufferElement>(otherEntity);

                    bool alreadyJumped = false;
                    for (int j = 0; j < jumpBuffer.Length; j++)
                    {
                        if (jumpBuffer[j].PadId == jumpPad.PadId)
                        {
                            alreadyJumped = true;
                            break;
                        }
                    }

                    if (alreadyJumped) continue;
                    uint seed = math.max(1u, math.hash(new uint3(
                        timeSeed,
                        (uint)entity.Index,
                        (uint)otherEntity.Index)));
                    var random = new Random(seed);
                    float verticalSpeed = jumpPad.JumpForce.y * random.NextFloat(0.9f, 1.1f);
                    float flightTime = 2f * verticalSpeed / gravity;
                    float targetX = random.NextFloat(jumpPad.JumpForceXMin, jumpPad.JumpForceXMax);
                    float currentX = SystemAPI.GetComponent<LocalTransform>(otherEntity).Position.x;
                    float horizontalSpeed = (targetX - currentX) / flightTime;
                    float3 jumpVelocity = new float3(horizontalSpeed, verticalSpeed, jumpPad.JumpForce.z);

                    var velocity = SystemAPI.GetComponent<PhysicsVelocity>(otherEntity);
                    velocity.Linear = jumpVelocity;
                    ecb.SetComponent(otherEntity, velocity);

                    if (!SystemAPI.HasComponent<BallJumpStateComponent>(otherEntity))
                    {
                        PhysicsCollider collider = SystemAPI.GetComponent<PhysicsCollider>(otherEntity);
                        CollisionFilter originalFilter = collider.Value.Value.GetCollisionFilter();
                        collider.MakeUnique(otherEntity, ecb);
                        collider.Value.Value.SetCollisionFilter(new CollisionFilter
                        {
                            BelongsTo = originalFilter.BelongsTo,
                            CollidesWith = 0u,
                            GroupIndex = originalFilter.GroupIndex
                        });
                        ecb.SetComponent(otherEntity, collider);
                        ecb.AddComponent(otherEntity, new BallJumpStateComponent
                        {
                            OriginalFilter = originalFilter
                        });
                    }

                    jumpBuffer.Add(new PadJumpHistoryBufferElement { PadId = jumpPad.PadId });
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
