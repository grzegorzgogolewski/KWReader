using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KW_Tools;

namespace KWReader
{
    class Program
    {
        static void Main(string[] args)
        {

            StreamReader htmlFile = new StreamReader(new FileStream(@"c:\LU1A-00001473-3.html", FileMode.Open), Encoding.UTF8);

            KwFromHtml kw = new KwFromHtml(htmlFile.ReadToEnd());

            Dictionary<string, string> miejscowosc = kw.GetMiejscowosc();

            Console.ReadKey();

        }
    }
}
