using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Core.Core
{
    public static class Sleep
    {
        private static Func<ISleep> currentFunc;

        public static ISleep Current => currentFunc();

        static Sleep()
        {
            ThreadSleep ts = new ThreadSleep();
            currentFunc = () => ts;
        }

        public static void SetSleep(Func<ISleep> sleep)
        {
            currentFunc = sleep;
        }
    }
}
