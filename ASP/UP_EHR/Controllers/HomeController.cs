using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using UP_EHR.Models;

namespace UP_EHR.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var mvcName = typeof(Controller).Assembly.GetName();
            var isMono = Type.GetType("Mono.Runtime") != null;

            ViewData["Version"] = mvcName.Version.Major + "." + mvcName.Version.Minor;
            ViewData["Runtime"] = isMono ? "Mono" : ".NET";

            return View();
        }

        public ActionResult Login(string UserName, string Password)
        {
            //logic to get incorrect user/pass to popup.
            //TODO: need to put username and password on the server
            //and need to check against those instead.

            if (UserName.Equals("admin") && Password.Equals("admin"))
            {
                return RedirectToAction("Summary");
            }
            else {
                //user/pass failure
                return RedirectToAction("Index");
            }
        }

        public ActionResult Summary()
        {
            //Log in logic can go here, and we can use a redirect if the login is unsuccessful
            //or we can use more involved ways of getting values from the textboxes
            var model = new SummaryViewModel();
            model.username = "Hello";
            model.password = "Welcome to FlowSheets";
            return View(model);
        }

        public ActionResult FlowSheets()
        {
            return View();
        }
    }
}
