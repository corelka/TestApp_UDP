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
        static int ListeningPort = 5000; //port will be used by default
        static string path = @"data.bin"; //file will be created in the root folder of application

        static void Main()
        {
            Console.WriteLine("Please specify port for listening, please (by default '5000' will be used):");
            var _port = Console.ReadLine();
            if (_port != null && _port != "")
                ListeningPort = int.Parse(_port);

            Thread receiveThread = new Thread(new ThreadStart(Receiver));
            receiveThread.Start();//receiving a message will be performed in separate thread for supporting of asynchronicity
            //code of Main method will work in its thread without corrupting Receiver thread
        }


        public static void Receiver()
        {
            UdpClient receiver = new UdpClient(ListeningPort);
            IPEndPoint remoteEP = null; //application will listen all connections but only to specified port of UDPClient
            Console.WriteLine("Server is listening on port "+ListeningPort.ToString()+"...");
            BinaryFormatter formatter = new BinaryFormatter(); //Binary serializator
            try
            {
                while (true)
                {
                    TempData temp = new TempData();
                    using (var ms = new MemoryStream(receiver.Receive(ref remoteEP)))
                    {
                        temp = formatter.Deserialize(ms) as TempData;
                        if(FileAvailable(path)) //check if file stream can be opened for writing 
                            //all the received messages will be added to file if access will be
                            //fixed in less then 10 seconds.
                            File.AppendAllText(path, temp.ToString()); //writing data to file
                        else
                        {
                            throw new Exception("Message is received. But file cannot be opened more then 10 seconds.");
                        }
                        Console.WriteLine("New message received: \n" + temp.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                receiver.Close();
            }
        }

        public static bool FileAvailable(string path)
        {
            /*
             function checks whether file is busy at the moment or not.
             if file cannot be opened, function wait for a second and tries again.
             Whole number of tries is 10. Function is created for synchronisation of threads.
             Monitor would be better on my oppinion but in case of using Monitor
             in main class we need to perform additional lock opening new thread.
             So due to fact that receiveThread should be standalone thread such approach is 
             more suitable.
             */
            int numTries = 0;
            while (true)
            {
                try
                {
                    using(FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                    {
                        fs.ReadByte();
                        break;
                    }
                }
                catch (IOException)
                {
                    if (++numTries >= 10)
                    {
                        return false;
                    }
                    else
                        Thread.Sleep(1000);
                }
                catch (Exception)
                {
                    return false;
                }
            }            
            return true;
        }
    }
}
