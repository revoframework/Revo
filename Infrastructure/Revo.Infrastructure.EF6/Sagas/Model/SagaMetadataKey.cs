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
        [Key, Column(Order = 0)]
        public Guid SagaId { get; set; }

        [Key, Column(Order = 1)]
        public string KeyName { get; set; }

        public string KeyValue { get; set; }
    }
}
