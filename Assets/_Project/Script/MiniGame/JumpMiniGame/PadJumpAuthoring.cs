using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Wizard
{
    public class PadJumpAuthoring : MonoBehaviour
    {
        public float3 jumpForce;
        public int padId;
        public int jumpForceXMin;
        public int jumpForceXMax;
        class Baker : Baker<PadJumpAuthoring>
        {
            public override void Bake(PadJumpAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PadJumpComponent
                {
                    JumpForce = authoring.jumpForce,
                    PadId = authoring.padId,
                    JumpForceXMin = authoring.jumpForceXMin,
                    JumpForceXMax = authoring.jumpForceXMax
                }); }
        }
    }
}
