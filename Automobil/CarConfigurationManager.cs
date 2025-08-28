using System;

namespace Automobil
{
    public class CarConfigurationManager
    {
        public KonfiguracijaAutomobila Konfiguracija { get; private set; }
        public KomponentaGuma Gume { get; private set; }
        public double TrenutnoGorivo { get; private set; }

        public void IzaberiKonfiguraciju()
        {
            IzaberiProizvodjaca();
            IzaberiGume();
            PostaviGorivo();
        }

        private void IzaberiProizvodjaca()
        {
            Console.WriteLine("\n--- BRAND IZBOR PROIZVODJACA ---");
            Console.WriteLine("1. Mercedes");
            Console.WriteLine("2. Ferrari");
            Console.WriteLine("3. Renault");
            Console.WriteLine("4. Honda");
            
            while (true)
            {
                Console.Write("Izaberite proizvodjaca (1-4): ");
                string izbor = Console.ReadLine();
                
                switch (izbor)
                {
                    case "1":
                        Konfiguracija = new KonfiguracijaAutomobila("Mercedes");
                        Console.WriteLine("OK Izabran Mercedes");
                        return;
                    case "2":
                        Konfiguracija = new KonfiguracijaAutomobila("Ferrari");
                        Console.WriteLine("OK Izabran Ferrari");
                        return;
                    case "3":
                        Konfiguracija = new KonfiguracijaAutomobila("Renault");
                        Console.WriteLine("OK Izabran Renault");
                        return;
                    case "4":
                        Konfiguracija = new KonfiguracijaAutomobila("Honda");
                        Console.WriteLine("OK Izabran Honda");
                        return;
                    default:
                        Console.WriteLine("ERROR Nevalidan izbor! Molimo unesite broj od 1 do 4.");
                        break;
                }
            }
        }

        private void IzaberiGume()
        {
            Console.WriteLine("\n--- TIRE IZBOR KOMPONENTE GUME ---");
            Console.WriteLine("1. Meke gume (M) - 80km");
            Console.WriteLine("2. Srednje gume (S) - 100km");
            Console.WriteLine("3. Tvrde gume (T) - 120km");
            
            while (true)
            {
                Console.Write("Izaberite tip guma (1-3): ");
                string izbor = Console.ReadLine();
                
                switch (izbor)
                {
                    case "1":
                        Gume = new KomponentaGuma(KomponentaGuma.TipGuma.Meke);
                        Console.WriteLine("OK Izabrane meke gume");
                        return;
                    case "2":
                        Gume = new KomponentaGuma(KomponentaGuma.TipGuma.Srednje);
                        Console.WriteLine("OK Izabrane srednje gume");
                        return;
                    case "3":
                        Gume = new KomponentaGuma(KomponentaGuma.TipGuma.Tvrde);
                        Console.WriteLine("OK Izabrane tvrde gume");
                        return;
                    default:
                        Console.WriteLine("ERROR Nevalidan izbor! Molimo unesite broj od 1 do 3.");
                        break;
                }
            }
        }

        private void PostaviGorivo()
        {
            while (true)
            {
                Console.Write("\nFUEL Unesite pocetnu kolicinu goriva (litara): ");
                string input = Console.ReadLine();
                
                if (double.TryParse(input, out double gorivo) && gorivo > 0)
                {
                    TrenutnoGorivo = gorivo;
                    Console.WriteLine($"OK Postavljeno gorivo: {gorivo:F1}L");
                    break;
                }
                else
                {
                    Console.WriteLine("ERROR Molimo unesite validnu pozitivnu vrednost!");
                }
            }
        }

        public void AzurirajKonfiguracijuSaStaze(string oznakaGuma, double gorivo)
        {
            // Postavi nove gume na osnovu garazine komande
            switch (oznakaGuma)
            {
                case "M":
                    Gume = new KomponentaGuma(KomponentaGuma.TipGuma.Meke);
                    break;
                case "S":
                    Gume = new KomponentaGuma(KomponentaGuma.TipGuma.Srednje);
                    break;
                case "T":
                    Gume = new KomponentaGuma(KomponentaGuma.TipGuma.Tvrde);
                    break;
            }
            
            TrenutnoGorivo = gorivo;
            
            Console.WriteLine($"SYNC Azurirana konfiguracija:");
            Console.WriteLine($"   TIRE Gume: {Gume.Oznaka}");
            Console.WriteLine($"   FUEL Gorivo: {TrenutnoGorivo:F1}L");
        }

        public void SmaniGorivo(double kolicina)
        {
            TrenutnoGorivo -= kolicina;
            if (TrenutnoGorivo < 0) TrenutnoGorivo = 0;
        }

        public void IspisiKonfiguraciju()
        {
            Console.WriteLine("\n=== CAR KONFIGURACIJA AUTOMOBILA ===");
            Console.WriteLine($"BRAND Proizvodjac: {Konfiguracija?.Proizvodjac ?? "Nije izabran"}");
            Console.WriteLine($"TIRE Gume: {Gume?.ToString() ?? "Nisu izabrane"}");
            Console.WriteLine($"FUEL Gorivo: {TrenutnoGorivo:F1} litara");
            Console.WriteLine("====================================");
        }
    }
}