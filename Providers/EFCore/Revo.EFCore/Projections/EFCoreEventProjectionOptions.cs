using Revo.Infrastructure.Projections;

namespace Revo.EFCore.Projections
{
    public class EFCoreEventProjectionOptions(bool isSynchronousProjection) : EventProjectionOptions
    {
        public bool IsSynchronousProjection { get; private set; } = isSynchronousProjection;
    }
}
