using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming.DesignPattern.ObserverVsDelegateVsEvent
{
    class EventMethod
    {
        public static void Execute()
        {
            // 使用Event事件委派
            Console.WriteLine("Event Demo");
            var tempatureMonitorEvent = new TempatureMonitorUsingEvent();
            var desktopApp = new DesktopApp();
            var mobileApp = new MobileApp();

            tempatureMonitorEvent.OnTempatureChanged += desktopApp.OnTempatureChangedEvent;
            tempatureMonitorEvent.OnTempatureChanged += mobileApp.OnTempatureChangedEvent;
            // 額外自訂事件委派方法, 由於是宣告成事件委派, 輸入到+=時可以直接用TAB產生基本的程式碼
            tempatureMonitorEvent.OnTempatureChanged += TempatureMonitorEvent_OnTempatureChanged;

            Console.WriteLine("溫度變化了，現在是30.5度");
            tempatureMonitorEvent.Tempature = 30.5;

            Console.WriteLine("溫度沒變化，現在依然是30.5度");
            tempatureMonitorEvent.Tempature = 30.5;

            Console.WriteLine("溫度變化了，現在是28.6度");
            tempatureMonitorEvent.Tempature = 28.6;

            Console.WriteLine("mobileApp不再想觀察了");
            tempatureMonitorEvent.OnTempatureChanged -= mobileApp.OnTempatureChangedEvent;

            Console.WriteLine("溫度變化了，現在是27.6度");
            tempatureMonitorEvent.Tempature = 27.6;
            Console.WriteLine();
        }

        private static void TempatureMonitorEvent_OnTempatureChanged(object sender, double e)
        {
            Console.WriteLine($"自訂的委派方法得知溫度變化了: {e}");
        }
    }
}
