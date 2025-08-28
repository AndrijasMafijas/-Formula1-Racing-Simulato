using System;
using System.Collections.Generic;
using System.Linq;

namespace DirekcijaTrke
{
    public class DisplayManager
    {
        // IspisiAktivneKlijente metoda:
        public void IspisiAktivneKlijente(List<KlijentInfo> klijenti)
        {
            if (klijenti.Count > 0)
            {
                Console.WriteLine("\n=== AKTIVNI KLIJENTI ===");
                for (int i = 0; i < klijenti.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {klijenti[i]}");
                }
                Console.WriteLine("========================\n");
            }
        }

        // IspisiKonacneRezultate metoda:
        public void IspisiKonacneRezultate(Dictionary<string, List<double>> vremenaPokrugu, List<KlijentInfo> aktivniKlijenti)
        {
            Console.WriteLine("\nRACE RACE RACE KONACNI REZULTATI SESIJE RACE RACE RACE");
            
            var sviAutomobili = vremenaPokrugu
                .Where(kvp => kvp.Value.Count > 0)
                .Select(kvp => new AutomobilRezultat {
                    Automobil = kvp.Key,
                    NajboljeVreme = kvp.Value.Min(),
                    NajgoreVreme = kvp.Value.Max(),
                    BrojKrugova = kvp.Value.Count,
                    ProsecnoVreme = kvp.Value.Average(),
                    SviKrugovi = kvp.Value,
                    NaStazi = aktivniKlijenti.Any(k => k.GetId() == kvp.Key && k.NaStazi)
                })
                .OrderBy(x => x.NajboljeVreme)
                .ToList();

            if (sviAutomobili.Count == 0)
            {
                Console.WriteLine("Nema podataka o krugovima.");
                return;
            }

            double globalnoNajboljeVreme = sviAutomobili.Min(x => x.NajboljeVreme);
            
            Console.WriteLine($"FAST NAJBRZI KRUG SESIJE: {globalnoNajboljeVreme:F3}s");
            
            var najbrziAuto = sviAutomobili.First(x => Math.Abs(x.NajboljeVreme - globalnoNajboljeVreme) < 0.001);
            Console.WriteLine($"   KING Postavio: {najbrziAuto.Automobil}");
            Console.WriteLine();

            IspisiTabeluRezultata(sviAutomobili, globalnoNajboljeVreme);
            IspisiStatistikeSesije(sviAutomobili);
            
            Console.WriteLine("RACE RACE RACE RACE RACE RACE RACE RACE RACE RACE RACE\n");
        }

        // IspisiTabeluRezultata metoda:
        private void IspisiTabeluRezultata(List<AutomobilRezultat> sviAutomobili, double globalnoNajboljeVreme)
        {
            Console.WriteLine("BEST KONACNA TABELA:");
            for (int i = 0; i < sviAutomobili.Count; i++)
            {
                var auto = sviAutomobili[i];
                string pozicija = i == 0 ? "1st" : i == 1 ? "2nd" : i == 2 ? "3rd" : $"{i + 1}.";
                string status = auto.NaStazi ? "CAR" : "RACE";
                double razlika = auto.NajboljeVreme - globalnoNajboljeVreme;
                string razlikaStr = razlika > 0.001 ? $" (+{razlika:F3}s)" : " FAST";

                Console.WriteLine($"{pozicija} {status} {auto.Automobil}");
                Console.WriteLine($"     BEST Najbolji: {auto.NajboljeVreme:F3}s{razlikaStr}");
                Console.WriteLine($"     DATA Prosecno: {auto.ProsecnoVreme:F3}s | Najgore: {auto.NajgoreVreme:F3}s");
                Console.WriteLine($"     LAPS Krugova: {auto.BrojKrugova}");
                
                if (auto.SviKrugovi.Count > 1)
                {
                    double variance = auto.SviKrugovi.Select(t => Math.Pow(t - auto.ProsecnoVreme, 2)).Average();
                    double stdDev = Math.Sqrt(variance);
                    Console.WriteLine($"     CONSISTENCY Konzistentnost: ±{stdDev:F3}s");
                }
                Console.WriteLine();
            }
        }

        private void IspisiStatistikeSesije(List<AutomobilRezultat> sviAutomobili)
        {
            int ukupnoKrugova = sviAutomobili.Sum(x => x.BrojKrugova);
            int automobilaNaStazi = sviAutomobili.Count(x => x.NaStazi);
            double prosecnoVremeSvihKrugova = sviAutomobili.SelectMany(x => x.SviKrugovi).Average();

            Console.WriteLine("DATA STATISTIKE SESIJE:"); // ✅ Već ispravljen
            Console.WriteLine($"   CAR Ucesnika: {sviAutomobili.Count} (aktivnih: {automobilaNaStazi})"); // ✅ Već ispravljen
            Console.WriteLine($"   LAPS Ukupno krugova: {ukupnoKrugova}"); // ✅ Već ispravljen
            Console.WriteLine($"   TIME Prosek svih krugova: {prosecnoVremeSvihKrugova:F3}s"); // ✅ Već ispravljen
        }
    }

    // NOVA KLASA: Umesto anonimnog tipa
    public class AutomobilRezultat
    {
        public string Automobil { get; set; }
        public double NajboljeVreme { get; set; }
        public double NajgoreVreme { get; set; }
        public int BrojKrugova { get; set; }
        public double ProsecnoVreme { get; set; }
        public List<double> SviKrugovi { get; set; }
        public bool NaStazi { get; set; }
    }
}