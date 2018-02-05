using System;
using System.ComponentModel.DataAnnotations;

namespace UP_EHR.Models
{
    public class LoginModel
    {
        public string username { get; set; } = "";
        public string password { get; set; } = "";
        public string errorMsg { get; set; } = "";
    }
}
