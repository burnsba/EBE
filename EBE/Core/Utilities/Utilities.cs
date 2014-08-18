using System;
using System.IO;
using System.Reflection;

namespace EBE.Core.Utilities
{
    public static class Application
    {
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
                    /*Assembly loadedAssembly =*/ Assembly.LoadFile(dll);
                }
                catch /*(FileLoadException loadEx)*/
                { } // The Assembly has already been loaded.

                //catch (BadImageFormatException imgEx)
                //{ } // If a BadImageFormatException exception is thrown, the file is not an assembly.
            } // foreach dll
        }
    }
}

