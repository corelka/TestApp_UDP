using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace UDPServerApp_Listener
{
    public enum _Source : int {
        Source_1,
        Source_2,
        Source_3
    }

    [Serializable]
    public class TempData
    {
        [NonSerialized]
        static DateTime originDate = new DateTime(1970, 1, 1, 0, 0, 0); //this date is needed to convertion of unix timestamp to propper date view 
        public _Source Source { get; set; } //value from enumerable
        public double Time { get; set; } //time is stores like double representation of unix timestamp
        public int[] InternalTemp { get; set; } //digitalized data can be sequence of bits or decimal number
        //Array of int numbers was chosen. BitArray can be chosen for reducing of allocated memory.
        public double Temp { get; set; }

        public override string ToString()
        {
            return "Soure:" 
                + Source 
                + ";Time:" 
                + originDate.AddSeconds(Time).ToString("yyyy-MM-dd HH:mm") 
                + ";InternalTemp:" 
                + string.Join<int>("",InternalTemp) 
                + ";Temp:" 
                + Temp.ToString("000.0")+"\n";
        }
    }

    class Program
    {
        static int ListeningPort = 5000;
        static string path = @"data.bin";

        static void Main()
        {
            Console.WriteLine("Please specify port for listening, please (by default '5000' will be used):");
            var _port = Console.ReadLine();
            if (_port != null && _port != "")
                ListeningPort = int.Parse(_port);
            
            Thread receiveThread = new Thread(new ThreadStart(Receiver));
            receiveThread.Start();
            //code of Main method will work in its thread without corrupting Receiver thread             
        }

        public static void Receiver()
        {
            UdpClient receiver = new UdpClient(ListeningPort);
            IPEndPoint remoteEP = null; //application will listen all connections but only to specified port of UDPClient
            Console.WriteLine("Server is listening on port "+ListeningPort.ToString()+"...");
            try
            {
                BinaryFormatter formatter = new BinaryFormatter(); //Binary serialization
                while (true)
                {
                    TempData temp = new TempData();
                    using (var ms = new MemoryStream(receiver.Receive(ref remoteEP)))
                    {
                        temp = formatter.Deserialize(ms) as TempData;
                        File.AppendAllText(path, temp.ToString());
                        Console.WriteLine("New message received: \n"+temp.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
            finally
            {
                receiver.Close(); //closing of UDP Socket in case of crash while receiving data
            }
        }
    }
}
