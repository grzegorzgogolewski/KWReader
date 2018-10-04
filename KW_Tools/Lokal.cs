using System;
using System.Collections.Generic;

namespace KWTools
{
    public struct OpisLokaluStruct
    {
        public string IdIzby;
        public string RodzajIzby;
        public string LiczbaIzb;

        public OpisLokaluStruct(string idIzby, string rodzajIzby, string liczbaIzb)
        {
            IdIzby = idIzby;
            RodzajIzby = rodzajIzby;
            LiczbaIzb = liczbaIzb;
        }
    }

    public struct OpisPomieszczenPrzynaleznychStruct
    {
        public string IdPomieszczenia;
        public string RodzajPomieszczenia;
        public string LiczbaPomieszczen;
        public string PowierzchniaPomieszczenia;

        public OpisPomieszczenPrzynaleznychStruct(string idPomieszczenia, string rodzajPomieszczenia, string liczbaPomieszczen, string powierzchniaPom)
        {
            IdPomieszczenia = idPomieszczenia;
            RodzajPomieszczenia = rodzajPomieszczenia;
            LiczbaPomieszczen = liczbaPomieszczen;
            PowierzchniaPomieszczenia = powierzchniaPom;
        }
    }

    public class Lokal
    {
        public string IdentyfikatorLokalu;
        public string Ulica;
        public string NumerBudynku;
        public string NumerLokalu;
        public string PrzeznaczenieLokalu;
        public List<OpisLokaluStruct> OpisLokalu = new List<OpisLokaluStruct>();
        public int LiczbaIzb = 0;

        public List<OpisPomieszczenPrzynaleznychStruct> OpisPomieszczenPrzynaleznych = new List<OpisPomieszczenPrzynaleznychStruct>();
        public List<OpisPomieszczenPrzynaleznychStruct> OpisPomieszczenPrzynaleznychPiwnica = new List<OpisPomieszczenPrzynaleznychStruct>();
        public List<OpisPomieszczenPrzynaleznychStruct> OpisPomieszczenPrzynaleznychGaraz = new List<OpisPomieszczenPrzynaleznychStruct>();
        public List<OpisPomieszczenPrzynaleznychStruct> OpisPomieszczenPrzynaleznychPostoj = new List<OpisPomieszczenPrzynaleznychStruct>();
        public List<OpisPomieszczenPrzynaleznychStruct> OpisPomieszczenPrzynaleznychStrych = new List<OpisPomieszczenPrzynaleznychStruct>();
        public List<OpisPomieszczenPrzynaleznychStruct> OpisPomieszczenPrzynaleznychKomorka = new List<OpisPomieszczenPrzynaleznychStruct>();
        public List<OpisPomieszczenPrzynaleznychStruct> OpisPomieszczenPrzynaleznychInne = new List<OpisPomieszczenPrzynaleznychStruct>();
        public string Kondygnacja;
        public string Nieruchomosc;
        public string Odrebnosc;
        public List<string> PolozenieList = new List<string>();
        public bool PolozenieMulti = false;

        public string GetOpisLokalu()
        {
            string wynik = "";

            foreach (OpisLokaluStruct izba in OpisLokalu)
            {
                wynik = wynik + izba.RodzajIzby + ": " + izba.LiczbaIzb + "; ";
            }

            wynik = wynik.TrimEnd(' ').TrimEnd(';');


            return wynik;
        }

        public string GetOpisPomieszczenPrzynaleznych()
        {
            string wynik = "";

            foreach (OpisPomieszczenPrzynaleznychStruct pomieszczenie in OpisPomieszczenPrzynaleznych)
            {
                wynik = wynik + pomieszczenie.RodzajPomieszczenia + "; ";
            }

            wynik = wynik.TrimEnd(' ').TrimEnd(';');

            return wynik;
        }

