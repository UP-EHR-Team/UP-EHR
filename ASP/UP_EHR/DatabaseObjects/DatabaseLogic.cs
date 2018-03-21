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
                summaryModel.mrn = dataReader.GetInt32(17);
                summaryModel.inputData = dataReader.IsDBNull(18) ? "" : dataReader.GetString(18);

                DateTime birthdate = Convert.ToDateTime(summaryModel.birthDate);
                DateTime today = DateTime.Today;
                int calculatedAge = today.Year - birthdate.Year;
                if (today < birthdate.AddYears(calculatedAge))
                {
                    calculatedAge--;
                }
                summaryModel.age = calculatedAge.ToString();

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

            //Finds a random MRN that's not already in the database assigned to another patient
            //assigns that MRN to the new patient
            string randomizeMrn = "SELECT FLOOR(RAND() * 9000000 + 1000000) AS random_num FROM ehr_patients WHERE 'mrn' NOT IN (SELECT mrn FROM ehr_patients) LIMIT 1";
            MySqlCommand cmd1 = new MySqlCommand(randomizeMrn, connection);
            MySqlDataReader dataReader = cmd1.ExecuteReader();
            int random_mrn = 0;
            while (dataReader.Read())
            {
                random_mrn = dataReader.GetInt32(0);
            }
            model.mrn = random_mrn;

            connection.Close();


            connection.Open();
            //generate mysql query with data stored in model
            string query = $"INSERT INTO ehr_patients (first_name, last_name, gender, birthdate, weight, bmi, unit, admit_date, room, allergies, attending, isolation, infection, code_status, healthcare_directives, language, mrn) VALUES ('{model.firstName}', '{model.lastName}', '{model.gender}', '{model.birthDate}', '{model.weight}', '{model.bmi}', '{model.unit}', '{model.admitDate}', '{model.room}', '{model.allergies}', '{model.attending}', '{model.isolation}', '{model.infection}', '{model.codeStatus}', '{model.healthcareDirs}', '{model.language}', '{model.mrn}')";

            MySqlCommand cmd = new MySqlCommand(query, connection);

            //run query and insert data into the database
            cmd.ExecuteNonQuery();

            //create table for flow sheets based upon patient mrn
            string str_mrn = random_mrn.ToString();
            string query2 = $"CREATE TABLE fs_" + str_mrn + " (date_time VARCHAR(20) NOT NULL, bp VARCHAR(20), pulse VARCHAR(20), temp VARCHAR(20), respirations VARCHAR(20), spo2 VARCHAR(20), rr VARCHAR(20), quality VARCHAR(50), map VARCHAR(20), iv VARCHAR(20), oral_intake VARCHAR(50), uop VARCHAR(20), PRIMARY KEY (date_time))";
            MySqlCommand cmd2 = new MySqlCommand(query2, connection);
            cmd2.ExecuteNonQuery();
            connection.Close();
        }

        public void PostSummaryInputData(SummaryViewModel model)
        {
            connection.Open();
            string query = $"UPDATE ehr_patients SET input_data = \"{model.inputData}\" WHERE idehr_patients = {databaseId}";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public FlowSheetsModel GetFlowSheetsInputData(string mrn)
        {
            var flowSheetsModel = new FlowSheetsModel();

            /* List<string> bp = new List<string>();
             List<string> pulse = new List<string>();
             List<string> temp = new List<string>();
             List<string> respirations = new List<string>();
             List<string> spo2 = new List<string>();
             List<string> rr = new List<string>();
             List<string> quality = new List<string>();
             List<string> map = new List<string>();
             List<string> iv = new List<string>();
             List<string> oralIntake = new List<string>();
             List<string> uop = new List<string>();
             List<string> dateTime= new List<string>();
             */
            connection.Open();



            //string patient_mrn = model.mrn.ToString();
            string patient_mrn = mrn;
            string query = $"Select * FROM fs_" + patient_mrn;
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            // cmd.ExecuteNonQuery();


            //store every row of data from the patients flow sheet table into string lists
            //DATE FORMAT -> MM:dd:yyyy_HH   (month:day:year_hour)
            while (dataReader.Read())
            {
                //column values correspond to database columns for flowsheets, in order
                flowSheetsModel.dateTime.Add(dataReader.GetString(0));
                flowSheetsModel.bp.Add(dataReader.GetString(1));
                flowSheetsModel.pulse.Add(dataReader.GetString(2));
                flowSheetsModel.temp.Add(dataReader.GetString(3));
                flowSheetsModel.respirations.Add(dataReader.GetString(4));
                flowSheetsModel.spo2.Add(dataReader.GetString(5));
                flowSheetsModel.rr.Add(dataReader.GetString(6));
                flowSheetsModel.quality.Add(dataReader.GetString(7));
                flowSheetsModel.map.Add(dataReader.GetString(8));
                flowSheetsModel.iv.Add(dataReader.GetString(9));
                flowSheetsModel.oralIntake.Add(dataReader.GetString(10));
                flowSheetsModel.uop.Add(dataReader.GetString(11));

            }

            connection.Close();

            return flowSheetsModel;
        }
        /*
        public void PostFlowSheetsData(FlowSheetsModel model, string mrn)
        {
            connection.Open();
            string patient_mrn = mrn;

            string query = $"INSERT INTO fs_" + patient_mrn + " (date_time, bp, pulse, temp, respirations, spo2, rr, quality, map, iv, oral_intake, uop) VALUES ('{model.dateTime[0]}', '{model.bp[0]}', '{model.pulse[0]}', '{model.temp[0]}', '{model.respirations[0]}', '{model.spo2[0]}', '{model.rr[0]}', '{model.quality[0]}', '{model.pulse[0]}', '{model.map[0]}', '{model.pulse[0]}','{model.iv[0]}', '{model.oralIntake[0]}', '{model.uop[0]}')";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();
            connection.Close();
        }
        */

        public void DeleteAllRows(string tableName)
        {
            string query = $"TRUNCATE TABLE {tableName}";
            connection.Open();
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();


        }

        public void AddTableColumns()
        {
            connection.Open();

            //example command, mimic this
            //string query = $"ALTER TABLE ehr_patients ADD mrn text";
            //MySqlCommand cmd = new MySqlCommand(query, connection);
            //cmd.ExecuteNonQuery();

            connection.Close();
        }
    }
}

