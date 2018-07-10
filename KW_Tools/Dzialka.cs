using System;
using System.Collections.Generic;
using KWTools;

namespace KWTools
{
    public class Dzialka
    {
        public string IdentyfikatorDzialki;
        public string NumerDzialki;
        public string NumerObrebuEwidencyjnego;
        public string NazwaObrebuEwidencyjnego;
        public List<string> PolozenieList = new List<string>();
        public bool PolozenieMulti = false;
        public List<string> UliceList = new List<string>();
        public bool UlicaMulti = false;
        public string SposobKorzystania;
        public string OdlaczenieKw;

        public string GetUlicaForDzialka()
        {
            string wynik = "";

            if (UliceList.Count == 1)
            {
                wynik =  UliceList[0];
            }
            else
            {
                foreach (string ulica in UliceList)
                {
                    wynik = wynik + ulica + ", ";
                }

                wynik = wynik.Substring(0, wynik.Length - 2);
            }

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
