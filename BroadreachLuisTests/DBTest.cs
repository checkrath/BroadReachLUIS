using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuisBot.Data;
using System.Collections.Generic;


namespace BroadreachLuisTests
{
    [TestClass]
    public class DBTest
    {
        [TestMethod]
        public void DB_DistrictList()
        {
            //try create a new query and get back a list of districts
            EntitiesDBQuery query = new EntitiesDBQuery();
            List<string> districts = query.GetListOfDistricts("");
            //check that the list has a couple of items
            Assert.IsTrue(districts.Count > 1);

        }

        [TestMethod]
        public void DB_Get_District_String()
        {
            //try create a new query and get back a list of districts
            EntitiesDBQuery query = new EntitiesDBQuery();
            string districts = query.GetListOfDistrictsAsString("");
            //check that the list has a couple of items
            Assert.IsTrue(districts.Length > 5);
        }

        [TestMethod]
        public void DB_Get_Indicator_perfromance_for_program()
        {
            //Get the perf from the DB
            PerformanceDBQuery query = new PerformanceDBQuery();
            List<IndicatorPerformance> list = query.GetProgramPerformance("");
            //check that it has a couple of items
            Assert.IsTrue(list.Count > 1);

            //check that it has a percentage for YTD
            Assert.IsTrue(list[0].TargetPercentage > 0);
        }

        [TestMethod]
        public void DB_Get_Indicator_string_perfromance_for_program()
        {
            //Get the perf from the DB
            PerformanceDBQuery query = new PerformanceDBQuery();
            string indicatorList = query.GetProgramPerformanceAsString("",true, true);
            //check that it has a couple of items
            Assert.IsTrue(indicatorList.Length > 5);

            //test it with the verbose flag off
            indicatorList = query.GetProgramPerformanceAsString("", true, false);
            //check that it has a couple of items
            Assert.IsTrue(indicatorList.Length > 5);

            //test it with the YTD flag and a specific indicator
            indicatorList = query.GetProgramPerformanceAsString("", false, true, "TX_NEW");
            //check that it has a couple of items
            Assert.IsTrue(indicatorList.Length > 3);
        }

        [TestMethod]
        public void DB_Get_ListOf_Programs()
        {
            EntitiesDBQuery query = new EntitiesDBQuery();

            List<string> programs = query.GetListOfPrograms();

            Assert.IsTrue(programs.Count > 1);

            string programString = query.GetListOfProgramsAsString();

            Assert.IsTrue(programString.Length > 5);
        }

        


        [TestMethod]
        public void DB_Get_ListOf_Facilities()
        {
            EntitiesDBQuery query = new EntitiesDBQuery();

            List<string> districts = query.GetListOfFacilities("Ugu");

            Assert.IsTrue(districts.Count > 1);
        }

        [TestMethod]
        public void DB_Get_List_Facilities_String()
        {
            EntitiesDBQuery query = new EntitiesDBQuery();

            string facilities = query.GetListOfFacilitiesAsString("Ugu");

            Assert.IsTrue(facilities.Length > 5);
        }

        [TestMethod]
        public void DB_Performance_Of_District()
        {
            //Get as a list
            PerformanceDBQuery query = new PerformanceDBQuery();
            List<IndicatorPerformance> list = query.GetDistrictPerformance("Ugu");

            Assert.IsTrue(list.Count > 2);

            //get the annula performance of the Alfred Nzo district
            string perf = query.GetDistrictPerformanceAsString("Alfred Nzo",true);
            Assert.IsTrue(perf.Length > 20);

            //Check the YTD for a specific indicator "TX_NEW"
            perf = query.GetDistrictPerformanceAsString("Alfred Nzo", true,indicatorName: "TX_NEW");
            Assert.IsTrue(perf.Length > 5);






        }
    }
}
