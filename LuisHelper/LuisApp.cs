using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot.LuisHelper
{
    /// <summary>
    /// Defines a single LUIS app. This will be used to contruct a LUIS Util 
    /// </summary>
    public class LuisApp
    {
        public string AppName;
        public string AppID;
        public string LuisApiKey;

        public LuisApp(string appName, string appID, string luisApiKey)
        {
            this.AppName = appName;
            this.AppID = appID;
            this.LuisApiKey = luisApiKey;
        }
    }
}