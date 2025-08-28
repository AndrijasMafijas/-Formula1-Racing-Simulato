using System;
using System.Collections.Generic;

namespace DirekcijaTrke
{
    public class MessageProcessor
    {
        public event Action<KlijentInfo> AutomobilRegistrovan;
        public event Action<KlijentInfo, string, double> VremeKrugaPrimljeno;
        public event Action<KlijentInfo, string, int> AutomobilSeVratio;
        public event Action<KlijentInfo, string, double, double> AlarmPrimljen;

        public void ObradiPoruku(string poruka, KlijentInfo klijent)
        {
            if (string.IsNullOrWhiteSpace(poruka))
                return;

            try
            {
                string[] delovi = poruka.Split('|');
                
                switch (delovi[0])
                {
                    case "ALARM" when delovi.Length == 6:
                        ObradiAlarm(delovi, klijent);
                        break;
                        
                    case "REGISTRACIJA" when delovi.Length == 3:
                        ObradiRegistraciju(delovi, klijent);
                        break;
                        
                    case "IZLAZAK_SA_STAZE" when delovi.Length == 4:
                        ObradiIzlazakSaStaze(delovi, klijent);
                        break;
                        
                    default:
                        if (delovi.Length == 3 && double.TryParse(delovi[2], out _))
                        {
                            ObradiVremeKruga(delovi, klijent);
                        }
                        else
                        {
                            Console.WriteLine($"WARN Nepoznat format poruke: {poruka}");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri obradi poruke '{poruka}': {ex.Message}");
            }
        }

        private void ObradiAlarm(string[] delovi, KlijentInfo klijent)
        {
            string trkackiBroj = delovi[1];
            string proizvodjac = delovi[2];
            string tipAlarma = delovi[3];
            double vrednostGuma = double.Parse(delovi[4]);
            double vrednostGorivo = double.Parse(delovi[5]);
            
            AzurirajKlijenta(klijent, trkackiBroj, proizvodjac);
            klijent.NaStazi = false;
            
            Console.WriteLine($"\nALARM ALARM ALARM SA STAZE! ALARM ALARM ALARM");
            Console.WriteLine($"   CAR Automobil: {trkackiBroj}_{proizvodjac}");
            Console.WriteLine($"   WARN Tip alarma: {tipAlarma}");
            
            switch (tipAlarma)
            {
                case "GUME":
                    Console.WriteLine($"   TIRE KRITICNE GUME: {vrednostGuma:F1}% potroseno (< 25% preostalo)");
                    break;
                case "GORIVO":
                    Console.WriteLine($"   FUEL KRITICNO GORIVO: {vrednostGorivo:F1}L (nedovoljno za 2 kruga)");
                    break;
                case "GUME_I_GORIVO":
                    Console.WriteLine($"   TIRE KRITICNE GUME: {vrednostGuma:F1}% potroseno");
                    Console.WriteLine($"   FUEL KRITICNO GORIVO: {vrednostGorivo:F1}L");
                    break;
            }
            
            Console.WriteLine($"   RETURN Automobil se automatski vraca u garazu...");
            Console.WriteLine($"ALARM ALARM ALARM ALARM ALARM ALARM ALARM ALARM ALARM ALARM");
            
            AlarmPrimljen?.Invoke(klijent, tipAlarma, vrednostGuma, vrednostGorivo);
        }

        private void ObradiRegistraciju(string[] delovi, KlijentInfo klijent)
        {
            string trkackiBroj = delovi[1];
            string proizvodjac = delovi[2];
            
            AzurirajKlijenta(klijent, trkackiBroj, proizvodjac);
            klijent.NaStazi = true;
            
            Console.WriteLine($"CAR Registrovan automobil: {klijent.GetId()}");
            AutomobilRegistrovan?.Invoke(klijent);
        }

        private void ObradiIzlazakSaStaze(string[] delovi, KlijentInfo klijent)
        {
            string trkackiBroj = delovi[1];
            string proizvodjac = delovi[2];
            int brojKrugova = int.Parse(delovi[3]);
            
            AzurirajKlijenta(klijent, trkackiBroj, proizvodjac);
            klijent.NaStazi = false;
            
            Console.WriteLine($"HOME {klijent.GetId()} se vratio u garazu nakon {brojKrugova} krugova");
            AutomobilSeVratio?.Invoke(klijent, klijent.GetId(), brojKrugova);
        }

        private void ObradiVremeKruga(string[] delovi, KlijentInfo klijent)
        {
            string trkackiBroj = delovi[0];
            string proizvodjac = delovi[1];
            double vremeKruga = double.Parse(delovi[2]);
            
            AzurirajKlijenta(klijent, trkackiBroj, proizvodjac);
            klijent.NaStazi = true;
            klijent.BrojPrimljenihKrugova++;
            
            Console.WriteLine($"TIME {klijent.GetId()} -> {vremeKruga:F3}s (krug #{klijent.BrojPrimljenihKrugova})");
            VremeKrugaPrimljeno?.Invoke(klijent, klijent.GetId(), vremeKruga);
        }

        private void AzurirajKlijenta(KlijentInfo klijent, string trkackiBroj, string proizvodjac)
        {
            if (string.IsNullOrEmpty(klijent.TrkackiBroj))
            {
                klijent.TrkackiBroj = trkackiBroj;
                klijent.Proizvodjac = proizvodjac;
            }
        }
    }
}