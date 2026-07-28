using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Wizard
{
    [BurstCompile]
    public partial struct BallSpawnSystem : ISystem
    {
        private float _timer;
        private EntityQuery _poolQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BallSpawnConfigComponent>();
            state.RequireForUpdate<SourceCupDragComponent>();
            _poolQuery = BallPool.CreateQuery(ref state);
        }

        public void OnUpdate(ref SystemState state)
        {
            var miniStoneComponent = SystemAPI.GetSingleton<BallSpawnConfigComponent>();
            var miniDragBagComponent = SystemAPI.GetSingleton<SourceCupDragComponent>();
            if (!miniDragBagComponent.IsDragging) return;

            if (miniStoneComponent.Amount == 0) return;
            float elapsedTime = miniStoneComponent.ElapsedTime;
            _timer += SystemAPI.Time.DeltaTime;

            if (!(_timer >= elapsedTime)) return;
            _timer = 0f;

            Entity miniStoneEntity = SystemAPI.GetSingletonEntity<BallSpawnConfigComponent>();
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            using NativeArray<Entity> pooledBalls = _poolQuery.ToEntityArray(Allocator.Temp);
            int poolIndex = 0;
            foreach (var localTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<SourceCupComponent>())
            {
                Entity createStone =
                    BallPool.Get(miniStoneComponent.EntityStone, pooledBalls, ref poolIndex, ref ecb);
                LocalTransform spawnTransform = LocalTransform.FromPositionRotationScale(
                    localTransform.ValueRO.Position + new float3(0f, -1.1f, 0f), quaternion.identity, 0.3f);
                ecb.SetComponent(createStone, spawnTransform);
                ecb.SetComponent(createStone, new LocalToWorld
                {
                    Value = float4x4.TRS(spawnTransform.Position, spawnTransform.Rotation,
                        new float3(spawnTransform.Scale))
                });

                ecb.SetComponent(createStone, default(PhysicsVelocity));
                miniStoneComponent.Amount--;
                SystemAPI.SetComponent(miniStoneEntity, miniStoneComponent);
                if (miniStoneComponent.Amount == 0)
                    break;
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
