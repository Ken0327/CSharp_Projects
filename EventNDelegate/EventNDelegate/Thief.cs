using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventNDelegate
{
    class Thief
    {
        private string name;
        public Thief(string name)
        {
            this.name = name;
        }
        public void RunAway(object sender, PoliceCatchThiefEventArgs args)
        {
            Console.WriteLine("{0} 警察 \"{1}\"來了!, \"{2}\"快跑", args.CurrentTime.ToString("yyyy/MM/dd HH:mm:ss"), args.Name, name);
        }
    }
}
