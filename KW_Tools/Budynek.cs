using System;
using System.Collections.Generic;
using KWTools;

namespace KWTools
{   
    /// <summary>
    /// Klasa dla atrybutów budynku pozyskanego z KW
    /// </summary>
    public class Budynek
    {
        public string IdentyfikatorBudynku;
        public List<string> IdentyfikatorDzialkiList = new List<string>();
        public bool IdentyfikatorDzialkiMulti = false;
        public List<string> PolozenieList = new List<string>();
        public bool PolozenieMulti = false;
        public string NazwaUlicy;
        public string NumerPorzadkowy;
        public string LiczbaKondygnacji;
        public string LiczbaLokali;
        public string PowierzchniaUzytkowa;
        public string Przeznaczenie;
        public string DalszyOpis;
        public string Nieruchomosc;
        public string Odrebnosc;

        public string GetIdentyfikatorDzialkiForBudynek()
        {
            string wynik = "";

            if (IdentyfikatorDzialkiList.Count == 1)
            {
                wynik =  IdentyfikatorDzialkiList[0];
            }
            else
            {
                foreach (string identyfikator in IdentyfikatorDzialkiList)
                {
                    wynik = wynik + identyfikator + ", ";
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
