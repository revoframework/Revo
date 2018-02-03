using System;
using System.ComponentModel.DataAnnotations.Schema;
using Revo.Core.Globalization;
using Revo.Domain.Core;

namespace Revo.Domain.Entities.Basic
{
    public abstract class BasicClassifier : BasicAggregateRoot, IClassifier, ITranslatable
    {
        protected BasicClassifier(Guid id, string code): base(id)
        {
            Code = code;
        }

        protected BasicClassifier()
        {
        }

        public string Code { get; private set; }

        [NotMapped]
        public string Name { get; private set; }

        [NotMapped]
        public string Culture { get; set; }
    }
}
