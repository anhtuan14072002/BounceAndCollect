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
        class Baker : Baker<PadRandomAuthoring>
        {
            public override void Bake(PadRandomAuthoring authoring)
            {
                AddComponent(GetEntity(TransformUsageFlags.Dynamic), new PadRandomComponent
                {
                    PadTween = GetEntity(authoring.padRandom, TransformUsageFlags.Dynamic),
                    initializedUniformScale = authoring.initializedUniformScale,
                    targetUniformScale = authoring.targetUniformScale,
                    targetScale = authoring.targetPositionScale,
                });
            }
        }
    }
}
