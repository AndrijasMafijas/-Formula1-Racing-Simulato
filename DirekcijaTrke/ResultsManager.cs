using System;
using System.Collections.Generic;
using System.Linq;

namespace DirekcijaTrke
{
    public class ResultsManager
    {
        private readonly Dictionary<string, List<double>> vremenaPokrugu;
        private readonly List<KlijentInfo> aktivniKlijenti;

        public ResultsManager(List<KlijentInfo> aktivniKlijenti)
        {
            this.aktivniKlijenti = aktivniKlijenti;
            vremenaPokrugu = new Dictionary<string, List<double>>();
        }

        public void DodajVremeKruga(string automobilId, double vreme)
        {
            if (!vremenaPokrugu.ContainsKey(automobilId))
            {
                vremenaPokrugu[automobilId] = new List<double>();
            }
            vremenaPokrugu[automobilId].Add(vreme);
        }

        public void RegistrujAutomobil(string automobilId)
        {
            if (!vremenaPokrugu.ContainsKey(automobilId))
            {
                vremenaPokrugu[automobilId] = new List<double>();
                Console.WriteLine($" Kreiran novi entry za {automobilId}");
            }
        }

        public bool JeNovoNajboljeVreme(string automobilId, double vreme)
        {
            if (!vremenaPokrugu.ContainsKey(automobilId) || vremenaPokrugu[automobilId].Count == 0)
                return true;
            
            return vreme < vremenaPokrugu[automobilId].Min();
        }

        public void IspisiNajboljaVremena()
        {
            Console.WriteLine("\n=== BEST TRENUTNA NAJBOLJA VREMENA ===");
            
            var najboljaVremena = GetNajboljaVremenaData();
            
            if (najboljaVremena.Count == 0)
            {
                Console.WriteLine("Nema zabelezenih vremena.");
                return;
            }

            double globalnoNajboljeVreme = najboljaVremena.Min(x => x.NajboljeVreme);

            for (int i = 0; i < najboljaVremena.Count; i++)
            {
                var auto = najboljaVremena[i];
                string status = auto.NaStazi ? "CAR" : "RACE";
                string oznakaNajboljeg = GetPozicijuOznaku(i, auto.NajboljeVreme, globalnoNajboljeVreme);
                double razlika = auto.NajboljeVreme - globalnoNajboljeVreme;
                string razlikaStr = razlika > 0.001 ? $" (+{razlika:F3}s)" : "";

                Console.WriteLine($"{i + 1}. {status} {oznakaNajboljeg}{auto.Automobil}");
                Console.WriteLine($"     BEST Najbolje: {auto.NajboljeVreme:F3}s{razlikaStr}");
                Console.WriteLine($"     TIME Poslednje: {auto.PoslednjeVreme:F3}s | Krugova: {auto.BrojKrugova}");
                
                if (auto.SviKrugovi.Count >= 3)
                {
                    var poslednja3 = auto.SviKrugovi.Skip(Math.Max(0, auto.SviKrugovi.Count - 3)).ToList();
                    Console.WriteLine($"     DATA Poslednja 3: {string.Join(", ", poslednja3.Select(t => $"{t:F3}s"))}");
                }
                Console.WriteLine();
            }
            
            IspisiDodatneStatistike(najboljaVremena, globalnoNajboljeVreme);
            Console.WriteLine("======================================\n");
        }

        private List<AutomobilStatistika> GetNajboljaVremenaData()
        {
            return vremenaPokrugu
                .Where(kvp => kvp.Value.Count > 0)
                .Select(kvp => new AutomobilStatistika {
                    Automobil = kvp.Key,
                    NajboljeVreme = kvp.Value.Min(),
                    BrojKrugova = kvp.Value.Count,
                    PoslednjeVreme = kvp.Value.LastOrDefault(),
                    NaStazi = aktivniKlijenti.Any(k => k.GetId() == kvp.Key && k.NaStazi),
                    SviKrugovi = kvp.Value
                })
                .OrderBy(x => x.NajboljeVreme)
                .ToList();
        }

