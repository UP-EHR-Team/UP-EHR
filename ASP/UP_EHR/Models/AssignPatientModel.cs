using System;
using System.Collections.Generic;
using UP_EHR.DatabaseObjects;

namespace UP_EHR.Models
{
    public class AssignPatientModel
    {
        //to use list or IEnumerable? look into it, using list for now
        public List<Patient> Patients { get; set; }


        //consider method here that puts all patients retreived from database into list Patients
        //maybe just their names
        //AddToPatients()
    }
}
