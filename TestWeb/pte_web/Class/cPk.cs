using System;
using System.Collections.Generic;
using System.Linq;

namespace PTE_Web.Class
{
    public class cPk
    {
        private List<double> RawData = new List<double>();
        public double Cpk = 0.0;
        public double SD = 0.0;

        public double AVG = 0.0;

        public cPk(List<double> data, double usl, double lsl)

        {
            this.RawData = data;
            double _SD = StandardDeviation(this.RawData);
            double Cp = 0.0;
            if (_SD != 0)
            {
                Cp = (usl - lsl) / (6 * _SD);
            }
            //Cp=T/(6σp)，T=規格上限(USL) – 規格下限(LSL)
            //	a = M - X：	代表規格中心(也就是製程之期望中心)與實際製造出來之群體中心的距離。
            //M：產品中心位置（規格中心）
            //X：群體的中心（平均值）
            //b = T / 2：	代表規格的一半。
            //T：規格寬度（規格上限 - 規格下限）
            double M = (usl + lsl) / 2;
            double X = this.RawData.Average();
            double T = (usl - lsl);
            double Ck = T == 0 ? 0.0 : (M - X) / (T / 2);                             //Ck=a/b=(M-X)/(T/2)
            this.Cpk = Math.Round(Math.Abs(1 - Ck) * Cp, 2);
            this.SD = _SD;
            this.AVG = this.RawData.Count() > 0 ? this.RawData.Average() : 0;
        }

        public static double StandardDeviation(List<double> data)
        {
            double num;
            double num1 = 0;
            double num2 = 0;
            double num3 = 0;
            int length = 0;
            try
            {
                length = data.Count;
                if (length != 0)
                {
                    num2 = data.Average();
                    for (int i = 0; i < length; i++)
                    {
                        num3 = num3 + Math.Pow(data[i] - num2, 2);
                    }
                    num1 = Math.Sqrt(SafeDivide(num3, (double)length));
                    return num1;
                }
                else
                {
                    num = num1;
                }
            }
            catch (Exception exception)
            {
                throw;
            }
            return num;
        }

        public static double SafeDivide(double value1, double value2)
        {
            double num = 0;
            try
            {
                if (value1 == 0 || value2 == 0)
                {
                    return num;
                }
                else
                {
                    num = value1 / value2;
                }
            }
            catch
            {
            }
            return num;
        }
    }
}