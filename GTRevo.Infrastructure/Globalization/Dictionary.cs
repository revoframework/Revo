using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain.Basic;

namespace GTRevo.Infrastructure.Globalization
{
    [TablePrefix(NamespacePrefix = "REV", ColumnPrefix = "DIC")]
    public class Dictionary: BasicAggregateRoot
    {
        public Dictionary(Guid id): base(id)
        {
            
        }

        protected Dictionary()
        {
        }

        public string ClassName { get; set; }
        public string Key { get; set; }
        public string Translation { get; set; }
        public string Culture { get; set; }
    }
}
