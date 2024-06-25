using Revo.Infrastructure.Projections;

namespace Revo.EFCore.Projections
{
    public class EFCoreEventProjectionOptions : EventProjectionOptions
    {
        public EFCoreEventProjectionOptions(bool isSynchronousProjection)
        {
            IsSynchronousProjection = isSynchronousProjection;
        }

        public bool IsSynchronousProjection { get; private set; }
    }
}
