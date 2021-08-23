using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming.DesignPattern.ObserverVsDelegateVsEvent.Delegate
{
    class DelegateMethod
    {
        public static void Execute()
        {
            // 使用Delegate完成Observer pattern
            Console.WriteLine("Delegate Demo");
            var tempatureMonitorDelegate = new TempatureMonitorUsingDelegate();
            var desktopApp = new DesktopApp();
            var mobileApp = new MobileApp();

            tempatureMonitorDelegate.OnTempatureChanged += desktopApp.OnTempatureChanged;
            tempatureMonitorDelegate.OnTempatureChanged += mobileApp.OnTempatureChanged;

            Console.WriteLine("溫度變化了，現在是30.5度");
            tempatureMonitorDelegate.Tempature = 30.5;

            Console.WriteLine("溫度沒變化，現在依然是30.5度");
            tempatureMonitorDelegate.Tempature = 30.5;

            Console.WriteLine("溫度變化了，現在是28.6度");
            tempatureMonitorDelegate.Tempature = 28.6;

            Console.WriteLine("mobileApp不再想觀察了");
            tempatureMonitorDelegate.OnTempatureChanged -= mobileApp.OnTempatureChanged;

            Console.WriteLine("溫度變化了，現在是27.6度");
            tempatureMonitorDelegate.Tempature = 27.6;
            Console.WriteLine();
        }
    }
}
