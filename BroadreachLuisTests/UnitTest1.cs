using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuisBot.LuisHelper;
using LuisBot.Data;
using LuisBot.Intents;

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

        [TestMethod]
        public void Format_Indicator()
        {
            //Create a test indicator
            IndicatorPerformance perf = new IndicatorPerformance();
            perf.AnnualTarget = "20";
            perf.Indicatorname = "Score";
            perf.IndicatorValue = "18";
            perf.TargetPercentage = 90;
            perf.YTDPercentage = 100;
            perf.YTDTarget = "18";

            //create formatter
            MessageFormater messageFormater = new MessageFormater();

            //format it
            string perf1 = messageFormater.IndicatorPerformance(perf, true);
            Assert.IsTrue(perf1.Length > 5);

        }
    }
}
