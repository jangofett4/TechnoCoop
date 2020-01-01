using System;

namespace TechnoCoop
{
    public static class Extensions
    {
        public static string Format(this string str, params object[] fmt)
        {
            return string.Format(str, fmt);
        }
    }
}