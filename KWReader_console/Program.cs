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

            ExcelWorksheet xlsSheet = xlsWorkbook.Workbook.Worksheets.Add("KW");

            xlsSheet.Cells[1, 1].Value = "NumerKsiegi";
            xlsSheet.Cells[1, 2].Value = "IdentyfikatorDzialki";
            xlsSheet.Cells[1, 3].Value = "NumerDzialki";
            xlsSheet.Cells[1, 4].Value = "NumerObrebuEwidencyjnego";
            xlsSheet.Cells[1, 5].Value = "NazwaObrebuEwidencyjnego";
            xlsSheet.Cells[1, 6].Value = "Miejscowosc";
            xlsSheet.Cells[1, 7].Value = "Ulica";
            xlsSheet.Cells[1, 8].Value = "SposobKorzystania";

            int dzialkaCounter = 2;

            string [] fileEntries = Directory.GetFiles(args[0], "*.html");

            int i = 0;

            foreach (string file in fileEntries)
            {
                i++;

                StreamReader htmlFile = new StreamReader(new FileStream(file, FileMode.Open), Encoding.UTF8);

                KwFromHtml kw = new KwFromHtml(htmlFile.ReadToEnd());
            
                htmlFile.Close();

                kw.ParseKw();
                
                foreach (Dzialka dzialka in kw.KwDzialkaList)
                {
               
                    Console.WriteLine("[{0}/{1}]: {2}", i, fileEntries.Length, kw.KwInformacjePodstawowe.NumerKsiegi);
                    //Console.WriteLine("IdentyfikatorDzialki: {0}", dzialka.IdentyfikatorDzialki);
                    //Console.WriteLine("NumerDzialki: {0}", dzialka.NumerDzialki);
                    //Console.WriteLine("NumerObrebuEwidencyjnego: {0}", dzialka.NumerObrebuEwidencyjnego);
                    //Console.WriteLine("NazwaObrebuEwidencyjnego: {0}", dzialka.NazwaObrebuEwidencyjnego);

                    //Console.WriteLine("Miejscowosc: {0}", kw.GetMiejscowosc(dzialka));

                    //Console.WriteLine("Ulica: {0}", dzialka.Ulica);
                    //Console.WriteLine("Sposób korzystania: {0}", dzialka.SposobKorzystania);

                    //Console.WriteLine("---------------------------------------------------------");

                    xlsSheet.Cells[dzialkaCounter, 1].Value = kw.KwInformacjePodstawowe.NumerKsiegi;
                    xlsSheet.Cells[dzialkaCounter, 2].Value = dzialka.IdentyfikatorDzialki;
                    xlsSheet.Cells[dzialkaCounter, 3].Value = dzialka.NumerDzialki;
                    xlsSheet.Cells[dzialkaCounter, 4].Value = dzialka.NumerObrebuEwidencyjnego;
                    xlsSheet.Cells[dzialkaCounter, 5].Value = dzialka.NazwaObrebuEwidencyjnego;
                    xlsSheet.Cells[dzialkaCounter, 6].Value = kw.GetMiejscowosc(dzialka);
                    xlsSheet.Cells[dzialkaCounter, 7].Value = dzialka.Ulica;
                    xlsSheet.Cells[dzialkaCounter, 8].Value = dzialka.SposobKorzystania;

                    dzialkaCounter++;

                }
            }

            xlsWorkbook.Save();

            Console.ReadKey();

        }

    }
}

