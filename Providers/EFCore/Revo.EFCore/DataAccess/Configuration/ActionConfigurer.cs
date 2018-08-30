using System;
using Microsoft.EntityFrameworkCore;

namespace Revo.EFCore.DataAccess.Configuration
{
    public class ActionConfigurer : IEFCoreConfigurer
    {
        private readonly Action<DbContextOptionsBuilder> action;

        public ActionConfigurer(Action<DbContextOptionsBuilder> action)
        {
            this.action = action;
        }
        
        public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            action(optionsBuilder);
        }
    }
}
