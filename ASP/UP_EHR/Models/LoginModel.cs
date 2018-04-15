using System;
using System.ComponentModel.DataAnnotations;

namespace UP_EHR.Models
{
    /*
     * LoginModel
     * 
     * Data needed for the login page.
     * 
     */
    public class LoginModel
    {
        public string username { get; set; } = "";
        public string password { get; set; } = "";
        public string errorMsg { get; set; } = "";
    }
}
