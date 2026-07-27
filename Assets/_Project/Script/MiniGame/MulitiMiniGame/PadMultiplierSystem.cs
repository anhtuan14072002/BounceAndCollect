using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Stateful;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Wizard
{
    [BurstCompile]
    [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    public partial struct PadMultiplierSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BallSpawnConfigComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);
            BallSpawnConfigComponent miniStoneComponent = SystemAPI.GetSingleton<BallSpawnConfigComponent>();

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
                        !SystemAPI.HasBuffer<PadJumpHistoryBufferElement>(otherEntity) ||
                        !SystemAPI.HasBuffer<PadMultiplierHistoryBufferElement>(otherEntity)) continue;

                    float3 hitPosition = SystemAPI.GetComponent<LocalTransform>(otherEntity).Position;
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
                    for (int j = 1; j < multi.MultiNumber; j++)
                    {
                        float angle = (2 * math.PI / multi.MultiNumber) * j;
                        float xOffset = math.cos(angle) * multi.Radius;
                        float yOffset = math.sin(angle) * multi.Radius;

                        float3 positionMulti = hitPosition + new float3(xOffset, yOffset, 0);

                        Entity multiStone = ecb.Instantiate(miniStoneComponent.EntityStone);
                        ecb.SetComponent(multiStone, new LocalTransform
                        {
                            Position = positionMulti,
                            Rotation = quaternion.identity,
                            Scale = 0.3f,
                        });
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
