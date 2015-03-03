using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace SoketTool
{
    class Program
    {
        static void Main(string[] args)
        {
            IPAddress address;
            int port;
            GetRemoteServerAddress(out address, out port);

            while (true)
            {
                Console.WriteLine("Input request message>>");
                string request = Console.ReadLine();
                try
                {
                    string response = string.Empty;
                    SendRequest(address, port, request,out response);
                    Console.WriteLine("Response>>" + response);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Occur error:" + ex.Message);
                    GetRemoteServerAddress(out address, out port);
                }
            }
        }

        private static void GetRemoteServerAddress(out IPAddress address, out int port)
        {
            try
            {
                Console.WriteLine("Input remote IP>>");
                string IP = Console.ReadLine();

                while (!IPAddress.TryParse(IP, out address))
                {
                    Console.WriteLine("Input remote IP>>");
                    IP = Console.ReadLine();
                }

                Console.WriteLine("Input remote port>>");
                string input = Console.ReadLine();
                port = 0;

                while (!int.TryParse(input, out port))
                {
                    Console.WriteLine("Input remote port>>");
                    input = Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
                GetRemoteServerAddress(out address, out port);
            }
        }

        private static void SendRequest(IPAddress address, int port, string request, out string response)
        {
            response = string.Empty;
            using (TcpClient client = new TcpClient())
            {
                client.Connect(address, port);

                if (client.Connected)
                {
                    IPEndPoint endPoint = client.Client.RemoteEndPoint as IPEndPoint;
                    Console.WriteLine(String.Format("Connect {0}:{1} success.", endPoint.Address.ToString(), endPoint.Port));

                    using (NetworkStream ns = client.GetStream())
                    {
                        if (ns.CanWrite)
                        {
                            using (StreamWriter writer = new StreamWriter(ns))
                            {
                                writer.WriteLine(request);
                                writer.Flush();
                                client.Client.Shutdown(SocketShutdown.Send);
                                if (ns.CanRead)
                                {
                                    byte[] buffer = new byte[1024];
                                    do
                                    {
                                        int count = ns.Read(buffer, 0, buffer.Length);
                                        response += Encoding.UTF8.GetString(buffer,0,count);
                                    } while (ns.DataAvailable);
                                    client.Client.Shutdown(SocketShutdown.Receive);
                                }
                            }
                        }
                        
                    }
                }
            }
        }
    }
}
