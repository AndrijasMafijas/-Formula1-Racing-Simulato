using System;
using System.Threading.Tasks;

namespace Garaza
{
    public class Garaza
    {
        private readonly NetworkManager networkManager;
        private readonly TrackManager trackManager;
        private readonly CarCommandManager carCommandManager;
        private readonly TelemetryProcessor telemetryProcessor;

        public Garaza()
        {
            networkManager = new NetworkManager();
            trackManager = new TrackManager();
            carCommandManager = new CarCommandManager(networkManager, trackManager);
            telemetryProcessor = new TelemetryProcessor();
            
            // Povezivanje event-ova
            networkManager.TelemetrijaPorukaRostota += telemetryProcessor.ObradiTelemetrijskuPoruku;
        }

        public void PokreniGarazu()
        {
            Console.WriteLine("=== RACE GARAZA - FORMULA 1 TIM ===");
            
            // Uspostavi TCP konekciju
            if (networkManager.UspostaviTCPKonekciju())
            {
                // Pokreni UDP server
                if (networkManager.PokreniUDPServer())
                {
                    // Pokreni UDP listener u background thread-u
                    Task.Run(() => networkManager.PokreniUDPListener());
                    
                    // Unos potrebnih podataka o stazi
                    trackManager.UnesPodataka();
                    
                    // Prikaži podatke o stazi
                    trackManager.IspisiPodatke();
                    
                    Console.WriteLine("\nOK Garaza je spremna za rad!");
                }
                else
                {
                    Console.WriteLine("ERROR Neuspesno pokretanje UDP servera!");
                }
            }
            else
            {
                Console.WriteLine("ERROR Neuspesno uspostavljanje TCP konekcije!");
            }
        }

        // Delegiranje metoda komandama
        public void PosaljiAutomobilNaStazu()
        {
            carCommandManager.PosaljiAutomobilNaStazu();
        }

        public void PosaljiTempoDirectivu()
        {
            carCommandManager.PosaljiTempoDirectivu();
        }

        public void PozovZaVracanje()
        {
            carCommandManager.PozovZaVracanje();
        }

        public void PosaljiTestnuPoruku()
        {
            carCommandManager.PosaljiTestnuPoruku();
        }

        // Delegiranje gettera
        public double GetDuzinaStaze()
        {
            return trackManager.DuzinaStaze;
        }

        public double GetOsnovnoVremeKruga()
        {
            return trackManager.OsnovnoVremeKruga;
        }

        public void ZatvoriKonekciju()
        {
            networkManager.ZatvoriKonekcije();
        }

        public bool JeKonekcijuspostavljena()
        {
            return networkManager.JeKonekcijuspostavljena();
        }
    }
}
