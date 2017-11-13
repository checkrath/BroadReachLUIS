using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace LuisBot.Data
{
    
    public class EntitiesDBQuery
    {
        private const string DB_CONN= "Server=tcp:broadreachpoc.database.windows.net,1433;Initial Catalog=Checkrath_BroadreachPOC" +
            ";Integrated Security=False;User Id=checkrath;Password=brIIM@gic.17;Encrypt=True;";
        
        /// <summary>
        /// Return the list of districts as a delimited string
        /// </summary>
        /// <param name="programName"></param>
        /// <returns></returns>
        public string GetListOfDistrictsAsString(string programName)
        {
            List<string> districts = GetListOfDistricts(programName);
            //build string
            string districtsAsString = "";
            for (int i = 0; i < districts.Count; i++)
            {
                if (i == 0)
                    districtsAsString += districts[0];
                else if (i == districts.Count- 1)
                    districtsAsString += " and " + districts[i];
                else districtsAsString += ", " + districts[i];
            }

            return districtsAsString;
        }

        /// <summary>
        /// Return a list of districts from the DB
        /// </summary>
        /// <param name="programName"></param>
        public List<string> GetListOfDistricts(string programName)
        {
            List<string> districtList=new List<string>();
            using (SqlConnection connection = new SqlConnection(DB_CONN))
            using (SqlCommand cmd = new SqlCommand("SELECT distinct [DistrictName] FROM[dbo].[vw_DistrictsPerformance]", connection))
            {
                //cmd.Parameters.AddWithValue("FirstName", firstName);
                connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            districtList.Add(reader.GetString(0));
                        }
                }
            }
            
            //Return a list of districts
            return districtList;           
        }

        public List<string> GetListOfPrograms()
        {
            List<string> programList = new List<string>();
            using (SqlConnection connection = new SqlConnection(DB_CONN))
            using (SqlCommand cmd = new SqlCommand("select * from [dbo].[Contracts]", connection))
            {
                //cmd.Parameters.AddWithValue("FirstName", firstName);
                connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            programList.Add(reader.GetString(2));
                        }
                }
            }

            //Return a list of districts
            return programList;

        }

        public string GetListOfProgramsAsString()
        {
            List<string> programs = GetListOfPrograms();
            //build string
            string programsAsString = "";
            for (int i = 0; i < programs.Count; i++)
            {
                if (i == 0)
                    programsAsString += programs[0];
                else if (i == programs.Count - 1)
                    programsAsString += " and " + programs[i];
                else programsAsString += ", " + programs[i];
            }

            return programsAsString;
        }

        public List<string> GetListOfFacilities(string districtName)
        {
            List<string> facilityList = new List<string>();
            using (SqlConnection connection = new SqlConnection(DB_CONN))
            using (SqlCommand cmd = new SqlCommand("select [FacilityName] from [dbo].[vw_FacilityDistricts] where  [DistrictName] like @DistrictName", connection))
            {
                cmd.Parameters.AddWithValue("DistrictName", districtName);
                connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            facilityList.Add(reader.GetString(0));
                        }
                }
            }

            //Return a list of districts
            return facilityList;

        }

        public string GetListOfFacilitiesAsString(string districtName)
        {
            List<string> facilities = GetListOfFacilities(districtName);
            //build string
            string facilitiesAsString = "";
            for (int i = 0; i < facilities.Count; i++)
            {
                if (i == 0)
                    facilitiesAsString += facilities[0];
                else if (i == facilities.Count - 1)
                    facilitiesAsString += " and " + facilities[i];
                else facilitiesAsString += ", " + facilities[i];
            }

            return facilitiesAsString;
        }

        public UserInfo GetUserInfo(string userID)
        {
            //get the user from the DB
            UserInfo userInfo = new UserInfo();
            using (SqlConnection connection = new SqlConnection(DB_CONN))
            using (SqlCommand cmd = new SqlCommand("SELECT [UserRefID],[ProgrammeName],[DistrictName]  FROM [dbo].[vw_User] where UserRefID like @UserRefID", connection))
            {
                cmd.Parameters.AddWithValue("UserRefID", userID);
                connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        //get program and facility
                        userInfo.DefaultProgram = (reader.IsDBNull(1)) ? "": reader.GetString(1);
                        userInfo.DefaultFacility = (reader.IsDBNull(2)) ? "" : reader.GetString(2);
                    }
                    else
                    {
                        //if no user then default to all programs and all districts
                        userInfo.DefaultFacility = "";
                        userInfo.DefaultProgram = "";
                    }
                }
            }

            return userInfo;

        }

    }
}