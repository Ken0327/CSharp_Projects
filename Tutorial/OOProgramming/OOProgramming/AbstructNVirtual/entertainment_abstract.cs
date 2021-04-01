using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming
{
    abstract class entertainment_abstract
    {
        //娛樂費
        protected int RecreationFee = 1000;
        //消費清單
        protected List<string> consumptionList;

        //男女生皆有消費的行為
        abstract public void consumption(int money, byte flag);

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
