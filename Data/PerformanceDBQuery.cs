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

        public string GetProgramPerformanceAsString(string programName, bool annual, bool verbose=false, string indicatorName = "", bool best = true, int n = 10)
        {
            List<IndicatorPerformance> indicatorList = GetProgramPerformance(programName,indicatorName,best,n,annual);
            string indicatorOutput = "";
            if (verbose)
            {
                //loop through all indicators
                for (int i = 0; i < indicatorList.Count; i++)
                {
                    IndicatorPerformance indicator= indicatorList[i];
                    //check if it's annual or YTD
                    string percentTarget;
                    if (annual)
                        percentTarget = $"{indicator.TargetPercentage:0.0}% of annual target";
                    else
                        percentTarget = $"{indicator.YTDPercentage:0.0}% of YTD target";
                    //Create string
                        if (indicatorName=="") //No indicators specified
                            indicatorOutput += $"**{indicator.Indicatorname}**: {indicator.IndicatorValue} ({percentTarget})\n\n";
                        else
                            indicatorOutput += $"{indicator.IndicatorValue} ({percentTarget})\n\n";
                }
            }
            else
            {
                //todo: loop though and make an average
                float averagePercent = 55.2F;
                //check if annual or YTD
                if (annual)
                    indicatorOutput = $"{averagePercent:0.0}% of annual target";
                else
                    indicatorOutput = $"{averagePercent:0.0}% of YTD target";
            }

            return indicatorOutput;
        }

        public List<IndicatorPerformance> GetProgramPerformance(string programName,  string indicatorName="", bool best=true,int n=10, bool sortByAnnual=true)
        {
            //create sql
            string sortField = (sortByAnnual) ? "[PerformanceAgainstTarget]" : "[PerformanceAgainstYTDTarget]";
            string ascDesc = (best) ? " desc " : " asc ";
            string sql = $"select top {n} * from [dbo].[vw_ContractsPerformance] " +
                 $"order by {sortField} {ascDesc}";

            //call DB
            List<IndicatorPerformance> indicatorList = new List<IndicatorPerformance>();
            using (SqlConnection connection = new SqlConnection(DB_CONN))
            using (SqlCommand cmd = new SqlCommand(sql, connection))
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
                            //only add if it's the right indicator or all indicators
                            if (indicatorName == "" || indicatorName ==indicator.Indicatorname)
                                indicatorList.Add(indicator);
                        }
                }
            }

            //Return a list of districts
            return indicatorList;
        }

        public List<IndicatorPerformance> GetDistrictPerformance(string districtName, string indicatorName = "", bool best = true, int n = 10, bool sortByAnnual = true)
        {
            string sortField = (sortByAnnual) ? "[PerformanceAgainstTarget]" : "[PerformanceAgainstYTDTarget]";
            string ascDesc = (best) ? " desc " : " asc ";
            string sql = $"SELECT top {n} [IndicatorName],[Value],[AnnualTarget],[YTDTarget],[PerformanceAgainstTarget]" +
                ",[PerformanceAgainstYTDTarget] " +
                "FROM[dbo].[vw_DistrictsPerformance] where DistrictName like @DistrictName " +
                $"order by {sortField} {ascDesc}";
            List<IndicatorPerformance> indicatorList = new List<IndicatorPerformance>();
            using (SqlConnection connection = new SqlConnection(DB_CONN))
            using (SqlCommand cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("DistrictName", districtName);
                connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            //Create perfromance object
                            IndicatorPerformance indicator = new IndicatorPerformance();
                            indicator.Indicatorname = reader.GetString(0);
                            indicator.IndicatorValue = reader.GetValue(1).ToString();
                            indicator.AnnualTarget = reader.GetValue(2).ToString();
                            indicator.YTDTarget = reader.GetValue(3).ToString();
                            indicator.TargetPercentage = (float)reader.GetDouble(4) * 100;
                            indicator.YTDPercentage = (float)reader.GetDouble(5) * 100;
                            //only add if it's the right indicator or all indicators
                            if (indicatorName == "" || indicatorName == indicator.Indicatorname)
                                indicatorList.Add(indicator);
                        }
                }
            }

            //Return a list of districts
            return indicatorList;
        }

        public string GetDistrictPerformanceAsString(string districtName, bool annual, bool verbose = true, string indicatorName = "", bool best = true, int n = 10)
        {
            List<IndicatorPerformance> indicatorList = GetProgramPerformance(districtName, indicatorName,best,n,annual);
            string indicatorOutput = "";
            if (verbose)
            {
                //loop through all indicators
                for (int i = 0; i < indicatorList.Count; i++)
                {
                    IndicatorPerformance indicator = indicatorList[i];
                    //check if it's annual or YTD
                    string percentTarget;
                    if (annual)
                        percentTarget = $"{indicator.TargetPercentage:0.0}% of annual target";
                    else
                        percentTarget = $"{indicator.YTDPercentage:0.0}% of YTD target";
                    //Create string
                    if (indicatorName == "") //No indicators specified
                        indicatorOutput += $"**{indicator.Indicatorname}**: {indicator.IndicatorValue:0.0} ({percentTarget})\n\n";
                    else
                        indicatorOutput += $"{indicator.IndicatorValue:0.0} ({percentTarget})\n\n";

                    //if (i == 0)
                    //    if (indicatorName == "") //No indicators specified
                    //        indicatorOutput += $"**{indicator.Indicatorname}**: {indicator.IndicatorValue:0.0} ({percentTarget})";
                    //    else
                    //        indicatorOutput += $"{indicator.IndicatorValue:0.0} ({percentTarget})";

                    //else if (i == indicatorList.Count - 1)
                    //    indicatorOutput += " and " + $"{indicator.Indicatorname}: {indicator.IndicatorValue} ({percentTarget})";
                    //else indicatorOutput += ", " + $"{indicator.Indicatorname}: {indicator.IndicatorValue} ({percentTarget})";
                }
            }
            else
            {
                //todo: loop though and make an average
                float averagePercent = 55.2F;
                //check if annual or YTD
                if (annual)
                    indicatorOutput = $"{averagePercent:0.0}% of annual target";
                else
                    indicatorOutput = $"{averagePercent:0.0}% of YTD target";
            }

            return indicatorOutput;
        }

        /// <summary>
        /// Return the best/worst n districts in a program
        /// </summary>
        /// <param name="programName"></param>
        /// <param name="annual"></param>
        /// <param name="best"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public List<string> GetBestWorstDistrict(string programName, bool annual, bool best,int n=1, string whichIndicator = "")
        {
            //create the query
            string valField= (annual)? "[aveTargetPerf]" : "[aveYTDPerf]";
            string ascDesc= (best) ? " desc " : " asc ";
            string sql = $"select top {n} [DistrictName],{valField} from [dbo].[vw_DistrictsPerformanceSummary] " +
                $"order by {valField} {ascDesc}" ;

            //Execute the query
            List<string> districtList = new List<string>();
            using (SqlConnection connection = new SqlConnection(DB_CONN))
            using (SqlCommand cmd = new SqlCommand(sql, connection))
            {
                //cmd.Parameters.AddWithValue("DistrictName", districtName);
                connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            //create the output string
                            string districtText = reader.GetString(0) + ": " +
                                ((float)reader.GetDouble(1) * 100).ToString("0.00") + "%" ;
                            districtList.Add(districtText);
                        }
                }
            }

            //Return a list of districts
            return districtList;

        }

        /// <summary>
        /// Returns the best worst districts as a string
        /// </summary>
        /// <param name="programName">As above!</param>
        /// <param name="annual">As above!</param>
        /// <param name="best">As above!</param>
        /// <param name="n">As above!</param>
        /// <returns></returns>
        public string GetBestWorstDistrictAsString(string programName, bool annual, bool best, int n = 1, string whichIndicator = "")
        {
            List<string> districts = GetBestWorstDistrict(programName, annual, best, n);
            string returnString = "";
            foreach (string district in districts)
                returnString += (returnString == "" ? "" : ", ") + district;

            return returnString;
        }

        public List<string> GetBestWorstFacility(string district, bool annual, bool best, int n = 1)
        {
            throw new NotImplementedException(); //todo: fix below
            //create the query
            string valField = (annual) ? "[aveTargetPerf]" : "[aveYTDPerf]";
            string ascDesc = (best) ? " desc " : " asc ";
            string sql = $"select top {n} [DistrictName],{valField} from [dbo].[vw_DistrictsPerformanceSummary] " +
                $"order by {valField} {ascDesc}";

            //Execute the query
            List<string> facilityList = new List<string>();
            using (SqlConnection connection = new SqlConnection(DB_CONN))
            using (SqlCommand cmd = new SqlCommand(sql, connection))
            {
                //cmd.Parameters.AddWithValue("DistrictName", districtName);
                connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            //create the output string
                            string facilityText = reader.GetString(0) + ": " +
                                ((float)reader.GetDouble(1) * 100).ToString("0.00") + "%";
                            facilityList.Add(facilityText);
                        }
                }
            }

            //Return a list of districts
            return facilityList;

        }

        

    }
}