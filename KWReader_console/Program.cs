using System;
using System.IO;
using System.Text;
using KW_Tools;
using OfficeOpenXml;

namespace KWReader
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Jako parametr podaj ścieżkę do plików *.html");
                return;
            }

            FileInfo xlsFile = new FileInfo("c:\\wynik.xlsx");
            if (xlsFile.Exists) xlsFile.Delete();

            ExcelPackage xlsWorkbook = new ExcelPackage(xlsFile);

            xlsWorkbook.Workbook.Properties.Title = "Raport KW";
            xlsWorkbook.Workbook.Properties.Author = "Grzegorz Gogolewski";
            xlsWorkbook.Workbook.Properties.Comments = "Raport KW";
            xlsWorkbook.Workbook.Properties.Company = "GISNET";

            ExcelWorksheet xlsSheetDzialki = xlsWorkbook.Workbook.Worksheets.Add("Działki");

            xlsSheetDzialki.Cells[1, 1].Value = "NumerKsiegi";
            xlsSheetDzialki.Cells[1, 2].Value = "ChwilaZamkniecia";
            xlsSheetDzialki.Cells[1, 3].Value = "PodstawaZamkniecia";
            xlsSheetDzialki.Cells[1, 4].Value = "PolozenieMulti";
            xlsSheetDzialki.Cells[1, 5].Value = "Gmina";
            xlsSheetDzialki.Cells[1, 6].Value = "Miejscowosc";
            xlsSheetDzialki.Cells[1, 7].Value = "IdentyfikatorDzialki";
            xlsSheetDzialki.Cells[1, 8].Value = "NumerDzialki";
            xlsSheetDzialki.Cells[1, 9].Value = "NumerObrebuEwidencyjnego";
            xlsSheetDzialki.Cells[1, 10].Value = "NazwaObrebuEwidencyjnego";
            xlsSheetDzialki.Cells[1, 11].Value = "UlicaMulti";
            xlsSheetDzialki.Cells[1, 12].Value = "Ulica";
            xlsSheetDzialki.Cells[1, 13].Value = "SposobKorzystania";

            ExcelWorksheet xlsSheetBudynki = xlsWorkbook.Workbook.Worksheets.Add("Budynki");

            string [] fileEntries = Directory.GetFiles(args[0], "*.html");

            int i = 0;
            int dzialkaCounter = 2;
            int budynekCounter = 2;

            foreach (string file in fileEntries)
            {
                i++;

                StreamReader htmlFile = new StreamReader(new FileStream(file, FileMode.Open), Encoding.UTF8);

                KwFromHtml kw = new KwFromHtml(htmlFile.ReadToEnd());
            
                htmlFile.Close();

                kw.ParseKw();

                Console.WriteLine("[{0}/{1}]: {2}, Liczba działek: {3}, Liczba budynków: {4}", 
                                  i, fileEntries.Length, kw.KwInformacjePodstawowe.NumerKsiegi, kw.KwDzialkaList.Count, kw.KwBudynekList.Count);

                foreach (Dzialka dzialka in kw.KwDzialkaList)
                {
                    xlsSheetDzialki.Cells[dzialkaCounter, 1].Value = kw.KwInformacjePodstawowe.NumerKsiegi;
                    xlsSheetDzialki.Cells[dzialkaCounter, 2].Value = kw.KwZamkniecieKsiegi.ChwilaZamkniecia;
                    xlsSheetDzialki.Cells[dzialkaCounter, 3].Value = kw.KwZamkniecieKsiegi.PodstawaZamkniecia;
                    xlsSheetDzialki.Cells[dzialkaCounter, 4].Value = dzialka.PolozenieMulti;
                    xlsSheetDzialki.Cells[dzialkaCounter, 5].Value = kw.GetPolozenie(dzialka, PolozenieTyp.Gmina);
                    xlsSheetDzialki.Cells[dzialkaCounter, 6].Value = kw.GetPolozenie(dzialka, PolozenieTyp.Miejscowosc);
                    xlsSheetDzialki.Cells[dzialkaCounter, 7].Value = dzialka.IdentyfikatorDzialki;
                    xlsSheetDzialki.Cells[dzialkaCounter, 8].Value = dzialka.NumerDzialki;
                    xlsSheetDzialki.Cells[dzialkaCounter, 9].Value = dzialka.NumerObrebuEwidencyjnego;
                    xlsSheetDzialki.Cells[dzialkaCounter, 10].Value = dzialka.NazwaObrebuEwidencyjnego;
                    xlsSheetDzialki.Cells[dzialkaCounter, 11].Value = dzialka.UlicaMulti;
                    xlsSheetDzialki.Cells[dzialkaCounter, 12].Value = kw.GetUlica(dzialka);
                    xlsSheetDzialki.Cells[dzialkaCounter, 13].Value = dzialka.SposobKorzystania;

                    dzialkaCounter++;

                }

                foreach (Budynek budynek in kw.KwBudynekList)
                {
                    xlsSheetBudynki.Cells[budynekCounter, 1].Value = budynek.IdentyfikatorBudynku;

                    budynekCounter++;
                }

                StreamWriter logFile = new StreamWriter(new FileStream(file.Substring(0, file.LastIndexOf(".", StringComparison.Ordinal)) + ".log", FileMode.Create), Encoding.UTF8);

                foreach (string log in kw.KwLog)
                {
                    logFile.WriteLine(log);
                }

                logFile.Close();
            }

            Console.WriteLine("Formatowanie pliku...");

            xlsSheetDzialki.Cells["A1:M" + Convert.ToString(dzialkaCounter-1)].AutoFilter = true;
            xlsSheetDzialki.View.FreezePanes(2, 1);
            xlsSheetDzialki.Cells.Style.Font.Size = 10;
            xlsSheetDzialki.Cells.AutoFitColumns(0);
            xlsSheetDzialki.Column(3).Width = 24;

            Console.WriteLine("Zapisywanie pliku...");

            xlsWorkbook.Save();

            Console.WriteLine("Gotowe!");
            Console.ReadKey();

        }

    }
}

