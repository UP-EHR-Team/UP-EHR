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

        private MySqlDataReader DatabaseOpen(string query)
        {
            connection.Open();
            MySqlCommand cmd = new MySqlCommand(query, connection);
            return cmd.ExecuteReader();
        }

        public SummaryViewModel GetSummaryPatient()
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
    }
}
