using System;

namespace Revo.DataAccess.Entities
{
    public class DatabaseEntityAttribute : Attribute
    {
        public string SchemaSpace { get; set; }
    }
}
