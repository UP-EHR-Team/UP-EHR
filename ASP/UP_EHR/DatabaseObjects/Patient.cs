using System;
namespace UP_EHR.DatabaseObjects
{
    /*
     * Patient - class
     * 
     * Data needed to list a patient on the Assign Patient page.
     *
     */
    public class Patient
    {
        public int databaseId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
    }
}
