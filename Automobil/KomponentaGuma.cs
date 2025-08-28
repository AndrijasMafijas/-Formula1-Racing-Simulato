using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automobil
{
    public class KomponentaGuma
    {
        public enum TipGuma
        {
            Meke,     // M - 80km
            Srednje,  // S - 100km  
            Tvrde     // T - 120km
        }

        public TipGuma Tip { get; private set; }
        public double MaksimalnaDuzinaUkm { get; private set; }
        public double TrenutnaPotrosenost { get; private set; } // u km
        public string Oznaka { get; private set; }

        public KomponentaGuma(TipGuma tip)
        {
            Tip = tip;
            TrenutnaPotrosenost = 0;
            
            switch (tip)
            {
                case TipGuma.Meke:
                    MaksimalnaDuzinaUkm = 80;
                    Oznaka = "M";
                    break;
                case TipGuma.Srednje:
                    MaksimalnaDuzinaUkm = 100;
                    Oznaka = "S";
                    break;
                case TipGuma.Tvrde:
                    MaksimalnaDuzinaUkm = 120;
                    Oznaka = "T";
                    break;
            }
        }

        public void Potrosi(double kilometri)
        {
            TrenutnaPotrosenost += kilometri;
            if (TrenutnaPotrosenost > MaksimalnaDuzinaUkm)
            {
                TrenutnaPotrosenost = MaksimalnaDuzinaUkm;
            }
        }

        public double ProcentPotrosenosti()
        {
            return (TrenutnaPotrosenost / MaksimalnaDuzinaUkm) * 100;
        }

        public bool JeLiIstrosen()
        {
            return TrenutnaPotrosenost >= MaksimalnaDuzinaUkm;
        }

        public double PreostalaKilometraza()
        {
            return Math.Max(0, MaksimalnaDuzinaUkm - TrenutnaPotrosenost);
        }

        public override string ToString()
        {
            return $"Gume {Oznaka} - {ProcentPotrosenosti():F1}% potrošeno ({PreostalaKilometraza():F1} km preostalo)";
        }
    }
}