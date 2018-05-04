using System.Collections.Generic;

namespace KW_Tools
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
    }
}
