using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OOProgramming.DesignPattern
{
    class Singleton
    {
        public class SocketClass
        {
            private Socket socket;
            /// <summary>
            /// private constructor
            /// </summary>
            private SocketClass()
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream
                                    , ProtocolType.Tcp);
            }
            public void Send(byte[] bytes)
            {
                try
                {
                    socket.Send(bytes);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            public bool Connect(string target, int port)
            {
                IPAddress ip;

                if (IPAddress.TryParse(target, out ip))
                {
                    try
                    {
                        socket.Connect(ip, port);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                return socket.Connected;
            }
            public byte[] Receive()
            {
                byte[] buffer = new byte[1024];
                int receiveSize = socket.Receive(buffer);
                Array.Resize(ref buffer, receiveSize);
                return buffer;
            }

            public void Disconnect()
            {
                socket.Close();
            }

            private static SocketClass _SocketObject;
            private static object _syncRoot = new object();
            public static SocketClass SocketObject
            {
                get
                {
                    if (_SocketObject == null)
                    {
                        lock (_syncRoot)
                        {
                            // double locking
                            if (_SocketObject == null)
                            {
                                GetSingleton();
                            }

                        }
                    }
                    return _SocketObject;
                }
            }

            private static void GetSingleton()
            {
                _SocketObject = new SocketClass();
            }
        }
    }
}
