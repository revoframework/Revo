using System;

namespace Revo.DataAccess.Entities
{
    public class TablePrefixAttribute : Attribute
    {
        public TablePrefixAttribute()
        {
        }

        public string NamespacePrefix { get; set; }
        public string ColumnPrefix { get; set; }
    }
}
