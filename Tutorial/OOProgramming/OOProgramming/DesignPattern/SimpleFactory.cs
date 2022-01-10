using System;
using static OOProgramming.DesignPattern.Adapter;

namespace OOProgramming.DesignPattern
{
    class SimpleFactory
    {
        //定義工廠模式
        public enum CommucationType
        {
            Tcp,
            Udp
        }
        /// <summary>
        /// 使用簡單分支運算
        /// </summary>
        public class CommucationFactory
        {
            public static ICommunication GetInstance(CommucationType type)
            {
                switch (type)
                {
                    //case CommucationType.Tcp:
                    //    return new TcpCommunication();
                    case CommucationType.Udp:
                        return new UdpCommunication();
                    default:
                        throw new ArgumentOutOfRangeException();

                }
            }
        }
    }
}