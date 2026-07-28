using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Wizard
{
    [Serializable]
    public struct BallTrailSettings
    {
        public bool Enabled;
        public Material Material;
        [Min(0.01f)] public float Lifetime;
        [FormerlySerializedAs("Width")]
        [Min(0.01f)] public float WidthMultiplier;
        public AnimationCurve WidthOverLifetime;
        public Gradient ColorOverLifetime;
        [Range(2, BallTrailPoint.MaxPoints)] public int MaxPoints;
        [Min(0.001f)] public float MinPointDistance;

        public static BallTrailSettings Default
        {
            get
            {
                var gradient = new Gradient();
                gradient.SetKeys(
                    new[]
                    {
                        new GradientColorKey(new Color(0.75f, 0.95f, 1f), 0f),
                        new GradientColorKey(new Color(0.75f, 0.95f, 1f), 1f),
                    },
                    new[]
                    {
                        new GradientAlphaKey(0.65f, 0f),
                        new GradientAlphaKey(0f, 1f),
                    });

                return new BallTrailSettings
                {
                    Enabled = true,
                    Lifetime = 0.2f,
                    WidthMultiplier = 0.12f,
                    WidthOverLifetime = AnimationCurve.Linear(0f, 1f, 1f, 0f),
                    ColorOverLifetime = gradient,
                    MaxPoints = BallTrailPoint.MaxPoints,
                    MinPointDistance = 0.03f,
                };
            }
        }
    }

    public class BallSpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject stonePrefab;
        [SerializeField] private BallTrailSettings trail = BallTrailSettings.Default;

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

                if (!authoring.trail.Enabled) return;

                Material trailMaterial = authoring.trail.Material;
                if (trailMaterial == null)
                    trailMaterial = authoring.stonePrefab.GetComponentInChildren<TrailRenderer>()?.sharedMaterial;

                if (trailMaterial == null) return;

                AnimationCurve widthCurve = authoring.trail.WidthOverLifetime ??
                                            AnimationCurve.Linear(0f, 1f, 1f, 0f);
                Gradient colorGradient = authoring.trail.ColorOverLifetime ??
                                         BallTrailSettings.Default.ColorOverLifetime;
                DynamicBuffer<BallTrailSample> samples = AddBuffer<BallTrailSample>(entity);
                for (int i = 0; i < BallTrailSample.SampleCount; i++)
                {
                    float time = i / (BallTrailSample.SampleCount - 1f);
                    Color color = colorGradient.Evaluate(time);
                    samples.Add(new BallTrailSample
                    {
                        Width = widthCurve.Evaluate(time),
                        Color = new float4(color.r, color.g, color.b, color.a),
                    });
                }

                AddComponentObject(entity, new BallTrailRenderData
                {
                    Material = trailMaterial,
                    Lifetime = Mathf.Max(0.01f, authoring.trail.Lifetime),
                    WidthMultiplier = Mathf.Max(0.01f, authoring.trail.WidthMultiplier),
                    MaxPoints = Mathf.Clamp(authoring.trail.MaxPoints, 2, BallTrailPoint.MaxPoints),
                    MinPointDistanceSq = Mathf.Max(0.001f, authoring.trail.MinPointDistance) *
                                         Mathf.Max(0.001f, authoring.trail.MinPointDistance),
                });
            }
        }
    }
}
