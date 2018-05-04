using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace KW_Tools
{
    /// <summary>
    /// Typ wyliczeniowy dla atrybutu położenia, wymagany dla funkcji zwracającej atrybut danego rodzaju
    /// </summary>
    public enum PolozenieTyp {Wojewodztwo, Powiat, Gmina, Miejscowosc, Dzielnica}

    /// <summary>
    /// klasa parsująca księgę z KW
    /// </summary>
    public class KwFromHtml
    {
        public string KwBody { get; set; }
        
        public InformacjePodstawowe KwInformacjePodstawowe = new InformacjePodstawowe();
        public ZamkniecieKsiegi KwZamkniecieKsiegi = new ZamkniecieKsiegi();
        public List<Polozenie> KwPolozenieList = new List<Polozenie>();
        public List<Dzialka> KwDzialkaList = new List<Dzialka>();
        public List<Budynek> KwBudynekList = new List<Budynek>();
        public List<Lokal> KwLokalList = new List<Lokal>();
        public Obszar KwObszar = new Obszar();

        /// <summary>
        /// lista przechowująca błędy wychwycone podczas przetwarzania księgi wieczystej
        /// </summary>
        public List<string> KwLog = new List<string>();

        /// <summary>
        /// konstruktor klasy parsującej treść księgi wieczystej
        /// </summary>
        /// <param name="kwBody">przechowuje treść księgi wieczystej</param>
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

            // kolekcja wszystkich tabel w pliku html
            HtmlNodeCollection htmlTableCollection = htmlDoc.DocumentNode.SelectNodes("//table");

            // przetwarzanie każdej z tabel w pliku html
            foreach (HtmlNode tableNode in htmlTableCollection)
            {
                // --------------------------------------------------------------------------------
                // Rubryka 0.1 - Informacje podstawowe
                // --------------------------------------------------------------------------------

                // szukaj tabeli z rubryka informacji podstawowych
                if (tableNode.InnerText.IndexOf("Rubryka 0.1 - Informacje podstawowe", StringComparison.Ordinal) > 0)
                {
                    // przetwórz wszystkie wiersze w tabeli
                    foreach (HtmlNode row in tableNode.SelectNodes("tr"))
                    {
                        // przetwórz wszsytkie komórki danego wiersza, włącznie z nagłowkami wierszy
                        HtmlNodeCollection cells = row.SelectNodes("th|td");

                        // specjalnie "for" by móc przemieszczać się po konkretnych komórkach
                        for (int i = 0; i < cells.Count; i++)
                        {
                            // szukanie konkretnej komórki w wierszu by móc odnieść się do wartośći
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

                // szukaj tabeli z rubryka zamknięcia KW
                if (tableNode.InnerText.IndexOf("Rubryka 0.3 - Dane o zamknieciu ksiegi wieczystej", StringComparison.Ordinal) > 0)
                {
                    // przetwórz wszystkie wiersze w tabeli
                    foreach (HtmlNode row in tableNode.SelectNodes("tr"))
                    {
                        // przetwórz wszsytkie komórki danego wiersza, włącznie z nagłowkami wierszy
                        HtmlNodeCollection cells = row.SelectNodes("th|td");

                        // specjalnie "for" by móc przemieszczać się po konkretnych komórkach
                        for (int i = 0; i < cells.Count; i++)
                        {
                            switch (cells[i].InnerText)
                            {
                                case "Chwila zamkniecia ksiegi":
                                    if (cells.Count == 4) // testowanie ilości komórek w wierszu
                                    {
                                        KwZamkniecieKsiegi.ChwilaZamkniecia =  cells[i + 2].InnerText;
                                    }
                                    else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                    {
                                        KwZamkniecieKsiegi.ChwilaZamkniecia = "- - -";
                                        KwLog.Add("KwZamkniecieKsiegi.ChwilaZamkniecia:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
                                    }
                                    
                                    break;

                                case "Podstawa zamkniecia ksiegi":
                                    if (cells.Count == 4) // testowanie ilości komórek w wierszu
                                    {
                                        KwZamkniecieKsiegi.PodstawaZamkniecia = cells[i + 2].InnerText;
                                    }
                                    else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
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

                // szukaj tabeli z rubryka położenia
                if (tableNode.InnerText.IndexOf("Rubryka 1.3 - Położenie", StringComparison.Ordinal) > 0)
                {
                    // przetwórz wszystkie wiersze w tabeli
                    HtmlNodeCollection rows = tableNode.SelectNodes("tr");

                    // jeśli dane o położeniu zapisane są w postaci tabeli to musi ona posiadać conajmniej trzy wiersze
                    if (rows.Count <= 2)
                    {
                        // utworz obiek z pustym położeniem i dodaj go do kolekcji
                        Polozenie kwPolozenie = new Polozenie
                        {
                            NumerPorzadkowy = "brak",
                            Wojewodztwo = "- - -",
                            Dzielnica = "- - -",
                            Gmina = "- - -",
                            Miejscowosc = "- - -",
                            Powiat = "- - -"
                        };

                        KwPolozenieList.Add(kwPolozenie);

                        KwLog.Add("kwPolozenie:".PadRight(40) + "Brak informacji o położeniu, tabela nie właściwej liczby wierszy.");
                    }
                    else
                    {
                        // jeśli w tabeli z położeniem nie ma wiersza z dzielnicą, obiekt nie zostanie dodany
                        if (tableNode.InnerText.IndexOf("6. Dzielnica", StringComparison.Ordinal) <= 0)
                        {
                            // wygeneruj wyjątek bo plik html może być uszkodzony
                            Console.WriteLine("Położenie bez rekordu '6. Dzielnica'! Sprawdź poprawność pliku HTML.");
                            throw new Exception();
                        }

                        Polozenie  kwPolozenie = new Polozenie();

                        // zbadaj każdy wiersz tabeli położenia
                        foreach (HtmlNode row in rows)
                        {
                            // kolekcja wszystkich komórek danego wiersza włącznie z nagłowkiem
                            HtmlNodeCollection cells = row.SelectNodes("th|td");

                            // specjalnie 'for' by móc swobodnie poruszać się po komórkach
                            for (int i = 0; i < cells.Count; i++)
                            {
                                // testowanie wartośco każdej komórki w wierszu
                                switch (cells[i].InnerText)
                                {
                                    case "1. Numer porządkowy":
                                        if (cells.Count == 4) // testowanie ilości komórek w wierszu
                                        {
                                            kwPolozenie.NumerPorzadkowy = cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwPolozenie.NumerPorzadkowy = "brak";
                                            KwLog.Add("kwPolozenie.NumerPorzadkowy:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
                                        }
                                        break;

                                    case "2. Województwo":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwPolozenie.Wojewodztwo = cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwPolozenie.Wojewodztwo = "- - -";
                                            KwLog.Add("kwPolozenie.Wojewodztwo:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "3. Powiat":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwPolozenie.Powiat = cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwPolozenie.Powiat = "- - -";
                                            KwLog.Add("kwPolozenie.Powiat:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "4. Gmina":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwPolozenie.Gmina = cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwPolozenie.Gmina = "- - -";
                                            KwLog.Add("kwPolozenie.Gmina:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "5. Miejscowość":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwPolozenie.Miejscowosc = cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwPolozenie.Miejscowosc = "- - -";
                                            KwLog.Add("kwPolozenie.Miejscowosc:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "6. Dzielnica":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwPolozenie.Dzielnica = cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
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
                    // jeśli w tabeli z działką nie ma wiersza ze sposobem korzystania, obiekt nie zostanie dodany
                    if (tableNode.InnerText.IndexOf("6. Sposób korzystania", StringComparison.Ordinal) <= 0)
                    {
                        // wygeneruj wyjątek bo plik html może być uszkodzony
                        Console.WriteLine("Położenie bez rekordu '6. Sposób korzystania'! Sprawdź poprawność pliku HTML.");
                        throw new Exception();
                    }

                    Dzialka  kwDzialka = new Dzialka();

                    // działka ewidencyjna jes dodatkowo umieszczona między znacznikami tbody
                    foreach (HtmlNode tbody in tableNode.SelectNodes("tbody"))
                    {
                        // przetwórz każdy wiersz
                        foreach (HtmlNode row in tbody.SelectNodes("tr"))
                        {
                            // kolekcja wszystkich komórek w danym wierszu włącznie z nagłówkiem
                            HtmlNodeCollection cells = row.SelectNodes("th|td");

                            for (int i = 0; i < cells.Count; i++)
                            {
                                switch (cells[i].InnerText)
                                {
                                    case "1. Identyfikator działki":
                                        if (cells.Count == 4) // testowanie ilości komórek w wierszu
                                        {
                                            kwDzialka.IdentyfikatorDzialki =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwDzialka.IdentyfikatorDzialki = "- - -";
                                            KwLog.Add("kwDzialka.IdentyfikatorDzialki:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
                                        }
                                        break;

                                    case "2. Numer działki":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwDzialka.NumerDzialki = cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwDzialka.NumerDzialki = "- - -";
                                            KwLog.Add("kwDzialka.NumerDzialki:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "A: numer obrębu ewidencyjnego":
                                        if (cells.Count == 4) // testowanie ilości komórek w wierszu
                                        {
                                            kwDzialka.NumerObrebuEwidencyjnego = cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwDzialka.NumerObrebuEwidencyjnego = "- - -";
                                            KwLog.Add("kwDzialka.NumerObrebuEwidencyjnego:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
                                        }
                                        break;

                                    case "B: nazwa obrębu ewidencyjnego":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwDzialka.NazwaObrebuEwidencyjnego = cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
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

                                            // wybierz wiersze z tabeli
                                            HtmlNodeCollection htmlPolozenieRows = htmlPolozenieTable.SelectNodes("tr");

                                            // jeśli tabela zawiera wiersze
                                            if (htmlPolozenieRows != null)
                                            {
                                                // jeśli tabela zawiera więcej niż jeden wiersz
                                                kwDzialka.PolozenieMulti = htmlPolozenieRows.Count > 1;

                                                // przetwórz każdy wiersz tabeli
                                                foreach (HtmlNode htmlPolozenieRow in htmlPolozenieRows)
                                                {
                                                    // kolekcja wsystkich komórek tabeli
                                                    HtmlNodeCollection htmlPolozenieCells = htmlPolozenieRow.SelectNodes("td");

                                                    if (htmlPolozenieCells.Count == 3) // testowanie ilości komórek w wierszu
                                                    {
                                                        kwDzialka.PolozenieList.Add(htmlPolozenieCells[2].InnerText);
                                                    }
                                                    else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
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
                                            kwDzialka.PolozenieMulti = false;
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

                                                    if (htmlUlicaCells.Count == 3) // testowanie ilości komórek w wierszu
                                                    {
                                                        kwDzialka.UliceList.Add(htmlUlicaCells[2].InnerText);
                                                    }
                                                    else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
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
                                            kwDzialka.UlicaMulti = false;
                                            kwDzialka.UliceList.Add(cells[i + 1].InnerText);
                                            // KwLog.Add("kwDzialka.Ulica:".PadRight(40) + "Atrybut ulica nie jest zbudowany w postaci tabeli.");
                                        }
                                        
                                        break;

                                    case "6. Sposób korzystania":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwDzialka.SposobKorzystania = cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
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
                    // jeśli w tabeli z budynkiem nie ma wiersza z odręnością, obiekt nie zostanie dodany
                    if (tableNode.InnerText.IndexOf("11. Odrębność", StringComparison.Ordinal) <= 0)
                    {
                        // wygeneruj wyjątek bo plik html może być uszkodzony
                        Console.WriteLine("Położenie bez rekordu '11. Odrębność'! Sprawdź poprawność pliku HTML.");
                        throw new Exception();
                    }

                    Budynek kwBudynek = new Budynek();

                    // budynek jest dodatkowo umiesczony między znacznikami tbody
                    foreach (HtmlNode tbody in tableNode.SelectNodes("tbody"))
                    {
                        // przetwórz wszystkie wiersze tabeli
                        foreach (HtmlNode row in tbody.SelectNodes("tr"))
                        {
                            // kolekcja wszystkich komórek wiersza
                            HtmlNodeCollection cells = row.SelectNodes("th|td");

                            for (int i = 0; i < cells.Count; i++)
                            {
                                switch (cells[i].InnerText)
                                {
                                    case "1. Identyfikator budynku":
                                        if (cells.Count == 4) // testowanie ilości komórek w wierszu
                                        {
                                            kwBudynek.IdentyfikatorBudynku =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwBudynek.IdentyfikatorBudynku = "- - -";
                                            KwLog.Add("kwDzialka.IdentyfikatorBudynku:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
                                        }
                                        break;
                                    
                                    case "2. Identyfikator działki":

                                        // jeśli położenie jest zbudowane w postaci tabeli
                                        if (cells[i + 1].InnerHtml.IndexOf("table", StringComparison.Ordinal) > 0)
                                        {
                                            HtmlNode htmlIdentyfikatorTable = cells[i+1].SelectSingleNode("table");

                                            HtmlNodeCollection htmlIdentyfikatorRows = htmlIdentyfikatorTable.SelectNodes("tr");

                                            // jeśli tabela zawiera wiersze
                                            if (htmlIdentyfikatorRows != null)
                                            {
                                                // jeśli tabela zawiera więcej niż jeden wiersz
                                                kwBudynek.IdentyfikatorDzialkiMulti = htmlIdentyfikatorRows.Count > 1;

                                                foreach (HtmlNode htmlIdentyfikatorRow in htmlIdentyfikatorRows)
                                                {
                                                    HtmlNodeCollection htmlIdentyfikatorCells = htmlIdentyfikatorRow.SelectNodes("td");

                                                    if (htmlIdentyfikatorCells.Count == 3) // testowanie ilości komórek w wierszu
                                                    {
                                                        kwBudynek.IdentyfikatorDzialkiList.Add(htmlIdentyfikatorCells[2].InnerText);
                                                    }
                                                    else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                                    {
                                                        kwBudynek.IdentyfikatorDzialkiList.Add("- - -");
                                                        KwLog.Add("kwBudynek.Polozenie:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + htmlIdentyfikatorCells.Count);
                                                    }
                                                }
                                            }
                                            else // jeśli tabela nie ma wierszy
                                            {
                                                kwBudynek.IdentyfikatorDzialkiMulti = false;
                                                kwBudynek.IdentyfikatorDzialkiList.Add("- - -");
                                                KwLog.Add("kwBudynek.Polozenie:".PadRight(40) + "Tabela nie posiada wierszy.");
                                            }
                                        }
                                        else // jeśli tabela nie ma wierszy
                                        {
                                            kwBudynek.IdentyfikatorDzialkiMulti = false;
                                            kwBudynek.IdentyfikatorDzialkiList.Add("- - -");
                                            KwLog.Add("kwBudynek.IdentyfikatorDzialkiList:".PadRight(40) + "Tabela nie posiada wierszy.");
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

                                                    if (htmlPolozenieCells.Count == 3) // testowanie ilości komórek w wierszu
                                                    {
                                                        kwBudynek.PolozenieList.Add(htmlPolozenieCells[2].InnerText);
                                                    }
                                                    else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
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
                                            kwBudynek.PolozenieMulti = false;
                                            kwBudynek.PolozenieList.Add(cells[i + 1].InnerText);
                                            KwLog.Add("kwBudynek.Polozenie:".PadRight(40) + "Atrybut polozenie nie jest zbudowany w postaci tabeli.");
                                        }
                                        
                                        break;

                                    case "A: nazwa ulicy (alei, placu)":
                                        if (cells.Count == 4) // testowanie ilości komórek w wierszu
                                        {
                                            kwBudynek.NazwaUlicy =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwBudynek.NazwaUlicy = "- - -";
                                            KwLog.Add("kwBudynek.NazwaUlicy:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
                                        }
                                        break;

                                    case "B: numer porządkowy budynku":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwBudynek.NumerPorzadkowy =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwBudynek.NumerPorzadkowy = "- - -";
                                            KwLog.Add("kwBudynek.NumerPorzadkowy:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "5. Liczba kondygnacji":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwBudynek.LiczbaKondygnacji =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwBudynek.LiczbaKondygnacji = "- - -";
                                            KwLog.Add("kwBudynek.LiczbaKondygnacji:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "6. Liczba samodzielnych lokali":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwBudynek.LiczbaLokali =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwBudynek.LiczbaLokali = "- - -";
                                            KwLog.Add("kwBudynek.LiczbaLokali:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "7. Powierzchnia użytkowa budynku":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwBudynek.PowierzchniaUzytkowa =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwBudynek.PowierzchniaUzytkowa = "- - -";
                                            KwLog.Add("kwBudynek.PowierzchniaUzytkowa:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "8. Przeznaczenie budynku":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwBudynek.Przeznaczenie =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwBudynek.Przeznaczenie = "- - -";
                                            KwLog.Add("kwBudynek.Przeznaczenie:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "9. Dalszy opis budynku":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwBudynek.DalszyOpis =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwBudynek.DalszyOpis = "- - -";
                                            KwLog.Add("kwBudynek.DalszyOpis:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "10. Nieruchomość, na której usytuowany jest budynek":
                                        if (cells.Count == 2) // testowanie ilości komórek w wierszu
                                        {
                                            kwBudynek.Nieruchomosc =  cells[i + 1].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwBudynek.Nieruchomosc = "- - -";
                                            KwLog.Add("kwBudynek.Nieruchomosc:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 2, jest: " + cells.Count);
                                        }
                                        break;

                                    case "11. Odrębność":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwBudynek.Odrebnosc =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
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

                // --------------------------------------------------------------------------------
                // Podrubryka 1.4.4 - Lokal
                // --------------------------------------------------------------------------------

                if (tableNode.InnerText.IndexOf("Podrubryka 1.4.4 - Lokal", StringComparison.Ordinal) > 0)
                {
                    // jeśli w tabeli z lokalem nie ma wiersza z odręnością, obiekt nie zostanie dodany
                    if (tableNode.InnerText.IndexOf("11. Odrębność", StringComparison.Ordinal) <= 0)
                    {
                        // wygeneruj wyjątek bo plik html może być uszkodzony
                        Console.WriteLine("Położenie bez rekordu '11. Odrębność'! Sprawdź poprawność pliku HTML.");
                        throw new Exception();
                    }

                    Lokal kwLokal = new Lokal();

                    // lokal jest dodatkowo umiesczony między znacznikami tbody
                    foreach (HtmlNode tbody in tableNode.SelectNodes("tbody"))
                    {
                        // przetwórz wszystkie wiersze tabeli
                        foreach (HtmlNode row in tbody.SelectNodes("tr"))
                        {
                            // kolekcja wszystkich komórek wiersza
                            HtmlNodeCollection cells = row.SelectNodes("th|td");

                            for (int i = 0; i < cells.Count; i++)
                            {
                                switch (cells[i].InnerText)
                                {
                                    case "1. Identyfikator lokalu":
                                        if (cells.Count == 4) // testowanie ilości komórek w wierszu
                                        {
                                            kwLokal.IdentyfikatorLokalu =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwLokal.IdentyfikatorLokalu = "- - -";
                                            KwLog.Add("kwLokal.IdentyfikatorLokalu:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
                                        }
                                        break;

                                    case "2. Ulica":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwLokal.Ulica =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwLokal.Ulica = "- - -";
                                            KwLog.Add("kwLokal.Ulica:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "3. Numer budynku":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwLokal.NumerBudynku =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwLokal.NumerBudynku = "- - -";
                                            KwLog.Add("kwLokal.NumerBudynku:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                        break;

                                    case "4. Numer lokalu":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwLokal.NumerLokalu =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwLokal.NumerLokalu = "- - -";
                                            KwLog.Add("kwLokal.NumerLokalu:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }

                                        break;

                                    case "5. Przeznaczenie lokalu":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwLokal.PrzeznaczenieLokalu =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwLokal.PrzeznaczenieLokalu = "- - -";
                                            KwLog.Add("kwLokal.PrzeznaczenieLokalu:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }

                                        break;

                                    case "6. Opis lokalu":
                                        if (cells.Count == 2) // testowanie ilości komórek w wierszu
                                        {
                                            kwLokal.OpisLokalu =  cells[i + 1].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwLokal.OpisLokalu = "- - -";
                                            KwLog.Add("kwLokal.OpisLokalu:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 2, jest: " + cells.Count);
                                        }

                                        break;

                                    case "7. Opis pomieszczeń przynależnych":
                                        if (cells.Count == 2) // testowanie ilości komórek w wierszu
                                        {
                                            kwLokal.OpisPomieszczenPrzynależnych =  cells[i + 1].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwLokal.OpisPomieszczenPrzynależnych = "- - -";
                                            KwLog.Add("kwLokal.OpisPomieszczenPrzynależnych:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 2, jest: " + cells.Count);
                                        }

                                        break;

                                    case "8. Kondygnacja":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwLokal.Kondygnacja =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwLokal.Kondygnacja = "- - -";
                                            KwLog.Add("kwLokal.Kondygnacja:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }

                                        break;

                                    case "10. Nieruchomość zabudowana budynkiem":
                                        if (cells.Count == 2) // testowanie ilości komórek w wierszu
                                        {
                                            kwLokal.Nieruchomosc =  cells[i + 1].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwLokal.Nieruchomosc = "- - -";
                                            KwLog.Add("kwLokal.Nieruchomosc:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 2, jest: " + cells.Count);
                                        }

                                        break;

                                    case "11. Odrębność":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                            kwLokal.Odrebnosc =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwLokal.Odrebnosc = "- - -";
                                            KwLog.Add("kwLokal.Odrebnosc:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }

                                        // w lokalu nie ma atrybutu położenie
                                        kwLokal.PolozenieMulti = KwPolozenieList.Count > 1;
                                        kwLokal.PolozenieList.Add("1");

                                        KwLokalList.Add(kwLokal);
                                        kwLokal  = new Lokal();

                                        break;
                                }
                            }
                        }

                    }
                }

                // --------------------------------------------------------------------------------
                // Rubryka 1.5 - Obszar
                // --------------------------------------------------------------------------------

                if (tableNode.InnerText.IndexOf("Rubryka 1.5 - Obszar", StringComparison.Ordinal) > 0)
                {
                    // przetwórz wszystkie wiersze w tabeli
                    HtmlNodeCollection rows = tableNode.SelectNodes("tr");

                    // jeśli dane o położeniu zapisane są w postaci tabeli to musi ona posiadać conajmniej trzy wiersze
                    if (rows.Count <= 2)
                    {
                        KwObszar.ObszarHA = "- - -";
                        KwLog.Add("kwObszar:".PadRight(40) + "Brak informacji o obszar, tabela nie właściwej liczby wierszy.");
                    }
                    else
                    {
                        // przetwórz wszystkie wiersze w tabeli
                        foreach (HtmlNode row in tableNode.SelectNodes("tr"))
                        {
                            // przetwórz wszsytkie komórki danego wiersza, włącznie z nagłowkami wierszy
                            HtmlNodeCollection cells = row.SelectNodes("th|td");

                            // specjalnie "for" by móc przemieszczać się po konkretnych komórkach
                            for (int i = 0; i < cells.Count; i++)
                            {
                                switch (cells[i].InnerText)
                                {
                                    case "1. Obszar":
                                        if (cells.Count == 3) // testowanie ilości komórek w wierszu
                                        {
                                           KwObszar.ObszarHA =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            KwObszar.ObszarHA = "- - -";
                                            KwLog.Add("KwObszar.ObszarHA:".PadRight(40) + "Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                    
                                        break;
                                }
                            }
                        }
                    }
                }

            }

            return KwLog.Count;
        }

        /// <summary>
        /// metoda pobierająca ulicę z położenia przypisanego do danej działki
        /// </summary>
        /// <param name="dzialka">Obiekt działki, dla której ma być pobrana ulica</param>
        /// <returns></returns>
        public string GetUlicaForDzialka(Dzialka dzialka)
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

        public string GetIdentyfikatorDzialkiForBudynek(Budynek budynek)
        {
            string wynik = "";

            if (budynek.IdentyfikatorDzialkiList.Count == 1)
            {
                wynik =  budynek.IdentyfikatorDzialkiList[0];
            }
            else
            {
                foreach (string identyfikator in budynek.IdentyfikatorDzialkiList)
                {
                    wynik = wynik + identyfikator + ", ";
                }

                wynik = wynik.Substring(0, wynik.Length - 2);
            }

            return wynik;

        }

        public string GetPolozenie(object obiekt, PolozenieTyp atrybut)
        {
            string wynik = "";

            if (obiekt.GetType() == typeof(Dzialka))
            {
                Dzialka dzialka = (Dzialka) obiekt;

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
            }

            if (obiekt.GetType() == typeof(Budynek))
            {
                Budynek budynek = (Budynek) obiekt;

                if (budynek.PolozenieList.Count == 1)
                {
                    try
                    {
                        switch (atrybut)
                        {
                            case PolozenieTyp.Wojewodztwo:
                                wynik = KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(budynek.PolozenieList[0])).Wojewodztwo;
                                break;
                            case PolozenieTyp.Powiat:
                                wynik = KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(budynek.PolozenieList[0])).Powiat;
                                break;
                            case PolozenieTyp.Gmina:
                                wynik = KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(budynek.PolozenieList[0])).Gmina;
                                break;
                            case PolozenieTyp.Miejscowosc:
                                wynik = KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(budynek.PolozenieList[0])).Miejscowosc;
                                break;
                            case PolozenieTyp.Dzielnica:
                                wynik = KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(budynek.PolozenieList[0])).Dzielnica;
                                break;                    }
                    }
                    catch (Exception)
                    {
                        wynik =  "- - -";
                    }
                }
                else
                {
                    foreach (string polozenie in budynek.PolozenieList)
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
            }

            if (obiekt.GetType() == typeof(Lokal))
            {
                Lokal lokal = (Lokal) obiekt;

                if (lokal.PolozenieList.Count == 1)
                {
                    try
                    {
                        switch (atrybut)
                        {
                            case PolozenieTyp.Wojewodztwo:
                                wynik = KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(lokal.PolozenieList[0])).Wojewodztwo;
                                break;
                            case PolozenieTyp.Powiat:
                                wynik = KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(lokal.PolozenieList[0])).Powiat;
                                break;
                            case PolozenieTyp.Gmina:
                                wynik = KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(lokal.PolozenieList[0])).Gmina;
                                break;
                            case PolozenieTyp.Miejscowosc:
                                wynik = KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(lokal.PolozenieList[0])).Miejscowosc;
                                break;
                            case PolozenieTyp.Dzielnica:
                                wynik = KwPolozenieList.Find(x => x.NumerPorzadkowy.Contains(lokal.PolozenieList[0])).Dzielnica;
                                break;                    }
                    }
                    catch (Exception)
                    {
                        wynik =  "- - -";
                    }
                }
                else
                {
                    foreach (string polozenie in lokal.PolozenieList)
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
            }


            return wynik;
        }
    }
}
