using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Automobil
{
    public enum TempoVoznje
    {
        Brze,    // Povećana potrošnja (+0.3), brže vreme
        Sporije, // Normalna potrošnja, sporije vreme (+0.2 po krugu)
        Srednjo  // Normalna potrošnja i vreme (default)
    }

    public class Automobil
    {
        private readonly CarNetworkManager networkManager;
        private readonly CarConfigurationManager configManager;
        private readonly RaceSimulationManager simulationManager;
        private readonly AlarmManager alarmManager;
        private readonly MessageProcessor messageProcessor;

        public Automobil()
        {
            // Kreiranje komponenti
            networkManager = new CarNetworkManager();
            configManager = new CarConfigurationManager();
            alarmManager = new AlarmManager(networkManager);
            simulationManager = new RaceSimulationManager(networkManager, configManager, alarmManager);
            messageProcessor = new MessageProcessor(configManager, simulationManager);
            
            // Povezivanje event-ova
            ConnectEvents();
        }

        private void ConnectEvents()
        {
            // Network events
            networkManager.UDPPorukaRostota += messageProcessor.ObradiUDPPoruku;
            
            // Message processor events
            messageProcessor.IzlazakNaStazuEvent += OnIzlazakNaStazu;
            messageProcessor.TempoPromenaEvent += OnTempoPromenjen;
            messageProcessor.PozivZaVracanjeEvent += OnPozivZaVracanje;
            
            // Simulation events
            simulationManager.TempoPromenaEvent += OnSimulationTempoPromenjen;
            simulationManager.NoviKrugEvent += OnNoviKrug;
            simulationManager.SimulacijaZavrsenaEvent += OnSimulacijaZavrsena;
            
            // Alarm events
            alarmManager.AlarmAktiviran += OnAlarmAktiviran;
        }

        public void PokreniAutomobil()
        {
            Console.WriteLine("=== CAR FORMULA 1 AUTOMOBIL ===");
            
            // Izbor konfiguracije
            configManager.IzaberiKonfiguraciju();
            
            // Uspostavi mrežne konekcije
            if (networkManager.UspostaviUDPKonekciju())
            {
                Console.WriteLine("OK Konekcije uspostavljene");
                IspisiDetaljeKonekcije();
                
                // Pokreni UDP listener
                Task.Run(() => networkManager.PokreniUDPListener());
                
                // Prikaži status automobila
                IspisiStatusAutomobila();
            }
            else
            {
                Console.WriteLine("ERROR Neuspesno uspostavljanje konekcija!");
            }
        }

        // Event handlers
        private async void OnIzlazakNaStazu(double duzinaStaze, double osnovnoVremeKruga)
        {
            await simulationManager.PokreniSimulaciju(duzinaStaze, osnovnoVremeKruga);
        }

        private void OnTempoPromenjen(TempoVoznje noviTempo)
        {
            simulationManager.PromeniTempo(noviTempo);
        }

        private async void OnPozivZaVracanje()
        {
            if (simulationManager.NaStazi)
            {
                Console.WriteLine($"   LOC Trenutno na stazi - prekidam simulaciju...");
                await simulationManager.ZaustaviSimulaciju("Poziv garaze za vracanje");
                
                Console.WriteLine($"   OK Automobil #{simulationManager.TrkackiBroj} se vratio u garazu!");
                Console.WriteLine($"   DATA Finalni rezultat: {simulationManager.BrojKruga} krugova");
                Console.WriteLine($"   FUEL Preostalo gorivo: {configManager.TrenutnoGorivo:F1}L");
                Console.WriteLine($"   TIRE Stanje guma: {configManager.Gume.ProcentPotrosenosti():F1}% potroseno");
            }
            else
            {
                Console.WriteLine($"   WARN Automobil je vec u garazi - nema potrebe za vracanjem");
            }
        }

        private void OnSimulationTempoPromenjen(TempoVoznje stariTempo, TempoVoznje noviTempo)
        {
            // Dodatna logika ako je potrebna
        }

        private void OnNoviKrug(int brojKruga)
        {
            // Dodatna logika za novi krug
        }

        private void OnSimulacijaZavrsena()
        {
            Console.WriteLine("RACE Simulacija je zavrsena.");
        }

        private async void OnAlarmAktiviran(string tipAlarma, double procenatGuma, double trenutnoGorivo)
        {
            // Automatski zaustavi simulaciju zbog alarma
            await simulationManager.ZaustaviSimulaciju($"ALARM - {tipAlarma}");
        }

        // Helper metode
        private void IspisiDetaljeKonekcije()
        {
            Console.WriteLine("\n=== CONN DETALJI KONEKCIJE ===");
            Console.WriteLine($"NET UDP slusa na portu: 9092");
            Console.WriteLine("=============================");
        }

        private void IspisiStatusAutomobila()
        {
            Console.WriteLine("\n=== CAR STATUS AUTOMOBILA ===");
            Console.WriteLine($"RACE Trkacki broj: #{simulationManager.TrkackiBroj}");
            configManager.IspisiKonfiguraciju();
            Console.WriteLine($"RACE Tempo voznje: {GetTempoOpis(simulationManager.TrenutniTempo)}");
            Console.WriteLine($"LOC Na stazi: {(simulationManager.NaStazi ? "OK DA" : "ERROR NE")}");
            Console.WriteLine("============================");
        }

        private string GetTempoOpis(TempoVoznje tempo)
        {
            switch (tempo)
            {
                case TempoVoznje.Brze:
                    return "FAST BRZI TEMPO";
                case TempoVoznje.Sporije:
                    return "SLOW SPORI TEMPO";
                case TempoVoznje.Srednjo:
                    return "MID SREDNJI TEMPO";
                default:
                    return "UNKNOWN NEPOZNAT"; // ISPRAVKA: ? → UNKNOWN
            }
        }

        // Public metode za pristup podacima (delegiranje)
        public void ZatvoriKonekcije()
        {
            networkManager.ZatvoriKonekcije();
        }

        public bool JeKonekcijuspostavljena()
        {
            return networkManager.JeKonekcijuspostavljena();
        }

        public KonfiguracijaAutomobila GetKonfiguracija()
        {
            return configManager.Konfiguracija;
        }

        public KomponentaGuma GetGume()
        {
            return configManager.Gume;
        }

        public double GetTrenutnoGorivo()
        {
            return configManager.TrenutnoGorivo;
        }

        public bool JeNaStazi()
        {
            return simulationManager.NaStazi;
        }

        public int GetTrkackiBroj()
        {
            return simulationManager.TrkackiBroj;
        }

        public int GetBrojKruga()
        {
            return simulationManager.BrojKruga;
        }

        public double GetDuzinaStaze()
        {
            return simulationManager.DuzinaStaze;
        }

        public double GetOsnovnoVremeKruga()
        {
            return simulationManager.OsnovnoVremeKruga;
        }

        public TempoVoznje GetTrenutniTempo()
        {
            return simulationManager.TrenutniTempo;
        }

        public DateTime GetVremePokretanja()
        {
            return DateTime.Now; // placeholder
        }
        /*
        public void IspisiStatistikeSesije(List<AutomobilRezultat> sviAutomobili)
        {
            int ukupnoKrugova = sviAutomobili.Sum(x => x.BrojKrugova);
            int automobilaNaStazi = sviAutomobili.Count(x => x.NaStazi);
            double prosecnoVremeSvihKrugova = sviAutomobili.SelectMany(x => x.SviKrugovi).Average();

            Console.WriteLine("DATA STATISTIKE SESIJE:");
            Console.WriteLine($"   CAR Ucesnika: {sviAutomobili.Count} (aktivnih: {automobilaNaStazi})");
            Console.WriteLine($"   LAPS Ukupno krugova: {ukupnoKrugova}");
            Console.WriteLine($"   TIME Prosek svih krugova: {prosecnoVremeSvihKrugova:F3}s");
        }*/
    }
}
