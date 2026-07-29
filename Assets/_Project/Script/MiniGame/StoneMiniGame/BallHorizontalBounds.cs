using Unity.Entities;

namespace Wizard
{
    public struct BallHorizontalBounds : IComponentData
    {
        public float MinX;
        public float MaxX;
    }
}