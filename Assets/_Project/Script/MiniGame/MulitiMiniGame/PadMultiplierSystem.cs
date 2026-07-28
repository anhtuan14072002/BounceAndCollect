using Unity.Burst;
using Unity.Collections;
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
        private EntityQuery _poolQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BallSpawnConfigComponent>();
            _poolQuery = BallPool.CreateQuery(ref state);
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

            NativeArray<Entity> pooledBalls = default;
            int poolIndex = 0;
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
                    if (!pooledBalls.IsCreated)
                        pooledBalls = _poolQuery.ToEntityArray(Allocator.Temp);

                    for (int j = 1; j < multi.MultiNumber; j++)
                    {
                        float angle = (2 * math.PI / multi.MultiNumber) * j;
                        float xOffset = math.cos(angle) * spawnRadius;
                        float yOffset = math.sin(angle) * spawnRadius;

                        float3 positionMulti = hitPosition + new float3(xOffset, yOffset, 0);

                        Entity multiStone =
                            BallPool.Get(miniStoneComponent.EntityStone, pooledBalls, ref poolIndex, ref ecb);
                        var spawnTransform = new LocalTransform
                        {
                            Position = positionMulti,
                            Rotation = quaternion.identity,
                            Scale = ballScale,
                        };
                        ecb.SetComponent(multiStone, spawnTransform);
                        ecb.SetComponent(multiStone, new LocalToWorld
                        {
                            Value = float4x4.TRS(spawnTransform.Position, spawnTransform.Rotation,
                                new float3(spawnTransform.Scale))
                        });
                        ecb.SetComponent(multiStone, sourceVelocity);
                        ecb.AppendToBuffer(multiStone,
                            new PadMultiplierHistoryBufferElement { PadId = multi.PadId });
                        for (int k = 0; k < jumpBuffer.Length; k++)
                        {
                            ecb.AppendToBuffer(multiStone, jumpBuffer[k]);
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
            if (pooledBalls.IsCreated)
                pooledBalls.Dispose();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}          
