using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Wizard
{
    [Serializable]
    public struct PadRandomComponent : IComponentData
    {
        public Entity PadTween;
        public LocalTransform PadTweenInitialTransform;
        public bool isInitialized;
        public bool isTriggered;
        public float3 initializedUniformScale;
        public float3 targetUniformScale;
        public float targetScale;
        public int PadId;
        public float3 JumpForce;
        public int JumpForceXMin;
        public int JumpForceXMax;
        public int MultiplierMin;
        public int MultiplierMax;
        public float MultiplierRadius;
    }
}
