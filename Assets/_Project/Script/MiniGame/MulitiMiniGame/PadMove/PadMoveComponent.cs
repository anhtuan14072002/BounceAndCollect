using System;
using LitMotion;
using Unity.Entities;
using Unity.Transforms;

namespace Wizard
{
    [Serializable]
    public struct PadMoveComponent : IComponentData
    {
        public Entity PadTween;
        public LocalTransform PadTweenInitialTransform;
        public MotionHandle Motion;

        public float targetPosition;
        public bool isInitialized;
        public bool isStop;
    }
}
