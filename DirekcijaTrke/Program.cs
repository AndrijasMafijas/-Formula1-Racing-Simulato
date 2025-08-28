using System;
using System.Threading;

namespace DirekcijaTrke
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== DIREKCIJA TRKE - FORMULA 1 (Socket.Select Multipleksiranje) ===");
            
            DirekcijaTrke direkcijaServer = new DirekcijaTrke();
            
            // Pokretanje servera
            direkcijaServer.PokreniServer();
            
            // Kratko čekanje da se server pokrene
            Thread.Sleep(500);
            
            // Meni za upravljanje
            bool radi = true;
            while (radi)
            {
                Console.WriteLine("\nKOMANDE:");
                Console.WriteLine("1 - Ispisi statistike servera");
                Console.WriteLine("2 - Ispisi sva vremena");
                Console.WriteLine("3 - Ispisi konačne rezultate");
                Console.WriteLine("4 - Zaustavi server");
                Console.Write("Unesite komandu (1-4): ");
                
                string komanda = Console.ReadLine();
                
                switch (komanda)
                {
                    case "1":
                        direkcijaServer.IspisiStatistike();
                        break;
                    case "2":
                        direkcijaServer.IspisiSvaVremena();
                        break;
                    case "3":
                        direkcijaServer.IspisiKonacneRezultate();
                        break;
                    case "4":
                        direkcijaServer.ZaustaviServer();
                        radi = false;
                        break;
                    default:
                        Console.WriteLine("ERROR Nevalidna komanda!");
                        break;
                }
            }
            
            Console.WriteLine("Pritisnite bilo koji taster za izlaz...");
            Console.ReadKey();
        }
    }
}