        public string GetOpisPomieszczenPrzynaleznychPiwnica()
        {
            string wynik = "";

            foreach (OpisPomieszczenPrzynaleznychStruct pomieszczenie in OpisPomieszczenPrzynaleznychPiwnica)
            {
                wynik = wynik + pomieszczenie.RodzajPomieszczenia + "; ";
            }

            wynik = wynik.TrimEnd(' ').TrimEnd(';');


            return wynik;
        }

        public string GetOpisPomieszczenPrzynaleznychPiwnicaPow()
        {
            string wynik = "";

            foreach (OpisPomieszczenPrzynaleznychStruct pomieszczenie in OpisPomieszczenPrzynaleznychPiwnica)
            {
                if (pomieszczenie.PowierzchniaPomieszczenia != "") wynik = wynik + pomieszczenie.PowierzchniaPomieszczenia + "; ";
            }

            wynik = wynik.TrimEnd(' ').TrimEnd(';');


            return wynik;
        }


        public string GetOpisPomieszczenPrzynaleznychGaraz()
        {
            string wynik = "";

            foreach (OpisPomieszczenPrzynaleznychStruct pomieszczenie in OpisPomieszczenPrzynaleznychGaraz)
            {
                wynik = wynik + pomieszczenie.RodzajPomieszczenia + "; ";
            }

            wynik = wynik.TrimEnd(' ').TrimEnd(';');


            return wynik;
        }

        public string GetOpisPomieszczenPrzynaleznychGarazPow()
        {
            string wynik = "";

            foreach (OpisPomieszczenPrzynaleznychStruct pomieszczenie in OpisPomieszczenPrzynaleznychGaraz)
            {
                if (pomieszczenie.PowierzchniaPomieszczenia != "") wynik = wynik + pomieszczenie.PowierzchniaPomieszczenia + "; ";
            }

            wynik = wynik.TrimEnd(' ').TrimEnd(';');


            return wynik;
        }

        public string GetOpisPomieszczenPrzynaleznychPostoj()
        {
            string wynik = "";

            foreach (OpisPomieszczenPrzynaleznychStruct pomieszczenie in OpisPomieszczenPrzynaleznychPostoj)
            {
                wynik = wynik + pomieszczenie.RodzajPomieszczenia + "; ";
            }

            wynik = wynik.TrimEnd(' ').TrimEnd(';');


            return wynik;
        }

        public string GetOpisPomieszczenPrzynaleznychPostojPow()
        {
            string wynik = "";

            foreach (OpisPomieszczenPrzynaleznychStruct pomieszczenie in OpisPomieszczenPrzynaleznychPostoj)
            {
                if (pomieszczenie.PowierzchniaPomieszczenia != "") wynik = wynik + pomieszczenie.PowierzchniaPomieszczenia + "; ";
            }

            wynik = wynik.TrimEnd(' ').TrimEnd(';');


            return wynik;
        }

        public string GetOpisPomieszczenPrzynaleznychStrych()
        {
            string wynik = "";

            foreach (OpisPomieszczenPrzynaleznychStruct pomieszczenie in OpisPomieszczenPrzynaleznychStrych)
            {
                wynik = wynik + pomieszczenie.RodzajPomieszczenia + "; ";
            }

            wynik = wynik.TrimEnd(' ').TrimEnd(';');


            return wynik;
        }

        public string GetOpisPomieszczenPrzynaleznychStrychPow()
        {
            string wynik = "";

            foreach (OpisPomieszczenPrzynaleznychStruct pomieszczenie in OpisPomieszczenPrzynaleznychStrych)
            {
                if (pomieszczenie.PowierzchniaPomieszczenia != "") wynik = wynik + pomieszczenie.PowierzchniaPomieszczenia + "; ";
            }

            wynik = wynik.TrimEnd(' ').TrimEnd(';');


            return wynik;
        }

        public string GetOpisPomieszczenPrzynaleznychKomorka()
        {
            string wynik = "";

            foreach (OpisPomieszczenPrzynaleznychStruct pomieszczenie in OpisPomieszczenPrzynaleznychKomorka)
            {
                wynik = wynik + pomieszczenie.RodzajPomieszczenia + "; ";
            }

            wynik = wynik.TrimEnd(' ').TrimEnd(';');


            return wynik;
        }

