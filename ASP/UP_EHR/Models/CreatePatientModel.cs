using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UP_EHR.Models
{
    public class CreatePatientModel
    {
        //using strings for everything as of now, might need to change some of those in the future
        //such as birthDate, might want to force the user to type in DateTime object correctly
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string gender { get; set; }
        public string birthDate { get; set; }
        public string weight { get; set; }
        public string admitDate { get; set; }
        public string room { get; set; }
        public string unit { get; set; }
        public string allergies { get; set; }
        public string attending { get; set; }
        public string bmi { get; set; }
        public string isolation { get; set; }
        public string infection { get; set; }
        public string codeStatus { get; set; }
        public string healthcareDirs { get; set; }
        public string language { get; set; }
    }
}
