using System.Collections.Generic;

namespace KW_Tools
{
    public class Lokal
    {
        public string IdentyfikatorLokalu;
        public string Ulica;
        public string NumerBudynku;
        public string NumerLokalu;
        public string PrzeznaczenieLokalu;
        public string OpisLokalu;
        public string OpisPomieszczenPrzynależnych;
        public string Kondygnacja;
        public string Nieruchomosc;
        public string Odrebnosc;
        public List<string> PolozenieList = new List<string>();
        public bool PolozenieMulti = false;
    }
}
