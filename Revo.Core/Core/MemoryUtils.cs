using System;
using System.Runtime.InteropServices;

namespace Revo.Core.Core
{
    public static class MemoryUtils
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int memcmp(byte[] b1, byte[] b2, IntPtr count);

        public static bool Memcpm(this byte[] b1, byte[] b2)
        {
            return Memcpm(b1, b2);
        }

        public static bool Memcmp(byte[] b1, byte[] b2)
        {
            return b1.Length == b2.Length && memcmp(b1, b2, (IntPtr)b1.Length) == 0;
        }
    }
}
