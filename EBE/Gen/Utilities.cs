using System;
using System.IO;
using System.Reflection;

namespace EBE
{
	public static class Utilities
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

        public static void LoadLibraries()
        {
            // NHibernate was complaining about the mysql dll not being loaded.
            //
            // http://stackoverflow.com/questions/1288288/how-to-load-all-assemblies-from-within-your-bin-directory

            string binPath = System.AppDomain.CurrentDomain.BaseDirectory;

            foreach (string dll in Directory.GetFiles(binPath, "*.dll", SearchOption.AllDirectories))
            {
                try
                {                    
                    Assembly loadedAssembly = Assembly.LoadFile(dll);
                }
                catch (FileLoadException loadEx)
                { } // The Assembly has already been loaded.
                catch (BadImageFormatException imgEx)
                { } // If a BadImageFormatException exception is thrown, the file is not an assembly.

            } // foreach dll
        }

        public static byte[] GetBytes(string hex)
        {
            //http://stackoverflow.com/questions/321370/convert-hex-string-to-byte-array

            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
	}
}

