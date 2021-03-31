using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming
{
    class Woman_abstract : entertainment_abstract
    {
        //實作virtual的消費方法
        public override void consumption(int _money, byte _flag)
        {
            if (consumptionList == null)
                consumptionList = new List<string>();

            if (_flag == (byte)1)
                consumptionList.Add("花了" + _money + "買衣服");
            else if (_flag == (byte)2)
                consumptionList.Add("花了" + _money + "買包包");
            else
                consumptionList.Add("花了" + _money + "買雜物");

            RecreationFee -= _money;
        }
    }
}
