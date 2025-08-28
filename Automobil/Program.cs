using System;
using System.Threading;

namespace Automobil
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== CAR FORMULA 1 AUTOMOBIL - SIMULACIJA ===");
            
            Automobil auto = new Automobil();
            
            // Pokretanje automobila
            auto.PokreniAutomobil();
            
            // Meni za upravljanje
            if (auto.JeKonekcijuspostavljena())
            {
                Console.WriteLine("\nOK Automobil je spreman!");
                Console.WriteLine("INFO Ceka UDP poruku od Garaze da izadje na stazu...");
                
                bool radi = true;
                while (radi)
                {
                    Console.WriteLine("\nCAR KOMANDE:");
                    Console.WriteLine("1 - Prikazi status automobila");
                    Console.WriteLine("2 - Prikazi detaljne informacije");
                    Console.WriteLine("3 - Zatvori konekcije i izadji");
                    Console.Write("Unesite komandu (1-3): ");
                    
                    string komanda = Console.ReadLine();
                    
                    switch (komanda)
                    {
                        case "1":
                            PrikaziStatusAutomobila(auto);
                            break;
                        case "2":
                            PrikaziDetaljneInformacije(auto);
                            break;
                        case "3":
                            auto.ZatvoriKonekcije();
                            radi = false;
                            break;
                        default:
                            Console.WriteLine("ERROR Nevalidna komanda!");
                            break;
                    }
                    
                    // Kratka pauza da se vide poruke
                    Thread.Sleep(50);
                }
            }
            else
            {
                Console.WriteLine("ERROR Neuspesno pokretanje automobila!");
            }
            
            Console.WriteLine("\nPritisnite bilo koji taster za izlaz...");
            Console.ReadKey();
        }

        private static void PrikaziStatusAutomobila(Automobil auto)
        {
            Console.WriteLine("\n=== CAR BRZI PREGLED ===");
            Console.WriteLine($"BRAND {auto.GetKonfiguracija().Proizvodjac}");
            Console.WriteLine($"TIRE {auto.GetGume().Oznaka} gume ({auto.GetGume().ProcentPotrosenosti():F1}% potroseno)");
            Console.WriteLine($"FUEL {auto.GetTrenutnoGorivo():F1} litara goriva");
            
            // Tempo prikaz
            string tempoEmoji = auto.GetTrenutniTempo() == TempoVoznje.Brze ? "FAST" : 
                               auto.GetTrenutniTempo() == TempoVoznje.Sporije ? "SLOW" : "MID";
            Console.WriteLine($"Tempo: {tempoEmoji} {auto.GetTrenutniTempo()}");
            
            // Status sa dodatnim informacijama
            if (auto.JeNaStazi())
            {
                Console.WriteLine($"Status: CAR Na stazi (krug {auto.GetBrojKruga()})");
            }
            else
            {
                Console.WriteLine($"Status: HOME U garazi");
                if (auto.GetBrojKruga() > 0)
                {
                    Console.WriteLine($"   DATA Poslednja sesija: {auto.GetBrojKruga()} krugova");
                }
            }
            Console.WriteLine("=======================");
        }

        private static void PrikaziDetaljneInformacije(Automobil auto)
        {
            Console.WriteLine("\n=== DATA DETALJNE INFORMACIJE ===");
            
            var konfiguracija = auto.GetKonfiguracija();
            var gume = auto.GetGume();
            
            Console.WriteLine($"BRAND Proizvodjac: {konfiguracija.Proizvodjac}");
            Console.WriteLine($"DATA Performanse:");
            Console.WriteLine($"   FUEL Potrosnja goriva: {konfiguracija.PotrosnjaGoriva} l/km");
            Console.WriteLine($"   TIRE Potrosnja guma: {konfiguracija.PotrosnjaGuma}");
            
            Console.WriteLine($"\nTIRE Gume:");
            Console.WriteLine($"   Tip: {gume.Tip} ({gume.Oznaka})");
            Console.WriteLine($"   Maksimalno: {gume.MaksimalnaDuzinaUkm} km");
            Console.WriteLine($"   Potroseno: {gume.TrenutnaPotrosenost:F1} km ({gume.ProcentPotrosenosti():F1}%)");
            Console.WriteLine($"   Preostalo: {gume.PreostalaKilometraza():F1} km");
            
            Console.WriteLine($"\nFUEL Gorivo: {auto.GetTrenutnoGorivo():F1} L");
            
            // Tempo informacije
            string tempoEmoji = auto.GetTrenutniTempo() == TempoVoznje.Brze ? "FAST" : 
                               auto.GetTrenutniTempo() == TempoVoznje.Sporije ? "SLOW" : "MID";
            Console.WriteLine($"\nTempo voznje: {tempoEmoji} {auto.GetTrenutniTempo()}");
            
            switch (auto.GetTrenutniTempo())
            {
                case TempoVoznje.Brze:
                    Console.WriteLine($"   UP Efekt: +30% potrosnja, brza vremena krugova");
                    break;
                case TempoVoznje.Sporije:
                    Console.WriteLine($"   DOWN Efekt: Normalna potrosnja, +0.2s po krugu");
                    break;
                case TempoVoznje.Srednjo:
                    Console.WriteLine($"   BAL Efekt: Standardna potrosnja i tempo");
                    break;
            }

            Console.WriteLine($"\nStatus: {(auto.JeNaStazi() ? "CAR AKTIVNO NA STAZI" : "HOME U GARAZI")}");
            
            if (auto.JeNaStazi())
            {
                Console.WriteLine("\nSPEED Automobil trenutno vozi krugove automatski!");
                Console.WriteLine("   NET Vremena se salju Direkciji Trke u realnom vremenu");
                Console.WriteLine("   RADIO Prima tempo direktive od Garaze putem UDP");
                
                // Tempo-specijalna upozorenja
                if (auto.GetTrenutniTempo() == TempoVoznje.Brze)
                {
                    Console.WriteLine("   FAST BRZI TEMPO - Povecana potrosnja!");
                    if (auto.GetTrenutnoGorivo() < 20)
                        Console.WriteLine("   ALARM KRITICNO: Brzi tempo + malo goriva!");
                    if (gume.ProcentPotrosenosti() > 70)
                        Console.WriteLine("   ALARM KRITICNO: Brzi tempo + istrosene gume!");
                }
                
                // Standardna upozorenja
                if (auto.GetTrenutnoGorivo() < 15)
                {
                    Console.WriteLine("   WARN UPOZORENJE: Gorivo se smanjuje!");
                }
                
                if (gume.ProcentPotrosenosti() > 75)
                {
                    Console.WriteLine("   WARN UPOZORENJE: Gume se trose!");
                }
                
                if (auto.GetTrenutnoGorivo() < 5)
                {
                    Console.WriteLine("   ALARM KRITICNO: Vrlo malo goriva!");
                }
                
                if (gume.ProcentPotrosenosti() > 95)
                {
                    Console.WriteLine("   ALARM KRITICNO: Gume su skoro gotove!");
                }
            }
            else
            {
                Console.WriteLine("\nINFO Automobil ceka instrukcije od Garaze");
                Console.WriteLine("   RADIO Slusam UDP port za komande (Izlazak na stazu, Tempo direktive)");
            }
            
            Console.WriteLine("===============================");
        }
    }
}