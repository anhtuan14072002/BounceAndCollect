using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Wizard
{
    public sealed class PadResolvedLabelAuthoring : MonoBehaviour
    {
        private sealed class Baker : Baker<PadResolvedLabelAuthoring>
        {
            public override void Bake(PadResolvedLabelAuthoring authoring)
            {
                AddComponent<DisableRendering>(GetEntity(TransformUsageFlags.Dynamic));
            }
        }
    }
}
