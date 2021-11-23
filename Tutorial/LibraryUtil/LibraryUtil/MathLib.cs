using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryUtil
{
    public class MathLib
    {
        public MathLib() { }

        public void calculareSum(int x, int y)
        {
            int z = x + y;
            Console.WriteLine(z);
        }

        public void calculareSqrt(double x)
        {
            double z = Math.Sqrt(x);
            Console.WriteLine(z);
        }
    }
}
