using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace KWReader
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = @"c:\ZA1B-00021992-8.html";

            var doc = new HtmlDocument();

            doc.Load(path);

            var htmlBody = doc.DocumentNode.SelectNodes("//table//tr//td");

            


        }
    }
}
