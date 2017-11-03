using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace LuisBot.Data
{
    
    public class DataQuery
    {
        private const string DB_CONN= "Server=tcp:broadreachpoc.database.windows.net,1433;Initial Catalog=Checkrath_BroadreachPOC" +
            ";Integrated Security=False;User Id=checkrath;Password=brIIM@gic.17;Encrypt=True;";
        /// <summary>
        /// Return a list of districts from the DB
        /// </summary>
        /// <param name="programName"></param>
        public List<string> GetListOfDistricts(string programName)
        {
            List<string> districtList=new List<string>();
            using (SqlConnection connection = new SqlConnection(DB_CONN))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT distinct [DistrictName] FROM[dbo].[vw_DistrictsPerformance]", connection))
                {
                    //cmd.Parameters.AddWithValue("FirstName", firstName);
                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        int x=reader.FieldCount;
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                districtList.Add(reader.GetString(0));
                               
                            }
                        }
                    }
                }
            }

            //Return a list of districts
            return districtList;           

        }
    }
}