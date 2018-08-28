using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Configuration;
using Revo.DataAccess.EF6.Model;

namespace Revo.DataAccess.EF6
{
    public class EF6DataAccessConfigurationSection : IRevoConfigurationSection
    {
        public bool IsActive { get; set; }
        public bool UseAsPrimaryRepository { get; set; }
        public Type[] ConventionTypes { get; set; } = { typeof(CustomStoreConvention) };
        public EF6ConnectionConfiguration Connection { get; set; } = new EF6ConnectionConfiguration("EntityContext");
    }
}
