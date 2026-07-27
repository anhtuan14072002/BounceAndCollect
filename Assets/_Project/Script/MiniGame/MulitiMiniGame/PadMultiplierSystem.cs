using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Stateful;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Wizard
{
    [BurstCompile]
    [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    public partial struct PadMultiplierSystem : ISystem
    {
        private float _ballSeparation;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BallSpawnConfigComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            const float ballScale = 0.3f;
            const float separationMargin = 1.1f;

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);
            BallSpawnConfigComponent miniStoneComponent = SystemAPI.GetSingleton<BallSpawnConfigComponent>();
            if (_ballSeparation == 0f)
            {
                PhysicsCollider ballCollider =
                    SystemAPI.GetComponent<PhysicsCollider>(miniStoneComponent.EntityStone);
                Aabb ballAabb = ballCollider.Value.Value.CalculateAabb(RigidTransform.identity, ballScale);
                _ballSeparation =
                    math.max(ballAabb.Extents.x, ballAabb.Extents.y) * separationMargin;
            }

            foreach (var (multi, triggerEventsBuffer, entity) in SystemAPI
                         .Query<PadMultiplierComponent, DynamicBuffer<StatefulTriggerEvent>>()
                         .WithEntityAccess())
            {
                for (int i = 0; i < triggerEventsBuffer.Length; i++)
                {
                    StatefulTriggerEvent triggerEvent = triggerEventsBuffer[i];
                    Entity otherEntity = triggerEvent.GetOtherEntity(entity);

                    if (triggerEvent.State != StatefulEventState.Enter) continue;
                    if (!SystemAPI.HasComponent<BallTag>(otherEntity) ||
                        !SystemAPI.HasComponent<LocalTransform>(otherEntity) ||
                        !SystemAPI.HasComponent<PhysicsVelocity>(otherEntity) ||
                        !SystemAPI.HasBuffer<PadJumpHistoryBufferElement>(otherEntity) ||
                        !SystemAPI.HasBuffer<PadMultiplierHistoryBufferElement>(otherEntity)) continue;

                    float3 hitPosition = SystemAPI.GetComponent<LocalTransform>(otherEntity).Position;
                    PhysicsVelocity sourceVelocity = SystemAPI.GetComponent<PhysicsVelocity>(otherEntity);
                    var jumpBuffer = SystemAPI.GetBuffer<PadJumpHistoryBufferElement>(otherEntity);
                    var ballBuffer = SystemAPI.GetBuffer<PadMultiplierHistoryBufferElement>(otherEntity);
                    bool alreadyMulti = false;

                    for (int j = 0; j < ballBuffer.Length; j++)
                    {
                        if (ballBuffer[j].PadId != multi.PadId) continue;
                        alreadyMulti = true;
                        break;
                    }

                    if (alreadyMulti) continue;
                    ballBuffer.Add(new PadMultiplierHistoryBufferElement { PadId = multi.PadId });
                    float neighborSafeRadius =
                        _ballSeparation / (2f * math.sin(math.PI / multi.MultiNumber));
                    float spawnRadius = math.max(multi.Radius, math.max(_ballSeparation, neighborSafeRadius));

                    for (int j = 1; j < multi.MultiNumber; j++)
                    {
                        float angle = (2 * math.PI / multi.MultiNumber) * j;
                        float xOffset = math.cos(angle) * spawnRadius;
                        float yOffset = math.sin(angle) * spawnRadius;

                        float3 positionMulti = hitPosition + new float3(xOffset, yOffset, 0);

                        Entity multiStone = ecb.Instantiate(miniStoneComponent.EntityStone);
                        ecb.SetComponent(multiStone, new LocalTransform
                        {
                            Position = positionMulti,
                            Rotation = quaternion.identity,
                            Scale = ballScale,
                        });
                        ecb.SetComponent(multiStone, sourceVelocity);
                        ecb.AddComponent<BallTag>(multiStone);
                        var multiCheckBuffer = ecb.AddBuffer<PadMultiplierHistoryBufferElement>(multiStone);
                        multiCheckBuffer.Add(new PadMultiplierHistoryBufferElement { PadId = multi.PadId });

                        var newJumpBuffer = ecb.AddBuffer<PadJumpHistoryBufferElement>(multiStone);
                        for (int k = 0; k < jumpBuffer.Length; k++)
                        {
                            newJumpBuffer.Add(jumpBuffer[k]);
                        }
                    }

                    if (multi.PadId == multi.PadIdMove)
                    {
                        foreach (var (tweenPadMove, entityTween) 
                                 in SystemAPI.Query<RefRW<PadMoveComponent>>().WithEntityAccess())
                        {
                            tweenPadMove.ValueRW.isStop = true;
                        }
                    }
                }
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}          
