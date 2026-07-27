using Unity.Entities;

namespace Wizard
{
    [InternalBufferCapacity(3)]
    public struct PadMultiplierHistoryBufferElement : IBufferElementData
    {
        public int PadId;
    }
}
