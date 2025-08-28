using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Automobil
{
    public class CarNetworkManager
    {
        private Socket udpSocket;
        private bool konekcijuspostavljena;
        
        // Koristite vasu IP adresu
        private const string GARAZA_IP = "192.168.1.2";
        private const int UDP_PORT = 9092;
        private const string DIREKCIJA_IP = "192.168.1.2";
        private const int DIREKCIJA_PORT = 8080;

        public event Action<string> UDPPorukaRostota;

        public bool UspostaviUDPKonekciju()
        {
            try
            {
                udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                udpSocket.Bind(new IPEndPoint(IPAddress.Any, UDP_PORT));
                
                konekcijuspostavljena = true;
                Console.WriteLine($"OK UDP socket bind na port {UDP_PORT}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri UDP konekciji: {ex.Message}");
                return false;
            }
        }

        public async void PokreniUDPListener()
        {
            try
            {
                byte[] buffer = new byte[1024];
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                
                while (konekcijuspostavljena)
                {
                    udpSocket.Blocking = false;
                    
                    try
                    {
                        int bytesReceived = udpSocket.ReceiveFrom(buffer, ref remoteEndPoint);
                        string poruka = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                        
                        Console.WriteLine($"\nNET Primljena UDP poruka: {poruka}");
                        UDPPorukaRostota?.Invoke(poruka);
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.WouldBlock)
                        {
                            await Task.Delay(50);
                            continue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri slusanju UDP poruka: {ex.Message}");
            }
        }

        public async Task<bool> RegistrujSeDirekciji(int trkackiBroj, string proizvodjac)
        {
            try
            {
                TcpClient tcpDirekcija = new TcpClient();
                await tcpDirekcija.ConnectAsync(DIREKCIJA_IP, DIREKCIJA_PORT);
                
                NetworkStream streamDirekcija = tcpDirekcija.GetStream();
                string poruka = $"REGISTRACIJA|{trkackiBroj}|{proizvodjac}\n";
                byte[] data = Encoding.UTF8.GetBytes(poruka);
                
                await streamDirekcija.WriteAsync(data, 0, data.Length);
                Console.WriteLine($"NET Registrovan kod direkcije - trkacki broj: #{trkackiBroj}");
                
                streamDirekcija.Close();
                tcpDirekcija.Close();
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri registraciji kod direkcije: {ex.Message}");
                return false;
            }
        }

        public async Task PosaljiVremeDirekciji(int trkackiBroj, string proizvodjac, double vremeKruga)
        {
            try
            {
                TcpClient tcpDirekcija = new TcpClient();
                await tcpDirekcija.ConnectAsync(DIREKCIJA_IP, DIREKCIJA_PORT);
                
                NetworkStream streamDirekcija = tcpDirekcija.GetStream();
                string poruka = $"{trkackiBroj}|{proizvodjac}|{vremeKruga:F3}\n";
                byte[] data = Encoding.UTF8.GetBytes(poruka);
                
                await streamDirekcija.WriteAsync(data, 0, data.Length);
                
                streamDirekcija.Close();
                tcpDirekcija.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri slanju vremena direkciji: {ex.Message}");
            }
        }

        public async Task PosaljiAlarmDirekciji(int trkackiBroj, string proizvodjac, string tipAlarma, double gume, double gorivo)
        {
            try
            {
                TcpClient tcpDirekcija = new TcpClient();
                await tcpDirekcija.ConnectAsync(DIREKCIJA_IP, DIREKCIJA_PORT);
                
                NetworkStream streamDirekcija = tcpDirekcija.GetStream();
                string poruka = $"ALARM|{trkackiBroj}|{proizvodjac}|{tipAlarma}|{gume:F1}|{gorivo:F1}\n";
                byte[] data = Encoding.UTF8.GetBytes(poruka);
                
                await streamDirekcija.WriteAsync(data, 0, data.Length);
                
                Console.WriteLine($"NET Alarm poslat direkciji: {tipAlarma}");
                
                streamDirekcija.Close();
                tcpDirekcija.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri slanju alarma direkciji: {ex.Message}");
            }
        }

        public async Task ObvestiDirekcijuOIzlasku(int trkackiBroj, string proizvodjac, int brojKruga)
        {
            try
            {
                TcpClient tcpDirekcija = new TcpClient();
                await tcpDirekcija.ConnectAsync(DIREKCIJA_IP, DIREKCIJA_PORT);
                
                NetworkStream streamDirekcija = tcpDirekcija.GetStream();
                string poruka = $"IZLAZAK_SA_STAZE|{trkackiBroj}|{proizvodjac}|{brojKruga}\n";
                byte[] data = Encoding.UTF8.GetBytes(poruka);
                
                await streamDirekcija.WriteAsync(data, 0, data.Length);
                
                Console.WriteLine($"NET Direkcija obavesena: Automobil #{trkackiBroj} vise nije na stazi");
                
                streamDirekcija.Close();
                tcpDirekcija.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri obavestavanju direkcije: {ex.Message}");
            }
        }

        public async Task PosaljiTelemetrijuGarazi(int trkackiBroj, string proizvodjac, double potrosnjaGoriva, 
                                                  double potrosnjaGuma, double vremeKruga, double preostaloGorivo, double procenatGuma)
        {
            try
            {
                using (Socket udpGaraza = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    IPEndPoint garazaEndPoint = new IPEndPoint(IPAddress.Parse(GARAZA_IP), 9091);
                    
                    string poruka = $"TELEMETRIJA|{trkackiBroj}|{proizvodjac}|{potrosnjaGoriva:F3}|{potrosnjaGuma:F3}|{vremeKruga:F3}|{preostaloGorivo:F1}|{procenatGuma:F1}";
                    
                    byte[] data = Encoding.UTF8.GetBytes(poruka);
                    udpGaraza.SendTo(data, garazaEndPoint);
                    
                    Console.WriteLine($"NET Telemetrija poslata garazi: Gorivo -{potrosnjaGoriva:F2}L, Gume -{potrosnjaGuma:F2}km");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri slanju telemetrije garazi: {ex.Message}");
            }
        }

        public async Task PosaljiAlarmGarazi(int trkackiBroj, string proizvodjac, string tipAlarma, double gume, double gorivo)
        {
            try
            {
                using (Socket udpGaraza = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    IPEndPoint garazaEndPoint = new IPEndPoint(IPAddress.Parse(GARAZA_IP), 9091);
                    
                    string poruka = $"ALARM|{trkackiBroj}|{proizvodjac}|{tipAlarma}|{gume:F1}|{gorivo:F1}";
                    
                    byte[] data = Encoding.UTF8.GetBytes(poruka);
                    udpGaraza.SendTo(data, garazaEndPoint);
                    
                    Console.WriteLine($"NET Alarm poslat garazi: {tipAlarma}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri slanju alarma garazi: {ex.Message}");
            }
        }

        public async Task PosaljiFinalneStatistikeGarazi(int trkackiBroj, string proizvodjac, int brojKruga, double preostaloGorivo, double procenatGuma)
        {
            try
            {
                using (Socket udpGaraza = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    IPEndPoint garazaEndPoint = new IPEndPoint(IPAddress.Parse(GARAZA_IP), 9091);
                    
                    string poruka = $"KRAJ_SIMULACIJE|{trkackiBroj}|{proizvodjac}|{brojKruga}|{preostaloGorivo:F1}|{procenatGuma:F1}";
                    
                    byte[] data = Encoding.UTF8.GetBytes(poruka);
                    udpGaraza.SendTo(data, garazaEndPoint);
                    
                    Console.WriteLine($"RACE Finalne statistike poslate garazi: {brojKruga} krugova");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri slanju finalnih statistika: {ex.Message}");
            }
        }

        public void ZatvoriKonekcije()
        {
            try
            {
                udpSocket?.Close();
                konekcijuspostavljena = false;
                Console.WriteLine("CONN Mrezne konekcije zatvorene.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri zatvaranju konekcija: {ex.Message}");
            }
        }

        public bool JeKonekcijuspostavljena()
        {
            return konekcijuspostavljena;
        }
    }
}