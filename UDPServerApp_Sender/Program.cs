using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using UDPServerApp_Listener;


namespace UDPServerApp_Sender
{
    /*simple application for creating of 
     * data to send it to main application.
     * The object of class TempData is created here
     * with dummy data, then it should br serialised 
     * and sent to localhost address and specific port
     * of the main Listener application.     
    */
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
