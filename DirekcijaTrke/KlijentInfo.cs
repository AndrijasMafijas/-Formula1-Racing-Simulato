using System;
using System.Net.Sockets;
using System.Text;

namespace DirekcijaTrke
{
    public class KlijentInfo
    {
        public Socket Socket { get; set; }
        public string TrkackiBroj { get; set; }
        public string Proizvodjac { get; set; }
        public bool NaStazi { get; set; }
        public DateTime VremeKonekcije { get; set; }
        public int BrojPrimljenihKrugova { get; set; }
        public byte[] Buffer { get; set; }
        public StringBuilder PartialMessage { get; set; }

        public KlijentInfo(Socket socket)
        {
            Socket = socket;
            NaStazi = false;
            VremeKonekcije = DateTime.Now;
            BrojPrimljenihKrugova = 0;
            Buffer = new byte[1024];
            PartialMessage = new StringBuilder();
        }

        public string GetId()
        {
            return $"{TrkackiBroj}_{Proizvodjac}";
        }

        public override string ToString()
        {
            return $"{GetId()} ({Socket.RemoteEndPoint}) - {(NaStazi ? "Na stazi" : "U garaži")} - Krugova: {BrojPrimljenihKrugova}";
        }
    }
}