using System;

namespace Tools
{
    public static class SplitRegels
    {
        public static string[] Split(string list)
        {
            if (list == null)
                return new string[0];
            return list.Replace("\r", "").Split(new[] { "\n" }, StringSplitOptions.None);
        }
    }
}
