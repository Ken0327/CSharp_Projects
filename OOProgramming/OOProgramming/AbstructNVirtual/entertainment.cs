using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming
{
    class entertainment
    {
        //娛樂費
        protected int RecreationFee = 1000;
        //消費清單
        protected List<string> consumptionList;

        //男女生皆有消費的行為
        public virtual void consumption(int money, byte flag)
        {
            if (consumptionList == null)
                consumptionList = new List<string>();

            if (flag == (byte)1)
                consumptionList.Add("花了" + money + "吃早餐");
            else if (flag == (byte)2)
                consumptionList.Add("花了" + money + "吃午餐");
            else if (flag == (byte)3)
                consumptionList.Add("花了" + money + "吃晚餐");
            else
                consumptionList.Add("花了" + money + "吃零食");
            RecreationFee -= money;
        }

        //取得剩餘的錢
        public int getMoney()
        {
            return RecreationFee;
        }
        //取得消費清單
        public List<string> GetconsumptionList()
        {
            return consumptionList;
        }
    }
}
