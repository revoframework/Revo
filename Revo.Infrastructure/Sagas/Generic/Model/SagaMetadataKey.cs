using System;
using Revo.DataAccess.Entities;

namespace Revo.Infrastructure.Sagas.Generic.Model
{
    [TablePrefix(NamespacePrefix = "REV", ColumnPrefix = "SMK")]
    [DatabaseEntity]
    public class SagaMetadataKey
    {
        public SagaMetadataKey(Guid id, Guid sagaId, string keyName, string keyValue)
        {
            Id = id;
            SagaId = sagaId;
            KeyName = keyName;
            KeyValue = keyValue;
        }

        protected SagaMetadataKey()
        {
        }

        public Guid Id { get; private set; }
        public Guid SagaId { get; private set; }
        public SagaMetadataRecord Saga { get; private set; }
        public string KeyName { get; private set; }
        public string KeyValue { get; set; }
    }
}
