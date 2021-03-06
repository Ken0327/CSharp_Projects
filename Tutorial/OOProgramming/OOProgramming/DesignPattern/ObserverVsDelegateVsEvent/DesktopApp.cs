using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming.DesignPattern.ObserverVsDelegateVsEvent
{
    public class DesktopApp : ITempatureMonitorObserver
    {
        public void OnTempatureChanged(double tempature)
        {
            Console.WriteLine($"Desktop App被通知溫度變化了: {tempature}");
        }

        public void OnTempatureChangedEvent(object sender, double tempature)
        {
            Console.WriteLine($"Desktop App使用事件委派方法得知溫度變化了: {tempature}");
        }
    }
}
