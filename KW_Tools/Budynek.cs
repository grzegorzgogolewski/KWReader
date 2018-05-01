using System.Collections.Generic;

namespace KW_Tools
{
    public class Budynek
    {
        public string IdentyfikatorBudynku;
        public string IdentyfikatorDzialki;
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
