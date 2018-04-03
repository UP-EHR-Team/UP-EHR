using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using UP_EHR.Models;
using UP_EHR.DatabaseObjects;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.SessionState;
using System.IO;

namespace UP_EHR.Controllers
{
    public class HomeController : Controller
    {
        //Set the database connection variables
        static string dbuser = "upehr";
        static string dbpass = "graduate";
        static string dbhost = "aa182qkpqn2nq7j.czhw4bdantwp.us-west-2.rds.amazonaws.com";
        static string dbname = "upehr";
        static string dbconnect = "Data Source=" + dbhost + ";Initial Catalog=" + dbname + ";User ID=" + dbuser + ";Password=" + dbpass + ";";

        //Initilalize conneciton to be opened and closed during later HTTP responses
        MySqlConnection connection = new MySqlConnection(dbconnect);

        [HttpGet]
        public ActionResult Login()
        {
            LoginModel model = new LoginModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            string UserName;
            string Password;
            if (model.username != null && model.password != null)
            {
                UserName = model.username;
                Password = model.password;
            }
            else
            {
                model.errorMsg = "Please make sure both fields are filled";
                return View(model);
            }


            //logic to get incorrect user/pass to popup.
            //TODO: need to put username and password on the server
            //and need to check against those instead.
            if (UserName.Equals("admin") && Password.Equals("admin"))
            {
                return RedirectToAction("AssignPatient");
            }
            else
            {
                //user/pass failure
                model.username = "";
                model.password = "";
                model.errorMsg = "Error: Incorrect credentials";
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Summary(int databaseId)
        {
            /*var model = new SummaryViewModel();
            var db_logic = new DatabaseLogic(connection, databaseId);
            model = db_logic.GetSummaryData();

            Session["patientId"] = databaseId;*/
            var model = new SummaryViewModel();
            var db_logic = new DatabaseLogic(connection, databaseId);
            model = db_logic.GetSummaryData();

            Session["patientId"] = databaseId;
            Session["mrn"] = model.mrn;


            return View(model);
        }

        [HttpPost]
        public ActionResult Summary(SummaryViewModel model)
        {
            var dbid = Session["patientId"].ToString();
            int db_id = Convert.ToInt32(dbid);
            var db_logic = new DatabaseLogic(connection, db_id);

            db_logic.PostSummaryInputData(model);

            return RedirectToAction("Summary", new { databaseId = db_id});
        }

        [HttpPost]
        public ActionResult SummaryImage(HttpPostedFileBase file)
        {
            if (file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                file.SaveAs(path);
                //SummaryViewModel svm 
            }

            //return RedirectToAction("Summary");

            return View();
        }

        [HttpGet]
        public ActionResult FlowSheets()
        {
            //no longer uses databaseID
            //uses the mrn stored in session variable during the summary page GET

            //model.databaseId = Convert.ToInt32(Session["patientId"].ToString());
            var model = new FlowSheetsModel();

            var db_logic = new DatabaseLogic(connection);
            string patient_mrn = Session["mrn"].ToString();


            model.databaseId = Convert.ToInt32(Session["patientId"].ToString());

            if ((Session["fs_am"]) == null)
            {
                model.curDate = DateTime.Today.Date;
                //model.curDate = DateTime.Today.Date.AddDays(-5);
                if (model.am)
                {
                    model.sqlTime = model.curDate.ToString("MM:dd:yyyy") + ":" + model.amTimes[0].Substring(0, 2);
                }
                else
                {
                    model.sqlTime = model.curDate.ToString("MM:dd:yyyy") + ":" + model.pmTimes[0].Substring(0, 2);
                }
                Session["fs_date"] = model.curDate;
                Session["fs_am"] = model.am;
                model = verifyModel(model);
                db_logic.DatabaseIntegrityCheck(patient_mrn, model);
                model = db_logic.GetFlowSheetsInputData(patient_mrn, model);
                if (DateTime.Now.Hour < 12)
                {
                    model.am = true;
                    //model.displayTimes = model.amTimes;
                    string[] temp = new string[12];
                    model.displayTimes = temp;
                    for (int i = 0; i < model.displayTimes.Length; i++)
                    {
                        model.displayTimes[i] = model.amTimes[i];
                    }
                }
                else
                {
                    model.am = false;
                    //model.displayTimes = model.pmTimes;
                    model.displayTimes = new string[12];
                    for (int i = 0; i < model.displayTimes.Length; i++)
                    {
                        model.displayTimes[i] = model.pmTimes[i];
                    }
                }
            }
            else
            {
                model.am = Convert.ToBoolean(Session["fs_am"]);
                model.curDate = Convert.ToDateTime(Session["fs_date"]);
                if (model.am)
                {
                    model.sqlTime = model.curDate.ToString("MM:dd:yyyy") + ":" + model.amTimes[0].Substring(0, 2);
                }
                else
                {
                    model.sqlTime = model.curDate.ToString("MM:dd:yyyy") + ":" + model.pmTimes[0].Substring(0, 2);
                }
                model = verifyModel(model);
                db_logic.DatabaseIntegrityCheck(patient_mrn, model);
                model = db_logic.GetFlowSheetsInputData(patient_mrn, model);
            }


            return View(model);
        }

        /*
        [HttpPost]
        public ActionResult FlowSheets(FlowSheetsModel model)
        {
            var db_logic = new DatabaseLogic(connection);
            var patient_mrn = Session["mrn"].ToString();
            db_logic.PostFlowSheetsData(model, patient_mrn);

            return RedirectToAction("FlowSheets");
        }
        */

        [HttpGet]
        public ActionResult AssignPatient()
        {
            var model = new AssignPatientModel();
            var db_logic = new DatabaseLogic(connection);
            model = db_logic.GetAssignPatientData();

            return View(model);
        }

        [HttpGet]
        public ActionResult CreatePatient()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreatePatient(CreatePatientModel model)
        {
            var db_logic = new DatabaseLogic(connection);
            db_logic.PostCreatePatientData(model);

            return RedirectToAction("AssignPatient");
        }

        [ChildActionOnly]
        public ActionResult _PatientOverview() 
        {
            int id = Convert.ToInt32(Session["patientId"].ToString());
            var db_logic = new DatabaseLogic(connection, id);
            var model = db_logic.GetSummaryData();
            return PartialView(model);
        }

        //[ChildActionOnly]
        public ActionResult _FlowSheets(FlowSheetsModel model)
        {

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult SaveFlowsheets(FlowSheetsModel model)
        {
            /*DateTime _curDate = Convert.ToDateTime(Session["fs_date"]);
            bool _am = Convert.ToBoolean(Session["fs_am"]);
            model.am = _am;
            model.curDate = _curDate;

            for (int i = 0; i < 12; i++){
                if (model.am)
                { 
                    model.dateTime.Add(model.curDate.ToString("MM:dd:yyyy") + ":" + model.amTimes[i].Substring(0, 2));
                }
                else {
                    model.dateTime.Add(model.curDate.ToString("MM:dd:yyyy") + ":" + model.pmTimes[i].Substring(0, 2));
                }
            }*/
            model = verifyModel(model);
                                
            int id = Convert.ToInt32(Session["patientId"].ToString());
            var db_logic = new DatabaseLogic(connection, id);
            string mrn = Session["mrn"].ToString();
            db_logic.PostFlowSheetsData(model, mrn);

           
            return RedirectToAction("FlowSheets", model);

        }

        public FlowSheetsModel verifyModel(FlowSheetsModel model){
            model.dateTime.Clear();
            DateTime _curDate = Convert.ToDateTime(Session["fs_date"]);
            bool _am = Convert.ToBoolean(Session["fs_am"]);
            model.am = _am;
            model.curDate = _curDate;

            for (int i = 0; i < 12; i++)
            {
                if (model.am)
                {
                    model.dateTime.Add(model.curDate.ToString("MM:dd:yyyy") + ":" + model.amTimes[i].Substring(0, 2));
                }
                else
                {
                    model.dateTime.Add(model.curDate.ToString("MM:dd:yyyy") + ":" + model.pmTimes[i].Substring(0, 2));
                }
            }

            return model;
        }

        //[HttpPost]
        public ActionResult _FSForward(FlowSheetsModel model)
        {
            /*DateTime curDate = Convert.ToDateTime(Session["fs_date"]);
            bool am = Convert.ToBoolean(Session["fs_am"]);
            model.am = am;
            model.curDate = curDate;
            model.forwardTime();
            Session["fs_date"] = model.curDate;
            Session["fs_am"] = model.am;
            return RedirectToAction("FlowSheets", model);*/

            /*DateTime curDate = Convert.ToDateTime(Session["fs_date"]);
            bool am = Convert.ToBoolean(Session["fs_am"]);
            model.am = am;
            model.curDate = curDate;*/

            model = verifyModel(model);
            model.forwardTime();
            Session["fs_date"] = model.curDate;
            Session["fs_am"] = model.am;

            model = verifyModel(model);

            int id = Convert.ToInt32(Session["patientId"].ToString());
            var db_logic = new DatabaseLogic(connection, id);
            db_logic.DatabaseIntegrityCheck(Session["mrn"].ToString(), model);
            Session["fs_date"] = model.curDate;
            Session["fs_am"] = model.am;

            return RedirectToAction("FlowSheets", model);
        }



        //[HttpPost]
        public ActionResult _FSBackward(FlowSheetsModel model)
        {
            /*DateTime curDate = Convert.ToDateTime(Session["fs_date"]);
            bool am = Convert.ToBoolean(Session["fs_am"]);
            model.curDate = curDate;
            model.am = am;
            model.backwardTime();
            Session["fs_date"] = model.curDate;
            Session["fs_am"] = model.am;
            return RedirectToAction("FlowSheets", model);*/

            /*DateTime curDate = Convert.ToDateTime(Session["fs_date"]);
            bool am = Convert.ToBoolean(Session["fs_am"]);
            model.curDate = curDate;
            model.am = am;*/


            model = verifyModel(model);
            model.backwardTime();
            Session["fs_date"] = model.curDate;
            Session["fs_am"] = model.am;

            model = verifyModel(model);

            int id = Convert.ToInt32(Session["patientId"].ToString());
            var db_logic = new DatabaseLogic(connection, id);
            db_logic.DatabaseIntegrityCheck(Session["mrn"].ToString(), model);
            Session["fs_date"] = model.curDate;
            Session["fs_am"] = model.am;

            return RedirectToAction("FlowSheets", model);
        }

    }
}
