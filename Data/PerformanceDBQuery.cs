using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace LuisBot.Data
{
    public class PerformanceDBQuery
    {
        private const string DB_CONN = "Server=tcp:broadreachpoc.database.windows.net,1433;Initial Catalog=Checkrath_BroadreachPOC" +
    ";Integrated Security=False;User Id=checkrath;Password=brIIM@gic.17;Encrypt=True;";

        public string GetProgramPerformanceAsString(string programName, bool verbose=false)
        {
            throw new NotImplementedException();
            List<IndicatorPerformance> indicatorList = GetProgramPerformance(programName);
            if (verbose)
            {

            }
        }

        public List<IndicatorPerformance> GetProgramPerformance(string programName)
        {
            List<IndicatorPerformance> indicatorList = new List<IndicatorPerformance>();
            using (SqlConnection connection = new SqlConnection(DB_CONN))
            using (SqlCommand cmd = new SqlCommand("select * from [dbo].[vw_ContractsPerformance]", connection))
            {
                //cmd.Parameters.AddWithValue("FirstName", firstName);
                connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            //Create perfromance object
                            IndicatorPerformance indicator = new IndicatorPerformance();
                            indicator.Indicatorname = reader.GetString(3);
                            indicator.IndicatorValue = reader.GetValue(4).ToString();
                            indicator.AnnualTarget = reader.GetValue(5).ToString();
                            indicator.YTDTarget = reader.GetValue(6).ToString();
                            indicator.TargetPercentage = (float)reader.GetDouble(7)*100;
                            indicator.YTDPercentage = (float)reader.GetDouble(8) * 100;
                            indicatorList.Add(indicator);
                        }
                }
            }

            //Return a list of districts
            return indicatorList;
        }
    }
}