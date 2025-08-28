using System;
using System.Threading.Tasks;

namespace Automobil
{
    public class RaceSimulationManager
    {
        private readonly CarNetworkManager networkManager;
        private readonly CarConfigurationManager configManager;
        private readonly AlarmManager alarmManager;
        
        private int trkackiBroj;
        private int brojKruga;
        private bool naStazi;
        private TempoVoznje trenutniTempo;
        private double duzinaStaze;
        private double osnovnoVremeKruga;

        public event Action<TempoVoznje, TempoVoznje> TempoPromenaEvent;
        public event Action<int> NoviKrugEvent;
        public event Action SimulacijaZavrsenaEvent;

        public RaceSimulationManager(CarNetworkManager networkManager, CarConfigurationManager configManager, AlarmManager alarmManager)
        {
            this.networkManager = networkManager;
            this.configManager = configManager;
            this.alarmManager = alarmManager;
            
            this.trkackiBroj = new Random().Next(1, 100);
            this.brojKruga = 0;
            this.naStazi = false;
            this.trenutniTempo = TempoVoznje.Srednjo;
        }

        public async Task<bool> PokreniSimulaciju(double duzinaStaze, double osnovnoVremeKruga)
        {
            this.duzinaStaze = duzinaStaze;
            this.osnovnoVremeKruga = osnovnoVremeKruga;
            this.naStazi = true;
            this.brojKruga = 0;
            
            Console.WriteLine($"\nAUTOMOBIL IZLAZI NA STAZU!");
            Console.WriteLine($"TIRE Gume: {configManager.Gume.Oznaka}, FUEL Gorivo: {configManager.TrenutnoGorivo:F1}L");
            Console.WriteLine($"LOC Staza: {duzinaStaze:F1}km, TIME Osnovno vreme: {osnovnoVremeKruga:F1}s");
            
            // Registruj se kod direkcije
            if (!await networkManager.RegistrujSeDirekciji(trkackiBroj, configManager.Konfiguracija.Proizvodjac))
            {
                Console.WriteLine("ERROR: Ne mogu da se registrujem kod direkcije! Prekidam simulaciju.");
                return false;
            }
            
            Console.WriteLine("\nCAR SIMULACIJA KRUGOVA POCINJE!");
            
            // Pokreni simulaciju krugova
            Task.Run(() => SimulirajKrugove());
            return true;
        }

        public void PromeniTempo(TempoVoznje noviTempo)
        {
            if (trenutniTempo != noviTempo)
            {
                TempoVoznje stariTempo = trenutniTempo;
                trenutniTempo = noviTempo;
                
                TempoPromenaEvent?.Invoke(stariTempo, noviTempo);
                
                string tempoEmoji = noviTempo == TempoVoznje.Brze ? "FAST" : 
                                  noviTempo == TempoVoznje.Sporije ? "SLOW" : "MID";
                
                Console.WriteLine($"\n{tempoEmoji} PROMENA TEMPA VOZNJE!");
                Console.WriteLine($"   Stari tempo: {GetTempoOpis(stariTempo)}");
                Console.WriteLine($"   Novi tempo: {GetTempoOpis(noviTempo)}");
                
                // Objašnenje uticaja
                switch (noviTempo)
                {
                    case TempoVoznje.Brze:
                        Console.WriteLine($"   UP Potrosnja goriva i guma ce se uvecati za 30%");
                        Console.WriteLine($"   TIME Vremena krugova ce biti brza");
                        break;
                    case TempoVoznje.Sporije:
                        Console.WriteLine($"   DOWN Potrosnja ostaje normalna");
                        Console.WriteLine($"   TIME Vreme kruga ce se uvecavati za 0.2s po krugu");
                        break;
                    case TempoVoznje.Srednjo:
                        Console.WriteLine($"   BAL Normalna potrosnja i tempo");
                        break;
                }
            }
        }

