using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Garaza
{
    public class NetworkManager
    {
        private TcpClient tcpClient;
        private NetworkStream tcpStream;
        private Socket udpSocket;
        private bool konekcijuspostavljena;
        
        private const string SERVER_IP = "192.168.1.2";
        private const int SERVER_PORT = 8080;
        private const int UDP_PORT = 9091;

        public event Action<string> TelemetrijaPorukaRostota;
        
        public bool UspostaviTCPKonekciju()
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(SERVER_IP, SERVER_PORT);
                tcpStream = tcpClient.GetStream();
                konekcijuspostavljena = true;
                
                Console.WriteLine("OK TCP konekcija sa Direkcijom Trke uspostavljena");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri TCP konekciji: {ex.Message}");
                Console.WriteLine("Proverite da li je Direkcija Trke pokrenuta!");
                return false;
            }
        }

        public bool PokreniUDPServer()
        {
            try
            {
                udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                udpSocket.Bind(new IPEndPoint(IPAddress.Any, UDP_PORT));
                Console.WriteLine($"OK UDP server pokrenut na portu {UDP_PORT}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri pokretanju UDP servera: {ex.Message}");
                return false;
            }
        }

        public async void PokreniUDPListener()
        {
            Console.WriteLine("NET Pokrenut UDP listener za telemetriju automobila...");
            
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
                        
                        TelemetrijaPorukaRostota?.Invoke(poruka);
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
                Console.WriteLine($"ERROR Greska pri slusanju telemetrije: {ex.Message}");
            }
        }

        public void PosaljiTCPPoruku(string poruka)
        {
            if (konekcijuspostavljena && tcpStream != null)
            {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(poruka);
                    tcpStream.Write(data, 0, data.Length);
                    Console.WriteLine($"NET TCP poruka poslata Direkciji: {poruka}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR Greska pri slanju TCP poruke: {ex.Message}");
                }
            }
        }

        public void PosaljiUDPPoruku(string poruka, int automobilPort = 9092)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(poruka);
                IPEndPoint automobilEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.2"), automobilPort);
                udpSocket.SendTo(data, automobilEndPoint);
                
                Console.WriteLine($"NET UDP poruka poslata automobilu: {poruka}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri slanju UDP poruke: {ex.Message}");
            }
        }

        public void ZatvoriKonekcije()
        {
            try
            {
                tcpStream?.Close();
                tcpClient?.Close();
                udpSocket?.Close();
                konekcijuspostavljena = false;
                Console.WriteLine("CONN Konekcije zatvorene.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri zatvaranju konekcija: {ex.Message}");
            }
        }

        public bool JeKonekcijuspostavljena()
        {
            return konekcijuspostavljena && tcpClient != null && tcpClient.Connected;
        }
    }
}