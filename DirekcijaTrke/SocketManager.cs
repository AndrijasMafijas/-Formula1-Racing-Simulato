using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DirekcijaTrke
{
    public class SocketManager
    {
        public event Action<KlijentInfo> NovaKonekcija;
        public event Action<string, KlijentInfo> PorukaPrimljena;
        public event Action<KlijentInfo> KlijentOtklonjen; // ISPRAVKA: Otkljonjen → Otklonjen

        private readonly List<KlijentInfo> aktivniKlijenti;
        private readonly Dictionary<Socket, KlijentInfo> socketKlijentMapping;
        private Socket serverSocket;
        private bool serverPokrenut;
        
        private const int PORT = 8080;
        private const int SELECT_TIMEOUT_MS = 1000;
        
        public SocketManager()
        {
            aktivniKlijenti = new List<KlijentInfo>();
            socketKlijentMapping = new Dictionary<Socket, KlijentInfo>();
        }

        public bool PokreniServer()
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
                serverSocket.Listen(10);
                serverSocket.Blocking = false;
                
                serverPokrenut = true;
                Console.WriteLine($"NET TCP server pokrenut na portu {PORT}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri pokretanju servera: {ex.Message}");
                return false;
            }
        }

        public void ZaustaviServer()
        {
            serverPokrenut = false;
            
            lock (aktivniKlijenti)
            {
                foreach (var klijent in aktivniKlijenti.ToList())
                {
                    UkloniKlijenta(klijent);
                }
            }
            
            serverSocket?.Close();
        }

        public void ObradiKonekcije()
        {
            if (!serverPokrenut) return;

            try
            {
                List<Socket> readSockets = new List<Socket> { serverSocket };
                
                lock (aktivniKlijenti)
                {
                    foreach (var klijent in aktivniKlijenti)
                    {
                        if (klijent.Socket.Connected)
                        {
                            readSockets.Add(klijent.Socket);
                        }
                    }
                }

                if (readSockets.Count > 0)
                {
                    Socket.Select(readSockets, null, null, SELECT_TIMEOUT_MS * 1000);
                }

                foreach (Socket socket in readSockets)
                {
                    if (socket == serverSocket)
                    {
                        ObradNovuKonekciju();
                    }
                    else
                    {
                        ObradPodatkeOdKlijenta(socket);
                    }
                }

                OcistiNeaktivneKlijente();
            }
            catch (Exception ex)
            {
                if (serverPokrenut)
                {
                    Console.WriteLine($"ERROR Greska u socket manager: {ex.Message}");
                }
            }
        }

        private void ObradNovuKonekciju()
        {
            try
            {
                Socket klijentSocket = serverSocket.Accept();
                klijentSocket.Blocking = false;
                
                KlijentInfo noviKlijent = new KlijentInfo(klijentSocket);
                
                lock (aktivniKlijenti)
                {
                    aktivniKlijenti.Add(noviKlijent);
                    socketKlijentMapping[klijentSocket] = noviKlijent;
                }

                Console.WriteLine($"OK Nova konekcija: {klijentSocket.RemoteEndPoint}");
                NovaKonekcija?.Invoke(noviKlijent);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode != SocketError.WouldBlock)
                {
                    Console.WriteLine($"ERROR Greska pri konekciji: {ex.Message}");
                }
            }
        }

        private void ObradPodatkeOdKlijenta(Socket klijentSocket)
        {
            KlijentInfo klijent = null;
            
            lock (aktivniKlijenti)
            {
                if (!socketKlijentMapping.TryGetValue(klijentSocket, out klijent))
                    return;
            }

            try
            {
                int bytesReceived = klijentSocket.Receive(klijent.Buffer, 0, klijent.Buffer.Length, SocketFlags.None);
                
                if (bytesReceived == 0)
                {
                    UkloniKlijenta(klijent);
                    return;
                }

                string primljeniTekst = Encoding.UTF8.GetString(klijent.Buffer, 0, bytesReceived);
                klijent.PartialMessage.Append(primljeniTekst);

                string kompletanTekst = klijent.PartialMessage.ToString();
                string[] poruke = kompletanTekst.Split(new char[] { '\n', '\0' }, StringSplitOptions.RemoveEmptyEntries);

                if (poruke.Length > 0)
                {
                    for (int i = 0; i < poruke.Length - 1; i++)
                    {
                        PorukaPrimljena?.Invoke(poruke[i].Trim(), klijent);
                    }

                    if (kompletanTekst.EndsWith("\n") || kompletanTekst.EndsWith("\0"))
                    {
                        PorukaPrimljena?.Invoke(poruke[poruke.Length - 1].Trim(), klijent);
                        klijent.PartialMessage.Clear();
                    }
                    else
                    {
                        klijent.PartialMessage.Clear();
                        klijent.PartialMessage.Append(poruke[poruke.Length - 1]);
                    }
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode != SocketError.WouldBlock)
                {
                    UkloniKlijenta(klijent);
                }
            }
        }

        private void UkloniKlijenta(KlijentInfo klijent)
        {
            try
            {
                lock (aktivniKlijenti)
                {
                    aktivniKlijenti.Remove(klijent);
                    socketKlijentMapping.Remove(klijent.Socket);
                }

                if (klijent.Socket.Connected)
                {
                    klijent.Socket.Shutdown(SocketShutdown.Both);
                }
                klijent.Socket.Close();

                Console.WriteLine($"DISCONNECT Klijent {klijent.GetId()} otklonjen");
                KlijentOtklonjen?.Invoke(klijent); // ISPRAVKA: Otkljonjen → Otklonjen
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Greska pri uklanjanju klijenta: {ex.Message}");
            }
        }

        private void OcistiNeaktivneKlijente()
        {
            lock (aktivniKlijenti)
            {
                var neaktivni = aktivniKlijenti.Where(k => !k.Socket.Connected).ToList();
                foreach (var klijent in neaktivni)
                {
                    UkloniKlijenta(klijent);
                }
            }
        }

        public List<KlijentInfo> GetAktivniKlijenti()
        {
            lock (aktivniKlijenti)
            {
                return aktivniKlijenti.ToList();
            }
        }

        public int BrojKlijenata => aktivniKlijenti.Count;
        public bool JePokrenut => serverPokrenut;
    }
}