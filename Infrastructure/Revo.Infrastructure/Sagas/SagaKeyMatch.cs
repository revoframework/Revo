using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Sagas
{
    public struct SagaKeyMatch
    {
        public Guid Id { get; set; }
        public Guid ClassId { get; set; }
    }
}