        public string GetOpisPomieszczenPrzynaleznychKomorkaPow()
        {
            string wynik = "";

            foreach (OpisPomieszczenPrzynaleznychStruct pomieszczenie in OpisPomieszczenPrzynaleznychKomorka)
            {
                if (pomieszczenie.PowierzchniaPomieszczenia != "") wynik = wynik + pomieszczenie.PowierzchniaPomieszczenia + "; ";
            }

            wynik = wynik.TrimEnd(' ').TrimEnd(';');


            return wynik;
        }

        public string GetOpisPomieszczenPrzynaleznychInne()
        {
            string wynik = "";

            foreach (OpisPomieszczenPrzynaleznychStruct pomieszczenie in OpisPomieszczenPrzynaleznychInne)
            {
                wynik = wynik + pomieszczenie.RodzajPomieszczenia + "; ";
            }

            wynik = wynik.TrimEnd(' ').TrimEnd(';');


            return wynik;
        }

        public string GetOpisPomieszczenPrzynaleznychInnePow()
        {
            string wynik = "";

            foreach (OpisPomieszczenPrzynaleznychStruct pomieszczenie in OpisPomieszczenPrzynaleznychInne)
            {
                if (pomieszczenie.PowierzchniaPomieszczenia != "") wynik = wynik + pomieszczenie.PowierzchniaPomieszczenia + "; ";
            }

            wynik = wynik.TrimEnd(' ').TrimEnd(';');


            return wynik;
        }

        public string GetPolozenie(List<Polozenie> polozenieList, PolozenieTyp atrybut)
        {
            string wynik = "";

                if (polozenieList.Count == 1)
                {
                    try
                    {
                        switch (atrybut)
                        {
                            case PolozenieTyp.Wojewodztwo:
                                wynik = polozenieList.Find(x => x.NumerPorzadkowy == PolozenieList[0]).Wojewodztwo;
                                break;
                            case PolozenieTyp.Powiat:
                                wynik = polozenieList.Find(x => x.NumerPorzadkowy == PolozenieList[0]).Powiat;
                                break;
                            case PolozenieTyp.Gmina:
                                wynik = polozenieList.Find(x => x.NumerPorzadkowy == PolozenieList[0]).Gmina;
                                break;
                            case PolozenieTyp.Miejscowosc:
                                wynik = polozenieList.Find(x => x.NumerPorzadkowy == PolozenieList[0]).Miejscowosc;
                                break;
                            case PolozenieTyp.Dzielnica:
                                wynik = polozenieList.Find(x => x.NumerPorzadkowy == PolozenieList[0]).Dzielnica;
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        wynik =  "- - -";
                    }
                }
                else
                {
                    foreach (string polozenie in PolozenieList)
                    {
                        try
                        {
                            switch (atrybut)
                            {
                                case PolozenieTyp.Wojewodztwo:
                                    wynik = wynik + polozenieList.Find(x => x.NumerPorzadkowy == polozenie).Wojewodztwo + ", ";
                                    break;
                                case PolozenieTyp.Powiat:
                                    wynik = wynik + polozenieList.Find(x => x.NumerPorzadkowy == polozenie).Powiat + ", ";
                                    break;
                                case PolozenieTyp.Gmina:
                                    wynik = wynik + polozenieList.Find(x => x.NumerPorzadkowy == polozenie).Gmina + ", ";
                                    break;
                                case PolozenieTyp.Miejscowosc:
                                    wynik = wynik + polozenieList.Find(x => x.NumerPorzadkowy == polozenie).Miejscowosc + ", ";
                                    break;
                                case PolozenieTyp.Dzielnica:
                                    wynik = wynik + polozenieList.Find(x => x.NumerPorzadkowy == polozenie).Dzielnica + ", ";
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
