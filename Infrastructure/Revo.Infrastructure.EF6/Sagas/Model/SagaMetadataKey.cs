using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Revo.DataAccess.Entities;

namespace Revo.Infrastructure.EF6.Sagas.Model
{
    [TablePrefix(NamespacePrefix = "REV", ColumnPrefix = "SMK")]
    [DatabaseEntity]
    public class SagaMetadataKey
    {
        public Guid Id { get; set; }
        public Guid SagaId { get; set; }
        public string KeyName { get; set; }
        public string KeyValue { get; set; }
    }
}
