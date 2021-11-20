using System;
using System.Collections.Generic;

namespace AbstructNVirtualNInterface
{
    public class Practice2_Door
    {
        public abstract class Door
        {
            public virtual void Open()
            {
                Console.WriteLine("Open Door");
            }
            public virtual void Close()
            {
                Console.WriteLine("Close Door");
            }
        }

        public class HorizontalDoor : Door { }

        public class VerticalDoor : Door
        {
            public override void Open()
            {
                Console.WriteLine("Open vertically");
            }
            public override void Close()
            {
                Console.WriteLine("Close vertically");
            }
        }

        interface IAlarm
        {
            void Alert();
        }

        class Alarm : IAlarm
        {
            public void Alert()
            {
                Console.WriteLine("Ring ~~");
            }
        }

        public class AlarmDoor : Door
        {
            private IAlarm _alarm;

            public AlarmDoor()
            {
                _alarm = new Alarm();
            }

            public void Alert()
            {
                _alarm.Alert();
            }
        }

        public class AutoAlarmDoor : AlarmDoor
        {
            public override void Open()
            {
                base.Open();
                Alert();
            }
        }

        public class DoorController
        {
            protected List<Door> _dootList = new List<Door>();

            public void AddDoor(Door Door)
            {
                _dootList.Add(Door);
            }

            public void OpenDoor()
            {
                foreach (var item in _dootList)
                {
                    item.Open();
                }
            }
        }
    }
}