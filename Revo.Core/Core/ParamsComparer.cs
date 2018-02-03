using System;

namespace Revo.Core.Core
{
    public static class ParamsComparer
    {
        /// <summary>
        /// Compares equality of passed objects - items in the first half are compared to the items in the second one.
        /// </summary>
        /// <param name="args">Objects to be compared.</param>
        /// <returns>Comparation result.</returns>
        public static bool Same(params object[] args)
        {
            if (args.Length % 2 != 0)
                throw new ArgumentException("Number of args must be even.");
            var o = args.Length / 2;
            for (int i = 0; i < args.Length / 2; i++)
            {
                if ((args[i] == null && args[i + o] == null))
                    continue;
                if ((args[i] != null && args[i + o] == null) ||
                    (args[i] == null && args[i + o] != null) ||
                    !args[i].Equals(args[i + o])
                )
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Compares equality of passed objects - compared pairs are sequentially after each other (a,a,b,b,c,c...)
        /// </summary>
        /// <param name="args">Objects to be compared.</param>
        /// <returns>Comparation result.</returns>
        public static bool SamePairs(params object[] args)
        {
            if (args.Length % 2 != 0)
                throw new ArgumentException("Number of args must be even.");
            for (int i = 0; i < args.Length; i += 2)
            {
                if ((args[i] == null && args[i + 1] == null))
                    continue;
                if ((args[i] != null && args[i + 1] == null) ||
                    (args[i] == null && args[i + 1] != null) ||
                    !args[i].Equals(args[i + 1])
                )
                    return false;
            }
            return true;
        }
    }
}
