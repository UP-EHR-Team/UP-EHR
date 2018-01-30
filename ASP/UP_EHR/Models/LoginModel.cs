using System;
using System.ComponentModel.DataAnnotations;

namespace UP_EHR.Models
{
    public class LoginModel
    {
        public string username { get; set; } = "";
        public string password { get; set; } = "";

        //try this in the index.cshtml when you have other stuff working to verify this specifically breaks things
        //@Html.Label(@Model.errorMsg, new {style = color:#ff0000})
        public string errorMsg { get; set; } = "";
    }
}
