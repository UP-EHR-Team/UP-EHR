using System;
namespace UP_EHR.Models
{
    public class SummaryViewModel
    {
        //grab this data from 'summary' table
        public string inputData { get; set; }
        public int mrn { get; set; }

        //internet says to store image path here and store actual images in S3 bucket
        public string image { get; set; }

        //calculate this from D.O.B.
        public string age { get; set; }

        //same info included in AssignPatientModel
        //but, we will not be putting this info in the 'summary' table of our DB
        //we'll grab it from the 'patients' table
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