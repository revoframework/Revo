using System;
using Microsoft.EntityFrameworkCore;

namespace Revo.EFCore.DataAccess.Configuration
{
    public class ActionConfigurer(Action<DbContextOptionsBuilder> action) : IEFCoreConfigurer
    {
        public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            action(optionsBuilder);
        }
    }
}
