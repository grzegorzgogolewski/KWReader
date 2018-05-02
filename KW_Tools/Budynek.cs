using System.Collections.Generic;

namespace KW_Tools
{   
    /// <summary>
    /// Klasa dla atrybutów budynku pozyskanego z KW
    /// </summary>
    public class Budynek
    {
        public string IdentyfikatorBudynku;
        public string IdentyfikatorDzialki;
        /// <summary>
        /// Położenie może być listą atrybutów
        /// </summary>
        public List<string> PolozenieList = new List<string>();
        public bool PolozenieMulti;
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
