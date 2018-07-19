using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace KWTools
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
        
        public string File;
        public InformacjePodstawowe KwInformacjePodstawowe = new InformacjePodstawowe();
        public ZamkniecieKsiegi KwZamkniecieKsiegi = new ZamkniecieKsiegi();
        public List<Polozenie> KwPolozenieList = new List<Polozenie>();
        public List<Dzialka> KwDzialkaList = new List<Dzialka>();
        public List<Budynek> KwBudynekList = new List<Budynek>();
        public List<Lokal> KwLokalList = new List<Lokal>();
        public Obszar KwObszar = new Obszar();
        public Komentarz KwKomentarz19 = new Komentarz();

        /// <summary>
        /// lista przechowująca błędy wychwycone podczas przetwarzania księgi wieczystej
        /// </summary>
        public List<string> KwLog = new List<string>();

        private readonly LokalSlowniki _lokalSlowniki;

        public KwFromHtml(string kwBody, LokalSlowniki lokalSlowniki)
        {
            KwBody = kwBody;
            _lokalSlowniki = lokalSlowniki;
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
                                        KwLog.Add("Rubryka 0.3 - Dane o zamknieciu ksiegi wieczystej;Chwila zamkniecia ksiegi;Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
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
                                        KwLog.Add("Rubryka 0.3 - Dane o zamknieciu ksiegi wieczystej;Podstawa zamkniecia ksiegi;Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
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

                        KwLog.Add("Rubryka 1.3 - Położenie;;Brak informacji o położeniu, tabela nie właściwej liczby wierszy.");
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
                                            KwLog.Add("Rubryka 1.3 - Położenie;1. Numer porządkowy;Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
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
                                            KwLog.Add("Rubryka 1.3 - Położenie;2. Województwo;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                                            KwLog.Add("Rubryka 1.3 - Położenie;3. Powiat;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                                            KwLog.Add("Rubryka 1.3 - Położenie;4. Gmina;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                                            KwLog.Add("Rubryka 1.3 - Położenie;5. Miejscowość;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                                            KwLog.Add("Rubryka 1.3 - Położenie;6. Dzielnica;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                    if (tableNode.InnerText.IndexOf("7. Odłączenie", StringComparison.Ordinal) <= 0)
                    {
                        // wygeneruj wyjątek bo plik html może być uszkodzony
                        Console.WriteLine("Położenie bez rekordu '7. Odłączenie'! Sprawdź poprawność pliku HTML.");
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
                                            KwLog.Add("Podrubryka 1.4.1 - Działka ewidencyjna;1. Identyfikator działki;Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.1 - Działka ewidencyjna;2. Numer działki;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.1 - Działka ewidencyjna;A: numer obrębu ewidencyjnego;Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.1 - Działka ewidencyjna;B: nazwa obrębu ewidencyjnego;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                                                        KwLog.Add("Podrubryka 1.4.1 - Działka ewidencyjna;4. Położenie;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + htmlPolozenieCells.Count);
                                                    }
                                                }
                                            }
                                            else // jeśli tabela nie ma wierszy
                                            {
                                                kwDzialka.PolozenieMulti = false;
                                                kwDzialka.PolozenieList.Add("- - -");
                                                KwLog.Add("Podrubryka 1.4.1 - Działka ewidencyjna;4. Położenie;Zadeklarowana tabela nie posiada wierszy.");
                                            }
                                        }
                                        else // jeśli położenie jest pojedynczym tekstem
                                        {
                                            kwDzialka.PolozenieMulti = false;
                                            kwDzialka.PolozenieList.Add(cells[i + 1].InnerText);
                                            KwLog.Add("Podrubryka 1.4.1 - Działka ewidencyjna;4. Położenie;Atrybut nie jest zbudowany w postaci tabeli:" + cells[i + 1].InnerText);
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
                                                        KwLog.Add("Podrubryka 1.4.1 - Działka ewidencyjna;5. Ulica;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + htmlUlicaCells.Count);
                                                    }
                                                }
                                            }
                                            else // jeśli tabela nie ma wierszy
                                            {
                                                kwDzialka.UlicaMulti = false;
                                                kwDzialka.UliceList.Add("- - -");
                                                KwLog.Add("Podrubryka 1.4.1 - Działka ewidencyjna;5. Ulica;Zadeklarowana tabela nie posiada wierszy.");
                                            }
                                        }
                                        else // jeśli ulica jest pojedynczym tekstem
                                        {
                                            kwDzialka.UlicaMulti = false;
                                            kwDzialka.UliceList.Add(cells[i + 1].InnerText);
                                            KwLog.Add("Podrubryka 1.4.1 - Działka ewidencyjna;Ulica;Atrybut nie jest zbudowany w postaci tabeli: " + cells[i + 1].InnerText);
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
                                            KwLog.Add("Podrubryka 1.4.1 - Działka ewidencyjna;6. Sposób korzystania;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }

                                        break;

                                    case "7. Odłączenie":
                                        if (cells.Count == 4) // testowanie ilości komórek w wierszu
                                        {
                                            kwDzialka.OdlaczenieKw = cells[i + 3].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwDzialka.OdlaczenieKw = "- - -";
                                            KwLog.Add("Podrubryka 1.4.1 - Działka ewidencyjna;7. Odłączenie;Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.2 - Budynek;1. Identyfikator budynku;Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
                                        }
                                        break;
                                    
                                    case "2. Identyfikator działki":

                                        // jeśli Identyfikator jest zbudowane w postaci tabeli
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
                                                        KwLog.Add("Podrubryka 1.4.2 - Budynek;2. Identyfikator działki;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + htmlIdentyfikatorCells.Count);
                                                    }
                                                }
                                            }
                                            else // jeśli tabela nie ma wierszy
                                            {
                                                kwBudynek.IdentyfikatorDzialkiMulti = false;
                                                kwBudynek.IdentyfikatorDzialkiList.Add("- - -");
                                                KwLog.Add("Podrubryka 1.4.2 - Budynek;  2. Identyfikator działki;Zadeklarowana tabela nie posiada wierszy.");
                                            }
                                        }
                                        else // jeśli Identyfikator jest pojedynczym tekstem
                                        {
                                            kwBudynek.IdentyfikatorDzialkiMulti = false;
                                            kwBudynek.IdentyfikatorDzialkiList.Add(cells[i + 1].InnerText);
                                            KwLog.Add("Podrubryka 1.4.2 - Budynek;2. Identyfikator działki;Atrybut nie jest zbudowany w postaci tabeli: " + cells[i + 1].InnerText);
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
                                                        KwLog.Add("Podrubryka 1.4.2 - Budynek;3. Położenie;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + htmlPolozenieCells.Count);
                                                    }
                                                }
                                            }
                                            else // jeśli tabela nie ma wierszy
                                            {
                                                kwBudynek.PolozenieMulti = false;
                                                kwBudynek.PolozenieList.Add("- - -");
                                                KwLog.Add("Podrubryka 1.4.2 - Budynek;3. Położenie;Zadeklarowana tabela nie posiada wierszy.");
                                            }
                                        }
                                        else // jeśli położenie jest pojedynczym tekstem
                                        {
                                            kwBudynek.PolozenieMulti = false;
                                            kwBudynek.PolozenieList.Add(cells[i + 1].InnerText);
                                            KwLog.Add("Podrubryka 1.4.2 - Budynek;3. Położenie;Atrybut nie jest zbudowany w postaci tabeli: " + cells[i + 1].InnerText);
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
                                            KwLog.Add("Podrubryka 1.4.2 - Budynek;A: nazwa ulicy (alei, placu);Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.2 - Budynek;B: numer porządkowy budynku;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.2 - Budynek;5. Liczba kondygnacji;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.2 - Budynek;6. Liczba samodzielnych lokali;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.2 - Budynek;7. Powierzchnia użytkowa budynku;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.2 - Budynek;8. Przeznaczenie budynku;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.2 - Budynek;9. Dalszy opis budynku;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.2 - Budynek;10. Nieruchomość, na której usytuowany jest budynek;Niezgodna liczba kolumn. Oczekiwana: 2, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.2 - Budynek;11. Odrębność;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.4 - Lokal;1. Identyfikator lokalu;Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.4 - Lokal;2. Ulica;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.4 - Lokal;3. Numer budynku;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.4 - Lokal;4. Numer lokalu;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.4 - Lokal;5. Przeznaczenie lokalu;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }

                                        break;

                                    case "6. Opis lokalu":
                                        if (cells.Count == 2) // testowanie ilości komórek w wierszu
                                        {
                                            // jeśli opis lokalu jest zbudowane w postaci tabeli
                                            if (cells[i + 1].InnerHtml.IndexOf("table", StringComparison.Ordinal) > 0)
                                            {
                                                HtmlNode htmlOpisLokaluTable = cells[i+1].SelectSingleNode("table");

                                                HtmlNodeCollection htmlOpisLokaluRows = htmlOpisLokaluTable.SelectNodes("tr");

                                                // jeśli tabela zawiera wiersze
                                                if (htmlOpisLokaluRows != null)
                                                {
                                                    if( htmlOpisLokaluRows.Count % 2 != 0 ) throw new Exception();

                                                    for (int j = 0; j < htmlOpisLokaluRows.Count; j = j + 2)
                                                    {
                                                        HtmlNodeCollection htmlOpisLokaluCells = htmlOpisLokaluRows[j].SelectNodes("td");

                                                        string idIzby = htmlOpisLokaluCells[0].InnerText;
                                                        if (htmlOpisLokaluCells[1].InnerText != "A: rodzaj izby") throw new Exception();
                                                        string rodzajIzby = htmlOpisLokaluCells[3].InnerText;

                                                        htmlOpisLokaluCells = htmlOpisLokaluRows[j+1].SelectNodes("td");

                                                        if (htmlOpisLokaluCells[0].InnerText != "B: liczba izb") throw new Exception();
                                                        string liczbaIzb = htmlOpisLokaluCells[2].InnerText;

                                                        kwLokal.OpisLokalu.Add(new OpisLokaluStruct(idIzby, rodzajIzby, liczbaIzb));

                                                        if (liczbaIzb == "- - -") liczbaIzb = "1";

                                                        if (_lokalSlowniki.RodzajIzbaTak.Exists(x => x == rodzajIzby)) kwLokal.LiczbaIzb = kwLokal.LiczbaIzb + Convert.ToInt32(liczbaIzb);
                                                    }

                                                }
                                                else // jeśli tabela nie ma wierszy
                                                {
                                                    kwLokal.OpisLokalu.Add(new OpisLokaluStruct("- - -", "- - -", "- - -")); 
                                                    KwLog.Add("Podrubryka 1.4.4 - Lokal;6. Opis lokalu;Zadeklarowana tabela nie posiada wierszy.");
                                                }
                                            }
                                            else // jeśli brak jest tabeli opisujące lokal
                                            {
                                                kwLokal.OpisLokalu.Add(new OpisLokaluStruct("- - -", "- - -", "- - -")); 
                                                KwLog.Add("Podrubryka 1.4.4 - Lokal;6. Opis lokalu;Atrybut nie jest zbudowany w postaci tabeli: " + cells[i + 1].InnerText);
                                            }


                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwLokal.OpisLokalu.Add(new OpisLokaluStruct("- - -", "- - -", "- - -")); 
                                            KwLog.Add("Podrubryka 1.4.4 - Lokal;6. Opis lokalu;Niezgodna liczba kolumn. Oczekiwana: 2, jest: " + cells.Count);
                                        }

                                        break;

                                    case "7. Opis pomieszczeń przynależnych":
                                        if (cells.Count == 2) // testowanie ilości komórek w wierszu
                                        {
                                            // jeśli opis pomieszczeń jest zbudowane w postaci tabeli
                                            if (cells[i + 1].InnerHtml.IndexOf("table", StringComparison.Ordinal) > 0)
                                            {
                                                HtmlNode htmlOpisPomieszczenPrzynaleznychTable = cells[i + 1].SelectSingleNode("table");

                                                HtmlNodeCollection htmlOpisPomieszczenPrzynaleznychRows = htmlOpisPomieszczenPrzynaleznychTable.SelectNodes("tr");

                                                // jeśli tabela zawiera wiersze
                                                if (htmlOpisPomieszczenPrzynaleznychRows != null)
                                                {
                                                    if( htmlOpisPomieszczenPrzynaleznychRows.Count % 2 != 0 ) throw new Exception();

                                                    for (int j = 0; j < htmlOpisPomieszczenPrzynaleznychRows.Count; j = j + 2)
                                                    {
                                                        HtmlNodeCollection htmlOpisPomieszczenPrzynaleznychCells = htmlOpisPomieszczenPrzynaleznychRows[j].SelectNodes("td");

                                                        string idPomieszczenia = "";
                                                        string rodzajPomieszczenia = "";
                                                        string powierzchniaPomieszczenia = "";

                                                        if (htmlOpisPomieszczenPrzynaleznychCells.Count == 4)
                                                        {
                                                            idPomieszczenia = htmlOpisPomieszczenPrzynaleznychCells[0].InnerText;
                                                            if (htmlOpisPomieszczenPrzynaleznychCells[1].InnerText != "A: rodzaj pomieszczenia") throw new Exception();
                                                            rodzajPomieszczenia = htmlOpisPomieszczenPrzynaleznychCells[3].InnerText;

                                                            if (Regex.IsMatch(rodzajPomieszczenia, @"O POW\.\s*(\d+)(\.?|,?)(\d*)\s*M2"))                       // _O POW.___M2
                                                            {
                                                                int start = rodzajPomieszczenia.IndexOf("O POW.", StringComparison.Ordinal) + 6;
                                                                int koniec = rodzajPomieszczenia.IndexOf("M2", start, StringComparison.Ordinal);
                                                                string powierzchnia = rodzajPomieszczenia.Substring(start, koniec - start).Replace(",",".");

                                                                powierzchniaPomieszczenia = $"{Convert.ToDouble(powierzchnia):0.00}";
                                                            } 
                                                            else if (Regex.IsMatch(rodzajPomieszczenia, @"O POWIERZCHNI\s*(\d+)(\.?|,?)(\d*)\s*M2"))            // _O POWIERZCHNI___M2
                                                            {
                                                                int start = rodzajPomieszczenia.IndexOf("O POWIERZCHNI", StringComparison.Ordinal) + 13;
                                                                int koniec = rodzajPomieszczenia.IndexOf("M2", start, StringComparison.Ordinal);
                                                                string powierzchnia = rodzajPomieszczenia.Substring(start, koniec - start).Replace(",",".");

                                                                powierzchniaPomieszczenia = $"{Convert.ToDouble(powierzchnia):0.00}";
                                                            }
                                                            else if (Regex.IsMatch(rodzajPomieszczenia, @"O POW\.\s*(\d+)(\.?|,?)(\d*)\s*M 2"))                 // _O POW.___M 2
                                                            {
                                                                int start = rodzajPomieszczenia.IndexOf("O POW.", StringComparison.Ordinal) + 6;
                                                                int koniec = rodzajPomieszczenia.IndexOf("M 2", start, StringComparison.Ordinal);
                                                                string powierzchnia = rodzajPomieszczenia.Substring(start, koniec - start).Replace(",",".");

                                                                powierzchniaPomieszczenia = $"{Convert.ToDouble(powierzchnia):0.00}";
                                                            } 
                                                            else if (Regex.IsMatch(rodzajPomieszczenia, @"O POWIERZCHNI\s*(\d+)(\.?|,?)(\d*)\s*M 2"))           // _O POWIERZCHNI___M 2
                                                            {
                                                                int start = rodzajPomieszczenia.IndexOf("O POWIERZCHNI", StringComparison.Ordinal) + 13;
                                                                int koniec = rodzajPomieszczenia.IndexOf("M 2", start, StringComparison.Ordinal);
                                                                string powierzchnia = rodzajPomieszczenia.Substring(start, koniec - start).Replace(",",".");

                                                                powierzchniaPomieszczenia = $"{Convert.ToDouble(powierzchnia):0.00}";
                                                            }
                                                            else if (Regex.IsMatch(rodzajPomieszczenia, @" POW\.\s*(\d+)(\.?|,?)(\d*)\s*M2"))                   // _ POW.___M2
                                                            {
                                                                int start = rodzajPomieszczenia.IndexOf(" POW.", StringComparison.Ordinal) + 5;
                                                                int koniec = rodzajPomieszczenia.IndexOf("M2", start, StringComparison.Ordinal);
                                                                string powierzchnia = rodzajPomieszczenia.Substring(start, koniec - start).Replace(",",".");

                                                                powierzchniaPomieszczenia = $"{Convert.ToDouble(powierzchnia):0.00}";
                                                            } 
                                                            else if (Regex.IsMatch(rodzajPomieszczenia, @"O POW\s*(\d+)(\.?|,?)(\d*)\s*M2"))                    // _O POW ___M2
                                                            {
                                                                int start = rodzajPomieszczenia.IndexOf("O POW", StringComparison.Ordinal) + 5;
                                                                int koniec = rodzajPomieszczenia.IndexOf("M2", start, StringComparison.Ordinal);
                                                                string powierzchnia = rodzajPomieszczenia.Substring(start, koniec - start).Replace(",",".");

                                                                powierzchniaPomieszczenia = $"{Convert.ToDouble(powierzchnia):0.00}";
                                                            } 
                                                        }
                                                        else
                                                        {
                                                            KwLog.Add("Podrubryka 1.4.4 - Lokal;7. Opis pomieszczeń przynależnych;Niezgodna liczba kolumn. Oczekiwana: 4, jest: " + htmlOpisPomieszczenPrzynaleznychCells.Count);
                                                        }

                                                        htmlOpisPomieszczenPrzynaleznychCells = htmlOpisPomieszczenPrzynaleznychRows[j+1].SelectNodes("td");

                                                        string liczbaPomieszczen = "";

                                                        if (htmlOpisPomieszczenPrzynaleznychCells.Count == 3)
                                                        {
                                                            if (htmlOpisPomieszczenPrzynaleznychCells[0].InnerText != "B: liczba pomieszczeń") throw new Exception();
                                                            liczbaPomieszczen = htmlOpisPomieszczenPrzynaleznychCells[2].InnerText;
                                                        }
                                                        else
                                                        {
                                                            KwLog.Add("Podrubryka 1.4.4 - Lokal;7. Opis pomieszczeń przynależnych;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + htmlOpisPomieszczenPrzynaleznychCells.Count);
                                                        }

                                                        kwLokal.OpisPomieszczenPrzynaleznych.Add(new OpisPomieszczenPrzynaleznychStruct(idPomieszczenia, rodzajPomieszczenia, liczbaPomieszczen, powierzchniaPomieszczenia));

                                                        if (_lokalSlowniki.RodzajPiwnica.Exists(x => x == rodzajPomieszczenia)) kwLokal.OpisPomieszczenPrzynaleznychPiwnica.Add(new OpisPomieszczenPrzynaleznychStruct(idPomieszczenia, rodzajPomieszczenia, liczbaPomieszczen, powierzchniaPomieszczenia));
                                                        if (_lokalSlowniki.RodzajGaraz.Exists(x => x == rodzajPomieszczenia)) kwLokal.OpisPomieszczenPrzynaleznychGaraz.Add(new OpisPomieszczenPrzynaleznychStruct(idPomieszczenia, rodzajPomieszczenia, liczbaPomieszczen, powierzchniaPomieszczenia));
                                                        if (_lokalSlowniki.RodzajPostoj.Exists(x => x == rodzajPomieszczenia)) kwLokal.OpisPomieszczenPrzynaleznychPostoj.Add(new OpisPomieszczenPrzynaleznychStruct(idPomieszczenia, rodzajPomieszczenia, liczbaPomieszczen, powierzchniaPomieszczenia));
                                                        if (_lokalSlowniki.RodzajStrych.Exists(x => x == rodzajPomieszczenia)) kwLokal.OpisPomieszczenPrzynaleznychStrych.Add(new OpisPomieszczenPrzynaleznychStruct(idPomieszczenia, rodzajPomieszczenia, liczbaPomieszczen, powierzchniaPomieszczenia));
                                                        if (_lokalSlowniki.RodzajKomorka.Exists(x => x == rodzajPomieszczenia)) kwLokal.OpisPomieszczenPrzynaleznychKomorka.Add(new OpisPomieszczenPrzynaleznychStruct(idPomieszczenia, rodzajPomieszczenia, liczbaPomieszczen, powierzchniaPomieszczenia));
                                                        if (_lokalSlowniki.RodzajInne.Exists(x => x == rodzajPomieszczenia)) kwLokal.OpisPomieszczenPrzynaleznychInne.Add(new OpisPomieszczenPrzynaleznychStruct(idPomieszczenia, rodzajPomieszczenia, liczbaPomieszczen, powierzchniaPomieszczenia));
                                                    }
                                                }
                                                else // jeśli tabela nie ma wierszy
                                                {
                                                    kwLokal.OpisPomieszczenPrzynaleznych.Add(new OpisPomieszczenPrzynaleznychStruct("- - -", "- - -", "- - -", "- - -")); 
                                                    KwLog.Add("Podrubryka 1.4.4 - Lokal;7. Opis pomieszczeń przynależnych;Zadeklarowana tabela nie posiada wierszy.");
                                                }
                                            }
                                            else // jeśli brak jest tabeli opisującej pomieszczenia
                                            {
                                                kwLokal.OpisPomieszczenPrzynaleznych.Add(new OpisPomieszczenPrzynaleznychStruct("- - -", "- - -", "- - -", "- - -")); 
                                                KwLog.Add("Podrubryka 1.4.4 - Lokal;7. Opis pomieszczeń przynależnych;Atrybut nie jest zbudowany w postaci tabeli: " + cells[i + 1].InnerText);
                                            }

                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            kwLokal.OpisPomieszczenPrzynaleznych.Add(new OpisPomieszczenPrzynaleznychStruct("- - -", "- - -", "- - -", "- - -"));
                                            KwLog.Add("Podrubryka 1.4.4 - Lokal;7. Opis pomieszczeń przynależnych;Niezgodna liczba kolumn. Oczekiwana: 2, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.4 - Lokal;8. Kondygnacja;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.4 - Lokal;10. Nieruchomość zabudowana budynkiem;Niezgodna liczba kolumn. Oczekiwana: 2, jest: " + cells.Count);
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
                                            KwLog.Add("Podrubryka 1.4.4 - Lokal;11. Odrębność;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
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
                        KwObszar.ObszarHa = "- - -";
                        KwLog.Add("Rubryka 1.5 - Obszar;;Brak informacji o obszar, tabela nie ma właściwej liczby wierszy.");
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
                                           KwObszar.ObszarHa =  cells[i + 2].InnerText;
                                        }
                                        else // jeśli liczba komórek jest inna niż oczekiwana przyjmowana wartość domyślna i zapis raportu błędów
                                        {
                                            KwObszar.ObszarHa = "- - -";
                                            KwLog.Add("Rubryka 1.5 - Obszar;1. Obszar;Niezgodna liczba kolumn. Oczekiwana: 3, jest: " + cells.Count);
                                        }
                                    
                                        break;
                                }
                            }
                        }
                    }
                }

                // --------------------------------------------------------------------------------
                // Rubryka 1.9 - Komentarz
                // --------------------------------------------------------------------------------

                if (tableNode.InnerText.IndexOf("Rubryka 1.9 - Komentarz", StringComparison.Ordinal) > 0)
                {
                    // przetwórz wszystkie wiersze w tabeli
                    HtmlNodeCollection rows = tableNode.SelectNodes("tr");

                    // jeśli dane komentarza zapisane są w postaci tabeli to musi ona posiadać 5 wierszy
                    if (rows.Count != 5)
                    {
                        KwKomentarz19.Wpis = "- - -";
                        KwKomentarz19.NumerWpisu = "- - -";
                        KwLog.Add("Rubryka 1.9 - Komentarz;;Brak informacji o komentarzu, tabela nie ma właściwej liczby wierszy. Jest: " + rows.Count);
                    } 
                    else 
                    {
                        KwKomentarz19.Wpis = rows[3].SelectNodes("td")[2].InnerText;
                        KwKomentarz19.NumerWpisu = rows[4].SelectNodes("td")[2].InnerText;
                        
                    }

                }

                // --------------------------------------------------------------------------------
                // Rubryka 2.2 - Właściciel
                // --------------------------------------------------------------------------------

                // szukaj tabeli z rubryka udziału
                if (tableNode.InnerText.IndexOf("Rubryka 2.2 - Właściciel", StringComparison.Ordinal) > 0)
                {
                    HtmlNodeCollection rows = tableNode.SelectNodes("tr");

                    // tabela opisująca właścicieli ma więcej niż dwa wiersze
                    if (rows.Count > 2)
                    {
                        int udzialRow;          // Podrubryka 2.2.1 - Udział
                        int skarbPanstwaRow;    // Podrubryka 2.2.2 - Skarb Państwa
                        int samorzadRow;        // Podrubryka 2.2.3 - jednostka samorządu terytorialnego (związek międzygminny)
                        int innaOsobaRow;       // Podrubryka 2.2.4 - inna osoba prawna lub jednostka organizacyjna niebędąca osobą prawną
                        int osobaFizycznaRow;   // Podrubryka 2.2.5 - Osoba fizyczna

                        bool udzialExists = false;
                        bool skarbPanstaExists = false;
                        bool samorzadExists = false;
                        bool innaOsobaExists = false;
                        bool osobaFizycznaExists = false;
                     
                        for (int i = 0; i < rows.Count - 1; i++)
                        {
                            if (rows[i].InnerText.IndexOf("Podrubryka 2.2.1", StringComparison.Ordinal) > 0)
                            {
                                udzialRow = i;
                                udzialExists = true;
                            } 
                            else if (rows[i].InnerText.IndexOf("Podrubryka 2.2.2", StringComparison.Ordinal) > 0)
                            {
                                skarbPanstwaRow = i;
                                skarbPanstaExists = true;
                            } 
                            else if (rows[i].InnerText.IndexOf("Podrubryka 2.2.3", StringComparison.Ordinal) > 0)
                            {
                                samorzadRow = i;
                                samorzadExists = true;
                            } else if (rows[i].InnerText.IndexOf("Podrubryka 2.2.4", StringComparison.Ordinal) > 0)
                            {
                                innaOsobaRow = i;
                                innaOsobaExists = true;
                            } else if (rows[i].InnerText.IndexOf("Podrubryka 2.2.5", StringComparison.Ordinal) > 0)
                            {
                                osobaFizycznaRow = i;
                                osobaFizycznaExists = true;
                            }
                        }

                        // jeśli brak jest udziału wygeneruj wyjątek
                        if (!udzialExists) throw new Exception();
                        
                    }
                    else // jeśli tabela z właścicielami jest pusta
                    {
                        KwLog.Add("Rubryka 2.2 - Właściciel;;Brak wartości w tabeli Właściciel");
                    }

                }

            }

            return KwLog.Count;
        }

    }
}
