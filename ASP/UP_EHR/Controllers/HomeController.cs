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

        public ActionResult Login()
        {
            RedirectToAction("Summary");
            return View();
        }

        public ActionResult Summary(string UserName, string Password)
        {
            //Log in logic can go here, and we can use a redirect if the login is unsuccessful
            //or we can use more involved ways of getting values from the textboxes
            var model = new SummaryViewModel();
            model.username = UserName;
            model.password = Password;
            return View(model);
        }

        public ActionResult FlowSheets()
        {
            return View();
        }
    }
}
