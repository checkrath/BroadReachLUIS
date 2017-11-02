using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace LuisBot
{
    [Serializable]
    public class BotManager
    {
        // Define all the classes for the JSON Object
        [Serializable]
        public class BotDescription
        {
            public Luisapplication[] LUISApplications { get; set; }
            public Conversation conversation { get; set; }
        }
        [Serializable]
        public class Conversation
        {
            public Maintopic mainTopic { get; set; }
        }
        [Serializable]
        public class Maintopic
        {
            public string greeting { get; set; }
            public Subconvelement[] subConvElements { get; set; }
        }
        [Serializable]
        public class Subconvelement
        {
            public string convName { get; set; }
            public string intent { get; set; }
            public string text { get; set; }
            public string familiarText { get; set; }
            public string ShortDescription { get; set; }
            public double boost { get; set; }
        }
        [Serializable]
        public class Luisapplication
        {
            public string name { get; set; }
            public string requestURI { get; set; }
            public string AppId { get; set; }
            public string AppKey { get; set; }
            public string[] intents { get; set; }
        }

        /// <summary>
        /// Description of the bot, in JSON
        /// </summary>
        private BotDescription _bot;
        private Subconvelement _currentConvElement;


        public BotManager(string BotConfigFile)
        {
            // Todo: Not sure what will happen here once this is in Azure?
            string fileLocation = System.Web.HttpContext.Current.Server.MapPath("/") + BotConfigFile;
            // Load the file
            TextReader tr = File.OpenText(fileLocation);
            string json = tr.ReadToEnd();
            // Load the JSON Object up
            _bot = new JavaScriptSerializer().Deserialize<BotDescription>(json);

            // Set that we are currently in the main conversation element
            _currentConvElement = null;
        }

        public async Task<List<LuisBot.LuisHelper.LuisFullResult>> ExecuteQuery(string query)
        {
            double topIntentScore = 0;
            LuisHelper.LuisIntent topIntent = null;
            Subconvelement topConvElement = null;

            List<LuisBot.LuisHelper.LuisFullResult> returnedLuisResponses = new List<LuisHelper.LuisFullResult>();
            // Run through each LUIS entity, and execute the query
            foreach (Luisapplication app in _bot.LUISApplications)
            {
                returnedLuisResponses.Add(await GetIndividualQuery(app, query));
            }

            // Now, we have the returned intents. Cycle through them, boost and then find the top intent
            foreach (LuisBot.LuisHelper.LuisFullResult result in returnedLuisResponses)
            {
                // Cycle each intent
                foreach (LuisHelper.LuisIntent intent in result.Intents)
                {
                    // Capture if this is the highest intent before adjustment?
                    
                    if (intent.Score > topIntentScore)
                    {
                        topIntentScore = intent.Score;
                        topIntent = intent;
                    }

                    // For each returned intent, we need to check if it is one of the current returned intents
                    if (_currentConvElement == null)
                    {
                        // Work with the main conversation element
                        foreach (Subconvelement convElement in _bot.conversation.mainTopic.subConvElements)
                        {
                            // Check if this element matches the intent returned, and boost if so
                            if (convElement.intent == intent.Name)
                            {
                                // Boost
                                // This means we had a returned intent that we were expecting
                                double thisIntentScore = intent.Score * convElement.boost;
                                if (thisIntentScore > topIntentScore)
                                {
                                    topIntentScore = thisIntentScore;
                                    topIntent = intent;
                                    topConvElement = convElement;
                                }
                            }
                        }
                    }
                    else
                    {
                        //// Work with the current conv element
                        //foreach (Subconvelement convElement in _currentConvElement.subConvElements)
                        //{

                        //}
                    }

                    // Update the boost amount
                }
            }

            return returnedLuisResponses;
        }

        /// <summary>
        /// Execute the query and return the result
        /// </summary>
        /// <param name="app"></param>
        /// <param name="userQuery"></param>
        /// <returns></returns>
        private async Task<LuisBot.LuisHelper.LuisFullResult> GetIndividualQuery(Luisapplication app, string userQuery)
        {
            // Escape the query
            string query = Uri.EscapeDataString(userQuery);

            using (HttpClient client = new HttpClient())
            {
                string RequestURI = app.requestURI.Replace("{AppId}", app.AppId).Replace("{AppKey}", app.AppKey).Replace("{Query}", query);
                HttpResponseMessage msg = await client.GetAsync(RequestURI);
                if (msg.IsSuccessStatusCode)
                {
                    string JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    // Serialize
                    LuisBot.LuisHelper.LuisFullResult returnedResult = new LuisHelper.LuisFullResult(JsonDataResponse);
                    return returnedResult;
                }
                else
                {
                    throw new Exception("Invalid respose from LUIS service");
                }
            }
        }
    
    }
}