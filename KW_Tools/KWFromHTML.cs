using System;
using System.Collections.Generic;
using System.Globalization;
using HtmlAgilityPack;

namespace KW_Tools
{
    public enum PolozenieTyp {Wojewodztwo, Powiat, Gmina, Miejscowosc, Dzielnica}

    public class KwFromHtml
    {
        public string KwBody { get; set; }
        
        public InformacjePodstawowe KwInformacjePodstawowe = new InformacjePodstawowe();
        public ZamkniecieKsiegi KwZamkniecieKsiegi = new ZamkniecieKsiegi();

        public List<Polozenie> KwPolozenieList = new List<Polozenie>();
        public List<Dzialka> KwDzialkaList = new List<Dzialka>();
        public List<Budynek> KwBudynekList = new List<Budynek>();

        public List<string> KwLog = new List<string>();

        public KwFromHtml(string kwBody)
        {
            KwBody = kwBody;
        }

        /// <summary>
        /// Funkcja przetwarzająca księgę wieczystą
        /// </summary>
        /// <returns>Zwraca liczbę błędów, uzyskanych podczas przetwarzania</returns>
        public int ParseKw()
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(KwBody);

            HtmlNodeCollection htmlTableCollection = htmlDoc.DocumentNode.SelectNodes("//table");

            foreach (HtmlNode tableNode in htmlTableCollection)
            {
                // --------------------------------------------------------------------------------
                // Rubryka 0.1 - Informacje podstawowe
                // --------------------------------------------------------------------------------

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
                } 

                // --------------------------------------------------------------------------------
                // Rubryka 0.3 - Dane o zamknieciu ksiegi wieczystej
                // --------------------------------------------------------------------------------

