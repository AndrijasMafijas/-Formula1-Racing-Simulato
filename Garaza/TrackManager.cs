using System;

namespace Garaza
{
    public class TrackManager
    {
        public double DuzinaStaze { get; private set; }
        public double OsnovnoVremeKruga { get; private set; }

        public void UnesPodataka()
        {
            Console.WriteLine("\n--- TRACK UNOS PODATAKA O STAZI ---");
            
            // Unos dužine staze
            while (true)
            {
                Console.Write("TRACK Unesite duzinu staze (u kilometrima): ");
                string input = Console.ReadLine();
                
                if (double.TryParse(input, out double duzinaStaze) && duzinaStaze > 0)
                {
                    DuzinaStaze = duzinaStaze;
                    break;
                }
                else
                {
                    Console.WriteLine("ERROR Molimo unesite validnu pozitivnu vrednost za duzinu staze!");
                }
            }
            
            // Unos osnovnog vremena kruga
            while (true)
            {
                Console.Write("TIME Unesite osnovno vreme kruga (u sekundama): ");
                string input = Console.ReadLine();
                
                if (double.TryParse(input, out double osnovnoVremeKruga) && osnovnoVremeKruga > 0)
                {
                    OsnovnoVremeKruga = osnovnoVremeKruga;
                    break;
                }
                else
                {
                    Console.WriteLine("ERROR Molimo unesite validnu pozitivnu vrednost za vreme kruga!");
                }
            }
        }

        public void IspisiPodatke()
        {
            Console.WriteLine("\n=== TRACK PODACI O STAZI ===");
            Console.WriteLine($"DISTANCE Duzina staze: {DuzinaStaze:F2} km");
            Console.WriteLine($"TIME Osnovno vreme kruga: {OsnovnoVremeKruga:F2} s");
            Console.WriteLine($"SPEED Prosecna brzina: {(DuzinaStaze * 3.6 / OsnovnoVremeKruga):F2} km/h");
            Console.WriteLine("==========================");
        }

        public TrackData GetTrackData()
        {
            return new TrackData
            {
                DuzinaStaze = DuzinaStaze,
                OsnovnoVremeKruga = OsnovnoVremeKruga
            };
        }
    }

    public class TrackData
    {
        public double DuzinaStaze { get; set; }
        public double OsnovnoVremeKruga { get; set; }
    }
}