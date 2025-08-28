using System;

namespace Garaza
{
    public class CarCommandManager
    {
        private readonly NetworkManager networkManager;
        private readonly TrackManager trackManager;

        public CarCommandManager(NetworkManager networkManager, TrackManager trackManager)
        {
            this.networkManager = networkManager;
            this.trackManager = trackManager;
        }

        public void PosaljiAutomobilNaStazu()
        {
            Console.WriteLine("\n--- CAR SLANJE AUTOMOBILA NA STAZU ---");
            
            // Izbor komponente guma
            string gume = IzaberiKomponentuGuma();
            
            // Unos količine goriva
            double kolicina = UnesKolicinuGoriva();
            
            // Kreiraj i pošalji poruku
            var trackData = trackManager.GetTrackData();
            string poruka = $"Izlazak na stazu: {gume},{kolicina},{trackData.DuzinaStaze},{trackData.OsnovnoVremeKruga}";
            
            networkManager.PosaljiUDPPoruku(poruka);
            Console.WriteLine("OK Automobil je pozvan na stazu!");
        }

        public void PosaljiTempoDirectivu()
        {
            Console.WriteLine("\n--- CONTROL UPRAVLJANJE TEMPOM VOZNJE ---");
            Console.WriteLine("1 - Brzi tempo (FAST +30% potrosnja, brza vremena)");
            Console.WriteLine("2 - Sporiji tempo (SLOW normalna potrosnja, +0.2s po krugu)");
            Console.WriteLine("3 - Srednji tempo (MID normalna potrosnja i vreme)");
            
            while (true)
            {
                Console.Write("Izaberite tempo (1-3): ");
                string izbor = Console.ReadLine();
                
                string tempoKomanda = GetTempoKomandu(izbor);
                if (tempoKomanda != null)
                {
                    networkManager.PosaljiUDPPoruku(tempoKomanda);
                    
                    string tempoOpis = izbor == "1" ? "FAST BRZI" : izbor == "2" ? "SLOW SPORIJI" : "MID SREDNJI";
                    Console.WriteLine($"OK Tempo direktiva poslata: {tempoOpis}");
                    break;
                }
                else
                {
                    Console.WriteLine("ERROR Nevalidan izbor! Molimo unesite broj od 1 do 3.");
                }
            }
        }

        public void PozovZaVracanje()
        {
            Console.WriteLine("\n--- HOME POZIV AUTOMOBILA ZA VRACANJE ---");
            
            Console.WriteLine("WARN Da li ste sigurni da zelite da pozovete automobil da se vrati?");
            Console.WriteLine("   Ovo ce prekinuti trenutnu simulaciju krugova.");
            Console.WriteLine("1 - DA, pozovi automobil");
            Console.WriteLine("2 - NE, otkazi");
            
            while (true)
            {
                Console.Write("Izbor (1-2): ");
                string izbor = Console.ReadLine();
                
                switch (izbor)
                {
                    case "1":
                        networkManager.PosaljiUDPPoruku("VRACANJE_U_GARAZU");
                        Console.WriteLine("OK Poziv za vracanje poslat!");
                        Console.WriteLine("   Automobil ce prekinuti simulaciju i vratiti se u garazu.");
                        return;
                        
                    case "2":
                        Console.WriteLine("CANCEL Poziv otkazan.");
                        return;
                        
                    default:
                        Console.WriteLine("ERROR Nevalidan izbor! Molimo unesite 1 ili 2.");
                        continue;
                }
            }
        }

        public void PosaljiTestnuPoruku()
        {
            var trackData = trackManager.GetTrackData();
            string testPoruka = $"test_garaza|Mercedes|{trackData.OsnovnoVremeKruga:F3}";
            networkManager.PosaljiTCPPoruku(testPoruka);
        }

        private string IzaberiKomponentuGuma()
        {
            Console.WriteLine("TIRE Dostupne komponente guma:");
            Console.WriteLine("M - Meke gume (80km)");
            Console.WriteLine("S - Srednje gume (100km)");  
            Console.WriteLine("T - Tvrde gume (120km)");
            
            while (true)
            {
                Console.Write("Izaberite komponentu guma (M/S/T): ");
                string gume = Console.ReadLine()?.ToUpper();
                
                if (gume == "M" || gume == "S" || gume == "T")
                {
                    return gume;
                }
                else
                {
                    Console.WriteLine("ERROR Molimo unesite M, S ili T!");
                }
            }
        }

        private double UnesKolicinuGoriva()
        {
            while (true)
            {
                Console.Write("FUEL Unesite kolicinu goriva (litara): ");
                string input = Console.ReadLine();
                
                if (double.TryParse(input, out double kolicina) && kolicina > 0)
                {
                    return kolicina;
                }
                else
                {
                    Console.WriteLine("ERROR Molimo unesite validnu pozitivnu vrednost!");
                }
            }
        }

        private string GetTempoKomandu(string izbor)
        {
            switch (izbor)
            {
                case "1": return "TEMPO:BRZE";
                case "2": return "TEMPO:SPORIJE";
                case "3": return "TEMPO:SREDNJO";
                default: return null;
            }
        }
    }
}