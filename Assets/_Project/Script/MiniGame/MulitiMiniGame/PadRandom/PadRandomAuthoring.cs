using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Wizard
{
    public class PadRandomAuthoring : MonoBehaviour
    {
        [SerializeField] public GameObject padRandom;
        [SerializeField] public float3 initializedUniformScale;
        [SerializeField] public float3 targetUniformScale;
        [SerializeField] public float targetPositionScale;
        [SerializeField] private int padId = 5;
        [SerializeField] private float3 jumpForce = new float3(0f, 8f, 0f);
        [SerializeField] private int jumpForceXMin = -3;
        [SerializeField] private int jumpForceXMax = 4;
        [SerializeField] private int multiplierMin = 2;
        [SerializeField] private int multiplierMax = 4;
        [SerializeField] private float multiplierRadius = 0.22f;

        class Baker : Baker<PadRandomAuthoring>
        {
            public override void Bake(PadRandomAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PadRandomComponent
                {
                    PadTween = GetEntity(authoring.padRandom, TransformUsageFlags.Dynamic),
                    initializedUniformScale = authoring.initializedUniformScale,
                    targetUniformScale = authoring.targetUniformScale,
                    targetScale = authoring.targetPositionScale,
                    PadId = authoring.padId,
                    JumpForce = authoring.jumpForce,
                    JumpForceXMin = authoring.jumpForceXMin,
                    JumpForceXMax = authoring.jumpForceXMax,
                    MultiplierMin = authoring.multiplierMin,
                    MultiplierMax = authoring.multiplierMax,
                    MultiplierRadius = authoring.multiplierRadius
                });
            }
        }
    }
}
