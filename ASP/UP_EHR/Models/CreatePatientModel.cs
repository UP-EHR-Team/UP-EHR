using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UP_EHR.Models
{
    /*
     * CreatePatientModel
     * 
     * Holds values necessary for creating a new patient.
     *
     */
    public class CreatePatientModel
    {
        //using strings for most things to avoid casting issues, especially with
        //user input. MRN is int.
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
        public int mrn { get; set; } //7-digit number
    }
}