        private string GetPozicijuOznaku(int pozicija, double vreme, double najboljeVreme)
        {
            if (Math.Abs(vreme - najboljeVreme) < 0.001)
                return "FAST NAJBRZI! ";
            else if (pozicija == 0)
                return "1st ";
            else if (pozicija == 1)
                return "2nd ";
            else if (pozicija == 2)
                return "3rd ";
            else
                return "RACE "; // ISPRAVKA: umesto praznog stringa
        }

        private void IspisiDodatneStatistike(List<AutomobilStatistika> najboljaVremena, double globalnoNajboljeVreme)
        {
            if (najboljaVremena.Count > 0)
            {
                double prosecnoVreme = najboljaVremena.Average(x => x.NajboljeVreme);
                int ukupnoKrugova = najboljaVremena.Sum(x => x.BrojKrugova);
                int automobilaNaStazi = najboljaVremena.Count(x => x.NaStazi);
                
                Console.WriteLine(" DODATNE STATISTIKE:");
                Console.WriteLine($"    Automobila na stazi: {automobilaNaStazi}/{najboljaVremena.Count}");
                Console.WriteLine($"    Ukupno krugova: {ukupnoKrugova}");
                Console.WriteLine($"    Prosečno najbolje vreme: {prosecnoVreme:F3}s");
                Console.WriteLine($"    Globalno najbrži krug: {globalnoNajboljeVreme:F3}s");
            }
        }

        public void IspisiStatistike(int brojKlijenata)
        {
            Console.WriteLine("\n=== DATA STATISTIKE SERVERA ===");
            Console.WriteLine($"Aktivnih klijenata: {brojKlijenata}");
            Console.WriteLine($"Ukupno automobila: {vremenaPokrugu.Count}");
            Console.WriteLine($"Ukupno krugova: {vremenaPokrugu.Values.Sum(v => v.Count)}");

            var najbrziKrug = GetNajbrziKrug();
            if (najbrziKrug != null)
            {
                Console.WriteLine($"FAST Najbrzi krug: {najbrziKrug.Auto} - {najbrziKrug.Vreme:F3}s");
            }
            Console.WriteLine("===============================\n");
        }

        private NajbrziKrug GetNajbrziKrug()
        {
            return vremenaPokrugu.Where(kvp => kvp.Value.Count > 0)
                                 .SelectMany(kvp => kvp.Value.Select(v => new NajbrziKrug { Auto = kvp.Key, Vreme = v }))
                                 .OrderBy(x => x.Vreme)
                                 .FirstOrDefault();
        }

        public void IspisiSvaVremena()
        {
            Console.WriteLine("\n=== SVA ZABELEŽENA VREMENA ===");
            foreach (var kvp in vremenaPokrugu)
            {
                Console.WriteLine($"\n Automobil: {kvp.Key}");
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    Console.WriteLine($"  Krug {i + 1}: {kvp.Value[i]:F3}s");
                }
            }
            Console.WriteLine("===============================\n");
        }

        public void IspisiFinaleStatistike(string automobilId, int brojKrugova)
        {
            if (vremenaPokrugu.ContainsKey(automobilId) && vremenaPokrugu[automobilId].Count > 0)
            {
                var vremena = vremenaPokrugu[automobilId];
                Console.WriteLine($"    Najbolje vreme: {vremena.Min():F3}s");
                Console.WriteLine($"    Prosečno vreme: {vremena.Average():F3}s");
                Console.WriteLine($"    Ukupno krugova: {vremena.Count}");
            }
        }

        // DODAJ OVU METODU na kraj klase:
        public Dictionary<string, List<double>> GetVremenaPokrugu()
        {
            return vremenaPokrugu;
        }
    }

    // NOVE POMOĆNE KLASE umesto anonimnih tipova
    public class AutomobilStatistika
    {
        public string Automobil { get; set; }
        public double NajboljeVreme { get; set; }
        public int BrojKrugova { get; set; }
        public double PoslednjeVreme { get; set; }
        public bool NaStazi { get; set; }
        public List<double> SviKrugovi { get; set; }
    }

    public class NajbrziKrug
    {
        public string Auto { get; set; }
        public double Vreme { get; set; }
    }
}