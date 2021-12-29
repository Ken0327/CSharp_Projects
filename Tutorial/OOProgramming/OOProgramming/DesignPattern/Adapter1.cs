using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OOProgramming.DesignPattern
{
    class Adapter1
    {
        public void Main()
        {
            ILightningPhone applePhone = new ApplePhone();
            IUsbPhone adapterCable = new LightningToUsbAdapter(applePhone);
            adapterCable.ConnectUsb();
            adapterCable.Recharge();
        }

        public interface ILightningPhone
        {
            void ConnectLightning();
            void Recharge();
        }

        public interface IUsbPhone
        {
            void ConnectUsb();
            void Recharge();
        }

        public sealed class AndroidPhone : IUsbPhone
        {
            private bool isConnected;

            public void ConnectUsb()
            {
                this.isConnected = true;
                Console.WriteLine("Android phone connected.");
            }

            public void Recharge()
            {
                if (this.isConnected)
                {
                    Console.WriteLine("Android phone recharging.");
                }
                else
                {
                    Console.WriteLine("Connect the USB cable first.");
                }
            }
        }

        public sealed class ApplePhone : ILightningPhone
        {
            private bool isConnected;

            public void ConnectLightning()
            {
                this.isConnected = true;
                Console.WriteLine("Apple phone connected.");
            }

            public void Recharge()
            {
                if (this.isConnected)
                {
                    Console.WriteLine("Apple phone recharging.");
                }
                else
                {
                    Console.WriteLine("Connect the Lightning cable first.");
                }
            }
        }

        public sealed class LightningToUsbAdapter : IUsbPhone
        {
            private readonly ILightningPhone lightningPhone;

            private bool isConnected;

            public LightningToUsbAdapter(ILightningPhone lightningPhone)
            {
                this.lightningPhone = lightningPhone;
                this.lightningPhone.ConnectLightning();
            }

            public void ConnectUsb()
            {
                this.isConnected = true;
                Console.WriteLine("Adapter cable connected.");
            }

            public void Recharge()
            {
                if (this.isConnected)
                {
                    this.lightningPhone.Recharge();
                }
                else
                {
                    Console.WriteLine("Connect the USB cable first.");
                }
            }
        }
    }
}
