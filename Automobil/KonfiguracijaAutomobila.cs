using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automobil
{
    public class KonfiguracijaAutomobila
    {
        public string Proizvodjac { get; private set; }
        public double PotrosnjaGuma { get; private set; }
        public double PotrosnjaGoriva { get; private set; } // litara po kilometru

        public KonfiguracijaAutomobila(string proizvodjac)
        {
            Proizvodjac = proizvodjac;
            PostaviPerformanse(proizvodjac);
        }

        private void PostaviPerformanse(string proizvodjac)
        {
            switch (proizvodjac.ToLower())
            {
                case "mercedes":
                    PotrosnjaGuma = 0.3;
                    PotrosnjaGoriva = 0.6;
                    break;
                case "ferrari":
                    PotrosnjaGuma = 0.3;
                    PotrosnjaGoriva = 0.5;
                    break;
                case "renault":
                    PotrosnjaGuma = 0.4;
                    PotrosnjaGoriva = 0.7;
                    break;
                case "honda":
                    PotrosnjaGuma = 0.2;
                    PotrosnjaGoriva = 0.6;
                    break;
                default:
                    throw new ArgumentException($"Nepoznat proizvođač: {proizvodjac}");
            }
        }

        public override string ToString()
        {
            return $"{Proizvodjac} - Potrošnja guma: {PotrosnjaGuma}, Potrošnja goriva: {PotrosnjaGoriva} l/km";
        }
    }
}