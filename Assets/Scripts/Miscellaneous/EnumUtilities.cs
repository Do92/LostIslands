using System;
using System.Collections.Generic;

namespace Miscellaneous
{
    public class EnumUtilities
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return (T[])Enum.GetValues(typeof(T));
        }
    }
}