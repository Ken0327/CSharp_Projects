using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming.DesignPattern.ObserverVsDelegateVsEvent
{
    public interface ITempatureMonitorObserver
    {
        void OnTempatureChanged(double tempature);
    }
}
