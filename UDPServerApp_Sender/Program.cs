using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UDPServerApp_Listener;


namespace UDPServerApp_Sender
{
    
    class Program
    {       
        static void Main(string[] args)
        {
            UdpClient sender = new UdpClient();
            try
            {
                TempData InitData = new TempData()
                { 
                    Source = _Source.Source_1,
                    InternalTemp = new int[] { 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 1 },
                    Temp = 15.1,
                    Time = 1410715640.579
                };
                BinaryFormatter formatter = new BinaryFormatter();
                byte[] data;
                using (var ms = new MemoryStream())
                {
                    formatter.Serialize(ms, InitData);
                    data = ms.ToArray();
                }
                sender.Send(data, data.Length, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000));
                //while (true)
                //{
                //    string message = Console.ReadLine();
                //    byte[] data = Encoding.Unicode.GetBytes(message);
                //    sender.Send(data, data.Length, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000));
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                sender.Close();
            }
        }
    }
}
