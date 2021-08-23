using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming.DesignPattern.ObserverVsDelegateVsEvent
{
    public class TempatureMonitorUsingEvent
    {
        // 使用EventHandler<T>來省去自訂delegate的麻煩
        public event EventHandler<double> OnTempatureChanged;

        private double tempature;

        public double Tempature
        {
            get { return tempature; }
            set
            {
                var oldTempature = tempature;
                if (oldTempature != value)
                {
                    tempature = value;
                    if (OnTempatureChanged != null)
                    {
                        OnTempatureChanged(this, value);
                    }
                }
            }
        }
    }
}