                if (tableNode.InnerText.IndexOf("Rubryka 0.3 - Dane o zamknieciu ksiegi wieczystej", StringComparison.Ordinal) > 0)
                {
                    foreach (HtmlNode row in tableNode.SelectNodes("tr"))
                    {
                        HtmlNodeCollection cells = row.SelectNodes("th|td");

                        for (int i = 0; i < cells.Count; i++)
                        {
                            switch (cells[i].InnerText)
                            {
                                case "Chwila zamkniecia ksiegi":
                                    if (cells.Count == 4)
                                    {
                                        KwZamkniecieKsiegi.ChwilaZamkniecia =  cells[i + 2].InnerText;
                                    }
                                    else
                                    {
                                        KwZamkniecieKsiegi.ChwilaZamkniecia = "- - -";
                                        KwLog.Add("KwZamkniecieKsiegi.ChwilaZamkniecia:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
                                    }
                                    
                                    break;

                                case "Podstawa zamkniecia ksiegi":
                                    if (cells.Count == 4)
                                    {
                                        KwZamkniecieKsiegi.PodstawaZamkniecia = cells[i + 2].InnerText;
                                    }
                                    else
                                    {
                                        KwZamkniecieKsiegi.PodstawaZamkniecia = "- - -";
                                        KwLog.Add("KwZamkniecieKsiegi.PodstawaZamkniecia:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
                                    }

                                    break;
                            }
                        }
                    }
                } 

                // --------------------------------------------------------------------------------
                // Rubryka 1.3 - Położenie
                // --------------------------------------------------------------------------------

                if (tableNode.InnerText.IndexOf("Rubryka 1.3 - Położenie", StringComparison.Ordinal) > 0)
                {
                    HtmlNodeCollection rows = tableNode.SelectNodes("tr");

                    if (rows.Count <= 2)
                    {
                        KwLog.Add("kwPolozenie:".PadRight(40) + "Brak informacji o położeniu");
                    }
                    else
                    {
                        // jeśli w tabeli z położeniem nie ma wiersza z dzielnicą, obiekt nie zostanie dodany
                        if (tableNode.InnerText.IndexOf("6. Dzielnica", StringComparison.Ordinal) <= 0)
                        {
                            Console.WriteLine("Położenie bez rekordu '6. Dzielnica'");
                            throw new Exception();
                        }

                        Polozenie  kwPolozenie = new Polozenie();

                        foreach (HtmlNode row in rows)
                        {
                            HtmlNodeCollection cells = row.SelectNodes("th|td");

                            for (int i = 0; i < cells.Count; i++)
                            {
                                switch (cells[i].InnerText)
                                {
                                    case "1. Numer porządkowy":
                                        if (cells.Count == 4)
                                        {
                                            kwPolozenie.NumerPorzadkowy = cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwPolozenie.NumerPorzadkowy = "brak";
                                            KwLog.Add("kwPolozenie.NumerPorzadkowy:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
                                        }
                                        break;

                                    case "2. Województwo":
                                        if (cells.Count == 3)
                                        {
                                            kwPolozenie.Wojewodztwo = cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwPolozenie.Wojewodztwo = "- - -";
                                            KwLog.Add("kwPolozenie.Wojewodztwo:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "3. Powiat":
                                        if (cells.Count == 3)
                                        {
                                            kwPolozenie.Powiat = cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwPolozenie.Powiat = "- - -";
                                            KwLog.Add("kwPolozenie.Powiat:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "4. Gmina":
                                        if (cells.Count == 3)
                                        {
                                            kwPolozenie.Gmina = cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwPolozenie.Gmina = "- - -";
                                            KwLog.Add("kwPolozenie.Gmina:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "5. Miejscowość":
                                        if (cells.Count == 3)
                                        {
                                            kwPolozenie.Miejscowosc = cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwPolozenie.Miejscowosc = "- - -";
                                            KwLog.Add("kwPolozenie.Miejscowosc:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "6. Dzielnica":
                                        if (cells.Count == 3)
                                        {
                                            kwPolozenie.Dzielnica = cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwPolozenie.Dzielnica = "- - -";
                                            KwLog.Add("kwPolozenie.Dzielnica:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }

                                        KwPolozenieList.Add(kwPolozenie);
                                        kwPolozenie = new Polozenie();

                                        break;
                                }
                            }
                        }
                    }
                } 

                // --------------------------------------------------------------------------------
                // Podrubryka 1.4.1 - Działka ewidencyjna
                // --------------------------------------------------------------------------------

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
                                        if (cells.Count == 4)
                                        {
                                            kwDzialka.IdentyfikatorDzialki =  cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwDzialka.IdentyfikatorDzialki = "- - -";
                                            KwLog.Add("kwDzialka.IdentyfikatorDzialki:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
                                        }
                                        break;

                                    case "2. Numer działki":
                                        if (cells.Count == 3)
                                        {
                                            kwDzialka.NumerDzialki = cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwDzialka.NumerDzialki = "- - -";
                                            KwLog.Add("kwDzialka.NumerDzialki:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "A: numer obrębu ewidencyjnego":
                                        if (cells.Count == 4)
                                        {
                                            kwDzialka.NumerObrebuEwidencyjnego = cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwDzialka.NumerObrebuEwidencyjnego = "- - -";
                                            KwLog.Add("kwDzialka.NumerObrebuEwidencyjnego:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
                                        }
                                        break;

                                    case "B: nazwa obrębu ewidencyjnego":
                                        if (cells.Count == 3)
                                        {
                                            kwDzialka.NazwaObrebuEwidencyjnego = cells[i + 2].InnerText;
                                        }
                                        else
                                        { 
                                            kwDzialka.NazwaObrebuEwidencyjnego = "- - -";
                                            KwLog.Add("kwDzialka.NazwaObrebuEwidencyjnego:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "4. Położenie": 

                                        // jeśli położenie jest zbudowane w postaci tabeli
                                        if (cells[i + 1].InnerHtml.IndexOf("table", StringComparison.Ordinal) > 0)
                                        {
                                            HtmlNode htmlPolozenieTable = cells[i+1].SelectSingleNode("table");

                                            HtmlNodeCollection htmlPolozenieRows = htmlPolozenieTable.SelectNodes("tr");

                                            // jeśli tabela zawiera wiersze
                                            if (htmlPolozenieRows != null)
                                            {
                                                // jeśli tabela zawiera więcej niż jeden wiersz
                                                kwDzialka.PolozenieMulti = htmlPolozenieRows.Count > 1;

                                                foreach (HtmlNode htmlPolozenieRow in htmlPolozenieRows)
                                                {
                                                    HtmlNodeCollection htmlPolozenieCells = htmlPolozenieRow.SelectNodes("td");

                                                    if (htmlPolozenieCells.Count == 3)
                                                    {
                                                        kwDzialka.PolozenieList.Add(htmlPolozenieCells[2].InnerText);
                                                    }
                                                    else
                                                    {
                                                        kwDzialka.PolozenieList.Add("- - -");
                                                        KwLog.Add("kwDzialka.Polozenie:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + htmlPolozenieCells.Count);
                                                    }
                                                }
                                            }
                                            else // jeśli tabela nie ma wierszy
                                            {
                                                kwDzialka.PolozenieMulti = false;
                                                kwDzialka.PolozenieList.Add("- - -");
                                                KwLog.Add("kwDzialka.Polozenie:".PadRight(40) + "Tabela nie posiada wierszy.");
                                            }
                                        }
                                        else // jeśli położenie jest pojedynczym tekstem
                                        {
                                            kwDzialka.PolozenieList.Add(cells[i + 1].InnerText);
                                            KwLog.Add("kwDzialka.Polozenie:".PadRight(40) + "Atrybut polozenie nie jest zbudowany w postaci tabeli.");
                                        }
                                        
                                        break;

                                    case "5. Ulica":

                                        // jeśli Ulica jest zbudowana w postaci tabeli
                                        if (cells[i + 1].InnerHtml.IndexOf("table", StringComparison.Ordinal) > 0)
                                        {
                                            HtmlNode htmlUlicaTable = cells[i+1].SelectSingleNode("table");

                                            HtmlNodeCollection htmlUlicaRows = htmlUlicaTable.SelectNodes("tr");

                                            // jeśli tabela zawiera wiersze
                                            if (htmlUlicaRows != null)
                                            {
                                                // jeśli tabela zawiera więcej niż jeden wiersz
                                                kwDzialka.UlicaMulti = htmlUlicaRows.Count > 1;

                                                foreach (HtmlNode htmlUlicaRow in htmlUlicaRows)
                                                {
                                                    HtmlNodeCollection htmlUlicaCells = htmlUlicaRow.SelectNodes("td");

                                                    if (htmlUlicaCells.Count == 3)
                                                    {
                                                        kwDzialka.UliceList.Add(htmlUlicaCells[2].InnerText);
                                                    }
                                                    else
                                                    {
                                                        kwDzialka.UliceList.Add("- - -");
                                                        KwLog.Add("kwDzialka.Ulica:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + htmlUlicaCells.Count);
                                                    }
                                                }
                                            }
                                            else // jeśli tabela nie ma wierszy
                                            {
                                                kwDzialka.UlicaMulti = false;
                                                kwDzialka.UliceList.Add("- - -");
                                                KwLog.Add("kwDzialka.Ulica:".PadRight(40) + "Tabela nie posiada wierszy.");
                                            }
                                        }
                                        else // jeśli ulica jest pojedynczym tekstem
                                        {
                                            kwDzialka.UliceList.Add(cells[i + 1].InnerText);
                                            KwLog.Add("kwDzialka.Ulica:".PadRight(40) + "Atrybut ulica nie jest zbudowany w postaci tabeli.");
                                        }
                                        
                                        break;

                                    case "6. Sposób korzystania":
                                        if (cells.Count == 3)
                                        {
                                            kwDzialka.SposobKorzystania = cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwDzialka.SposobKorzystania = "- - -";
                                            KwLog.Add("kwDzialka.SposobKorzystania:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }

                                        KwDzialkaList.Add(kwDzialka);
                                        kwDzialka  = new Dzialka();
                                        break;
                                }
                            }
                        }
                    }
                } 

                // --------------------------------------------------------------------------------
                // Podrubryka 1.4.2 - Budynek
                // --------------------------------------------------------------------------------

                if (tableNode.InnerText.IndexOf("Podrubryka 1.4.2 - Budynek", StringComparison.Ordinal) > 0)
                {
                    Budynek kwBudynek = new Budynek();

                    foreach (HtmlNode tbody in tableNode.SelectNodes("tbody"))
                    {
                        foreach (HtmlNode row in tbody.SelectNodes("tr"))
                        {
                            HtmlNodeCollection cells = row.SelectNodes("th|td");

                            for (int i = 0; i < cells.Count; i++)
                            {
                                switch (cells[i].InnerText)
                                {
                                    case "1. Identyfikator budynku":
                                        if (cells.Count == 4)
                                        {
                                            kwBudynek.IdentyfikatorBudynku =  cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwBudynek.IdentyfikatorBudynku = "- - -";
                                            KwLog.Add("kwDzialka.IdentyfikatorBudynku:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
                                        }
                                        break;

                                    case "2. Identyfikator działki":
                                        if (cells.Count == 2)
                                        {
                                            kwBudynek.IdentyfikatorDzialki =  cells[i + 1].InnerText;
                                        }
                                        else
                                        {
                                            kwBudynek.IdentyfikatorDzialki = "- - -";
                                            KwLog.Add("kwBudynek.IdentyfikatorDzialki:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 2, jest: " + cells.Count);
                                        }
                                        break;

                                    case "3. Położenie": 

                                        // jeśli położenie jest zbudowane w postaci tabeli
                                        if (cells[i + 1].InnerHtml.IndexOf("table", StringComparison.Ordinal) > 0)
                                        {
                                            HtmlNode htmlPolozenieTable = cells[i+1].SelectSingleNode("table");

                                            HtmlNodeCollection htmlPolozenieRows = htmlPolozenieTable.SelectNodes("tr");

                                            // jeśli tabela zawiera wiersze
                                            if (htmlPolozenieRows != null)
                                            {
                                                // jeśli tabela zawiera więcej niż jeden wiersz
                                                kwBudynek.PolozenieMulti = htmlPolozenieRows.Count > 1;

                                                foreach (HtmlNode htmlPolozenieRow in htmlPolozenieRows)
                                                {
                                                    HtmlNodeCollection htmlPolozenieCells = htmlPolozenieRow.SelectNodes("td");

                                                    if (htmlPolozenieCells.Count == 3)
                                                    {
                                                        kwBudynek.PolozenieList.Add(htmlPolozenieCells[2].InnerText);
                                                    }
                                                    else
                                                    {
                                                        kwBudynek.PolozenieList.Add("- - -");
                                                        KwLog.Add("kwBudynek.Polozenie:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + htmlPolozenieCells.Count);
                                                    }
                                                }
                                            }
                                            else // jeśli tabela nie ma wierszy
                                            {
                                                kwBudynek.PolozenieMulti = false;
                                                kwBudynek.PolozenieList.Add("- - -");
                                                KwLog.Add("kwBudynek.Polozenie:".PadRight(40) + "Tabela nie posiada wierszy.");
                                            }
                                        }
                                        else // jeśli położenie jest pojedynczym tekstem
                                        {
                                            kwBudynek.PolozenieList.Add(cells[i + 1].InnerText);
                                            KwLog.Add("kwBudynek.Polozenie:".PadRight(40) + "Atrybut polozenie nie jest zbudowany w postaci tabeli.");
                                        }
                                        
                                        break;

                                    case "A: nazwa ulicy (alei, placu)":
                                        if (cells.Count == 4)
                                        {
                                            kwBudynek.NazwaUlicy =  cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwBudynek.NazwaUlicy = "- - -";
                                            KwLog.Add("kwBudynek.NazwaUlicy:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
                                        }
                                        break;

                                    case "B: numer porządkowy budynku":
                                        if (cells.Count == 3)
                                        {
                                            kwBudynek.NumerPorzadkowy =  cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwBudynek.NumerPorzadkowy = "- - -";
                                            KwLog.Add("kwBudynek.NumerPorzadkowy:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "5. Liczba kondygnacji":
                                        if (cells.Count == 3)
                                        {
                                            kwBudynek.LiczbaKondygnacji =  cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwBudynek.LiczbaKondygnacji = "- - -";
                                            KwLog.Add("kwBudynek.LiczbaKondygnacji:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "6. Liczba samodzielnych lokali":
                                        if (cells.Count == 3)
                                        {
                                            kwBudynek.LiczbaLokali =  cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwBudynek.LiczbaLokali = "- - -";
                                            KwLog.Add("kwBudynek.LiczbaLokali:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "7. Powierzchnia użytkowa budynku":
                                        if (cells.Count == 3)
                                        {
                                            kwBudynek.PowierzchniaUzytkowa =  cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwBudynek.PowierzchniaUzytkowa = "- - -";
                                            KwLog.Add("kwBudynek.PowierzchniaUzytkowa:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "8. Przeznaczenie budynku":
                                        if (cells.Count == 3)
                                        {
                                            kwBudynek.Przeznaczenie =  cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwBudynek.Przeznaczenie = "- - -";
                                            KwLog.Add("kwBudynek.Przeznaczenie:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "9. Dalszy opis budynku":
                                        if (cells.Count == 3)
                                        {
                                            kwBudynek.DalszyOpis =  cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwBudynek.DalszyOpis = "- - -";
                                            KwLog.Add("kwBudynek.DalszyOpis:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "10. Nieruchomość, na której usytuowany jest budynek":
                                        if (cells.Count == 2)
                                        {
                                            kwBudynek.Nieruchomosc =  cells[i + 1].InnerText;
                                        }
                                        else
                                        {
                                            kwBudynek.Nieruchomosc = "- - -";
                                            KwLog.Add("kwBudynek.Nieruchomosc:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 2, jest: " + cells.Count);
                                        }
                                        break;

                                    case "11. Odrębność":
                                        if (cells.Count == 3)
                                        {
                                            kwBudynek.Odrebnosc =  cells[i + 2].InnerText;
                                        }
                                        else
                                        {
                                            kwBudynek.Odrebnosc = "- - -";
                                            KwLog.Add("kwBudynek.Odrebnosc:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }

                                        KwBudynekList.Add(kwBudynek);
                                        kwBudynek  = new Budynek();

                                        break;
                                }
                            }
                        }
                    }
                }
            }

            return KwLog.Count;
        }

        public string GetUlica(Dzialka dzialka)
        {
            string wynik = "";

            if (dzialka.UliceList.Count == 1)
            {
                wynik =  dzialka.UliceList[0];
            }
            else
            {
                foreach (string ulica in dzialka.UliceList)
                {
                    wynik = wynik + ulica + ", ";
                }

                wynik = wynik.Substring(0, wynik.Length - 2);
            }

            return wynik;

        }

        public string GetPolozenie(Dzialka dzialka, PolozenieTyp atrybut)
        {
            string wynik = "";

            if (dzialka.PolozenieList.Count == 1)
            {
                try
                {
                    switch (atrybut)
                    {
                        case PolozenieTyp.Wojewodztwo:
                            wynik = KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(dzialka.PolozenieList[0])).Wojewodztwo;
                            break;
                        case PolozenieTyp.Powiat:
                            wynik = KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(dzialka.PolozenieList[0])).Powiat;
                            break;
                        case PolozenieTyp.Gmina:
                            wynik = KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(dzialka.PolozenieList[0])).Gmina;
                            break;
                        case PolozenieTyp.Miejscowosc:
                            wynik = KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(dzialka.PolozenieList[0])).Miejscowosc;
                            break;
                        case PolozenieTyp.Dzielnica:
                            wynik = KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(dzialka.PolozenieList[0])).Dzielnica;
                            break;                    }
                }
                catch (Exception)
                {
                    wynik =  "- - -";
                }
            }
            else
            {
                foreach (string polozenie in dzialka.PolozenieList)
                {
                    try
                    {
                        switch (atrybut)
                        {
                            case PolozenieTyp.Wojewodztwo:
                                wynik = wynik + KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(polozenie)).Wojewodztwo + ", ";
                                break;
                            case PolozenieTyp.Powiat:
                                wynik = wynik + KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(polozenie)).Powiat + ", ";
                                break;
                            case PolozenieTyp.Gmina:
                                wynik = wynik + KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(polozenie)).Gmina + ", ";
                                break;
                            case PolozenieTyp.Miejscowosc:
                                wynik = wynik + KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(polozenie)).Miejscowosc + ", ";
                                break;
                            case PolozenieTyp.Dzielnica:
                                wynik = wynik + KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(polozenie)).Dzielnica + ", ";
                                break;
                        }
                        
                    }
                    catch (Exception)
                    {
                        wynik = wynik  + "- - -" + ", ";
                    }
                }

                wynik = wynik.Substring(0, wynik.Length - 2);
            }

            return wynik;
        }
    }
}
