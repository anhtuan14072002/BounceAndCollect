using Unity.Entities;
using Unity.Physics;

namespace Wizard
{
    public struct BallJumpStateComponent : IComponentData
    {
        public CollisionFilter OriginalFilter;
    }
}
