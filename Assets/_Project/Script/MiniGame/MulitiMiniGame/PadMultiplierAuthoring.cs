using System;
using Unity.Entities;
using UnityEngine;

namespace Wizard
{
    [Serializable]
    public class PadMultiplierAuthoring : MonoBehaviour
    {
        public int multiNumber;
        public int padId;
        public int padIdMove;
        public float radius;
        class Baker : Baker<PadMultiplierAuthoring>
        {
            public override void Bake(PadMultiplierAuthoring padAuthoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PadMultiplierComponent
                {
                    MultiNumber = padAuthoring.multiNumber,
                    PadId = padAuthoring.padId,
                    Radius = padAuthoring.radius,
                    PadIdMove = padAuthoring.padIdMove
                });
            }
        }
    }
}
