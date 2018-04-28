using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace KW_Tools
{
    public class KwFromHtml
    {
        public string KwBody { get; set; }

        public KwFromHtml(string kwBody)
        {
            KwBody = kwBody;
        }

        public Dictionary<string, string> GetMiejscowosc()
        {
            Dictionary<string, string> miejscowoscDictionary = new Dictionary<string, string>();

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(KwBody);

            HtmlNodeCollection htmlNodes = htmlDoc.DocumentNode.SelectNodes("//td");

            for (int i = 0; i < htmlNodes.Elements().Count(); i++)
            {
                HtmlNode elem = htmlNodes.Elements().ElementAt(i);

                if (elem.InnerText == "5. Miejscowość")
                {
                    string miejscowoscIndeks = htmlNodes.Elements().ElementAt(i+1).InnerText;
                    string miejscowosc = htmlNodes.Elements().ElementAt(i+2).InnerText;
                    miejscowoscDictionary.Add(miejscowoscIndeks, miejscowosc);
                    Console.WriteLine("Miejscowosc: {0} : {1}", miejscowoscIndeks, miejscowosc);
                }

            }

            return miejscowoscDictionary;
        }

    }
}
