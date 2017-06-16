using System;

namespace GTRevo.DataAccess.Entities
{
    public class DatabaseEntityAttribute : Attribute
    {
        public string SchemaSpace { get; set; }
    }
}
