using System.Collections.Generic;

namespace VsMapleBuilder
{
    public static class IEnumerableHelpers
    {
        public static string JoinAsString(this IEnumerable<string> list, string sep)
        {
            return string.Join(sep, list);
        }
    }
}
