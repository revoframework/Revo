using System;

namespace Revo.Infrastructure.Sagas
{
    public class LocatedSaga
    {
        private LocatedSaga(Guid? id, Type sagaType)
        {
            Id = id;
            SagaType = sagaType;
        }

        public Guid? Id { get; }
        public Type SagaType { get; }
        
        public static LocatedSaga FromId(Guid id, Type sagaType)
        {
            return new LocatedSaga(id, sagaType);
        }

        public static LocatedSaga CreateNew(Type sagaType)
        {
            return new LocatedSaga(null, sagaType);
        }

        public override bool Equals(object obj)
        {
            return obj is LocatedSaga other
                   && other.SagaType == SagaType
                   && other.Id == Id;
        }

        public override int GetHashCode() => (SagaType.GetHashCode() * 397) ^ (Id != null ? Id.GetHashCode() : 0);
    }
}
