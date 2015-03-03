using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Threading;
using System.Windows.Forms;

namespace DebatchControlEmulator
{
    public class SocketListener
    {
        private IPAddress address;
        private int port;
        private TcpListener listener;
        public event Action<string> ReceiveSignalEventHandler;
        private Socket socket;

        public SocketListener(IPAddress address, int port)
        {
            this.address = address;
            this.port = port;
            listener = new TcpListener(address, port);
        }

        public void Start()
        {
            Thread thread = new Thread(() =>
            {

                this.listener.Start();
                while (true)
                {
                    try
                    {
                        if (this.listener.Pending())
                        {
                            if (socket == null || !socket.Connected)
                            {
                                socket = this.listener.AcceptSocket();
                            }
                        }

                        if (socket != null && socket.Available > 0)
                        {
                            try
                            {
                                int size = 1024;
                                byte[] buffer = new byte[size];
                                int cnt = socket.Receive(buffer, size, SocketFlags.None);
                                string message = ASCIIEncoding.ASCII.GetString(buffer, 0, cnt);
                                
                                CommandHelper.SplitMultipleCommand(message).ForEach(command =>
                                {
                                    if (this.ReceiveSignalEventHandler != null)
                                    {
                                        this.ReceiveSignalEventHandler(CommandHelper.GetRealCommand(command));
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                this.socket.Shutdown(SocketShutdown.Both);
                                this.socket.Dispose();
                                Console.WriteLine(ex.Message);
                                MessageBox.Show(ex.Message);
                            }
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.socket.Shutdown(SocketShutdown.Both);
                        this.socket.Dispose();
                        Console.WriteLine(ex.Message);
                        MessageBox.Show(ex.Message);
                    }
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public void SendReply(string message)
        {
            try
            {
                this.socket.Send(ASCIIEncoding.ASCII.GetBytes(message));
            }
            catch
            {
                this.socket.Shutdown(SocketShutdown.Both);
                this.socket.Dispose();
            }
        }
    }

    public enum CommandType
    {
        TCommand,
        OCommand,
        LCommand,
        DCommand,
        PCommand,
        ZCommand
    }

    public class CommandHelper
    {
        private const char stx = (char)0x02;
        private const char etx = (char)0x03;

        public static List<string> SplitMultipleCommand(string command)
        {
            return command.Split(new char[] { stx, etx }, StringSplitOptions.RemoveEmptyEntries).Select(cmd => stx + cmd + etx).ToList();
        }

        public static string GetSequenceNumberOfCommand(string command)
        {
            return command.Substring(1, 3);
        }

        public static int GetCommandLength(string command)
        {
            return Convert.ToInt32(command.Substring(4, 4));
        }

        public static string GetRealCommand(string command)
        {
            return command.Substring(8, command.Length - 9);
        }

        public static CommandType GetCommandType(string realCommand)
        {
            string commandSymbol = realCommand.Substring(0, 1);
            switch (commandSymbol)
            {
                case "t":
                    return CommandType.TCommand;
                case "L":
                    return CommandType.LCommand;
                case "D":
                    return CommandType.DCommand;
                case "Z":
                    return CommandType.ZCommand;
                case "O":
                    return CommandType.OCommand;
                default:
                    return CommandType.PCommand;

            }
        }

        public static string GetLightAddress(string realCommand)
        {
            string lightAdress = String.Empty;
            switch (GetCommandType(realCommand))
            {
                case CommandType.TCommand:
                case CommandType.LCommand:
                    lightAdress = realCommand.Substring(1, 4);
                    break;
                case CommandType.DCommand:
                    lightAdress = realCommand.Substring(1, realCommand.Length - 1);
                    break;
                case CommandType.PCommand:
                    lightAdress = realCommand.Substring(2, realCommand.Length - 2);
                    break;
                default:
                    lightAdress = string.Empty;
                    break;
            }
            return lightAdress;
        }

        public static string GetReplyTCommand(string address)
        {
            string squenceNumber = Sequencer.getNextSquenceNo();
            return stx + squenceNumber + string.Format("{0:D4}", 4) + "t" + address + etx;
        }

        public static string GetReplyOCommand()
        {
            string squenceNumber = Sequencer.getNextSquenceNo();
            return stx + squenceNumber + string.Format("{0:D4}", 1) + "o" + etx;
        }
    }

    public static class Sequencer
    {
        private static object locker = new object();
        private static int sequence = 0;
        public static string getNextSquenceNo()
        {
            lock (locker)
            {
                if (sequence > 999)
                    sequence = 0;

                return string.Format("{0:d3}", sequence++);
            }
        }
        public static void RestSequence()
        {
            lock (locker)
            {
                sequence = 0;
            }
        }
    }
}
