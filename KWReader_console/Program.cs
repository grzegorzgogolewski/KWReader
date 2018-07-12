using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KWTools;
using OfficeOpenXml;
using Tools;

namespace KWReader
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length < 1)
            {
                Console.WriteLine("Aplikacja do pozyskiwania danych z plików *.html dla KW");
                Console.WriteLine("Copyright © 2018 GISNET\n");
                Console.WriteLine("Jako parametr podaj ścieżkę do plików *.html\n");
                Console.WriteLine("Wciśnij dowolny klawisz...");
                Console.ReadKey();
                return;
            }

            LokalSlowniki lokalSlowniki = new LokalSlowniki();

            using (StreamReader iniFile = new StreamReader(new FileStream(Funkcje.GetExecutingDirectoryName() + "\\Konfiguracja\\LokaleIzbyTak.txt", FileMode.Open), Encoding.UTF8))
            {
                while (iniFile.Peek() >= 0) lokalSlowniki.RodzajIzbaTak.Add(iniFile.ReadLine());
            }

            using (StreamReader iniFile = new StreamReader(new FileStream(Funkcje.GetExecutingDirectoryName() + "\\Konfiguracja\\LokaleIzbyNie.txt", FileMode.Open), Encoding.UTF8))
            {
                while (iniFile.Peek() >= 0) lokalSlowniki.RodzajIzbaNie.Add(iniFile.ReadLine());
            }

            using (StreamReader iniFile = new StreamReader(new FileStream(Funkcje.GetExecutingDirectoryName() + "\\Konfiguracja\\LokalePiwnice.txt", FileMode.Open), Encoding.UTF8))
            {
                while (iniFile.Peek() >= 0) lokalSlowniki.RodzajPiwnica.Add(iniFile.ReadLine());
            }

            using (StreamReader iniFile = new StreamReader(new FileStream(Funkcje.GetExecutingDirectoryName() + "\\Konfiguracja\\LokaleGaraz.txt", FileMode.Open), Encoding.UTF8))
            {
                while (iniFile.Peek() >= 0) lokalSlowniki.RodzajGaraz.Add(iniFile.ReadLine());
            }

            using (StreamReader iniFile = new StreamReader(new FileStream(Funkcje.GetExecutingDirectoryName() + "\\Konfiguracja\\LokalePostoj.txt", FileMode.Open), Encoding.UTF8))
            {
                while (iniFile.Peek() >= 0) lokalSlowniki.RodzajPostoj.Add(iniFile.ReadLine());
            }

            using (StreamReader iniFile = new StreamReader(new FileStream(Funkcje.GetExecutingDirectoryName() + "\\Konfiguracja\\LokaleStrych.txt", FileMode.Open), Encoding.UTF8))
            {
                while (iniFile.Peek() >= 0) lokalSlowniki.RodzajStrych.Add(iniFile.ReadLine());
            }

            using (StreamReader iniFile = new StreamReader(new FileStream(Funkcje.GetExecutingDirectoryName() + "\\Konfiguracja\\LokaleKomorka.txt", FileMode.Open), Encoding.UTF8))
            {
                while (iniFile.Peek() >= 0) lokalSlowniki.RodzajKomorka.Add(iniFile.ReadLine());
            }

            using (StreamReader iniFile = new StreamReader(new FileStream(Funkcje.GetExecutingDirectoryName() + "\\Konfiguracja\\LokaleInne.txt", FileMode.Open), Encoding.UTF8))
            {
                while (iniFile.Peek() >= 0) lokalSlowniki.RodzajInne.Add(iniFile.ReadLine());
            }

            List<KwFromHtml> listaKw = new List<KwFromHtml>();
            
            List<string> listaKwDzialki = new List<string>();
            List<string> listaKwBudynki = new List<string>();
            List<string> listaKwLokale = new List<string>();
            List<string> listaKwZamkniete = new List<string>();

            Dictionary<string, List<string>> listaLog = new Dictionary<string, List<string>>();

            string [] fileEntries = Directory.GetFiles(args[0], "*.html");
            
            Console.WriteLine("Przetwarzanie {0} ksiąg wieczystych...", fileEntries.Length);

            int kwCounter = 0;

            // czytanie KW i ich parsowanie + statystyki z operacji
            foreach (string file in fileEntries)
            {
                kwCounter++;

                StreamReader htmlFile = new StreamReader(new FileStream(file, FileMode.Open), Encoding.UTF8);

                KwFromHtml kw = new KwFromHtml(htmlFile.ReadToEnd(), lokalSlowniki);
            
                htmlFile.Close();

                kw.File = file;

                kw.ParseKw();

                listaKw.Add(kw);

                // dodaj do listy numery ksiąg wieczystych, które mają działki
                if (kw.KwDzialkaList.Count > 0)
                {
                    listaKwDzialki.Add(kw.KwInformacjePodstawowe.NumerKsiegi);
                }

                // dodaj do listy numery ksiąg wieczystych, które mają budynki
                if (kw.KwBudynekList.Count > 0)
                {
                    listaKwBudynki.Add(kw.KwInformacjePodstawowe.NumerKsiegi);
                }

                // dodaj do listy numery ksiąg wieczystych, które mają lokale
                if (kw.KwLokalList.Count > 0)
                {
                    listaKwLokale.Add(kw.KwInformacjePodstawowe.NumerKsiegi);
                }

                // dodaj liste błedów danej kw do listy globalnej
                listaLog.Add(kw.File, kw.KwLog);

                //Console.WriteLine("Odczyt KW: [{0, 6}/{1, 6}]: {2}, Liczba działek: {3, 3}, Liczba budynków: {4, 3}, Liczba lokali: {5, 3}", 
                //kwCounter, fileEntries.Length, kw.KwInformacjePodstawowe.NumerKsiegi, kw.KwDzialkaList.Count, kw.KwBudynekList.Count, kw.KwLokalList.Count);

            }

            // ====================================================================================
            // Zapisywanie danych do Excela
            // ====================================================================================

            Console.WriteLine("Zapisywanie danych o {0} księgach wieczystych...", fileEntries.Length);

            if (!Directory.Exists(args[0].TrimEnd('\\') + "\\wynik")) Directory.CreateDirectory(args[0].TrimEnd('\\') + "\\wynik");

            FileInfo xlsFile = new FileInfo(args[0].TrimEnd('\\') + "\\wynik\\KW.xlsx");

            if (xlsFile.Exists)
            {
                try
                {
                    xlsFile.Delete();
                }
                catch (IOException)
                {
                    Console.WriteLine("Zamknij plik z KW!");
                    Console.ReadKey();
                    return;
                }
            }

            ExcelPackage xlsWorkbook = new ExcelPackage(xlsFile);

            xlsWorkbook.Workbook.Properties.Title = "Raport KW";
            xlsWorkbook.Workbook.Properties.Author = "Grzegorz Gogolewski";
            xlsWorkbook.Workbook.Properties.Comments = "Raport KW";
            xlsWorkbook.Workbook.Properties.Company = "GISNET";

            ExcelWorksheet xlsSheetKw = xlsWorkbook.Workbook.Worksheets.Add("KW");

            xlsSheetKw.Cells[1, 1].Value = "NazwaPliku";
            xlsSheetKw.Cells[1, 2].Value = "NumerKsiegi";
            xlsSheetKw.Cells[1, 3].Value = "LiczbaDzialek";
            xlsSheetKw.Cells[1, 4].Value = "LiczbaBudynkow";
            xlsSheetKw.Cells[1, 5].Value = "LiczbaLokali";
            xlsSheetKw.Cells[1, 6].Value = "Zamknieta";

            ExcelWorksheet xlsSheetDzialki = xlsWorkbook.Workbook.Worksheets.Add("Działki");

            xlsSheetDzialki.Cells[1, 1].Value = "NumerKsiegi";
            xlsSheetDzialki.Cells[1, 2].Value = "ChwilaZamkniecia";
            xlsSheetDzialki.Cells[1, 3].Value = "PodstawaZamkniecia";

            xlsSheetDzialki.Cells[1, 4].Value = "PolozenieMulti";
            xlsSheetDzialki.Cells[1, 5].Value = "Gmina";
            xlsSheetDzialki.Cells[1, 6].Value = "Miejscowosc";
            xlsSheetDzialki.Cells[1, 7].Value = "Dzielnica";

            xlsSheetDzialki.Cells[1, 8].Value = "IdDzialki";
            xlsSheetDzialki.Cells[1, 9].Value = "NumerDzialki";
            xlsSheetDzialki.Cells[1, 10].Value = "NumerObrebuEwid";
            xlsSheetDzialki.Cells[1, 11].Value = "NazwaObrebuEwid";
            xlsSheetDzialki.Cells[1, 12].Value = "UlicaMulti";
            xlsSheetDzialki.Cells[1, 13].Value = "Ulica";
            xlsSheetDzialki.Cells[1, 14].Value = "SposobKorzystania";
            xlsSheetDzialki.Cells[1, 15].Value = "OdlaczenieKW";

            xlsSheetDzialki.Cells[1, 16].Value = "LiczbaDZwKW";
            xlsSheetDzialki.Cells[1, 17].Value = "PowObszaru";

            ExcelWorksheet xlsSheetBudynki = xlsWorkbook.Workbook.Worksheets.Add("Budynki");

            xlsSheetBudynki.Cells[1, 1].Value = "NumerKsiegi";
            xlsSheetBudynki.Cells[1, 2].Value = "ChwilaZamkniecia";
            xlsSheetBudynki.Cells[1, 3].Value = "PodstawaZamkniecia";

            xlsSheetBudynki.Cells[1, 4].Value = "PolozenieMulti";
            xlsSheetBudynki.Cells[1, 5].Value = "Gmina";
            xlsSheetBudynki.Cells[1, 6].Value = "Miejscowosc";
            xlsSheetBudynki.Cells[1, 7].Value = "Dzielnica";

            xlsSheetBudynki.Cells[1, 8].Value = "IdBudynku";
            xlsSheetBudynki.Cells[1, 9].Value = "IdDzialkiMulti";
            xlsSheetBudynki.Cells[1, 10].Value = "IdDzialki";
            xlsSheetBudynki.Cells[1, 11].Value = "NazwaUlicy";
            xlsSheetBudynki.Cells[1, 12].Value = "NumerPorzadkowy";
            xlsSheetBudynki.Cells[1, 13].Value = "LiczbaKondygnacji";
            xlsSheetBudynki.Cells[1, 14].Value = "LiczbaLokali";
            xlsSheetBudynki.Cells[1, 15].Value = "PowierzchniaUzytkowa";
            xlsSheetBudynki.Cells[1, 16].Value = "Przeznaczenie";
            xlsSheetBudynki.Cells[1, 17].Value = "DalszyOpis";
            xlsSheetBudynki.Cells[1, 18].Value = "Nieruchomosc";
            xlsSheetBudynki.Cells[1, 19].Value = "Odrebnosc";

            ExcelWorksheet xlsSheetLokale = xlsWorkbook.Workbook.Worksheets.Add("Lokale");

            xlsSheetLokale.Cells[1, 1].Value = "NumerKsiegi";
            xlsSheetLokale.Cells[1, 2].Value = "ChwilaZamkniecia";
            xlsSheetLokale.Cells[1, 3].Value = "PodstawaZamkniecia";

            xlsSheetLokale.Cells[1, 4].Value = "PolozenieMulti";
            xlsSheetLokale.Cells[1, 5].Value = "Gmina";
            xlsSheetLokale.Cells[1, 6].Value = "Miejscowosc";
            xlsSheetLokale.Cells[1, 7].Value = "Dzielnica";

            xlsSheetLokale.Cells[1, 8].Value = "IdLokalu";
            xlsSheetLokale.Cells[1, 9].Value = "Ulica";
            xlsSheetLokale.Cells[1, 10].Value = "NumerBudynku";
            xlsSheetLokale.Cells[1, 11].Value = "NumerLokalu";
            xlsSheetLokale.Cells[1, 12].Value = "PrzeznaczenieLokalu";
            xlsSheetLokale.Cells[1, 13].Value = "OpisLokalu";
            xlsSheetLokale.Cells[1, 14].Value = "LiczbaIzb";
            xlsSheetLokale.Cells[1, 15].Value = "OpisPomPrzyn";
            xlsSheetLokale.Cells[1, 16].Value = "Piwnica";
            xlsSheetLokale.Cells[1, 17].Value = "PiwnicaPow";
            xlsSheetLokale.Cells[1, 18].Value = "Garaz";
            xlsSheetLokale.Cells[1, 19].Value = "GarazPow";
            xlsSheetLokale.Cells[1, 20].Value = "Postoj";
            xlsSheetLokale.Cells[1, 21].Value = "PostojPow";
            xlsSheetLokale.Cells[1, 22].Value = "Strych";
            xlsSheetLokale.Cells[1, 23].Value = "StrychPow";
            xlsSheetLokale.Cells[1, 24].Value = "Komorka";
            xlsSheetLokale.Cells[1, 25].Value = "KomorkaPow";
            xlsSheetLokale.Cells[1, 26].Value = "Inne";
            xlsSheetLokale.Cells[1, 27].Value = "InnePow";
            xlsSheetLokale.Cells[1, 28].Value = "Kondygnacja";
            xlsSheetLokale.Cells[1, 29].Value = "Nieruchomosc";
            xlsSheetLokale.Cells[1, 30].Value = "Odrebnosc";  
            xlsSheetLokale.Cells[1, 31].Value = "PowObszaru";
            
            int dzialkaCounter = 2;
            int budynekCounter = 2;
            int lokalCounter = 2;

            kwCounter = 2;

            List<string> lokalRodzajeIzb = new List<string>();
            List<string> lokalRodzajePomieszczen = new List<string>();

            foreach (KwFromHtml kw in listaKw)
            {
                //Console.WriteLine("Zapis KW: [{0, 6}/{1, 6}]: {2}, Liczba działek: {3, 3}, Liczba budynków: {4, 3}, Liczba lokali: {5, 3}",  
                //kwCounter - 1, fileEntries.Length, kw.KwInformacjePodstawowe.NumerKsiegi, kw.KwDzialkaList.Count, kw.KwBudynekList.Count, kw.KwLokalList.Count);

                xlsSheetKw.Cells[kwCounter, 1].Value = kw.File;
                xlsSheetKw.Cells[kwCounter, 2].Value = kw.KwInformacjePodstawowe.NumerKsiegi;
                xlsSheetKw.Cells[kwCounter, 3].Value = kw.KwDzialkaList.Count;
                xlsSheetKw.Cells[kwCounter, 4].Value = kw.KwBudynekList.Count;
                xlsSheetKw.Cells[kwCounter, 5].Value = kw.KwLokalList.Count;

                // dodaj do listy numery ksiąg wieczystych, które są zamknięte
                if (kw.KwZamkniecieKsiegi.ChwilaZamkniecia != "- - -" || kw.KwZamkniecieKsiegi.PodstawaZamkniecia != "- - -")
                {
                    listaKwZamkniete.Add(kw.KwInformacjePodstawowe.NumerKsiegi);
                    xlsSheetKw.Cells[kwCounter, 6].Value = "TAK";
                }
                else
                {
                    xlsSheetKw.Cells[kwCounter, 6].Value = "NIE";
                }

                kwCounter++;

                foreach (Dzialka dzialka in kw.KwDzialkaList)
                {
                    xlsSheetDzialki.Cells[dzialkaCounter, 1].Value = kw.KwInformacjePodstawowe.NumerKsiegi;
                    xlsSheetDzialki.Cells[dzialkaCounter, 2].Value = kw.KwZamkniecieKsiegi.ChwilaZamkniecia;
                    xlsSheetDzialki.Cells[dzialkaCounter, 3].Value = kw.KwZamkniecieKsiegi.PodstawaZamkniecia;

                    xlsSheetDzialki.Cells[dzialkaCounter, 4].Value = dzialka.PolozenieMulti;
                    xlsSheetDzialki.Cells[dzialkaCounter, 5].Value = dzialka.GetPolozenie(kw.KwPolozenieList, PolozenieTyp.Gmina);
                    xlsSheetDzialki.Cells[dzialkaCounter, 6].Value = dzialka.GetPolozenie(kw.KwPolozenieList, PolozenieTyp.Miejscowosc);
                    xlsSheetDzialki.Cells[dzialkaCounter, 7].Value = dzialka.GetPolozenie(kw.KwPolozenieList, PolozenieTyp.Dzielnica);

                    xlsSheetDzialki.Cells[dzialkaCounter, 8].Value = dzialka.IdentyfikatorDzialki;
                    xlsSheetDzialki.Cells[dzialkaCounter, 9].Value = dzialka.NumerDzialki;
                    xlsSheetDzialki.Cells[dzialkaCounter, 10].Value = dzialka.NumerObrebuEwidencyjnego;
                    xlsSheetDzialki.Cells[dzialkaCounter, 11].Value = dzialka.NazwaObrebuEwidencyjnego;
                    xlsSheetDzialki.Cells[dzialkaCounter, 12].Value = dzialka.UlicaMulti;
                    xlsSheetDzialki.Cells[dzialkaCounter, 13].Value = dzialka.GetUlicaForDzialka();
                    xlsSheetDzialki.Cells[dzialkaCounter, 14].Value = dzialka.SposobKorzystania;
                    xlsSheetDzialki.Cells[dzialkaCounter, 15].Value = dzialka.OdlaczenieKw;

                    xlsSheetDzialki.Cells[dzialkaCounter, 16].Value = kw.KwDzialkaList.Count;
                    xlsSheetDzialki.Cells[dzialkaCounter, 17].Value = kw.KwObszar.ObszarHa;

                    dzialkaCounter++;

                }

                foreach (Budynek budynek in kw.KwBudynekList)
                {
                    xlsSheetBudynki.Cells[budynekCounter, 1].Value = kw.KwInformacjePodstawowe.NumerKsiegi;
                    xlsSheetBudynki.Cells[budynekCounter, 2].Value = kw.KwZamkniecieKsiegi.ChwilaZamkniecia;
                    xlsSheetBudynki.Cells[budynekCounter, 3].Value = kw.KwZamkniecieKsiegi.PodstawaZamkniecia;

                    xlsSheetBudynki.Cells[budynekCounter, 4].Value = budynek.PolozenieMulti;
                    xlsSheetBudynki.Cells[budynekCounter, 5].Value = budynek.GetPolozenie(kw.KwPolozenieList, PolozenieTyp.Gmina);
                    xlsSheetBudynki.Cells[budynekCounter, 6].Value = budynek.GetPolozenie(kw.KwPolozenieList, PolozenieTyp.Miejscowosc);
                    xlsSheetBudynki.Cells[budynekCounter, 7].Value = budynek.GetPolozenie(kw.KwPolozenieList, PolozenieTyp.Dzielnica);

                    xlsSheetBudynki.Cells[budynekCounter, 8].Value = budynek.IdentyfikatorBudynku;
                    xlsSheetBudynki.Cells[budynekCounter, 9].Value = budynek.IdentyfikatorDzialkiMulti;
                    xlsSheetBudynki.Cells[budynekCounter, 10].Value = budynek.GetIdentyfikatorDzialkiForBudynek();
                    xlsSheetBudynki.Cells[budynekCounter, 11].Value = budynek.NazwaUlicy;
                    xlsSheetBudynki.Cells[budynekCounter, 12].Value = budynek.NumerPorzadkowy;
                    xlsSheetBudynki.Cells[budynekCounter, 13].Value = budynek.LiczbaKondygnacji;
                    xlsSheetBudynki.Cells[budynekCounter, 14].Value = budynek.LiczbaLokali;
                    xlsSheetBudynki.Cells[budynekCounter, 15].Value = budynek.PowierzchniaUzytkowa;
                    xlsSheetBudynki.Cells[budynekCounter, 16].Value = budynek.Przeznaczenie;
                    xlsSheetBudynki.Cells[budynekCounter, 17].Value = budynek.DalszyOpis;
                    xlsSheetBudynki.Cells[budynekCounter, 18].Value = budynek.Nieruchomosc;
                    xlsSheetBudynki.Cells[budynekCounter, 19].Value = budynek.Odrebnosc;

                    budynekCounter++;
                }

                foreach (Lokal lokal in kw.KwLokalList)
                {
                   
                    foreach (OpisLokaluStruct izba in lokal.OpisLokalu)
                    {
                        lokalRodzajeIzb.Add(izba.RodzajIzby); // dodaj rodzaj lokalu do słownika
                    }

                    foreach (OpisPomieszczenPrzynaleznychStruct pomieszczenie in lokal.OpisPomieszczenPrzynaleznych)
                    {
                        lokalRodzajePomieszczen.Add(pomieszczenie.RodzajPomieszczenia);
                    }

                    xlsSheetLokale.Cells[lokalCounter, 1].Value = kw.KwInformacjePodstawowe.NumerKsiegi;
                    xlsSheetLokale.Cells[lokalCounter, 2].Value = kw.KwZamkniecieKsiegi.ChwilaZamkniecia;
                    xlsSheetLokale.Cells[lokalCounter, 3].Value = kw.KwZamkniecieKsiegi.PodstawaZamkniecia;

                    xlsSheetLokale.Cells[lokalCounter, 4].Value = lokal.PolozenieMulti;
                    xlsSheetLokale.Cells[lokalCounter, 5].Value = lokal.GetPolozenie(kw.KwPolozenieList, PolozenieTyp.Gmina);
                    xlsSheetLokale.Cells[lokalCounter, 6].Value = lokal.GetPolozenie(kw.KwPolozenieList, PolozenieTyp.Miejscowosc);
                    xlsSheetLokale.Cells[lokalCounter, 7].Value = lokal.GetPolozenie(kw.KwPolozenieList, PolozenieTyp.Dzielnica);

                    xlsSheetLokale.Cells[lokalCounter, 8].Value = lokal.IdentyfikatorLokalu;
                    xlsSheetLokale.Cells[lokalCounter, 9].Value = lokal.Ulica;
                    xlsSheetLokale.Cells[lokalCounter, 10].Value = lokal.NumerBudynku;
                    xlsSheetLokale.Cells[lokalCounter, 11].Value = lokal.NumerLokalu;
                    xlsSheetLokale.Cells[lokalCounter, 12].Value = lokal.PrzeznaczenieLokalu;
                    xlsSheetLokale.Cells[lokalCounter, 13].Value = lokal.GetOpisLokalu();
                    xlsSheetLokale.Cells[lokalCounter, 14].Value = lokal.LiczbaIzb;
                    xlsSheetLokale.Cells[lokalCounter, 15].Value = lokal.GetOpisPomieszczenPrzynaleznych();
                    xlsSheetLokale.Cells[lokalCounter, 16].Value = lokal.GetOpisPomieszczenPrzynaleznychPiwnica();
                    xlsSheetLokale.Cells[lokalCounter, 17].Value = lokal.GetOpisPomieszczenPrzynaleznychPiwnicaPow();
                    xlsSheetLokale.Cells[lokalCounter, 18].Value = lokal.GetOpisPomieszczenPrzynaleznychGaraz();
                    xlsSheetLokale.Cells[lokalCounter, 19].Value = lokal.GetOpisPomieszczenPrzynaleznychGarazPow();
                    xlsSheetLokale.Cells[lokalCounter, 20].Value = lokal.GetOpisPomieszczenPrzynaleznychPostoj();
                    xlsSheetLokale.Cells[lokalCounter, 21].Value = lokal.GetOpisPomieszczenPrzynaleznychPostojPow();
                    xlsSheetLokale.Cells[lokalCounter, 22].Value = lokal.GetOpisPomieszczenPrzynaleznychStrych();
                    xlsSheetLokale.Cells[lokalCounter, 23].Value = lokal.GetOpisPomieszczenPrzynaleznychStrychPow();
                    xlsSheetLokale.Cells[lokalCounter, 24].Value = lokal.GetOpisPomieszczenPrzynaleznychKomorka();
                    xlsSheetLokale.Cells[lokalCounter, 25].Value = lokal.GetOpisPomieszczenPrzynaleznychKomorkaPow();
                    xlsSheetLokale.Cells[lokalCounter, 26].Value = lokal.GetOpisPomieszczenPrzynaleznychInne();
                    xlsSheetLokale.Cells[lokalCounter, 27].Value = lokal.GetOpisPomieszczenPrzynaleznychInnePow();
                    xlsSheetLokale.Cells[lokalCounter, 28].Value = lokal.Kondygnacja;
                    xlsSheetLokale.Cells[lokalCounter, 29].Value = lokal.Nieruchomosc;
                    xlsSheetLokale.Cells[lokalCounter, 30].Value = lokal.Odrebnosc;

                    xlsSheetLokale.Cells[lokalCounter, 31].Value = kw.KwObszar.ObszarHa;

                    lokalCounter++;
                }

            }

            // ------------------------------------------------------------------------------------
            
            lokalRodzajeIzb = lokalRodzajeIzb.Distinct().ToList();
            lokalRodzajeIzb.Sort();

            ExcelWorksheet xlsSheetLokaleIzby = xlsWorkbook.Workbook.Worksheets.Add("LokaleIzby");

            xlsSheetLokaleIzby.Cells[1, 1].Value = "RodzajIzby";
            xlsSheetLokaleIzby.Cells[1, 2].Value = "CzyIzba";

            for (int i = 0; i < lokalRodzajeIzb.Count; i++)
            {
                xlsSheetLokaleIzby.Cells[i + 2, 1].Value = lokalRodzajeIzb[i];

                if (lokalSlowniki.RodzajIzbaTak.Exists(x => x == lokalRodzajeIzb[i])) xlsSheetLokaleIzby.Cells[i + 2, 2].Value = "TAK";
                if (lokalSlowniki.RodzajIzbaNie.Exists(x => x == lokalRodzajeIzb[i])) xlsSheetLokaleIzby.Cells[i + 2, 2].Value = "NIE";
            }

            // ------------------------------------------------------------------------------------

            lokalRodzajePomieszczen = lokalRodzajePomieszczen.Distinct().ToList();
            lokalRodzajePomieszczen.Sort();

            ExcelWorksheet xlsSheetLokalePomieszczenia = xlsWorkbook.Workbook.Worksheets.Add("LokalePomieszczenia");

            xlsSheetLokalePomieszczenia.Cells[1, 1].Value = "NazwaPomieszczenia";
            xlsSheetLokalePomieszczenia.Cells[1, 2].Value = "RodzajPomieszczenia";

            for (int i = 0; i < lokalRodzajePomieszczen.Count; i++)
            {
                xlsSheetLokalePomieszczenia.Cells[i + 2, 1].Value = lokalRodzajePomieszczen[i];

                if (lokalSlowniki.RodzajPiwnica.Exists(x => x == lokalRodzajePomieszczen[i])) xlsSheetLokalePomieszczenia.Cells[i + 2, 2].Value = "PIWNICA";
                if (lokalSlowniki.RodzajGaraz.Exists(x => x == lokalRodzajePomieszczen[i])) xlsSheetLokalePomieszczenia.Cells[i + 2, 2].Value = "GARAZ";
                if (lokalSlowniki.RodzajPostoj.Exists(x => x == lokalRodzajePomieszczen[i])) xlsSheetLokalePomieszczenia.Cells[i + 2, 2].Value = "POSTOJOWE";
                if (lokalSlowniki.RodzajStrych.Exists(x => x == lokalRodzajePomieszczen[i])) xlsSheetLokalePomieszczenia.Cells[i + 2, 2].Value = "STRYCH";
                if (lokalSlowniki.RodzajKomorka.Exists(x => x == lokalRodzajePomieszczen[i])) xlsSheetLokalePomieszczenia.Cells[i + 2, 2].Value = "KOMORKA";
                if (lokalSlowniki.RodzajInne.Exists(x => x == lokalRodzajePomieszczen[i])) xlsSheetLokalePomieszczenia.Cells[i + 2, 2].Value = "INNE";
            }
            
            // ------------------------------------------------------------------------------------

            Console.WriteLine("Formatowanie arkusza KW...");

            xlsSheetKw.Cells["A1:F" + Convert.ToString(kwCounter - 1)].AutoFilter = true;
            xlsSheetKw.View.FreezePanes(2, 2);
            xlsSheetKw.Cells.Style.Font.Size = 10;
            xlsSheetKw.Cells.AutoFitColumns(0);

            Console.WriteLine("Formatowanie arkusza działek...");

            xlsSheetDzialki.Cells["A1:Q" + Convert.ToString(dzialkaCounter - 1)].AutoFilter = true;
            xlsSheetDzialki.View.FreezePanes(2, 2);
            xlsSheetDzialki.Cells.Style.Font.Size = 10;
            xlsSheetDzialki.Cells.AutoFitColumns(0);
            xlsSheetDzialki.Column(3).Width = 24;
            xlsSheetDzialki.Column(14).Width = 50;

            Console.WriteLine("Formatowanie arkusza budynków...");

            xlsSheetBudynki.Cells["A1:S" + Convert.ToString(budynekCounter - 1)].AutoFilter = true;
            xlsSheetBudynki.View.FreezePanes(2, 2);
            xlsSheetBudynki.Cells.Style.Font.Size = 10;
            xlsSheetBudynki.Cells.AutoFitColumns(0);
            xlsSheetBudynki.Column(3).Width = 24;

            Console.WriteLine("Formatowanie arkusza lokali...");

            xlsSheetLokale.Cells["A1:AE" + Convert.ToString(lokalCounter - 1)].AutoFilter = true;
            xlsSheetLokale.View.FreezePanes(2, 2);
            xlsSheetLokale.Cells.Style.Font.Size = 10;
            xlsSheetLokale.Cells.AutoFitColumns(0);
            xlsSheetLokale.Column(3).Width = 24;
            xlsSheetLokale.Column(13).Width = 30;
            xlsSheetLokale.Column(15).Width = 30;
            xlsSheetLokale.Column(16).Width = 30;
            xlsSheetLokale.Column(18).Width = 30;
            xlsSheetLokale.Column(20).Width = 30;
            xlsSheetLokale.Column(22).Width = 30;
            xlsSheetLokale.Column(24).Width = 30;
            xlsSheetLokale.Column(26).Width = 30;

            Console.WriteLine("Formatowanie arkusza izb lokali...");

            xlsSheetLokaleIzby.Cells["A1:B" + Convert.ToString(lokalRodzajeIzb.Count + 1)].AutoFilter = true;
            xlsSheetLokaleIzby.View.FreezePanes(2, 2);
            xlsSheetLokaleIzby.Cells.Style.Font.Size = 10;
            xlsSheetLokaleIzby.Cells.AutoFitColumns(0);

            Console.WriteLine("Formatowanie arkusza pomieszczeń przynaleznych do lokali...");

            xlsSheetLokalePomieszczenia.Cells["A1:B" + Convert.ToString(lokalRodzajePomieszczen.Count + 1)].AutoFilter = true;
            xlsSheetLokalePomieszczenia.View.FreezePanes(2, 2);
            xlsSheetLokalePomieszczenia.Cells.Style.Font.Size = 10;
            xlsSheetLokalePomieszczenia.Cells.AutoFitColumns(0);

            Console.WriteLine("Zapisywanie pliku...");

            xlsWorkbook.Save();

            Console.WriteLine("Zapis logów przetarzania KW...");

            // ------------------------------------------------------------------------------------
            // lista ksiąg które mają działki
            // ------------------------------------------------------------------------------------
            StreamWriter outputFile = new StreamWriter(new FileStream(args[0].TrimEnd('\\') + "\\wynik\\KW_Dzialki.txt", FileMode.Create), Encoding.UTF8);
            foreach (string ksiega in listaKwDzialki)
            {
                outputFile.WriteLine(ksiega);
            }
            outputFile.Close();
            // ------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------
            // lista ksiąg które mają budynki
            // ------------------------------------------------------------------------------------
            outputFile = new StreamWriter(new FileStream(args[0].TrimEnd('\\') + "\\wynik\\KW_Budynki.txt", FileMode.Create), Encoding.UTF8);
            foreach (string ksiega in listaKwBudynki)
            {
                outputFile.WriteLine(ksiega);
            }
            outputFile.Close();
            // ------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------
            // lista ksiąg które mają lokale
            // ------------------------------------------------------------------------------------
            outputFile = new StreamWriter(new FileStream(args[0].TrimEnd('\\') + "\\wynik\\KW_Lokale.txt", FileMode.Create), Encoding.UTF8);
            foreach (string ksiega in listaKwLokale)
            {
                outputFile.WriteLine(ksiega);
            }
            outputFile.Close();
            // ------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------
            // lista ksiąg zamkniętych
            // ------------------------------------------------------------------------------------
            outputFile = new StreamWriter(new FileStream(args[0].TrimEnd('\\') + "\\wynik\\KW_Zamkniete.txt", FileMode.Create), Encoding.UTF8);
            foreach (string ksiega in listaKwZamkniete)
            {
                outputFile.WriteLine(ksiega);
            }
            outputFile.Close();
            // ------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------
            // lista danych dla pliku log
            // ------------------------------------------------------------------------------------
            outputFile = new StreamWriter(new FileStream(args[0].TrimEnd('\\') + "\\wynik\\raport.csv", FileMode.Create), Encoding.UTF8);

            outputFile.WriteLine("NazwaPliku;Rubryka;Pole;Opis");

            foreach (KeyValuePair<string, List<string>> loglist in listaLog)
            {
                foreach (string logText in loglist.Value)
                {
                    outputFile.WriteLine(loglist.Key + ";" + logText );
                }

            }
            outputFile.Close();
            // ------------------------------------------------------------------------------------

            Console.WriteLine("Gotowe!");
            Console.ReadKey();

        }

    }
}

