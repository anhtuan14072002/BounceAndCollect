using Unity.Entities;
using UnityEngine;

namespace Wizard
{
    public struct BallHorizontalBounds : IComponentData
    {
        public float MinX;
        public float MaxX;
    }

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
                Debug.Assert(leftBounds.max.x < rightBounds.min.x, "Ball horizontal bounds are invalid.");

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
