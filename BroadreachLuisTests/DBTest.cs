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

            //check if i can get the districts as a string
            string districtString = query.GetListOfDistrictsAsString("");
            //check that the list has a couple of items
            Assert.IsTrue(districtString.Length > 5);

        }

        [TestMethod]
        public void DB_GetUser()
        {
            //get a user from the DB
            EntitiesDBQuery query = new EntitiesDBQuery();
            UserInfo userInfo;
            //get a known user
            userInfo = query.GetUserInfo("Test",new UserInfo { DefaultProgram = "All Programs" });
            Assert.IsTrue(userInfo.DefaultProgram!= "All Programs");
            Assert.IsTrue(userInfo.DefaultProgram.Length>1);
            //get an unknown user
            userInfo = query.GetUserInfo("Blah", new UserInfo { DefaultProgram = "All Programs" });
            Assert.IsTrue(userInfo.DefaultProgram== "All Programs");

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

            //Get the perf from the DB as a string
            string indicatorList = query.GetProgramPerformanceAsString("", true, true);
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

            List<string> facilityList = query.GetListOfFacilities("Ugu");

            Assert.IsTrue(facilityList.Count > 1);

            //try get the list as a string
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
            string perf = query.GetDistrictPerformanceAsString("Alfred Nzo", true);
            Assert.IsTrue(perf.Length > 20);

            //Check the YTD for a specific indicator "TX_NEW"
            perf = query.GetDistrictPerformanceAsString("Alfred Nzo", true, indicatorName: "TX_NEW");
            Assert.IsTrue(perf.Length > 5);

        }

        [TestMethod]
        public void DB_BestWorst_District()
        {
            //Get as a list
            PerformanceDBQuery query = new PerformanceDBQuery();
            //get best district (annual average)
            List<string> list = query.GetBestWorstDistrict("", true, true);
            Assert.IsTrue(list.Count == 1);

            //get worst 2 districts (YTD average)
            list = query.GetBestWorstDistrict("", false, false, 2);
            Assert.IsTrue(list.Count == 2);

            //Get the 2 best performing districts for a specified indicator
            list = query.GetBestWorstDistrict("", true, true, 2, "HTS_TST");
            Assert.IsTrue(list.Count == 2);

            //Get the 2 worst YTD performing districts for a specified indicator
            list = query.GetBestWorstDistrict("", false, false, 3, "HTS_TST");
            Assert.IsTrue(list.Count == 3);
        }

        [TestMethod]
        public void DB_BestWorst_Indicator()
        {
            //Get the top performing indicators from the DB
            PerformanceDBQuery query = new PerformanceDBQuery();
            List<IndicatorPerformance> list = query.GetProgramPerformance("","",true,2,true);
            //check that it has a couple of items
            Assert.IsTrue(list.Count == 2);

            //Get worst perfoming YTD indicators
            string indicatorList = query.GetProgramPerformanceAsString("", false, true,"",false,2);
            //check that it has a couple of items
            Assert.IsTrue(indicatorList.Length > 5);

            //get the top indicator for ugu
            List<IndicatorPerformance> districtList = query.GetDistrictPerformance("Ugu",best:true,n:1);
            Assert.IsTrue(districtList.Count ==1);

            //get the worst 2 YTD indicators of the Alfred Nzo district
            string perf = query.GetDistrictPerformanceAsString("Alfred Nzo", true,best:false,n:2);
            Assert.IsTrue(perf.Length > 10);
        }
    }
}
