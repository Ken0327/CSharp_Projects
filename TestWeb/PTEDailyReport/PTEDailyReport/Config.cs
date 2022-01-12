using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEDailyReport
{
    internal class MassProductionDefiniton
    {
        public int ProductionCounts { get; set; }
        public double FYR { get; set; }
        public double UPHAchievement { get; set; }
        public int QuanityRank { get; set; }
        public int ProductionCounts_UPH { get; set; }
        public double RetryRate { get; set; }

        public MassProductionDefiniton()
        {
            ProductionCounts = 500;
            FYR = 90.0;
            UPHAchievement = 0.8;
            QuanityRank = 10;
            ProductionCounts_UPH = 1000;
            RetryRate = 10.0;

        }
    }

    public class MailList
    {
        public string Org { get; set; }
        public string Member_MailList { get; set; }
    }

    public enum Mode
    {
        Production,
        Test
    }

    public enum MailGroup
    {
        T1,
        T2,
        T3,
        T5,
        C5,
        ALL
    }

    internal class Config
    {
        public Mode Mode { get; set; } //mode:1 Production Mode , Mode:0 Test Mode
        public MailGroup MailGroup { get; set; } // 0: All , 1:T1 , 2:T2 , 3:T3 , 4:T5
        public List<MaillReciver> MailAddress { get; set; }

        public Config(Mode mode, MailGroup mailid)
        {
            Mode = mode;
            MailGroup = mailid;
            MailAddress = DataProcess.GetMailListFromServer();

            MailAddress = new List<MaillReciver>();
            if (MailGroup == MailGroup.ALL || MailGroup == MailGroup.T1)
            {
                MailAddress.Add(new MaillReciver { Org = "T1", Name = "MFGENG - Testing Process Engineering at T1", Email = "MFGENG-TestingProcessEngineeringatT1@garmin.com", Developer = 0 });
            }
            if (MailGroup == MailGroup.ALL || MailGroup == MailGroup.T2)
            {
                MailAddress.Add(new MaillReciver { Org = "T2", Name = "MFGENG - Testing Process Engineering at T2", Email = "MFGENG-TestingProcessEngineeringatT2@garmin.com", Developer = 0 });
            }
            if (MailGroup == MailGroup.ALL || MailGroup == MailGroup.T3)
            {
                MailAddress.Add(new MaillReciver { Org = "T3", Name = "MFGENG - Testing Process Engineering at T3", Email = "MFGENG-TestingProcessEngineeringatT3@garmin.com", Developer = 0 });
            }
            if (MailGroup == MailGroup.ALL || MailGroup == MailGroup.T5)
            {
                MailAddress.Add(new MaillReciver { Org = "T5", Name = "MFGENG - Testing Process Engineering at T5", Email = "MFGENG-TestingProcessEngineeringatT5@garmin.com", Developer = 0 });
            }
            if (Mode == Mode.Test || MailAddress.Count == 0)//Test
            {
                MailAddress.Clear();
                MailAddress.Add(new MaillReciver { Org = "T1", Name = "Jasper.Fang", Email = "Jasper.Fang@garmin.com", ProductOwnerList = new List<int>() });
                MailAddress.Add(new MaillReciver { Org = "T2", Name = "Jasper.Fang", Email = "Jasper.Fang@garmin.com", ProductOwnerList = new List<int>() });
                MailAddress.Add(new MaillReciver { Org = "T3", Name = "Jasper.Fang", Email = "Jasper.Fang@garmin.com", ProductOwnerList = new List<int>() });
                MailAddress.Add(new MaillReciver { Org = "T5", Name = "Jasper.Fang", Email = "Jasper.Fang@garmin.com", ProductOwnerList = new List<int>() });
            }
        }
    }
}