        public async Task ZaustaviSimulaciju(string razlog)
        {
            if (naStazi)
            {
                Console.WriteLine($"\nSTOP Prekidam simulaciju - {razlog}");
                
                naStazi = false;
                
                // Obavesti direkciju
                await networkManager.ObvestiDirekcijuOIzlasku(trkackiBroj, configManager.Konfiguracija.Proizvodjac, brojKruga);
                
                // Pošalji finalne statistike garaži
                await networkManager.PosaljiFinalneStatistikeGarazi(trkackiBroj, configManager.Konfiguracija.Proizvodjac, 
                    brojKruga, configManager.TrenutnoGorivo, configManager.Gume.ProcentPotrosenosti());
                
                Console.WriteLine($"OK Simulacija zavrsena!");
                Console.WriteLine($"DATA Razlog: {razlog}");
                Console.WriteLine($"DATA Ukupno krugova: {brojKruga}");
                Console.WriteLine($"FUEL Preostalo gorivo: {configManager.TrenutnoGorivo:F1}L");
                Console.WriteLine($"TIRE Stanje guma: {configManager.Gume.ProcentPotrosenosti():F1}% potroseno");
                
                // Reset tempo na srednji
                trenutniTempo = TempoVoznje.Srednjo;
                
                SimulacijaZavrsenaEvent?.Invoke();
            }
        }

        private async void SimulirajKrugove()
        {
            while (naStazi && configManager.TrenutnoGorivo > 0 && !configManager.Gume.JeLiIstrosen())
            {
                brojKruga++;
                NoviKrugEvent?.Invoke(brojKruga);
                
                // Sačuvaj stanje pre kruga za poređenje
                double gorivoPreKruga = configManager.TrenutnoGorivo;
                double potrosenostGumaPreKruga = configManager.Gume.TrenutnaPotrosenost;
                
                // Izračunaj vreme kruga
                double vremeKruga = IzracunajVremeKruga();
                
                // Pošalji vreme direkciji trke
                await networkManager.PosaljiVremeDirekciji(trkackiBroj, configManager.Konfiguracija.Proizvodjac, vremeKruga);
                
                // Prikaži vreme na konzoli
                Console.WriteLine($"LAP Krug {brojKruga}: {vremeKruga:F3}s");
                
                // Smanji potrošnju
                PotrošiResurse();
                
                // Proveri kritične nivoe i aktiviraj alarm
                await alarmManager.ProveriKriticneNivoe(trkackiBroj, configManager.Konfiguracija.Proizvodjac, 
                    configManager.Gume.ProcentPotrosenosti(), configManager.TrenutnoGorivo, 
                    IzracunajPotrebnoGorivoZaDvaKruga());
                
                // Ako je alarm aktiviran, simulacija će biti prekinuta
                if (!naStazi)
                {
                    Console.WriteLine("\nALARM Simulacija prekinuta zbog alarma!");
                    break;
                }
                
                // Izračunaj koliko je potrošeno u ovom krugu
                double potrsenjaGorivaUKrugu = gorivoPreKruga - configManager.TrenutnoGorivo;
                double potrosnjaGumaUKrugu = configManager.Gume.TrenutnaPotrosenost - potrosenostGumaPreKruga;
                
                // Pošalji podatke o potrošnji garaži
                await networkManager.PosaljiTelemetrijuGarazi(trkackiBroj, configManager.Konfiguracija.Proizvodjac,
                    potrsenjaGorivaUKrugu, potrosnjaGumaUKrugu, vremeKruga, 
                    configManager.TrenutnoGorivo, configManager.Gume.ProcentPotrosenosti());
                
                // Prikaži trenutno stanje
                IspisiStanjeUToku();
                
                // Pauza (simulacija vremena kruga)
                await Task.Delay((int)(vremeKruga * 100)); // skaliran delay
                
                // Proveri da li treba da se vrati u garažu (stara logika)
                if (TrebaSeVratitiUGarazu())
                {
                    await ZaustaviSimulaciju("Kriticni nivoi resursa");
                    break;
                }
            }
            
            if (naStazi) // Ako simulacija nije već prekinuta alarmom
            {
                await ZaustaviSimulaciju("Prirodan zavrsetak");
            }
        }

        private double IzracunajVremeKruga()
        {
            // Osnovno vreme kruga
            double tempoGoriva = 1.0 / configManager.TrenutnoGorivo;
            double tempoGuma = IzracunajTempoGuma();
            
            double osnovnoVreme = osnovnoVremeKruga - tempoGoriva - tempoGuma;
            
            // Primeni modifikacije na osnovu tempa vožnje
            double modifikovanoVreme = osnovnoVreme;
            
            switch (trenutniTempo)
            {
                case TempoVoznje.Brze:
                    modifikovanoVreme *= 0.85; // 15% brže
                    break;
                    
                case TempoVoznje.Sporije:
                    modifikovanoVreme += (0.2 * brojKruga);
                    break;
                    
                case TempoVoznje.Srednjo:
                    // Ostaje isto
                    break;
            }
            
            // Osiguraj da vreme bude pozitivno
            return Math.Max(modifikovanoVreme, osnovnoVremeKruga * 0.3);
        }

