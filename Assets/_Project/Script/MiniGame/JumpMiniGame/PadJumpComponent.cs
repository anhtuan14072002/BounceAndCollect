using Unity.Entities;
using Unity.Mathematics;
namespace Wizard
{
    public struct PadJumpComponent : IComponentData
    {
        public float3 JumpForce;
        public int PadId;
        public int JumpForceXMin;
        public int JumpForceXMax;
    }
}
