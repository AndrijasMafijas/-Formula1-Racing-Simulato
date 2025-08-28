using System;

namespace Garaza
{
    class Program
    {
        static void Main(string[] args)
        {
            Garaza garaza = new Garaza();
            
            // Pokretanje garaže
            garaza.PokreniGarazu();
            
            // Meni za upravljanje
            if (garaza.JeKonekcijuspostavljena())
            {
                bool radi = true;
                while (radi)
                {
                    Console.WriteLine("\nCAR KOMANDE GARAZE:");
                    Console.WriteLine("1 - Posalji automobil na stazu");
                    Console.WriteLine("2 - Upravljaj tempom voznje");
                    Console.WriteLine("3 - Pozovi automobil za vracanje");
                    Console.WriteLine("4 - Posalji test poruku Direkciji");
                    Console.WriteLine("5 - Ispisi podatke o stazi");
                    Console.WriteLine("6 - Zatvori konekciju i izadji");
                    Console.Write("Unesite komandu (1-6): ");
                    
                    string komanda = Console.ReadLine();
                    
                    switch (komanda)
                    {
                        case "1":
                            garaza.PosaljiAutomobilNaStazu();
                            break;
                        case "2":
                            garaza.PosaljiTempoDirectivu();
                            break;
                        case "3":
                            garaza.PozovZaVracanje();
                            break;
                        case "4":
                            garaza.PosaljiTestnuPoruku();
                            break;
                        case "5":
                            Console.WriteLine($"Duzina staze: {garaza.GetDuzinaStaze():F2} km");
                            Console.WriteLine($"Osnovno vreme kruga: {garaza.GetOsnovnoVremeKruga():F2} s");
                            break;
                        case "6":
                            garaza.ZatvoriKonekciju();
                            radi = false;
                            break;
                        default:
                            Console.WriteLine("ERROR Nevalidna komanda!");
                            break;
                    }
                }
            }
            
            Console.WriteLine("Pritisnite bilo koji taster za izlaz...");
            Console.ReadKey();
        }
    }
}