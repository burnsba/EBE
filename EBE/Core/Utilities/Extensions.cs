using System;

namespace EBE.Core.Utilities
{
    public static class Extensions
    {
        /// <summary>
        /// Replaces the first instance of a sub-string in a string.
        /// </summary>
        /// <returns>String with contents replaced.</returns>
        /// <param name="text">Original string.</param>
        /// <param name="search">Value to search for.</param>
        /// <param name="replace">Value to replace with.</param>
        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);

            if (pos < 0)
            {
                return text;
            }

            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}

