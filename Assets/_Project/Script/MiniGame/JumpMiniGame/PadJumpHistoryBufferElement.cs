using Unity.Entities;

namespace Wizard
{
    [InternalBufferCapacity(3)]
    public struct PadJumpHistoryBufferElement : IBufferElementData
    {
        public int PadId;
    }
}
