using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revo.DataAccess.EF6
{
    public class EF6ConnectionConfiguration
    {
        public EF6ConnectionConfiguration(string nameOrConnectionString)
        {
            NameOrConnectionString = nameOrConnectionString;
        }

        public string NameOrConnectionString { get; }
    }
}
