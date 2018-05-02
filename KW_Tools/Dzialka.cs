using System.Collections.Generic;

namespace KW_Tools
{
    public class Dzialka
    {
        public string IdentyfikatorDzialki;
        public string NumerDzialki;
        public string NumerObrebuEwidencyjnego;
        public string NazwaObrebuEwidencyjnego;
        /// <summary>
        /// Położenie może być listą atrybutów
        /// </summary>
        public List<string> PolozenieList = new List<string>();
        public bool PolozenieMulti;
        /// <summary>
        /// Ulica może być listą atrybutów
        /// </summary>
        public List<string> UliceList = new List<string>();
        public bool UlicaMulti;
        public string SposobKorzystania;
    }
}
