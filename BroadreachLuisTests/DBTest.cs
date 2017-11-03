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
        public void TestBasicListFromDB()
        {
            //try create a new query and get back a list of districts
            DataQuery query = new DataQuery();
            List<string> districts= query.GetListOfDistricts("");
            //check that the list has a couple of items
            Assert.IsTrue(districts.Count > 1);


        }
    }
}
