using System;

namespace GTRevo.DataAccess.EF6
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
