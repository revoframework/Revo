using System;

namespace GTRevo.Core
{
    public static class Clock
    {
        private static Func<IClock> currentFunc;

        public static IClock Current => currentFunc();

        static Clock()
        {
            SystemClock sc = new SystemClock();
            currentFunc = () => sc;
        }
        
        public static void SetClock(Func<IClock> clock)
        {
            currentFunc = clock;
        }
    }
}
