using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OOProgramming.DesignPattern
{
    class Adapter
    {
        public interface ICommunication
        {
            bool Connect(string targetm, int Port);

            void Disconnect();
            void Send(byte[] buffer);

            byte[] Receive();
        }

        public class UdpCommunication : ICommunication, IDisposable
        {
            UdpClient listener;
            public bool Connect(string targetm, int Port)
            {
                try
                {
                    listener = new UdpClient(Port);
                    // IPEndPoint groupEP = new IPEndPoint(long.Parse(targetm), Port);
                    listener.Connect(targetm, Port);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return false;
                }
            }

            public void Disconnect()
            {
                try
                {
                    listener.Close();
                    Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            public void Dispose()
            {
                try
                {
                    listener.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            public byte[] Receive()
            {
                //自行Google實做
                throw new NotImplementedException();
            }

            public void Send(byte[] buffer)
            {
                //自行Google實做
                throw new NotImplementedException();
            }
        }
    }
}
