using System;

namespace Garaza
{
    public class TelemetryProcessor
    {
        public void ObradiTelemetrijskuPoruku(string poruka)
        {
            try
            {
                string[] delovi = poruka.Split('|');
                
                switch (delovi[0])
                {
                    case "ALARM" when delovi.Length == 6:
                        ObradiAlarm(delovi);
                        break;
                        
                    case "TELEMETRIJA" when delovi.Length == 8:
                        ObradiTelemetriju(delovi);
                        break;
                        
                    case "KRAJ_SIMULACIJE" when delovi.Length == 6:
                        ObradiKrajSimulacije(delovi);
                        break;
                        
                    default:
                        Console.WriteLine($"WARN Nepoznat format telemetrije: {poruka}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri obradi telemetrije: {ex.Message}");
            }
        }

        private void ObradiAlarm(string[] delovi)
        {
            string trkackiBroj = delovi[1];
            string proizvodjac = delovi[2];
            string tipAlarma = delovi[3];
            double vrednostGuma = double.Parse(delovi[4]);
            double vrednostGorivo = double.Parse(delovi[5]);
            
            Console.WriteLine($"\nALARM ALARM ALARM PRIMLJEN! ALARM ALARM ALARM");
            Console.WriteLine($"   CAR Automobil: {trkackiBroj}_{proizvodjac}");
            Console.WriteLine($"   WARN Tip alarma: {tipAlarma}");
            
            switch (tipAlarma)
            {
                case "GUME":
                    Console.WriteLine($"   TIRE KRITICNE GUME: {vrednostGuma:F1}% potroseno (< 25% preostalo)");
                    Console.WriteLine($"   INFO Preporuka: Odmah zameniti gume!");
                    break;
                    
                case "GORIVO":
                    Console.WriteLine($"   FUEL KRITICNO GORIVO: {vrednostGorivo:F1}L (nedovoljno za 2 kruga)");
                    Console.WriteLine($"   INFO Preporuka: Napuniti rezervoar!");
                    break;
                    
                case "GUME_I_GORIVO":
                    Console.WriteLine($"   TIRE KRITICNE GUME: {vrednostGuma:F1}% potroseno");
                    Console.WriteLine($"   FUEL KRITICNO GORIVO: {vrednostGorivo:F1}L");
                    Console.WriteLine($"   INFO Preporuka: HITNO servisirati automobil!");
                    break;
            }
            
            Console.WriteLine($"   RETURN Automobil se automatski vraca u garazu...");
            Console.WriteLine($"ALARM ALARM ALARM ALARM ALARM ALARM ALARM ALARM ALARM ALARM");
        }

        private void ObradiTelemetriju(string[] delovi)
        {
            string trkackiBroj = delovi[1];
            string proizvodjac = delovi[2];
            double potrosnjaGoriva = double.Parse(delovi[3]);
            double potrosnjaGuma = double.Parse(delovi[4]);
            double vremeKruga = double.Parse(delovi[5]);
            double preostaloGorivo = double.Parse(delovi[6]);
            double procenatGuma = double.Parse(delovi[7]);
            
            Console.WriteLine($"\nDATA TELEMETRIJA: {trkackiBroj}_{proizvodjac}");
            Console.WriteLine($"   TIME Vreme kruga: {vremeKruga:F3}s");
            Console.WriteLine($"   FUEL Potrosnja goriva: -{potrosnjaGoriva:F3}L (preostalo: {preostaloGorivo:F1}L)");
            Console.WriteLine($"   TIRE Potrosnja guma: -{potrosnjaGuma:F2}km (potroseno: {procenatGuma:F1}%)");
            
            // Analiza i upozorenja
            AnalizirajStanje(procenatGuma, preostaloGorivo, potrosnjaGoriva, potrosnjaGuma, vremeKruga, trkackiBroj, proizvodjac);
        }

        private void ObradiKrajSimulacije(string[] delovi)
        {
            string trkackiBroj = delovi[1];
            string proizvodjac = delovi[2];
            int brojKrugova = int.Parse(delovi[3]);
            double preostaloGorivo = double.Parse(delovi[4]);
            double procenatGuma = double.Parse(delovi[5]);
            
            Console.WriteLine($"\nRACE KRAJ SIMULACIJE: {trkackiBroj}_{proizvodjac}");
            Console.WriteLine($"   DATA Ukupno krugova: {brojKrugova}");
            Console.WriteLine($"   FUEL Preostalo gorivo: {preostaloGorivo:F1}L");
            Console.WriteLine($"   TIRE Potrosenost guma: {procenatGuma:F1}%");
            
            string razlog = preostaloGorivo <= 0 ? "NEMA GORIVA" : 
                           procenatGuma >= 100 ? "GUME POTROSENE" : 
                           procenatGuma >= 75 ? "ALARM - KRITICNE GUME" :
                           "SIMULACIJA ZAVRSENA";
            Console.WriteLine($"   FINISH Razlog zavrsetka: {razlog}");
        }

        private void AnalizirajStanje(double procenatGuma, double preostaloGorivo, double potrosnjaGoriva, 
                                    double potrosnjaGuma, double vremeKruga, string trkackiBroj, string proizvodjac)
        {
            // Upozorenja za gume
            if (procenatGuma >= 70)
            {
                Console.WriteLine($"   WARN UPOZORENJE: Gume se priblizavaju kriticnom nivou!");
                if (procenatGuma >= 75)
                {
                    Console.WriteLine($"   ALARM KRITICNO: Gume su na alarmnom nivou (< 25% preostalo)!");
                }
            }
            
            // Upozorenja za gorivo
            double aproksimacijaPotrosnje = potrosnjaGoriva > 0 ? potrosnjaGoriva : 2.0;
            double potrebnoZaDvaKruga = aproksimacijaPotrosnje * 2;
            
            if (preostaloGorivo <= potrebnoZaDvaKruga * 1.5)
            {
                Console.WriteLine($"   WARN UPOZORENJE: Gorivo se priblizava kriticnom nivou!");
                if (preostaloGorivo <= potrebnoZaDvaKruga)
                {
                    Console.WriteLine($"   ALARM KRITICNO: Gorivo je na alarmnom nivou (< 2 kruga)!");
                }
            }
            
            // Analiza tempa
            if (potrosnjaGoriva > 2.0 || potrosnjaGuma > 1.5)
            {
                Console.WriteLine($"   FAST Indikator: Verovatno vozi BRZIM tempom (visoka potrosnja)");
            }
            else if (vremeKruga > 95.0)
            {
                Console.WriteLine($"   SLOW Indikator: Verovatno vozi SPORIJIM tempom (duze vreme)");
            }
            else
            {
                Console.WriteLine($"   MID Indikator: Verovatno vozi SREDNJIM tempom");
            }
            
            // Preporuke
            if (preostaloGorivo < 15 && (potrosnjaGoriva > 2.0))
            {
                Console.WriteLine($"   INFO PREPORUKA: Posaljite direktivu za sporiji tempo!");
            }
            
            if (procenatGuma > 60 && (potrosnjaGuma > 1.5))
            {
                Console.WriteLine($"   INFO PREPORUKA: Smanjite tempo da sacuvate gume!");
            }
            
            // Kritična upozorenja
            if (preostaloGorivo < 10)
            {
                Console.WriteLine($"   ALARM UPOZORENJE: {trkackiBroj}_{proizvodjac} - MALO GORIVA! ({preostaloGorivo:F1}L)");
            }
            
            if (procenatGuma > 85)
            {
                Console.WriteLine($"   ALARM UPOZORENJE: {trkackiBroj}_{proizvodjac} - GUME SKORO GOTOVE! ({procenatGuma:F1}%)");
            }
        }
    }
}