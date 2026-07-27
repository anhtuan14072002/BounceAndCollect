using Unity.Entities;
using UnityEngine;

namespace Wizard
{
    public class BallSpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject stonePrefab;
        public int amount;
        public int spawnedAmount;
        public float elapsedTime;

        public int minMapFew;
        public int maxMapFew;
        public int minMapMedium;
        public int maxMapMedium;
        public int minMapMany;
        public int maxMapMany;

        public float ElapsedTime => elapsedTime;
        public int SpawnedAmount => spawnedAmount;

        class Baker : Baker<BallSpawnerAuthoring>
        {
            public override void Bake(BallSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new BallSpawnConfigComponent
                {
                    EntityStone = GetEntity(authoring.stonePrefab, TransformUsageFlags.Dynamic),
                    Amount = authoring.amount,
                    ElapsedTime = authoring.elapsedTime,
                    
                    MinMapFew = authoring.minMapFew,
                    MaxMapFew = authoring.maxMapFew,
                    MinMapMedium = authoring.minMapMedium,
                    MaxMapMedium = authoring.maxMapMedium,  
                    MinMapMany = authoring.minMapMany,
                    MaxMapMany = authoring.maxMapMany,
                    
                });
            }
        }
    }
}
