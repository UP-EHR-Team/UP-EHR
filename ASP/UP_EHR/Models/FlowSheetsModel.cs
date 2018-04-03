using System;
using System.Collections.Generic;
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
        public string sqlTime { get; set; }
        public string mrn { get; set; }
        //dateTime is the primary key
        public List<string> dateTime { get; set; }
        public List<string> bp { get; set; }
        public List<string> pulse { get; set; }
        public List<string> temp { get; set; }
        public List<string> respirations { get; set; }
        public List<string> spo2 { get; set; }
        public List<string> rr { get; set; }
        public List<string> quality { get; set; }
        public List<string> map { get; set; }
        public List<string> iv { get; set; }
        public List<string> oralIntake { get; set; }
        public List<string> uop { get; set; }

        public FlowSheetsModel () {
            dateTime = new List<string>();
            bp = new List<string>();
            pulse = new List<string>();
            temp = new List<string>();
            respirations = new List<string>();
            spo2 = new List<string>();
            rr = new List<string>();
            quality = new List<string>();
            map = new List<string>();
            iv = new List<string>();
            oralIntake = new List<string>();
            uop = new List<string>();

        }

        public void forwardTime(){
            /*for (int i = 0; i < bp.Count; i++){
                bp[i]= 
            }*/


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

            if (am)
            {
                sqlTime = curDate.ToString("MM:dd:yyyy") + ":" + amTimes[0].Substring(0, 2);
            }
            else
            {
                sqlTime = curDate.ToString("MM:dd:yyyy") + ":" + pmTimes[0].Substring(0, 2);
            }

        }
        public void backwardTime(){
            if (am)
            {
                curDate = curDate.AddDays(-1);
            }
            am = !am;

            if (am){
                sqlTime = curDate.ToString("MM:dd:yyyy")+ ":" + amTimes[0].Substring(0, 2);
            }
            else{
                sqlTime = curDate.ToString("MM:dd:yyyy") + ":" + pmTimes[0].Substring(0, 2);
            }
        }
    }


}
