using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Wizard
{
    public class PadMoveAuthoring : MonoBehaviour
    {
        [SerializeField] public GameObject padTween;
        [SerializeField] public float targetPosition;

        class Baker : Baker<PadMoveAuthoring>
        {
            public override void Bake(PadMoveAuthoring authoring)
            {
                AddComponent(GetEntity(TransformUsageFlags.None), new PadMoveComponent
                {
                    PadTween = GetEntity(authoring.padTween, TransformUsageFlags.Dynamic),
                    targetPosition = authoring.targetPosition,
                });
            }
        }
    }
}
