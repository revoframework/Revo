using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.DataAccess
{
    public abstract class DatabaseInitializerStub : IDatabaseInitializer
    {
        private bool isInitialized = false;

        public void Initialize()
        {
            if (!isInitialized)
            {
                DoInitialize();
                isInitialized = true;
            }
        }

        protected abstract void DoInitialize();
    }
}
