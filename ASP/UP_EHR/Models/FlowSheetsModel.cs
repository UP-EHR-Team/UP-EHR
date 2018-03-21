using System;
namespace UP_EHR.Models
{
    public class FlowSheetsModel
    {
        public int databaseId { get; set; }
        public DateTime curDate { get; set; }
        public bool am { get; set; }
        public string[] amTimes = { "0000", "0100", "0200", "0300", "0400", "0500", "0600", "0700", "0800", "0900", "1000", "1100" };
        public string[] pmTimes = { "1200", "1300", "1400", "1500", "1600", "1700", "1800", "1900", "2000", "2100", "2200", "2300" };
        public string[] displayTimes = { "1200", "1300", "1400", "1500", "1600", "1700", "1800", "1900", "2000", "2100", "2200", "2300" };

        public void forwardTime(){
            if (!am)
            {
                curDate = curDate.AddDays(1);
                for (int i = 0; i < displayTimes.Length; i++)
                {
                    displayTimes[i] = amTimes[i];
                }
                //displayTimes = amTimes;
            }
            else {
                for (int i = 0; i < displayTimes.Length; i++){
                    displayTimes[i] = pmTimes[i];
                }
                //displayTimes = pmTimes;
            }
            am = !am;


        }
        public void backwardTime(){
            if (am)
            {
                curDate = curDate.AddDays(-1);
            }
            am = !am;
        }
    }


}
