#if NET35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public class StringNet35
    {
        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null)
                return true;
            for (int index = 0; index < value.Length; ++index)
            {
                if (!char.IsWhiteSpace(value[index]))
                    return false;
            }
            return true;
        }
    }
}
#endif