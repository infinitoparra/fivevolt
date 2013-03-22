using System;
using System.Linq;
using System.Text;

namespace ZigBeeCoordinator.Lib.Infrastructure
{
    public static class ArrayHelpers
    {
        public static string ToHtml(this byte[] array)
        {
            return array
                .Select(b => Convert.ToString(b, 16).PadLeft(2, '0'))
                .JoinAsString(" ");
        }

        public static string ToHexString(this byte[] array, string delimeter = " ")
        {
            return array
                .Select(b => Convert.ToString(b, 16).PadLeft(2, '0'))
                .JoinAsString(delimeter);
        }

        public static string ToHexStringWithAscii(this byte[] array, int lineLength)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < array.Length; i += lineLength)
            {
                if (i > 0)
                {
                    result.AppendLine();
                }

                // hex
                for (int x = 0; x < lineLength; x++)
                {
                    int idx = x + i;
                    if (idx < array.Length)
                    {
                        result.Append(Convert.ToString(array[idx], 16).PadLeft(2, '0') + " ");
                    }
                    else
                    {
                        result.Append("   ");
                    }
                }

                // ascii
                for (int x = 0; x < lineLength; x++)
                {
                    int idx = x + i;
                    if (idx < array.Length)
                    {
                        byte b = array[idx];
                        if (b >= ' ' && b <= '~')
                        {
                            result.Append((char)b);
                        }
                        else
                        {
                            result.Append('.');
                        }
                    }
                    else
                    {
                        result.Append(" ");
                    }
                }
            }
            return result.ToString();
        }

        public static T[] Copy<T>(this T[] source)
        {
            T[] result = new T[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                result[i] = source[i];
            }
            return result;
        }

        public static T[] SelectRange<T>(this T[] source, int startIndex, int length)
        {
            T[] result = new T[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = source[i + startIndex];
            }
            return result;
        }

        public static T[] SelectRange<T>(this T[] source, int startIndex)
        {
            return SelectRange(source, startIndex, source.Length - startIndex);
        }

        public static bool EqualsAllItems<T>(this T[] first, T[] second)
        {
            if (first.Length != second.Length)
            {
                return false;
            }
            for (int i = 0; i < first.Length; i++)
            {
                if (!object.Equals(first[i], second[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
