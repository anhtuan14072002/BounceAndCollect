using Unity.Entities;
using UnityEngine;

namespace Wizard
{
    public class SourceCupAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject _bagPrefab;
        public bool _isDrag;
        public bool _isCreate;

        class Baker : Baker<SourceCupAuthoring>
        {
            public override void Bake(SourceCupAuthoring authoring)
            {
                var bagEntity = GetEntity(authoring._bagPrefab, TransformUsageFlags.Dynamic);
                AddComponent(bagEntity, new SourceCupComponent
                {
                    EntityBag = bagEntity,
                });
                AddComponent(bagEntity, new SourceCupDragComponent
                {
                    IsDrag = authoring._isDrag,
                    IsCreate = authoring._isCreate,
                });
            }
        }
    }
}
