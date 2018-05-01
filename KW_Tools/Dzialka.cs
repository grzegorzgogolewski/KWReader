using System.Collections.Generic;

namespace KW_Tools
{
    public class Dzialka
    {
        public string IdentyfikatorDzialki;
        public string NumerDzialki;
        public string NumerObrebuEwidencyjnego;
        public string NazwaObrebuEwidencyjnego;
        public List<string> PolozenieList = new List<string>();
        public bool PolozenieMulti;
        public List<string> UliceList = new List<string>();
        public bool UlicaMulti;
        public string SposobKorzystania;
    }
}
