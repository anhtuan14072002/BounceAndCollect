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
        public Entity RandomLabel;
        public Entity JumpLabel;
        public Entity Multiplier2Label;
        public Entity Multiplier3Label;
        public Entity Multiplier4Label;
        public LocalTransform PadTweenInitialTransform;
        public LocalTransform ResolvedLabelTransform;
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
        public float4 ResolvedColor;
    }
}
