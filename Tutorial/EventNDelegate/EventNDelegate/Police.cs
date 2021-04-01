using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventNDelegate
{
    class Police
    {
        private string name;
        public Police(string name)
        {
            this.name = name;
        }
        // 警察抓小偷委派(方法類別)
        public delegate void PoliceCatchThiefHandler(object obj, PoliceCatchThiefEventArgs args);
        // 警察抓小偷事件(方法變數)
        public event PoliceCatchThiefHandler PoliceCatchThiefEvent;
        public void FindBadGuys()
        {
            Console.WriteLine("喂! 我是{0}", name);
            if (PoliceCatchThiefEvent != null)
            {
                PoliceCatchThiefEventArgs args = new PoliceCatchThiefEventArgs();
                args.Name = name;
                args.CurrentTime = DateTime.Now;
                PoliceCatchThiefEvent(this, args);
            }
        }
    }

    // 版本2: 事件加入輸入參數:可以知道誰觸發(sender/obj)以及觸發時間(args.CurrentTime)
    // 委派名稱: void PoliceCatchThiefHandler(object se nder, PoliceCatchThiefEventArgs args)
    // 事件名稱: PoliceCatchThiefEvent
    class PoliceCatchThiefEventArgs : EventArgs
    {
        string name;
        DateTime dtime;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public DateTime CurrentTime
        {
            get { return dtime; }
            set { dtime = value; }
        }
    }
}
