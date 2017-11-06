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

            //test it with the YTD flag
            indicatorList = query.GetProgramPerformanceAsString("", false, true);
            //check that it has a couple of items
            Assert.IsTrue(indicatorList.Length > 5);
        }

        [TestMethod]
        public void DB_Get_ListOf_Programs()
        {

        }
    }
}
