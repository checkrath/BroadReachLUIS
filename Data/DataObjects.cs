using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot.Data
{
    public class IndicatorPerformance
    {
        public string Indicatorname;
        public string IndicatorValue;
        public string YTDTarget;
        public string AnnualTarget;
        public float TargetPercentage;
        public float YTDPercentage;
    }


    public class UserInfo
    {
        public string DefaultProgram="";
        public string DefaultFacility="";
    }
}