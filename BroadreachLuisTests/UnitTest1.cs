using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuisBot.LuisHelper;

namespace BroadReachLuisTest
{
    [TestClass]
    public class HappinesstrackerTest
    {
        [TestMethod]
        public void BasicHappinessTrackerTest()
        {
            HappinessTracker tracker = new HappinessTracker();
            //add 2 ratings and check
            tracker.AddRating(5);
            tracker.AddRating(1);
            Assert.AreEqual(tracker.AverageRating, 3);
            //add more ratings and check
            for (int i = 0; i < 10; i++)
                tracker.AddRating(-5);
            Assert.IsTrue(tracker.AverageRating < 0);
        }
    }
}