        private double IzracunajTempoGuma()
        {
            double tempoGuma;
            
            switch (configManager.Gume.Tip)
            {
                case KomponentaGuma.TipGuma.Meke:
                    tempoGuma = 1.2 * brojKruga;
                    break;
                case KomponentaGuma.TipGuma.Srednje:
                    tempoGuma = brojKruga;
                    break;
                case KomponentaGuma.TipGuma.Tvrde:
                    tempoGuma = 0.8 * brojKruga;
                    break;
                default:
                    tempoGuma = brojKruga;
                    break;
            }
            
            // Poseban slučaj: ako su gume potrošene ispod 35%, tempo se umanjuje za 0.6
            if (configManager.Gume.ProcentPotrosenosti() < 35)
            {
                tempoGuma -= 0.6;
            }
            
            return tempoGuma;
        }

        private void PotrošiResurse()
        {
            // Osnovne potrošnje
            double osnovnaPotrošnjaGuma = duzinaStaze * configManager.Konfiguracija.PotrosnjaGuma;
            double osnovnaPotrošnjaGoriva = duzinaStaze * configManager.Konfiguracija.PotrosnjaGoriva;
            
            // Primeni modifikacije na osnovu tempa
            double finalnaPotrosnjaGuma = osnovnaPotrošnjaGuma;
            double finalnaPotrosnjaGoriva = osnovnaPotrošnjaGoriva;
            
            switch (trenutniTempo)
            {
                case TempoVoznje.Brze:
                    // Brža vožnja - uvećana potrošnja za 30%
                    finalnaPotrosnjaGuma += 0.3 * osnovnaPotrošnjaGuma;
                    finalnaPotrosnjaGoriva += 0.3 * osnovnaPotrošnjaGoriva;
                    break;
                    
                case TempoVoznje.Sporije:
                case TempoVoznje.Srednjo:
                    // Osnovna potrošnja (bez promena)
                    break;
            }
            
            // Primeni potrošnju
            configManager.Gume.Potrosi(finalnaPotrosnjaGuma);
            configManager.SmaniGorivo(finalnaPotrosnjaGoriva);
        }

        private double IzracunajPotrebnoGorivoZaDvaKruga()
        {
            // Osnovne potrošnje
            double osnovnaPotrošnjaGoriva = duzinaStaze * configManager.Konfiguracija.PotrosnjaGoriva;
            
            // Primeni modifikacije na osnovu tempa
            double finalnaPotrosnjaGoriva = osnovnaPotrošnjaGoriva;
            
            switch (trenutniTempo)
            {
                case TempoVoznje.Brze:
                    finalnaPotrosnjaGoriva += 0.3 * osnovnaPotrošnjaGoriva;
                    break;
            }
            
            // Za dva kruga
            return finalnaPotrosnjaGoriva * 2;
        }

        private void IspisiStanjeUToku()
        {
            string tempoEmoji = trenutniTempo == TempoVoznje.Brze ? "FAST" : 
                              trenutniTempo == TempoVoznje.Sporije ? "SLOW" : "MID";
            
            Console.WriteLine($"   └─ {configManager.Gume} | Gorivo: {configManager.TrenutnoGorivo:F1}L | {tempoEmoji} {GetTempoOpis(trenutniTempo)}");
            
            if (configManager.TrenutnoGorivo < 10)
            {
                Console.WriteLine("   WARN UPOZORENJE: Malo goriva!");
            }
            
            if (configManager.Gume.ProcentPotrosenosti() > 90)
            {
                Console.WriteLine("   WARN UPOZORENJE: Gume su skoro istrosene!");
            }
            
            // Dodatna upozorenja na osnovu tempa
            if (trenutniTempo == TempoVoznje.Brze && (configManager.TrenutnoGorivo < 20 || configManager.Gume.ProcentPotrosenosti() > 70))
            {
                Console.WriteLine("   ALARM KRITICNO: Brzi tempo trosi previse resursa!");
            }
        }

        private bool TrebaSeVratitiUGarazu()
        {
            return configManager.TrenutnoGorivo <= 0 || configManager.Gume.JeLiIstrosen();
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

        // Getteri za pristup podacima
        public int TrkackiBroj => trkackiBroj;
        public int BrojKruga => brojKruga;
        public bool NaStazi => naStazi;
        public TempoVoznje TrenutniTempo => trenutniTempo;
        public double DuzinaStaze => duzinaStaze;
        public double OsnovnoVremeKruga => osnovnoVremeKruga;
    }
}