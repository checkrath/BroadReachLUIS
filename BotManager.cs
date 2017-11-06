using LuisBot.LuisHelper;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
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
    public class ConvElement : Attribute
    {
        string _convElementName;
        public ConvElement(string name)
        {
            _convElementName = name;
        }
        public string Value { get { return _convElementName; } }
    }
    [Serializable]
    public class IntentAttribute : Attribute
    {
        string _intentAttrName;
        public IntentAttribute(string name)
        {
            _intentAttrName = name;
        }
        public string Value { get { return _intentAttrName; } }
    }
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
            public Subconvelement[] subConvElements { get; set; }
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
        private object _callingObject;


        public BotManager(string BotConfigFile, object callingObject)
        {
            // Todo: Not sure what will happen here once this is in Azure?
            string fileLocation = System.Web.HttpContext.Current.Server.MapPath("/") + BotConfigFile;
            // Load the file
            TextReader tr = File.OpenText(fileLocation);
            string json = tr.ReadToEnd();
            // Load the JSON Object up
            _bot = new JavaScriptSerializer().Deserialize<BotDescription>(json);

            // Set the calling type
            _callingObject = callingObject;

            // Set that we are currently in the main conversation element
            _currentConvElement = null;
        }

        public async Task<List<LuisBot.LuisHelper.LuisFullResult>> ExecuteQuery(string query, IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            double topIntentScore = 0;
            LuisHelper.LuisIntent topIntent = null;
            Subconvelement topConvElement = null;
            LuisFullResult topLuisResult = null;

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
                        topLuisResult = result;
                    }

                    // For each returned intent, we need to check if it is one of the current returned intents
                    if ((_currentConvElement == null) || (_currentConvElement.subConvElements == null))
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
                                    topLuisResult = result;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Work with the main conversation element
                        foreach (Subconvelement convElement in _currentConvElement.subConvElements)
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
                                    topLuisResult = result;
                                }
                            }
                        }
                    }
                }
            }

            // Here we have the conv element that is highest, and the intent that is highest.
            if (topConvElement != null)
            {
                // Pick out the name and look for a method with this name
                string attributeName = topConvElement.convName;
                string result = await TryExecuteMethodWithAttributeName(typeof(ConvElement), attributeName, _callingObject, topLuisResult, topConvElement, topIntent);
                // Is it correct?
                if (result != null)
                {
                    await context.PostAsync(result);
                    return returnedLuisResponses;
                }

            }
            // Fall through; either no conv element (generic intent) or the method doesn't work
            if (topIntent != null)
            {
                // Here, we search for the generic Intent method and do the same thing
                // Pick out the name and look for a method with this name
                string intentName = topIntent.Name;
                string result = await TryExecuteMethodWithAttributeName(typeof(IntentAttribute), intentName, _callingObject, topLuisResult, null, topIntent);
                // Is it correct?
                if (result != null)
                {
                    await context.PostAsync(result);
                    return returnedLuisResponses;
                }
            }

            // At this point, call the "None" response


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

        /// <summary>
        /// Returns the greeting for the bot
        /// </summary>
        /// <returns></returns>
        public string GetGreetingMessage()
        {
            return _bot.conversation.mainTopic.greeting;
        }

        private async Task<string> TryExecuteMethodWithAttributeName(Type type, string attributeName, object callingObject,LuisFullResult luisResult, Subconvelement convElement, LuisHelper.LuisIntent intent)
        {
            // Use reflection to find the methods with this attribute?
            var methods = callingObject.GetType().GetMethods();
            foreach (var method in methods)
            {
                string methodName = method.Name;
                var attrList = method.GetCustomAttributes(type, false);
                if (attrList.Length != 0)
                {
                    // More hardcoding for now
                    string attrValue;
                    if (type == typeof(ConvElement))
                        attrValue = ((ConvElement)attrList[0]).Value;
                    else
                        attrValue = ((IntentAttribute)attrList[0]).Value;

                    if (attrValue == attributeName)
                    {
                        // Invoke Method
                        // Hardcode for now. Todo - change later
                        object[] paramList = null;

                        if (type == typeof(ConvElement))
                            paramList = new object[] { luisResult, convElement, intent };
                        else
                            paramList = new object[] { luisResult, intent };
                        Task<string> tReturn = (Task<string>)method.Invoke(_callingObject, paramList);

                        tReturn.Wait();
                        return tReturn.Result;
                    }
                }
            }

            return null;
        }

    }
}