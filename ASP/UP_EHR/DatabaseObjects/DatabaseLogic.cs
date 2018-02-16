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

namespace UP_EHR.DatabaseObjects
{
    public class DatabaseLogic
    {
        private MySqlConnection connection;
        private int databaseId;

        public DatabaseLogic()
        {
        }

        public DatabaseLogic(MySqlConnection conn, int dbId) 
        {
            connection = conn;
            databaseId = dbId;
        }

        public DatabaseLogic(MySqlConnection conn)
        {
            connection = conn;
        }

        private MySqlDataReader DatabaseOpen(string query)
        {
            connection.Open();
            MySqlCommand cmd = new MySqlCommand(query, connection);
            return cmd.ExecuteReader();
        }

        public SummaryViewModel GetSummaryData()
        {
            var summaryModel = new SummaryViewModel();
            connection.Open();

            string query = $"SELECT * FROM ehr_patients WHERE idehr_patients = {databaseId}";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                summaryModel.firstName = dataReader.GetString(1);
                summaryModel.lastName = dataReader.GetString(2);
                summaryModel.gender = dataReader.GetString(3);
                summaryModel.birthDate = dataReader.GetString(4);
                summaryModel.weight = dataReader.GetString(5);
                summaryModel.bmi = dataReader.GetString(6);
                summaryModel.unit = dataReader.GetString(7);
                summaryModel.admitDate = dataReader.GetString(8);
                summaryModel.room = dataReader.GetString(9);
                summaryModel.allergies = dataReader.GetString(10);
                summaryModel.attending = dataReader.GetString(11);
                summaryModel.isolation = dataReader.GetString(12);
                summaryModel.infection = dataReader.GetString(13);
                summaryModel.codeStatus = dataReader.GetString(14);
                summaryModel.healthcareDirs = dataReader.GetString(15);
                summaryModel.language = dataReader.GetString(16);


                //TODO: Calculate age based on DOB. This will be easy once DatePicker is implemented
                summaryModel.age = "TEST y.o.";

                //TODO: this line from other table that needs to be built
                //summaryModel.inputData = dataReader.GetString(2);

                //TODO: this line from other table that needs to be built
                //summaryModel.mrn = dataReader.GetString(2);

            }
            connection.Close();

            return summaryModel;
        }

        public AssignPatientModel GetAssignPatientData()
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
            while (dataReader.Read())
            {
                db_ids.Add(dataReader.GetInt32(0)); //try using getstring if this doesn't work
                first_names.Add(dataReader.GetString(1));
                last_names.Add(dataReader.GetString(2));
            }
            connection.Close();

            for (int j = 0; j < first_names.Count(); j++)
            {
                Patient temp = new Patient { firstName = "", lastName = "" };
                temp.databaseId = db_ids[j];
                temp.firstName = first_names[j];
                temp.lastName = last_names[j];
                listOfPatients.Add(temp);
            }
            //IMPLEMENTED DATABASE CONNECTION END//

            model.Patients = listOfPatients;

            return model;
        }

        public void PostCreatePatientData(CreatePatientModel model)
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
        }
    }
}
