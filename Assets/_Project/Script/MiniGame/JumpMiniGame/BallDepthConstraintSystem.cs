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
                velocity.ValueRW.Linear.z = 0f;

                float minX = bounds.MinX + _ballRadius * transform.ValueRO.Scale;
                float maxX = bounds.MaxX - _ballRadius * transform.ValueRO.Scale;
                float currentX = transform.ValueRO.Position.x;

                if (currentX < minX)
                {
                    transform.ValueRW.Position.x = minX;
                    velocity.ValueRW.Linear.x = math.max(0f, velocity.ValueRO.Linear.x);
                }
                else if (currentX > maxX)
                {
                    transform.ValueRW.Position.x = maxX;
                    velocity.ValueRW.Linear.x = math.min(0f, velocity.ValueRO.Linear.x);
                }
            }
        }
    }
}
