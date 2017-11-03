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
        public void TestDistrictListFromDB()
        {
            //try create a new query and get back a list of districts
            DataQuery query = new DataQuery();
            List<string> districts= query.GetListOfDistricts("");
            //check that the list has a couple of items
            Assert.IsTrue(districts.Count > 1);          

        }

        [TestMethod]
        public void Test_Get_District_String_From_DB()
        {
            //try create a new query and get back a list of districts
            DataQuery query = new DataQuery();
            string districts = query.GetListOfDistrictsAsString("");
            //check that the list has a couple of items
            Assert.IsTrue(districts.Length > 5);
        }
    }
}
