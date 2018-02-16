﻿using System;
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
            var db_logic = new DatabaseLogic(connection, databaseId);
            model = db_logic.GetSummaryData();

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
    }
}
