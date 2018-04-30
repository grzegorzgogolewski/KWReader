using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace KW_Tools
{
    public class KwFromHtml
    {
        public string KwBody { get; set; }
        
        public InformacjePodstawowe KwInformacjePodstawowe = new InformacjePodstawowe();
        public List<Polozenie> KwPolozenieList = new List<Polozenie>();
        public List<Dzialka> KwDzialkaList = new List<Dzialka>();

        public KwFromHtml(string kwBody)
        {
            KwBody = kwBody;
        }

        public bool ParseKw()
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(KwBody);

            HtmlNodeCollection htmlTableCollection = htmlDoc.DocumentNode.SelectNodes("//table");

            foreach (HtmlNode tableNode in htmlTableCollection)
            {
                if (tableNode.InnerText.IndexOf("Rubryka 0.1 - Informacje podstawowe", StringComparison.Ordinal) > 0)
                {
                    foreach (HtmlNode row in tableNode.SelectNodes("tr"))
                    {
                        HtmlNodeCollection cells = row.SelectNodes("th|td");

                        for (int i = 0; i < cells.Count; i++)
                        {
                            switch (cells[i].InnerText)
                            {
                                case "Numer ksiegi":
                                    KwInformacjePodstawowe.NumerKsiegi =  cells[i + 1].InnerText;
                                    break;
                                case "Oznaczenie wydzialu":
                                    KwInformacjePodstawowe.OznaczenieWydzialu = cells[i + 1].InnerText;
                                    break;
                                case "A: nazwa sądu":
                                    KwInformacjePodstawowe.NazwaSadu = cells[i + 1].InnerText;
                                    break;
                                case "B: siedziba sądu":
                                    KwInformacjePodstawowe.SiedzibaSadu = cells[i + 1].InnerText;
                                    break;
                                case "C: kod wydziału":
                                    KwInformacjePodstawowe.KodWydzialu = cells[i + 1].InnerText;
                                    break;
                                case "D: numer wydziału":
                                    KwInformacjePodstawowe.NumerWydzialu = cells[i + 1].InnerText;
                                    break;
                                case "E: nazwa wydziału":
                                    KwInformacjePodstawowe.NazwaWydzialu = cells[i + 1].InnerText;
                                    break;
                            }
                        }
                    }
                } // Rubryka 0.1 - Informacje podstawowe

                if (tableNode.InnerText.IndexOf("Rubryka 1.3 - Położenie", StringComparison.Ordinal) > 0)
                {
                    Polozenie  kwPolozenie = new Polozenie();

                    foreach (HtmlNode row in tableNode.SelectNodes("tr"))
                    {
                        HtmlNodeCollection cells = row.SelectNodes("th|td");

                        for (int i = 0; i < cells.Count; i++)
                        {
                            switch (cells[i].InnerText)
                            {
                                case "1. Numer porządkowy":
                                    kwPolozenie.NumerPorzadkowy = cells.Count == 4 ? cells[i + 2].InnerText : null;
                                    break;
                                case "2. Województwo":
                                    kwPolozenie.Wojewodztwo = cells.Count==3 ? cells[i + 2].InnerText : null;
                                    break;
                                case "3. Powiat":
                                    kwPolozenie.Powiat = cells.Count==3 ? cells[i + 2].InnerText : null;
                                    break;
                                case "4. Gmina":
                                    kwPolozenie.Gmina = cells.Count == 3 ? cells[i + 2].InnerText : null;
                                    break;
                                case "5. Miejscowość":
                                    kwPolozenie.Miejscowosc = cells.Count == 3 ? cells[i + 2].InnerText : null;
                                    break;
                                case "6. Dzielnica":
                                    kwPolozenie.Dzielnica = cells.Count == 3 ? cells[i + 2].InnerText : null;

                                    KwPolozenieList.Add(kwPolozenie);
                                    kwPolozenie = new Polozenie();

                                    break;
                            }
                        }
                    }
                } // Rubryka 1.3 - Położenie

                if (tableNode.InnerText.IndexOf("Podrubryka 1.4.1 - Działka ewidencyjna", StringComparison.Ordinal) > 0)
                {
                    Dzialka  kwDzialka = new Dzialka();

                    foreach (HtmlNode tbody in tableNode.SelectNodes("tbody"))
                    {
                        foreach (HtmlNode row in tbody.SelectNodes("tr"))
                        {
                            HtmlNodeCollection cells = row.SelectNodes("th|td");

                            for (int i = 0; i < cells.Count; i++)
                            {
                                switch (cells[i].InnerText)
                                {
                                    case "1. Identyfikator działki":
                                        kwDzialka.IdentyfikatorDzialki = cells.Count == 4 ? cells[i + 2].InnerText : null;
                                        break;
                                    case "2. Numer działki":
                                        kwDzialka.NumerDzialki = cells.Count == 3 ? cells[i + 2].InnerText : null;
                                        break;
                                    case "A: numer obrębu ewidencyjnego":
                                        kwDzialka.NumerObrebuEwidencyjnego = cells.Count == 3 ? cells[i + 2].InnerText : null;
                                        break;
                                    case "B: nazwa obrębu ewidencyjnego":
                                        kwDzialka.NazwaObrebuEwidencyjnego = cells.Count == 3 ? cells[i + 2].InnerText : null;
                                        break;
                                    case "4. Położenie":
                                        HtmlNode htmlPolozenie = cells[i+1].SelectSingleNode("table");
                                        if (htmlPolozenie!=null)
                                        {
                                            HtmlNode htmlRow = htmlPolozenie.SelectSingleNode("tr");
                                            if (htmlRow!=null)
                                            {
                                                HtmlNodeCollection htmlCells = htmlRow.SelectNodes("td");
                                                kwDzialka.Polozenie = htmlCells[2].InnerText;
                                            }
                                        }
                                        break;
                                    case "5. Ulica":
                                        kwDzialka.Ulica = cells[i + 1].InnerText;
                                        break;
                                    case "6. Sposób korzystania":
                                        kwDzialka.SposobKorzystania = cells.Count == 3 ? cells[i + 2].InnerText : null;

                                        KwDzialkaList.Add(kwDzialka);
                                        kwDzialka  = new Dzialka();
                                        break;
                                }
                            }
                        }
                    }


                } // Rubryka 1.3 - Położenie
            }

            return true;
        }

        public string GetWojewodztwo(Dzialka dzialka)
        {
            return KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(dzialka.Polozenie)).Wojewodztwo;
        }

        public string GetPowiat(Dzialka dzialka)
        {
            return KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(dzialka.Polozenie)).Powiat;
        }

        public string GetGmina(Dzialka dzialka)
        {
            return KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(dzialka.Polozenie)).Gmina;
        }


        public string GetMiejscowosc(Dzialka dzialka)
        {
            try
            {
                return KwPolozenieList.Find(x =>  x.NumerPorzadkowy.Contains(dzialka.Polozenie)).Miejscowosc;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string GetDzielnica(Dzialka dzialka)
        {
            return KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(dzialka.Polozenie)).Dzielnica;
        }


    }
}
