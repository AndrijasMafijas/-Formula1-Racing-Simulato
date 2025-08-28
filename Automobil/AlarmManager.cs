using System;
using System.Threading.Tasks;

namespace Automobil
{
    public class AlarmManager
    {
        private readonly CarNetworkManager networkManager;

        public event Action<string, double, double> AlarmAktiviran;

        public AlarmManager(CarNetworkManager networkManager)
        {
            this.networkManager = networkManager;
        }

        public async Task ProveriKriticneNivoe(int trkackiBroj, string proizvodjac, double procenatGuma, double trenutnoGorivo, double potrebnoZaDvaKruga)
        {
            bool kritičneGume = procenatGuma >= 75; // 25% preostalo = 75% potrošeno
            bool kritičnoGorivo = trenutnoGorivo < potrebnoZaDvaKruga;
            
            if (kritičneGume || kritičnoGorivo)
            {
                Console.WriteLine($"\nALARM ALARM ALARM AKTIVIRAN! ALARM ALARM ALARM");
                
                if (kritičneGume)
                {
                    Console.WriteLine($"   TIRE KRITICNE GUME: {procenatGuma:F1}% potroseno (preostalo < 25%)");
                }
                
                if (kritičnoGorivo)
                {
                    Console.WriteLine($"   FUEL KRITICNO GORIVO: {trenutnoGorivo:F1}L (potrebno {potrebnoZaDvaKruga:F1}L za 2 kruga)");
                }
                
                Console.WriteLine($"   NET Saljem alarm garazi i direkciji...");
                
                // Pripremi tip alarma
                string tipAlarma = "";
                if (kritičneGume && kritičnoGorivo) tipAlarma = "GUME_I_GORIVO";
                else if (kritičneGume) tipAlarma = "GUME";
                else if (kritičnoGorivo) tipAlarma = "GORIVO";
                
                // Pošalji alarm garaži
                await networkManager.PosaljiAlarmGarazi(trkackiBroj, proizvodjac, tipAlarma, procenatGuma, trenutnoGorivo);
                
                // Pošalji alarm direkciji
                await networkManager.PosaljiAlarmDirekciji(trkackiBroj, proizvodjac, tipAlarma, procenatGuma, trenutnoGorivo);
                
                Console.WriteLine($"   HOME AUTOMATSKI POZIV ZA VRACANJE U GARAZU!");
                
                // Aktiviraj event da se simulacija prekine
                AlarmAktiviran?.Invoke(tipAlarma, procenatGuma, trenutnoGorivo);
            }
        }
    }
}