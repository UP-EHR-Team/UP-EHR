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


        public ActionResult Index()
        {
            var mvcName = typeof(Controller).Assembly.GetName();
            var isMono = Type.GetType("Mono.Runtime") != null;

            ViewData["Version"] = mvcName.Version.Major + "." + mvcName.Version.Minor;
            ViewData["Runtime"] = isMono ? "Mono" : ".NET";

            return View();
        }

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
            var model = new SummaryViewModel();


            //DATABASE CONNECTION STUB START //
            connection.Open();
            string query = $"SELECT * FROM ehr_patients WHERE idehr_patients = {databaseId}";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                model.firstName = dataReader.GetString(1);
                model.lastName = dataReader.GetString(2);
                model.gender = dataReader.GetString(3);
                model.birthDate = dataReader.GetString(4);
                model.weight = dataReader.GetString(5);
                model.bmi = dataReader.GetString(6);
                model.unit = dataReader.GetString(7);
                model.admitDate = dataReader.GetString(8);
                model.room = dataReader.GetString(9);
                model.allergies = dataReader.GetString(10);
                model.attending = dataReader.GetString(11);
                model.isolation = dataReader.GetString(12);
                model.infection = dataReader.GetString(13);
                model.codeStatus = dataReader.GetString(14);
                model.healthcareDirs = dataReader.GetString(15);
                model.language = dataReader.GetString(16);


                //TODO: Everything below here needs to be put with the correct index in the database
                model.age = "TEST y.o.";

                //TODO: this line from other table that needs to be built
                //model.inputData = dataReader.GetString(2);

                //TODO: this line from other table that needs to be built
                //model.mrn = dataReader.GetString(2);

            }
            connection.Close();
            //DATABASE CONNECTION STUB STOP //

            return View(model);
        }

        [HttpPost]
        public ActionResult Summary(SummaryViewModel model)
        {
            return View();
        }

        public ActionResult FlowSheets()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AssignPatient()
        {
            var model = new AssignPatientModel();

            List<Patient> listOfPatients = new List<Patient>();

            //IMPLEMENTED DATABASE CONNECTION START//
            connection.Open();
            string query = "SELECT * FROM ehr_patients";
            //Create a list to store the result
            List<string> first_names = new List<string>();
            List<string> last_names = new List<string>();
            List<int> db_ids = new List<int>();

            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();
            //int i = 0;
            while(dataReader.Read())
            {
                db_ids.Add(dataReader.GetInt32(0)); //try using getstring if this doesn't work
                first_names.Add(dataReader.GetString(1));
                last_names.Add(dataReader.GetString(2));
            }
            connection.Close();

            for(int j = 0; j < first_names.Count(); j++)
            {
                Patient temp = new Patient { firstName = "", lastName = "" };
                temp.databaseId = db_ids[j];
                temp.firstName = first_names[j];
                temp.lastName = last_names[j];
                listOfPatients.Add(temp);
            }
            //IMPLEMENTED DATABASE CONNECTION END//

            model.Patients = listOfPatients;
            return View(model);
        }

        [HttpGet]
        public ActionResult CreatePatient()
        {
            //NO DATABASE CONNECTION NEEDED FOR GET REQUESTS TO THIS PAGE
            return View();
        }

        [HttpPost]
        public ActionResult CreatePatient(CreatePatientModel model)
        {
            //data entered by user is in 'model'.
            //send this data to the database here. ie Add patient to database
            //once successful, go to AssignPatient screen, as seen in Functional Spec Flow Chart
            connection.Open();

            //generate mysql query with data stored in model
            string query = $"INSERT INTO ehr_patients (first_name, last_name, gender, birthdate, weight, bmi, unit, admit_date, room, allergies, attending, isolation, infection, code_status, healthcare_directives, language) VALUES ('{model.firstName}', '{model.lastName}', '{model.gender}', '{model.birthDate}', '{model.weight}', '{model.bmi}', '{model.unit}', '{model.admitDate}', '{model.room}', '{model.allergies}', '{model.attending}', '{model.isolation}', '{model.infection}', '{model.codeStatus}', '{model.healthcareDirs}', '{model.language}')";

            MySqlCommand cmd = new MySqlCommand(query, connection);
            //run query and insert data into the database
            cmd.ExecuteNonQuery();
            connection.Close();

            return RedirectToAction("AssignPatient");
        }
    }
}