/*using System;
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
                summaryModel.mrn = dataReader.GetInt32(17);
                summaryModel.inputData = dataReader.IsDBNull(18) ? "" : dataReader.GetString(18);

                DateTime birthdate = Convert.ToDateTime(summaryModel.birthDate);
                DateTime today = DateTime.Today;
                int calculatedAge = today.Year - birthdate.Year;
                if (today < birthdate.AddYears(calculatedAge))
                {
                    calculatedAge--;
                }
                summaryModel.age = calculatedAge.ToString();

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

            //Finds a random MRN that's not already in the database assigned to another patient
            //assigns that MRN to the new patient
            string randomizeMrn = "SELECT FLOOR(RAND() * 9000000 + 1000000) AS random_num FROM ehr_patients WHERE 'mrn' NOT IN (SELECT mrn FROM ehr_patients) LIMIT 1";
            MySqlCommand cmd1 = new MySqlCommand(randomizeMrn, connection);
            MySqlDataReader dataReader = cmd1.ExecuteReader();
            int random_mrn = 0;
            while(dataReader.Read())
            {
                random_mrn = dataReader.GetInt32(0);
            }
            model.mrn = random_mrn;

            connection.Close();


            connection.Open();
            //generate mysql query with data stored in model
            string query = $"INSERT INTO ehr_patients (first_name, last_name, gender, birthdate, weight, bmi, unit, admit_date, room, allergies, attending, isolation, infection, code_status, healthcare_directives, language, mrn) VALUES ('{model.firstName}', '{model.lastName}', '{model.gender}', '{model.birthDate}', '{model.weight}', '{model.bmi}', '{model.unit}', '{model.admitDate}', '{model.room}', '{model.allergies}', '{model.attending}', '{model.isolation}', '{model.infection}', '{model.codeStatus}', '{model.healthcareDirs}', '{model.language}', '{model.mrn}')";

            MySqlCommand cmd = new MySqlCommand(query, connection);
            //run query and insert data into the database
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public void PostSummaryInputData(SummaryViewModel model)
        {
            connection.Open();
            string query = $"UPDATE ehr_patients SET input_data = \"{model.inputData}\" WHERE idehr_patients = {databaseId}";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public void DeleteAllRows(string tableName)
        {
            string query = $"TRUNCATE TABLE {tableName}";
            connection.Open();
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();


        }

        public void AddTableColumns()
        {
            connection.Open();

            //example command, mimic this
            //string query = $"ALTER TABLE ehr_patients ADD mrn text";
            //MySqlCommand cmd = new MySqlCommand(query, connection);
            //cmd.ExecuteNonQuery();

            connection.Close();
        }
    }
}*/
