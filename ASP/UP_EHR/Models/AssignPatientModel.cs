using System;
using System.Collections.Generic;
using UP_EHR.DatabaseObjects;

namespace UP_EHR.Models
{
    public class AssignPatientModel
    {
        //List of patients that will be used to display patient names on the 
        //assign patient page.
        public List<Patient> Patients { get; set; }
    }
}
