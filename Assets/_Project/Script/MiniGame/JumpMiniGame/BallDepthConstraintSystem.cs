using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Wizard
{
    [BurstCompile]
    [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    public partial struct BallDepthConstraintSystem : ISystem
    {
        private const float BallDepth = -0.11f;
        private float _ballRadius;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BallHorizontalBounds>();
            state.RequireForUpdate<BallSpawnConfigComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_ballRadius == 0f)
            {
                BallSpawnConfigComponent spawnConfig = SystemAPI.GetSingleton<BallSpawnConfigComponent>();
                PhysicsCollider ballCollider = SystemAPI.GetComponent<PhysicsCollider>(spawnConfig.EntityStone);
                _ballRadius = ballCollider.Value.Value
                    .CalculateAabb(RigidTransform.identity, 1f)
                    .Extents.x;
            }

            BallHorizontalBounds bounds = SystemAPI.GetSingleton<BallHorizontalBounds>();
            foreach (var (transform, velocity) in SystemAPI
                         .Query<RefRW<LocalTransform>, RefRW<PhysicsVelocity>>()
                         .WithAll<BallTag>())
            {
                float3 position = transform.ValueRO.Position;
                position.z = BallDepth;
                velocity.ValueRW.Linear.z = 0f;

                float minX = bounds.MinX + _ballRadius * transform.ValueRO.Scale;
                float maxX = bounds.MaxX - _ballRadius * transform.ValueRO.Scale;

                if (position.x < minX)
                {
                    position.x = minX;
                    velocity.ValueRW.Linear.x = math.max(0f, velocity.ValueRO.Linear.x);
                }
                else if (position.x > maxX)
                {
                    position.x = maxX;
                    velocity.ValueRW.Linear.x = math.min(0f, velocity.ValueRO.Linear.x);
                }

                transform.ValueRW.Position = position;
            }
        }
    }
}
