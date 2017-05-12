using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.DataAccess.EF6
{
    public class DatabaseEntityAttribute : Attribute
    {
        public string SchemaSpace { get; set; }
    }
}
