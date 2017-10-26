using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Cognitive.LUIS;
using System.Threading.Tasks;

namespace LuisBot.LuisHelper
{
    /// <summary>
    /// Call one or more LUIS calls and get back a collection of LUIS results
    /// </summary>
    public class LuisUtil
    {
        LuisApp[] LuisApps;

        /// <summary>
        /// constructor takes list of luis apps to call. 
        /// Order of apps determines order to call i.e. each one is a series of nets to be called in order
        /// </summary>
        /// <param name="luisApps"></param>
        public LuisUtil(LuisApp[] luisApps)
        {
            this.LuisApps = luisApps;
        }

        //call passes message text from user and asyncronously collects the results. todo: async this call
        public async Task<LuisResult> CallSequentially(string userText)
        {
            //call each of the services asyncronously
            LuisClient client = new LuisClient(this.LuisApps[0].AppID , this.LuisApps[0].LuisApiKey, true);
            LuisResult res = await client.Predict(userText);

            //return new Task<LuisResult>()            

            throw new NotImplementedException();


            //convert each of the results into an object from the Json

            //Put all the collections into the LuisResults class and return
        }

        
    }
}