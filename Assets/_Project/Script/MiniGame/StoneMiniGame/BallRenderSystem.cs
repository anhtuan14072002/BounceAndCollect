using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Wizard
{
    public struct BallTag : IComponentData
    {
    }

    [InternalBufferCapacity(8)]
    public struct BallTrailPoint : IBufferElementData
    {
        public const int MaxPoints = 8;

        public float3 Position;
        public float Age;
    }

    [InternalBufferCapacity(16)]
    public struct BallTrailSample : IBufferElementData
    {
        public const int SampleCount = 16;

        public float Width;
        public float4 Color;
    }

    public sealed class BallTrailRenderData : IComponentData
    {
        public Material Material;
        public float Lifetime;
        public float WidthMultiplier;
        public int MaxPoints;
        public float MinPointDistanceSq;
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class BallRenderSystem : SystemBase
    {
        private const int BatchSize = 1023;

        private readonly Matrix4x4[] _matrices = new Matrix4x4[BatchSize];
        private Mesh _mesh;

        protected override void OnCreate()
        {
            RequireForUpdate<BallTrailRenderData>();
            RequireForUpdate<BallTrailSample>();
        }

        protected override void OnStartRunning()
        {
            BallTrailRenderData renderData = SystemAPI.ManagedAPI.GetSingleton<BallTrailRenderData>();
            if (renderData.Material != null)
                renderData.Material.enableInstancing = true;

            _mesh = CreateTrailMesh(SystemAPI.GetSingletonBuffer<BallTrailSample>(true));
        }

        protected override void OnUpdate()
        {
            BallTrailRenderData renderData = SystemAPI.ManagedAPI.GetSingleton<BallTrailRenderData>();
            if (renderData.Material == null) return;

            float deltaTime = SystemAPI.Time.DeltaTime;
            int batchCount = 0;

            foreach (var (transform, trail) in SystemAPI
                         .Query<RefRO<LocalToWorld>, DynamicBuffer<BallTrailPoint>>()
                         .WithAll<BallTag>())
            {
                UpdateTrail(trail, transform.ValueRO.Position, deltaTime, renderData);
                if (trail.Length < 2) continue;

                float3 tailPosition = trail[0].Position;
                float3 headPosition = transform.ValueRO.Position;
                float3 direction = headPosition - tailPosition;
                float length = math.length(direction.xy);
                if (length <= float.Epsilon) continue;

                float angle = math.degrees(math.atan2(direction.y, direction.x));
                float3 center = (tailPosition + headPosition) * 0.5f;
                _matrices[batchCount] = Matrix4x4.TRS(center,
                    Quaternion.Euler(0f, 0f, angle),
                    new Vector3(length, renderData.WidthMultiplier, 1f));
                batchCount++;

                if (batchCount != BatchSize) continue;
                DrawBatch(renderData.Material, batchCount);
                batchCount = 0;
            }

            DrawBatch(renderData.Material, batchCount);
        }

        protected override void OnDestroy()
        {
            Object.Destroy(_mesh);
        }

        private static void UpdateTrail(DynamicBuffer<BallTrailPoint> trail, float3 position, float deltaTime,
            BallTrailRenderData settings)
        {
            for (int i = 0; i < trail.Length; i++)
            {
                BallTrailPoint point = trail[i];
                point.Age += deltaTime;
                trail[i] = point;
            }

            while (trail.Length > 0 && trail[0].Age >= settings.Lifetime)
                trail.RemoveAt(0);

            if (trail.Length > 0 &&
                math.distancesq(trail[trail.Length - 1].Position, position) < settings.MinPointDistanceSq)
                return;

            if (trail.Length == settings.MaxPoints)
                trail.RemoveAt(0);

            trail.Add(new BallTrailPoint { Position = position });
        }

        private void DrawBatch(Material material, int count)
        {
            if (count == 0) return;

            Graphics.DrawMeshInstanced(_mesh, 0, material, _matrices, count,
                null,
                ShadowCastingMode.Off, false);
        }

        private static Mesh CreateTrailMesh(DynamicBuffer<BallTrailSample> samples)
        {
            int sectionCount = samples.Length;
            var vertices = new Vector3[sectionCount * 2];
            var colors = new Color[sectionCount * 2];
            var triangles = new int[(sectionCount - 1) * 6];

            for (int i = 0; i < sectionCount; i++)
            {
                float position = i / (sectionCount - 1f);
                BallTrailSample sample = samples[sectionCount - 1 - i];
                float halfWidth = i == 0 ? 0f : sample.Width * 0.5f;
                Color color = new Color(sample.Color.x, sample.Color.y, sample.Color.z, sample.Color.w);
                if (i == 0) color.a = 0f;

                int vertexIndex = i * 2;
                vertices[vertexIndex] = new Vector3(position - 0.5f, -halfWidth);
                vertices[vertexIndex + 1] = new Vector3(position - 0.5f, halfWidth);
                colors[vertexIndex] = color;
                colors[vertexIndex + 1] = color;

                if (i == sectionCount - 1) continue;
                int triangleIndex = i * 6;
                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = vertexIndex + 1;
                triangles[triangleIndex + 2] = vertexIndex + 3;
                triangles[triangleIndex + 3] = vertexIndex;
                triangles[triangleIndex + 4] = vertexIndex + 3;
                triangles[triangleIndex + 5] = vertexIndex + 2;
            }

            var mesh = new Mesh { name = "Ball Trail Spike" };
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
