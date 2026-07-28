using Unity.Collections;
using Unity.Entities;

namespace Wizard
{
    public static class BallPool
    {
        public static EntityQuery CreateQuery(ref SystemState state)
        {
            return new EntityQueryBuilder(Allocator.Temp)
                .WithAll<BallTag, Disabled>()
                .WithOptions(EntityQueryOptions.IncludeDisabledEntities)
                .Build(ref state);
        }

        public static Entity Get(Entity prefab, NativeArray<Entity> pooledBalls, ref int poolIndex,
            ref EntityCommandBuffer ecb)
        {
            if (poolIndex < pooledBalls.Length)
            {
                Entity ball = pooledBalls[poolIndex++];
                ecb.RemoveComponent<Disabled>(ball);
                ecb.SetBuffer<BallTrailPoint>(ball);
                ecb.SetBuffer<PadJumpHistoryBufferElement>(ball);
                ecb.SetBuffer<PadMultiplierHistoryBufferElement>(ball);
                return ball;
            }

            Entity newBall = ecb.Instantiate(prefab);
            ecb.AddComponent<BallTag>(newBall);
            ecb.AddBuffer<BallTrailPoint>(newBall);
            ecb.AddBuffer<PadJumpHistoryBufferElement>(newBall);
            ecb.AddBuffer<PadMultiplierHistoryBufferElement>(newBall);
            return newBall;
        }
    }
}
