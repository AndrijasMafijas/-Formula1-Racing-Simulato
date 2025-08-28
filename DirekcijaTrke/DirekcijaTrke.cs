using System;
using System.Threading;

namespace DirekcijaTrke
{
    public class DirekcijaTrke
    {
        private readonly SocketManager socketManager;
        private readonly MessageProcessor messageProcessor;
        private readonly ResultsManager resultsManager;
        private readonly DisplayManager displayManager;
        
        private Thread serverThread;
        private bool serverPokrenut;

        public DirekcijaTrke()
        {
            socketManager = new SocketManager();
            messageProcessor = new MessageProcessor();
            displayManager = new DisplayManager();
            resultsManager = new ResultsManager(socketManager.GetAktivniKlijenti());
            
            PoveziEventove();
        }

        private void PoveziEventove()
        {
            socketManager.NovaKonekcija += OnNovaKonekcija;
            socketManager.KlijentOtklonjen += OnKlijentOtklonjen; // ISPRAVKA: Otkljonjen → Otklonjen
            
            messageProcessor.AutomobilRegistrovan += OnAutomobilRegistrovan;
            messageProcessor.VremeKrugaPrimljeno += OnVremeKrugaPrimljeno;
            messageProcessor.AutomobilSeVratio += OnAutomobilSeVratio;
            messageProcessor.AlarmPrimljen += OnAlarmPrimljen;
            
            socketManager.PorukaPrimljena += messageProcessor.ObradiPoruku;
        }

        public void PokreniServer()
        {
            if (socketManager.PokreniServer())
            {
                serverPokrenut = true;
                Console.WriteLine("Server koristi Socket.Select za multipleksiranje konekcija");
                
                serverThread = new Thread(ServerLoop)
                {
                    IsBackground = false,
                    Name = "DirekcijaTrkeServerThread"
                };
                serverThread.Start();
            }
        }

        private void ServerLoop()
        {
            Console.WriteLine("NET Server loop pokrenut - koristi Socket.Select");
            
            while (serverPokrenut && socketManager.JePokrenut)
            {
                socketManager.ObradiKonekcije();
                Thread.Sleep(10);
            }
            
            Console.WriteLine("NET Server loop zavrsen");
        }

        public void ZaustaviServer()
        {
            Console.WriteLine("STOP Zaustavljam server...");
            serverPokrenut = false;
            socketManager.ZaustaviServer();
            serverThread?.Join(2000);
            Console.WriteLine("OK Server zaustavljen.");
        }

        // Event handlers
        private void OnNovaKonekcija(KlijentInfo klijent)
        {
            displayManager.IspisiAktivneKlijente(socketManager.GetAktivniKlijenti());
        }

        private void OnKlijentOtklonjen(KlijentInfo klijent) // ISPRAVKA: Otkljonjen → Otklonjen
        {
            if (!string.IsNullOrEmpty(klijent.TrkackiBroj))
            {
                Console.WriteLine($"RACE {klijent.GetId()} zavrsio simulaciju (ukupno krugova: {klijent.BrojPrimljenihKrugova})");
            }
            Console.WriteLine($"DATA Aktivnih klijenata: {socketManager.BrojKlijenata}");
        }

        private void OnAutomobilRegistrovan(KlijentInfo klijent)
        {
            resultsManager.RegistrujAutomobil(klijent.GetId());
        }

        private void OnVremeKrugaPrimljeno(KlijentInfo klijent, string automobilId, double vreme)
        {
            resultsManager.DodajVremeKruga(automobilId, vreme);
            
            if (klijent.BrojPrimljenihKrugova % 5 == 0 || resultsManager.JeNovoNajboljeVreme(automobilId, vreme))
            {
                resultsManager.IspisiNajboljaVremena();
            }
        }

        private void OnAutomobilSeVratio(KlijentInfo klijent, string automobilId, int brojKrugova)
        {
            resultsManager.IspisiFinaleStatistike(automobilId, brojKrugova);
            resultsManager.IspisiNajboljaVremena();
        }

        private void OnAlarmPrimljen(KlijentInfo klijent, string tipAlarma, double gume, double gorivo)
        {
            // Alarm je već ispisan u MessageProcessor
        }

        // Javne metode za UI
        public void IspisiStatistike()
        {
            resultsManager.IspisiStatistike(socketManager.BrojKlijenata);
            displayManager.IspisiAktivneKlijente(socketManager.GetAktivniKlijenti());
        }

        public void IspisiSvaVremena()
        {
            resultsManager.IspisiSvaVremena();
        }

        public void IspisiKonacneRezultate()
        {
            displayManager.IspisiKonacneRezultate(resultsManager.GetVremenaPokrugu(), socketManager.GetAktivniKlijenti());
        }
    }
}