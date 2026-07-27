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
    }
}
