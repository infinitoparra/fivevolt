using System;
using System.Collections.Generic;

namespace ZigBeeCoordinator.Lib.Infrastructure
{
    public static class EnumerableHelpers
    {
        public static string JoinAsString<T>(this IEnumerable<T> list, string seperator)
        {
            return string.Join(seperator, list);
        }

        public static void RemoveUntil(this IList<byte> list, byte b)
        {
            while (list.Count > 0 && list[0] != b)
            {
                list.RemoveAt(0);
            }
        }

        public static T Smallest<T>(this IEnumerable<T> list, Func<T, double> func)
        {
            T smallestItem = default(T);
            double smallestValue = double.MaxValue;
            foreach (T item in list)
            {
                double v = func(item);
                if (v < smallestValue)
                {
                    smallestItem = item;
                    smallestValue = v;
                }
            }
            return smallestItem;
        }
    }
}
