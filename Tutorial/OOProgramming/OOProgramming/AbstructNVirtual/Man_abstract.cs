using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming
{
    class Man_abstract : entertainment_abstract
    {
        //實作abstract的消費方法
        public override void consumption(int _money, byte _flag)
        {
            if (consumptionList == null)
                consumptionList = new List<string>();

            if (_flag == (byte)1)
                consumptionList.Add("男生花了" + _money + "買遊戲");
            else if (_flag == (byte)2)
                consumptionList.Add("男生花了" + _money + "買電腦");
            else
                consumptionList.Add("男生花了" + _money + "買雜物");

            RecreationFee -= _money;
        }
    }
}
