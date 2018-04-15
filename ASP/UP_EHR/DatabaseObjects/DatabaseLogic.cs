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

        /*
         * DatabaseLogic - constructor
         * 
         * Constructor for when database id is needed (ie, summary page)
         * 
         */
        public DatabaseLogic(MySqlConnection conn, int dbId)
        {
            connection = conn;
            databaseId = dbId;
        }

        /*
         * DatabaseLogic - constructor
         * 
         * Basic constructor for non-summary pages
         * 
         */
        public DatabaseLogic(MySqlConnection conn)
        {
            connection = conn;
        }

        /*
         * GetSummaryData - SummaryViewModel
         * 
         * Fills in 'Summary' model for the patient summary page with values 
         * from the database. The model will be passed to the summary view via
         * the summary controller.
         * 
         */
        public SummaryViewModel GetSummaryData()
        {
            var summaryModel = new SummaryViewModel();
            connection.Open();

            string query = $"SELECT * FROM ehr_patients WHERE idehr_patients = {databaseId}";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            //Get patient summary data from database
            //When a patient is created, every value must be submitted except inputData
            //Therefore, do a check for null with inputData to avoid error
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

                //Calculate patients age
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

        /*
         * GetAssignPatientData - AssignePatientModel
         * 
         * Returns a model for the Assign patient controller, to be passed on to
         * the AssignPatient view. Takes all patients in the database and loads
         * their first and last names into the model, as well as their database
         * id numbers.
         * 
         */
        public AssignPatientModel GetAssignPatientData()
        {
            var model = new AssignPatientModel();

            List<Patient> listOfPatients = new List<Patient>();

            connection.Open();
            string query = "SELECT * FROM ehr_patients";

            //Create several lists to store the results
            //List indices will be constant for each patient
            List<string> first_names = new List<string>();
            List<string> last_names = new List<string>();
            List<int> db_ids = new List<int>();


            //Create and execute database command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            //load each patients info into our lists
            while (dataReader.Read())
            {
                db_ids.Add(dataReader.GetInt32(0));
                first_names.Add(dataReader.GetString(1));
                last_names.Add(dataReader.GetString(2));
            }
            connection.Close();

            //load each patients data from the lists into a list the model will
            //accept
            for (int j = 0; j < first_names.Count(); j++)
            {
                Patient temp = new Patient { firstName = "", lastName = "" };
                temp.databaseId = db_ids[j];
                temp.firstName = first_names[j];
                temp.lastName = last_names[j];
                listOfPatients.Add(temp);
            }

            model.Patients = listOfPatients;

            return model;
        }

        /*
         * PostCreatePatientData - void
         * 
         * Once the user is done entering new patient data on the CreatePatient
         * view, the information is saved to the database here. Method takes in
         * one parameter, a model from the CreatePatient view with all of the
         * user input.
         * 
         */
        public void PostCreatePatientData(CreatePatientModel model)
        {
            connection.Open();

            //Finds a random 7-digit MRN (medical record number) that's not 
            //already in the database assigned to another patient, assigns that
            //MRN to the new patient
            string randomizeMrn = "SELECT FLOOR(RAND() * 9000000 + 1000000) AS " +
                "random_num FROM ehr_patients WHERE 'mrn' NOT IN (SELECT mrn " +
                "FROM ehr_patients) LIMIT 1";


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

            //Generate mysql query from data stored in model
            string query = $"INSERT INTO ehr_patients (first_name, last_name, " +
                "gender, birthdate, weight, bmi, unit, admit_date, room, " +
                "allergies, attending, isolation, infection, code_status, " +
                "healthcare_directives, language, mrn) VALUES " +
                "('{model.firstName}', '{model.lastName}', '{model.gender}', " +
                "'{model.birthDate}', '{model.weight}', '{model.bmi}', " +
                "'{model.unit}', '{model.admitDate}', '{model.room}', " +
                "'{model.allergies}', '{model.attending}', '{model.isolation}', " +
                "'{model.infection}', '{model.codeStatus}', '{model.healthcareDirs}', " +
                "'{model.language}', '{model.mrn}')";

            MySqlCommand cmd = new MySqlCommand(query, connection);

            //Run query and insert data into the database
            cmd.ExecuteNonQuery();

            //Create unique patient table for flow sheets based upon patient mrn
            string str_mrn = random_mrn.ToString();
            string createTable = $"CREATE TABLE fs_" + str_mrn + " (date_time VARCHAR(20) NOT NULL, bp VARCHAR(20), pulse VARCHAR(20), temp VARCHAR(20), respirations VARCHAR(20), spo2 VARCHAR(20), rr VARCHAR(20), quality VARCHAR(50), map VARCHAR(20), iv VARCHAR(20), oral_intake VARCHAR(50), uop VARCHAR(20), PRIMARY KEY (date_time))";
            MySqlCommand cmd2 = new MySqlCommand(createTable, connection);
            cmd2.ExecuteNonQuery();

            connection.Close();
        }

        /*
         * PostSummaryInputData - void
         * 
         * Input data from the summary page is saved to the database here once
         * the user clicks the 'submit' button.
         * 
         */
        public void PostSummaryInputData(SummaryViewModel model)
        {
            connection.Open();
            string query = $"UPDATE ehr_patients SET input_data = \"{model.inputData}\" " +
                "WHERE idehr_patients = {databaseId}";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        /*
         * DatabaseIntegrityCheck - void
         * 
         * Saves empty strings to flow sheets tables if there isn't already data
         * saved in the fields. This makes sure that there will be available 
         * slots in the table for user input in the future.
         * 
         */
        public void DatabaseIntegrityCheck(string mrn, FlowSheetsModel model)
        {
            connection.Open();
            string patient_mrn = mrn;

            string dateTime0, dateTime1, dateTime2, dateTime3, dateTime4, dateTime5, dateTime6, dateTime7, dateTime8, dateTime9, dateTime10, dateTime11;

            dateTime0 = model.dateTime[0];
            dateTime1 = model.dateTime[1];
            dateTime2 = model.dateTime[2];
            dateTime3 = model.dateTime[3];
            dateTime4 = model.dateTime[4];
            dateTime5 = model.dateTime[5];
            dateTime6 = model.dateTime[6];
            dateTime7 = model.dateTime[7];
            dateTime8 = model.dateTime[8];
            dateTime9 = model.dateTime[9];
            dateTime10 = model.dateTime[10];
            dateTime11 = model.dateTime[11];

            //Scary looking query to input empty strings into the flow sheets
            //fields if there isn't something in there already. 
            string query = $"INSERT IGNORE INTO fs_" + patient_mrn + " (date_time, " +
                "bp, pulse, temp, respirations, spo2, rr, quality, map, iv, " +
                "oral_intake, uop) VALUES (\"" + dateTime0 + "\", \"\", \"\", \"\", " +
                "\"\",  \"\",  \"\",  \"\",  \"\",  \"\",  \"\",  \"\"), " +
                "(\"" + dateTime1 + "\", \"\", \"\", \"\", \"\",  \"\",  \"\",  " +
                "\"\",  \"\",  \"\",  \"\",  \"\"), (\"" + dateTime2 + "\", \"\"," +
                " \"\", \"\", \"\",  \"\",  \"\",  \"\",  \"\",  \"\",  \"\",  \"\"), " +
                "(\"" + dateTime3 + "\", \"\", \"\", \"\", \"\",  \"\",  \"\",  " +
                "\"\",  \"\",  \"\",  \"\",  \"\"), (\"" + dateTime4 + "\", \"\", " +
                "\"\", \"\", \"\",  \"\",  \"\",  \"\",  \"\",  \"\",  \"\",  " +
                "\"\"), (\"" + dateTime5 + "\", \"\", \"\", \"\", \"\",  \"\",  " +
                "\"\",  \"\",  \"\",  \"\",  \"\",  \"\"), (\"" + dateTime6 + "\", " +
                "\"\", \"\", \"\", \"\",  \"\",  \"\",  \"\",  \"\",  \"\",  \"\",  " +
                "\"\"), (\"" + dateTime7 + "\", \"\", \"\", \"\", \"\",  \"\",  " +
                "\"\",  \"\",  \"\",  \"\",  \"\",  \"\"), (\"" + dateTime8 + "\", " +
                "\"\", \"\", \"\", \"\",  \"\",  \"\",  \"\",  \"\",  \"\",  " +
                "\"\",  \"\"), (\"" + dateTime9 + "\", \"\", \"\", \"\", \"\",  " +
                "\"\",  \"\",  \"\",  \"\",  \"\",  \"\",  \"\"), " +
                "(\"" + dateTime10 + "\", \"\", \"\", \"\", \"\",  \"\",  " +
                "\"\",  \"\",  \"\",  \"\",  \"\",  \"\"), " +
                "(\"" + dateTime11 + "\", \"\", \"\", \"\", \"\",  \"\",  " +
                "\"\",  \"\",  \"\",  \"\",  \"\",  \"\")";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        /*
         * GetFlowSheetsInputData - FlowSheetsModel
         * 
         * Grabs flow sheets data from the correct flow sheets table and saves
         * it back into the FlowSheets model, for display on the flow sheets
         * page. 
         * 
         */
        public FlowSheetsModel GetFlowSheetsInputData(string mrn, FlowSheetsModel flowSheetsModel)
        {
            connection.Open();

            //Save initial and final times of our window in flow sheets
            string initTime = flowSheetsModel.dateTime[0];
            string finalTime = flowSheetsModel.dateTime[11];
            string patient_mrn = mrn;

            //select all data from flow sheets table between initTime and 
            //finalTime
            string query = $"SELECT * FROM fs_" + patient_mrn +  " WHERE date_time " +
                "BETWEEN \"" + initTime + "\" AND \"" + finalTime + "\"";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            //store every row of data from the patients flow sheet table into string lists
            //DATE FORMAT -> MM:dd:yyyy_HH   (month:day:year_hour)
            while (dataReader.Read())
            {
                //column values correspond to database columns for flowsheets, in order
                if (!dataReader.IsDBNull(0)){
                    flowSheetsModel.dateTime.Add(dataReader.GetString(0));
                }
                if (!dataReader.IsDBNull(1)){
                    flowSheetsModel.bp.Add(dataReader.GetString(1));
                }
                if (!dataReader.IsDBNull(2))
                {
                    flowSheetsModel.pulse.Add(dataReader.GetString(2));
                }
                if (!dataReader.IsDBNull(3))
                {
                    flowSheetsModel.temp.Add(dataReader.GetString(3));
                }
                if (!dataReader.IsDBNull(4))
                {
                    flowSheetsModel.respirations.Add(dataReader.GetString(4));
                }
                if (!dataReader.IsDBNull(5))
                {
                    flowSheetsModel.spo2.Add(dataReader.GetString(5));
                }
                if (!dataReader.IsDBNull(6))
                {
                    flowSheetsModel.rr.Add(dataReader.GetString(6));
                }
                if (!dataReader.IsDBNull(7))
                {
                    flowSheetsModel.quality.Add(dataReader.GetString(7));
                }
                if (!dataReader.IsDBNull(8))
                {
                    flowSheetsModel.map.Add(dataReader.GetString(8));
                }
                if (!dataReader.IsDBNull(9))
                {
                    flowSheetsModel.iv.Add(dataReader.GetString(9));
                }
                if (!dataReader.IsDBNull(10))
                {
                    flowSheetsModel.oralIntake.Add(dataReader.GetString(10));
                }
                if (!dataReader.IsDBNull(11))
                {
                    flowSheetsModel.uop.Add(dataReader.GetString(11));
                }

            }

            connection.Close();

            return flowSheetsModel;
        }

        /*
         * PostFlowSheetsData - void
         * 
         * When the user clicks the save button on the Flow Sheets page, input
         * data is saved to the database here.
         * 
         */
        public void PostFlowSheetsData(FlowSheetsModel model, string mrn)
        {
            connection.Open();
            string patient_mrn = mrn;


            model.mrn = "fs_" + patient_mrn;
            string query = $"INSERT INTO {model.mrn} (date_time, bp, pulse, temp, respirations, spo2, rr, quality, map, iv, oral_intake, uop) VALUES ('{model.dateTime[0]}', '{model.bp[0]}', '{model.pulse[0]}', '{model.temp[0]}', '{model.respirations[0]}',  '{model.spo2[0]}',  '{model.rr[0]}',  '{model.quality[0]}',  '{model.map[0]}',  '{model.iv[0]}',  '{model.oralIntake[0]}',  '{model.uop[0]}'), ('{model.dateTime[1]}', '{model.bp[1]}', '{model.pulse[1]}', '{model.temp[1]}', '{model.respirations[1]}',  '{model.spo2[1]}',  '{model.rr[1]}',  '{model.quality[1]}',  '{model.map[1]}',  '{model.iv[1]}',  '{model.oralIntake[1]}',  '{model.uop[1]}'), ('{model.dateTime[2]}', '{model.bp[2]}', '{model.pulse[2]}', '{model.temp[2]}', '{model.respirations[2]}',  '{model.spo2[2]}',  '{model.rr[2]}',  '{model.quality[2]}',  '{model.map[2]}',  '{model.iv[2]}',  '{model.oralIntake[2]}',  '{model.uop[2]}'), ('{model.dateTime[3]}', '{model.bp[3]}', '{model.pulse[3]}', '{model.temp[3]}', '{model.respirations[3]}',  '{model.spo2[3]}',  '{model.rr[3]}',  '{model.quality[3]}',  '{model.map[3]}',  '{model.iv[3]}',  '{model.oralIntake[3]}',  '{model.uop[3]}'), ('{model.dateTime[4]}', '{model.bp[4]}', '{model.pulse[4]}', '{model.temp[4]}', '{model.respirations[4]}',  '{model.spo2[4]}',  '{model.rr[4]}',  '{model.quality[4]}',  '{model.map[4]}',  '{model.iv[4]}',  '{model.oralIntake[4]}',  '{model.uop[4]}'), ('{model.dateTime[5]}', '{model.bp[5]}', '{model.pulse[5]}', '{model.temp[5]}', '{model.respirations[5]}',  '{model.spo2[5]}',  '{model.rr[5]}',  '{model.quality[5]}',  '{model.map[5]}',  '{model.iv[5]}',  '{model.oralIntake[5]}',  '{model.uop[5]}'), ('{model.dateTime[6]}', '{model.bp[6]}', '{model.pulse[6]}', '{model.temp[6]}', '{model.respirations[6]}',  '{model.spo2[6]}',  '{model.rr[6]}',  '{model.quality[6]}',  '{model.map[6]}',  '{model.iv[6]}',  '{model.oralIntake[6]}',  '{model.uop[6]}'), ('{model.dateTime[7]}', '{model.bp[7]}', '{model.pulse[7]}', '{model.temp[7]}', '{model.respirations[7]}',  '{model.spo2[7]}',  '{model.rr[7]}',  '{model.quality[7]}',  '{model.map[7]}',  '{model.iv[7]}',  '{model.oralIntake[7]}',  '{model.uop[7]}'), ('{model.dateTime[8]}', '{model.bp[8]}', '{model.pulse[8]}', '{model.temp[8]}', '{model.respirations[8]}',  '{model.spo2[8]}',  '{model.rr[8]}',  '{model.quality[8]}',  '{model.map[8]}',  '{model.iv[8]}',  '{model.oralIntake[8]}',  '{model.uop[8]}'), ('{model.dateTime[9]}', '{model.bp[9]}', '{model.pulse[9]}', '{model.temp[9]}', '{model.respirations[9]}',  '{model.spo2[9]}',  '{model.rr[9]}',  '{model.quality[9]}',  '{model.map[9]}',  '{model.iv[9]}',  '{model.oralIntake[9]}',  '{model.uop[9]}'), ('{model.dateTime[10]}', '{model.bp[10]}', '{model.pulse[10]}', '{model.temp[10]}', '{model.respirations[10]}',  '{model.spo2[10]}',  '{model.rr[10]}',  '{model.quality[10]}',  '{model.map[10]}',  '{model.iv[10]}',  '{model.oralIntake[10]}',  '{model.uop[11]}'), ('{model.dateTime[11]}', '{model.bp[11]}', '{model.pulse[11]}', '{model.temp[11]}', '{model.respirations[11]}',  '{model.spo2[11]}',  '{model.rr[11]}',  '{model.quality[11]}',  '{model.map[11]}',  '{model.iv[11]}',  '{model.oralIntake[11]}',  '{model.uop[11]}') ON DUPLICATE KEY UPDATE date_time = VALUES(date_time), bp = VALUES(bp), pulse = VALUES(pulse), temp = VALUES(temp), respirations = VALUES(respirations), spo2 = VALUES(spo2), rr = VALUES(rr), quality = VALUES(quality), map = VALUES(map), iv = VALUES(iv), oral_intake = VALUES(oral_intake), uop = VALUES(uop)";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        /*
         * DeleteAllRows - void
         * 
         * Helper method that deletes the data of a specified table. Helpful for
         * debugging purposes. Can be done from command line MySql.
         * 
         */
        public void DeleteAllRows(string tableName)
        {
            string query = $"TRUNCATE TABLE {tableName}";
            connection.Open();
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();
        }

        /*
         * AddTableColumns - void
         * 
         * Helper method, adds columns to existing table. Can be done from
         * command line.
         * 
         */
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
