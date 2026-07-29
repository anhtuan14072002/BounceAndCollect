using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace Wizard
{
    public class PadRandomAuthoring : MonoBehaviour
    {
        [SerializeField] public GameObject padRandom;
        [SerializeField] public float3 initializedUniformScale;
        [SerializeField] public float3 targetUniformScale;
        [SerializeField] public float targetPositionScale;
        [SerializeField] private GameObject randomLabel;
        [SerializeField] private GameObject jumpLabel;
        [SerializeField] private GameObject multiplier2Label;
        [SerializeField] private GameObject multiplier3Label;
        [SerializeField] private GameObject multiplier4Label;
        [SerializeField] private Color resolvedColor = new Color(0.3f, 1f, 0.45f, 1f);
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
                Entity randomLabelEntity = GetEntity(authoring.randomLabel, TransformUsageFlags.Dynamic);
                Entity jumpLabelEntity = GetEntity(authoring.jumpLabel, TransformUsageFlags.Dynamic);
                Entity multiplier2LabelEntity = GetEntity(authoring.multiplier2Label, TransformUsageFlags.Dynamic);
                Entity multiplier3LabelEntity = GetEntity(authoring.multiplier3Label, TransformUsageFlags.Dynamic);
                Entity multiplier4LabelEntity = GetEntity(authoring.multiplier4Label, TransformUsageFlags.Dynamic);
                AddComponent(entity, new PadRandomComponent
                {
                    PadTween = GetEntity(authoring.padRandom, TransformUsageFlags.Dynamic),
                    RandomLabel = randomLabelEntity,
                    JumpLabel = jumpLabelEntity,
                    Multiplier2Label = multiplier2LabelEntity,
                    Multiplier3Label = multiplier3LabelEntity,
                    Multiplier4Label = multiplier4LabelEntity,
                    initializedUniformScale = authoring.initializedUniformScale,
                    targetUniformScale = authoring.targetUniformScale,
                    targetScale = authoring.targetPositionScale,
                    PadId = authoring.padId,
                    JumpForce = authoring.jumpForce,
                    JumpForceXMin = authoring.jumpForceXMin,
                    JumpForceXMax = authoring.jumpForceXMax,
                    MultiplierMin = authoring.multiplierMin,
                    MultiplierMax = authoring.multiplierMax,
                    MultiplierRadius = authoring.multiplierRadius,
                    ResolvedColor = new float4(
                        authoring.resolvedColor.r,
                        authoring.resolvedColor.g,
                        authoring.resolvedColor.b,
                        authoring.resolvedColor.a)
                });
                AddComponent(entity, new URPMaterialPropertyBaseColor
                {
                    Value = new float4(1f, 0.16f, 0.48f, 1f)
                });
            }
        }
    }
}
