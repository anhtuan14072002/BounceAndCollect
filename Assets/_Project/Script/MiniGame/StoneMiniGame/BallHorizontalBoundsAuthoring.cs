using Unity.Entities;
using UnityEngine;

namespace Wizard
{
    public sealed class BallHorizontalBoundsAuthoring : MonoBehaviour
    {
        [SerializeField] private Transform leftWall;
        [SerializeField] private Transform rightWall;

        private sealed class Baker : Baker<BallHorizontalBoundsAuthoring>
        {
            public override void Bake(BallHorizontalBoundsAuthoring authoring)
            {
                Bounds leftBounds = authoring.leftWall.GetComponent<Renderer>().bounds;
                Bounds rightBounds = authoring.rightWall.GetComponent<Renderer>().bounds;
                
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new BallHorizontalBounds
                {
                    MinX = leftBounds.max.x,
                    MaxX = rightBounds.min.x
                });
            }
        }
    }
}
