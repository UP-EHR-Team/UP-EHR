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
        public ActionResult Summary()
        {
            //Log in logic can go here, and we can use a redirect if the login is unsuccessful
            //or we can use more involved ways of getting values from the textboxes
            var model = new SummaryViewModel();


            //DATABASE CONNECTION STUB START //
            //connect to database and run desired query for retrieving data
            /*connection.Open();
            string query = "SELECT * FROM patients";
            List<string> temp_arr = new List<string>();
            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();
            int i = 0;
            while (dataReader.Read())
            {
                temp_arr[i] = dataReader.GetString(1);

            }
            connection.Close();*/
            //DATABASE CONNECTION STUB END //

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

            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();
            //int i = 0;
            while(dataReader.Read())
            {
                first_names.Add(dataReader.GetString(1));
                last_names.Add(dataReader.GetString(2));
            }
            connection.Close();

            for(int j = 0; j < first_names.Count(); j++)
            {
                Patient temp = new Patient { firstName = "", lastName = "" };
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
            //DATABASE CONNECTION STUB START //
            //connect to database and run desired query for retrieving data

            //connection.Open();
            /*string query = "SELECT * FROM patients";
            List<string> temp_arr = new List<string>();
            //Create Command
            //MySqlCommand cmd = new MySqlCommand(query, connection);
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();
            int i = 0;
            while (dataReader.Read())
            {
                temp_arr[i] = dataReader.GetString(1);

            }
            connection.Close();
            //DATABASE CONNECTION STUB END //*/

            return View();
        }

        [HttpPost]
        public ActionResult CreatePatient(CreatePatientModel model)
        {
            //data entered by user is in 'model'.
            //send this data to the database here. ie Add patient to database
            //once successful, go to AssignPatient screen, as seen in Functional Spec Flow Chart
            connection.Open();
            /*string query1 = $"DELETE FROM patients WHERE idpatients>4";
            MySqlCommand cm1 = new MySqlCommand(query1, connection);
            cm1.ExecuteNonQuery();*/

            string query = $"INSERT INTO ehr_patients (first_name, last_name, gender, birthdate, weight, bmi, unit, admit_date, room, allergies, attending, isolation, infection, code_status, healthcare_directives, language) VALUES ('{model.firstName}', '{model.lastName}', '{model.gender}', '{model.birthDate}', '{model.weight}', '{model.bmi}', '{model.unit}', '{model.admitDate}', '{model.room}', '{model.allergies}', '{model.attending}', '{model.isolation}', '{model.infection}', '{model.codeStatus}', '{model.healthcareDirs}', '{model.language}')";
            //string query = $"INSERT INTO patients (idpatients, first_name, last_name) VALUES (77, 'Timothy', 'MacNary')";

            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();

            /*
            //TESTING DATABASE UPLOAD, delete once you've verified it workds
            //TODO: Delete this
            query = "SELECT * FROM patients";
            //Create a list to store the result
            List<string> first_names = new List<string>();
            List<string> last_names = new List<string>();
            List<int> idpats = new List<int>();

            //Create Command
            MySqlCommand cmd2 = new MySqlCommand(query, connection);
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd2.ExecuteReader();
            //int i = 0;
            while (dataReader.Read())
            {
                idpats.Add(dataReader.GetInt32(0));
                first_names.Add(dataReader.GetString(1));
                last_names.Add(dataReader.GetString(2));
            }
            */

            connection.Close();
            return RedirectToAction("AssignPatient");
        }
    }
}
