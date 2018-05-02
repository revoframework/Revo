using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
                    (((args[i] is string)) && !args[i].Equals(args[i + o])) ||
                    (args[i] is IEnumerable && !(args[i] is string) && args[i + o] is IEnumerable && !(args[i + o] is string) && SameCollections((IEnumerable)args[i], (IEnumerable)args[i + o]))
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
                    (((args[i] is string)) && !args[i].Equals(args[i + 1])) ||
                    (args[i] is IEnumerable && !(args[i] is string) && args[i + 1] is IEnumerable && !(args[i+1] is string) && SameCollections((IEnumerable)args[i], (IEnumerable)args[i + 1]))
                )
                    return false;
            }
            return true;
        }


        public static bool SameCollections(IEnumerable left, IEnumerable right)
        {
            if ((left == null || right == null) && (left != right))
                return false;
            if (left == null && right == null)
                return true;
            var leftList = left.Cast<object>().ToList();
            var rightList = right.Cast<object>().ToList();
            return SameCollections(leftList, rightList);
        }

        public static bool SameCollections<T>(IEnumerable<T> left, IEnumerable<T> right)
        {
            if ((left == null || right == null) && (left != right))
                return false;
            if (left == null && right == null)
                return true;
            if (left.Count() != right.Count())
                return false;
            var leftNulls = left.Count(l => l == null);
            var rightNulls = right.Count(r => r == null);
            if (leftNulls != rightNulls)
                return false;
            var leftNN = left.Where(l => l != null).Distinct();
            var rightNN = right.Where(r => r != null).Distinct();

            foreach (var l in leftNN)
            {
                if (!rightNN.Any(r => r.Equals(l)))
                    return false;
            }
            return true;
        }

        public static bool SameCollections<S, T>(IEnumerable<S> left, IEnumerable<T> right, Func<S, T> projection)
        {
            var newleft = new List<T>();
            foreach (var l in left)
            {
                newleft.Add(projection.Invoke(l));
            }
            return SameCollections(newleft, right);
        }

        public static bool SameCollections<T>(IEnumerable<T> left, IEnumerable<T> right, out IEnumerable<T> common,
            out IEnumerable<T> leftHas, out IEnumerable<T> rightHas)
        {
            if (left == null && left != right)
            {
                common = null;
                leftHas = null;
                rightHas = right;
                return false;
            }
            if (right == null && left != right)
            {
                common = null;
                leftHas = left;
                rightHas = null;
                return false;
            }
            if (left == null && right == null)
            {
                common = null;
                leftHas = null;
                rightHas = null;
                return true;
            }
            common = left.Intersect(right);
            leftHas = left.Except(right);
            rightHas = right.Except(left);
            if (!(leftHas.Any() || rightHas.Any()))
            {
                return true;
            }
            return false;
        }

        public static bool SameCollections<S, T>(IEnumerable<S> left, IEnumerable<T> right, Func<S, T> projection,
            out IEnumerable<T> common,
            out IEnumerable<T> leftHas, out IEnumerable<T> rightHas)
        {
            var newleft = new List<T>();
            foreach (var l in left)
            {
                newleft.Add(projection.Invoke(l));
            }
            return SameCollections(newleft, right, out common, out leftHas, out rightHas);
        }
    }
}
