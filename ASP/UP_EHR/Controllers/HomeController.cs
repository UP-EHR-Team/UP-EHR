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
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;


namespace UP_EHR.Controllers
{
    public class HomeController : Controller
    {
        //AWS Keys to access S3 bucket to save patient images
        public string awsAccessKey = "";
        public string awsSecretKey = "";

        //Set the database connection variables
        static string dbuser = "";
        static string dbpass = "";
        static string dbhost = "";
        static string dbname = "";
        static string dbconnect = "Data Source=" + dbhost + ";Initial Catalog=" + dbname + ";User ID=" + dbuser + ";Password=" + dbpass + ";";

        //Initilalize conneciton to be opened and closed during later HTTP responses
        MySqlConnection connection = new MySqlConnection(dbconnect);

        /*
         * Login - HttpGet
         * 
         * Get the login page and display it to the user 
         *
         */
        [HttpGet]
        public ActionResult Login()
        {
            LoginModel model = new LoginModel();
            return View(model);
        }

        /*
         * Login - HttpPost
         * 
         * Called to verify credentials when user tries to login
         * 
         */
        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            string UserName;
            string Password;

            //if user hasn't filled in username & pass, error
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


            //if username and/or password incorrect, error
            if (UserName.ToLower().Equals("admin") && Password.Equals("admin"))
            {
                //if correct credentials display the assign patient page
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


        /*
         * Summary - HttpGet
         * 
         * Display summary page for selected patient, identified by databaseId
         * 
         */
        [HttpGet]
        public ActionResult Summary(int databaseId)
        {
            var model = new SummaryViewModel();
            var db_logic = new DatabaseLogic(connection, databaseId);
            model = db_logic.GetSummaryData();

            Session["patientId"] = databaseId;
            Session["mrn"] = model.mrn;

            String s = "https://s3-us-west-2.amazonaws.com/ehr-prod/Patient" + Session["mrn"].ToString() + ".png";
            model.imagePath = s;

            return View(model);
        }

        /*
         * Summary - HttpPost
         * 
         * Called when submit button clicked on Summary page,
         * saves the contents of the text box to the database.
         *
         */
        [HttpPost]
        public ActionResult Summary(SummaryViewModel model)
        {
            var dbid = Session["patientId"].ToString();
            int db_id = Convert.ToInt32(dbid);
            var db_logic = new DatabaseLogic(connection, db_id);
            db_logic.PostSummaryInputData(model);

            String s = "https://s3-us-west-2.amazonaws.com/ehr-prod/Patient" + Session["mrn"].ToString()+ ".png";
            model.imagePath = s;

            return RedirectToAction("Summary", new { databaseId = db_id});
        }


        /*
         * FlowSheets - HttpGet
         * 
         * Display FlowSheets page to user
         * 
         */
        [HttpGet]
        public ActionResult FlowSheets()
        {
            var model = new FlowSheetsModel();
            var db_logic = new DatabaseLogic(connection);
            // get the mrn of the current patient to display correct flow
            // sheets data
            string patient_mrn = Session["mrn"].ToString();

           
            model.databaseId = Convert.ToInt32(Session["patientId"].ToString());

            //if it's the first time the patient is visiting the page,
            //display the current day and time 
            if ((Session["fs_am"]) == null)
            {
                model.curDate = DateTime.Today.Date;
                //for SQL query, set the first datetime of the page range
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

                //verify that the model has all correct data
                model = verifyModel(model);
                //verify that all the flowsheets boxes are correct
                db_logic.DatabaseIntegrityCheck(patient_mrn, model);

                model = db_logic.GetFlowSheetsInputData(patient_mrn, model);
                // set the correct hours to display based on time of day
                if (DateTime.Now.Hour < 12)
                {
                    model.am = true;
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
                    model.displayTimes = new string[12];
                    for (int i = 0; i < model.displayTimes.Length; i++)
                    {
                        model.displayTimes[i] = model.pmTimes[i];
                    }
                }
            }
            // if the patient has already visited the page, display
            // the last day/time where they left off
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
         * SummaryImage - HttpPost
         * 
         * Saves patient image uploaded by user to the S3 bucket
         * 
         */
        [HttpPost]
        public ActionResult SummaryImage(HttpPostedFileBase file)
        {
            var dbid = Session["patientId"].ToString();
            int db_id = Convert.ToInt32(dbid);
            var db_logic = new DatabaseLogic(connection, db_id);

            if (file.ContentLength > 0)
            {
                //decide where to save the image in the bucket, filename based on patient mrn number
                string savepath = Path.Combine(Server.MapPath("~/Resources/PatientImages/Patient" + Session["mrn"].ToString() + ".png"));
                string bucketName = "ehr-prod";
                string keyName = "Patient" + Session["mrn"].ToString() + ".png";

                //path to image uplaoded by user
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Server.MapPath("~/Resources/PatientImages"), fileName);
                //temp save the image locally
                file.SaveAs(savepath);

                IAmazonS3 client;
                client = new AmazonS3Client(awsAccessKey, awsSecretKey, RegionEndpoint.USWest2);
                TransferUtilityConfig config = new TransferUtilityConfig();


                PutObjectRequest request = new PutObjectRequest()
                {
                    BucketName = bucketName,
                    Key = keyName,
                    FilePath = savepath,
                    CannedACL = S3CannedACL.PublicReadWrite
                };
                //save image to S3 bucket
                PutObjectResponse response = client.PutObject(request);
            }

            return RedirectToAction("Summary", new { databaseId = db_id });
        }

        /*
         * AssignPatient - HttpGet
         * 
         * Display Assign Patient page
         * 
         */
        [HttpGet]
        public ActionResult AssignPatient()
        {
            var model = new AssignPatientModel();
            var db_logic = new DatabaseLogic(connection);
            model = db_logic.GetAssignPatientData();

            return View(model);
        }

        /*
         * CreatePatient - HttpGet
         * 
         * Display create patient page
         * 
         */
        [HttpGet]
        public ActionResult CreatePatient()
        {
            return View();
        }

        /*
         * CreatePatient - HttpPost
         * 
         * Create the patient in the database based on user's entered
         * data, and create a flow sheets table for that user based
         * on their MRN.
         *
         */
        [HttpPost]
        public ActionResult CreatePatient(CreatePatientModel model)
        {
            var db_logic = new DatabaseLogic(connection);
            db_logic.PostCreatePatientData(model);

            return RedirectToAction("AssignPatient");
        }

        /*
         * _PatientOverview
         * 
         * Show partail view banner at top of current page,
         * which displays current patient's overview data.
         * 
         */
        [ChildActionOnly]
        public ActionResult _PatientOverview() 
        {
            int id = Convert.ToInt32(Session["patientId"].ToString());
            var db_logic = new DatabaseLogic(connection, id);
            var model = db_logic.GetSummaryData();
            return PartialView(model);
        }

        /*
         * _FlowSheets
         * 
         * Display partial view for flow sheets page. This partial view
         * contains the table populated with the patient data.
         * 
         */
        public ActionResult _FlowSheets(FlowSheetsModel model)
        {

            return PartialView(model);
        }

        /*
         * SaveFlowsheets - HttpPost
         * 
         * Save data input into flow sheets table to the database upon
         * clicking the save button.
         * 
         */
        [HttpPost]
        public ActionResult SaveFlowsheets(FlowSheetsModel model)
        {
            model = verifyModel(model);
                                
            int id = Convert.ToInt32(Session["patientId"].ToString());
            var db_logic = new DatabaseLogic(connection, id);
            string mrn = Session["mrn"].ToString();
            db_logic.PostFlowSheetsData(model, mrn);

           
            return RedirectToAction("FlowSheets", model);

        }

        /*
         * verifyModel
         * 
         * Verify that the flowsheets model is up to date with all fields
         * correcly filled.
         * 
         */
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

        /*
         * _FSForward
         * 
         * When forward button is clicked, move the flow sheets 
         * table forward by 12 hours.
         * 
         */
        public ActionResult _FSForward(FlowSheetsModel model)
        {
            
            model = verifyModel(model);
            //change hours and days to add 12 hours to current time
            model.forwardTime();
            Session["fs_date"] = model.curDate;
            Session["fs_am"] = model.am;

            model = verifyModel(model);

            int id = Convert.ToInt32(Session["patientId"].ToString());
            var db_logic = new DatabaseLogic(connection, id);
            db_logic.DatabaseIntegrityCheck(Session["mrn"].ToString(), model);
            Session["fs_date"] = model.curDate;
            Session["fs_am"] = model.am;

            //reload the page so that the correct data is filled in for new time
            return RedirectToAction("FlowSheets", model);
        }


        /*
         * _FSBackward
         * 
         * When back button is clicked, move the flow sheets 
         * table back by 12 hours.
         * 
         */
        public ActionResult _FSBackward(FlowSheetsModel model)
        {
            model = verifyModel(model);
            //move all times back by 12 hours, update AM/PM accordingly
            model.backwardTime();
            Session["fs_date"] = model.curDate;
            Session["fs_am"] = model.am;

            model = verifyModel(model);

            int id = Convert.ToInt32(Session["patientId"].ToString());
            var db_logic = new DatabaseLogic(connection, id);
            db_logic.DatabaseIntegrityCheck(Session["mrn"].ToString(), model);
            Session["fs_date"] = model.curDate;
            Session["fs_am"] = model.am;
             
            //reload the page so that the correct data is filled in for new time
            return RedirectToAction("FlowSheets", model);
        }

    }
}
