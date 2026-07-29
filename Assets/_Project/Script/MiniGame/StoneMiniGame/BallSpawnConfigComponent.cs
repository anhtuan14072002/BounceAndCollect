using Unity.Entities;
using Unity.Mathematics;

namespace Wizard
{
    public struct BallSpawnConfigComponent : IComponentData
    {
        public Entity EntityStone;
        public int Amount;
        public float ElapsedTime;
    }
}
