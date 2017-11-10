using LuisBot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot.Intents
{
    public class MessageFormater
    {
        public string IndicatorPerformance(IndicatorPerformance indicator, bool annual)
        {
            //check if it's annual or YTD
            string percentTarget;
            if (annual)
                percentTarget = $"{indicator.TargetPercentage:0.0}% of annual target";
            else
                percentTarget = $"{indicator.YTDPercentage:0.0}% of YTD target";

            return $"{indicator.IndicatorValue} ({percentTarget})";
        }
    }
}