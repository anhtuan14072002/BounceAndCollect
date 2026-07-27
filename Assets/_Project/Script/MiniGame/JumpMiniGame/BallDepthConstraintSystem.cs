using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Wizard
{
    [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    public partial struct BallDepthConstraintSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var velocity in SystemAPI.Query<RefRW<PhysicsVelocity>>().WithAll<BallTag>())
            {
                velocity.ValueRW.Linear.z = 0f;
            }
        }
    }
}
