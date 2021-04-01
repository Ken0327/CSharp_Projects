using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming.DesignPattern
{
    class Facade
    {
        public static int ByteToInt(byte[] b)
        {
            return b[0] + b[1] * 256 + b[2] * 256 * 256 + b[3] * 256 * 256 * 256;
            //return BitConverter.ToInt32(b, 0);
        }
    }
}
