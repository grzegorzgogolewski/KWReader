using System;
using System.IO;
using System.Reflection;

namespace Tools
{
    public static class Funkcje
    {
        public static string GetExecutingDirectoryName() 
        { 
            Uri location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase); 
            return new FileInfo(location.AbsolutePath).Directory?.FullName; 
        }
    }
}
