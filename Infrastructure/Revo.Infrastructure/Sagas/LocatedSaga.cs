using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Sagas
{
    public struct LocatedSaga
    {
        public LocatedSaga(Guid id, Type sagaType)
        {
            Id = id;
            SagaType = sagaType;
        }

        public LocatedSaga(Type sagaType)
        {
            SagaType = sagaType;
            Id = null;
        }

        public Guid? Id { get; set; }
        public Type SagaType { get; set; }
    }
}
