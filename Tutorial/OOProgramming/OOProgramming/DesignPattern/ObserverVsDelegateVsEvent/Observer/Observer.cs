using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming.DesignPattern.ObserverVsDelegateVsEvent
{
    class Observer
    {
        public static void Execute()
        {
            // 使用一般Observer pattern
            Console.WriteLine("Observer Pattern Demo");
            var tempatureMonitor = new TempatureMonitorSubject();

            var desktopApp = new DesktopApp();
            var mobileApp = new MobileApp();

            tempatureMonitor.RegisterObserver(desktopApp);
            tempatureMonitor.RegisterObserver(mobileApp);

            Console.WriteLine("溫度變化了，現在是30.5度");
            tempatureMonitor.Tempature = 30.5;

            Console.WriteLine("溫度沒變化，現在依然是30.5度");
            tempatureMonitor.Tempature = 30.5;

            Console.WriteLine("溫度變化了，現在是28.6度");
            tempatureMonitor.Tempature = 28.6;

            Console.WriteLine("mobileApp不再想觀察了");
            tempatureMonitor.UnregisterObserver(mobileApp);

            Console.WriteLine("溫度變化了，現在是27.6度");
            tempatureMonitor.Tempature = 27.6;
            Console.WriteLine();
        }
    }
}
