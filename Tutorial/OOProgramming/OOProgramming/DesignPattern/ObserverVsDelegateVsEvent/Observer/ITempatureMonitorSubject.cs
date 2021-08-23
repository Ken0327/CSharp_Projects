using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming.DesignPattern.ObserverVsDelegateVsEvent
{
    public interface ITempatureMonitorSubject
    {
        void RegisterObserver(ITempatureMonitorObserver observer);

        void UnregisterObserver(ITempatureMonitorObserver observer);

        void NotifyTempature();
    }
}
