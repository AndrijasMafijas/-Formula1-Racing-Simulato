using System;
using System.Threading.Tasks;

namespace Automobil
{
    public class MessageProcessor
    {
        private readonly CarConfigurationManager configManager;
        private readonly RaceSimulationManager simulationManager;

        public event Action<double, double> IzlazakNaStazuEvent;
        public event Action<TempoVoznje> TempoPromenaEvent;
        public event Action PozivZaVracanjeEvent;

        public MessageProcessor(CarConfigurationManager configManager, RaceSimulationManager simulationManager)
        {
            this.configManager = configManager;
            this.simulationManager = simulationManager;
        }

        public void ObradiUDPPoruku(string poruka)
        {
            try
            {
                if (poruka.StartsWith("Izlazak na stazu:"))
                {
                    ObradiKomandIzlaskaNaStazu(poruka);
                }
                else if (poruka.StartsWith("TEMPO:"))
                {
                    ObradiDirektivuTempo(poruka);
                }
                else if (poruka.StartsWith("VRACANJE_U_GARAZU"))
                {
                    ObradiPozivZaVracanje(poruka);
                }
                else
                {
                    Console.WriteLine($"WARN Nepoznata UDP poruka: {poruka}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri obradi UDP poruke: {ex.Message}");
            }
        }

        private void ObradiKomandIzlaskaNaStazu(string poruka)
        {
            try
            {
                // Format: "Izlazak na stazu: M,50,5.5,90.5"
                string[] delovi = poruka.Replace("Izlazak na stazu: ", "").Split(',');
                
                if (delovi.Length == 4)
                {
                    string oznakaGuma = delovi[0];
                    double gorivo = double.Parse(delovi[1]);
                    double duzinaStaze = double.Parse(delovi[2]);
                    double osnovnoVremeKruga = double.Parse(delovi[3]);
                    
                    // Ažuriraj konfiguraciju sa podacima sa staze
                    configManager.AzurirajKonfiguracijuSaStaze(oznakaGuma, gorivo);
                    
                    Console.WriteLine($"LOC Staza: {duzinaStaze:F1}km, TIME Osnovno vreme: {osnovnoVremeKruga:F1}s");
                    
                    // Aktiviraj event za pokretanje simulacije
                    IzlazakNaStazuEvent?.Invoke(duzinaStaze, osnovnoVremeKruga);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri obradi komande za izlazak: {ex.Message}");
            }
        }

        private void ObradiDirektivuTempo(string poruka)
        {
            try
            {
                // Format: "TEMPO:BRZE" ili "TEMPO:SPORIJE" ili "TEMPO:SREDNJO"
                string tempoStr = poruka.Replace("TEMPO:", "").Trim().ToUpper();
                
                TempoVoznje noviTempo;
                switch (tempoStr)
                {
                    case "BRZE":
                        noviTempo = TempoVoznje.Brze;
                        break;
                    case "SPORIJE":
                        noviTempo = TempoVoznje.Sporije;
                        break;
                    case "SREDNJO":
                        noviTempo = TempoVoznje.Srednjo;
                        break;
                    default:
                        Console.WriteLine($"WARN Nepoznat tempo: {tempoStr}");
                        return;
                }
                
                // Aktiviraj event za promenu tempa
                TempoPromenaEvent?.Invoke(noviTempo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri obradi tempo direktive: {ex.Message}");
            }
        }

        private void ObradiPozivZaVracanje(string poruka)
        {
            try
            {
                Console.WriteLine($"\nHOME POZIV ZA VRACANJE U GARAZU PRIMLJEN!");
                
                // Aktiviraj event za vraćanje
                PozivZaVracanjeEvent?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri obradi poziva za vracanje: {ex.Message}");
            }
        }
    }
}