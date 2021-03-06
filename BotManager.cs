﻿using LuisBot.LuisHelper;
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
    #region Attributes
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
    #endregion

    [Serializable]
    public class BotManager
    {
        #region JSON Classes for Config File

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
            public string topLevelHelp { get; set; }
            public string alwaysThereHelp { get; set; }
            public Subconvelement[] subConvElements { get; set; }
            public MemoryParameter[] memoryParameters { get; set; }
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
            public bool dontUseAsIntentHome { get; set; }
            public bool excludeFromHelpText { get; set; }
            public string[] alternativeText { get; set; }

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

        [Serializable]
        public class MemoryParameter
        {
            public string name { get; set; }
            public string defaultValue { get; set; }
            public int minutesToPersist { get; set; }
        }

        #endregion

        #region Auditing Conversation
        [Serializable]
        public class ConversationAuditElement
        {

            public string query { get; set; }
            public string stringResponse { get; set; }
            public string userName { get; set; }
            public string userId { get; set; }
            //public List<LuisBot.LuisHelper.LuisFullResult> luisFullResults;
            public string luisFullResults;
            public List<string> progressList;
            public ConversationAuditElement()
            {
                progressList = new List<string>();
                //luisFullResults = new List<LuisFullResult>();
                luisFullResults = "";
            }
        }

        #endregion

        private BotMemory _memory;
        /// <summary>
        /// Description of the bot, in JSON
        /// </summary>
        private BotDescription _bot;
        private Subconvelement _currentConvElement;
        private object _callingObject;
        // Use events for this later
        private string _noneEventMethodName;

        // User details
        private string _userId;
        private string _userName;

        public string Username { get { return _userName; } }
        public string UserID { get { return _userId; } }
        private List<ConversationAuditElement> _conversationAudit;


        public BotManager(string BotConfigFile, object callingObject, string noneEventMethodName)
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

            // None event
            _noneEventMethodName = noneEventMethodName;
            _conversationAudit = new List<ConversationAuditElement>();

            // Initialize Memory
            _memory = new BotMemory();

            // Load the parameters
            foreach (MemoryParameter memParam in _bot.conversation.mainTopic.memoryParameters)
                _memory.CreateField(memParam.name, (memParam.defaultValue == null ? "" : memParam.defaultValue));

        }

        #region Memory Area
        public BotMemory Memory
        {
            get { return _memory; }
        }
        #endregion

        public async void ExecuteQuery(string query, IDialogContext context, IAwaitable<IMessageActivity> message)
        {
            try
            {

                // Audit
                ConversationAuditElement audit = new ConversationAuditElement();
                // Add to the list in case of failure
                _conversationAudit.Add(audit);
                // Add the query
                audit.query = query;
                audit.progressList.Add($"Entered Execute method; query is {query}");

                //Get the username
                if (context.Activity.From.Id != null)
                {
                    _userId = context.Activity.From.Id;
                    _userName = context.Activity.From.Name;
                }
                else
                {
                    _userId = "Anonymous";
                    _userName = "Unknown User";
                }
                // Add username and id
                audit.userName = _userName;
                audit.userId = _userId;
                audit.progressList.Add($"Loaded username of {_userName} and ID {_userId}");


                double topIntentScore = 0;
                LuisHelper.LuisIntent topIntent = null;
                Subconvelement topConvElement = null;
                LuisFullResult topLuisResult = null;

                List<LuisBot.LuisHelper.LuisFullResult> returnedLuisResponses = new List<LuisHelper.LuisFullResult>();

                // Run through each LUIS entity, and execute the query
                foreach (Luisapplication app in _bot.LUISApplications)
                    returnedLuisResponses.Add(await GetIndividualQuery(app, query));

                // Audit
                //audit.luisFullResults = returnedLuisResponses;
                audit.progressList.Add($"Returned Luis Intents; {returnedLuisResponses.Count} responses. The first one has {returnedLuisResponses[0].Entities} entities and {returnedLuisResponses[0].Intents} intents");
                // Now, we have the returned intents. Cycle through them, boost and then find the top intent
                foreach (LuisBot.LuisHelper.LuisFullResult result in returnedLuisResponses)
                {
                    // Cycle each intent
                    foreach (LuisHelper.LuisIntent intent in result.Intents)
                    {
                        audit.progressList.Add($"Intent {intent.Name} with pre boosted score {intent.Score}");

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
                                    if (thisIntentScore >= topIntentScore)
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
                                    if (thisIntentScore >= topIntentScore)
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
                int x = 0 / 2;
                // Here we have the conv element that is highest, and the intent that is highest.
                if (topConvElement != null)
                {
                    audit.progressList.Add($"Conversation Element {topConvElement.convName} found as highest score");
                    // Pick out the name and look for a method with this name
                    string attributeName = topConvElement.convName;
                    // Add to the conversation flow
                    _currentConvElement = topConvElement;
                    string result = await TryExecuteMethodWithAttributeName(typeof(ConvElement), attributeName, _callingObject, context, message, topLuisResult, topConvElement, topIntent);

                    // Is it correct?
                    if (result != null)
                    {
                        audit.progressList.Add($"Conversation Element {topConvElement.convName} executed; result is {result}");
                        await context.PostAsync(result);
                        return;
                    }
                }

                if (topIntent != null)
                {
                    // Here, we search for the generic Intent method and do the same thing
                    // Pick out the name and look for a method with this name
                    string intentName = topIntent.Name;
                    // Only populate the intent if we didn't find a conv element
                    audit.progressList.Add($"Top Intent found with name {intentName}");
                    string result = await TryExecuteMethodWithAttributeName(typeof(IntentAttribute), intentName, _callingObject, context, message, topLuisResult, null, topIntent);
                    // Is it correct?
                    if (result != null)
                    {
                        audit.progressList.Add($"Found a method for intent {intentName}");
                        // Set the conversation element if we can
                        if (topConvElement == null)
                        {
                            var topLevelConvElement = GetConversationElementFromIntent(topIntent);
                            if (topLevelConvElement != null)
                                _currentConvElement = topLevelConvElement;
                        }
                        await context.PostAsync(result);
                        return;
                    }
                    else
                    {
                        audit.progressList.Add($"No method found for {intentName}");
                        // Here, we had no attribute or intent method. But we could have found a conv element?
                        if (topConvElement != null)
                        {
                            // Return the text and description
                            if (topConvElement.text != null)
                            {
                                await context.PostAsync(topConvElement.text);
                                return;
                            }
                            else
                                throw new Exception($"Found conversation element, but it had no execute or intent method and no text description");
                        }
                    }
                }




                // At this point, call the "None" response
                if (!String.IsNullOrEmpty(_noneEventMethodName))
                {
                    // private async Task NoneIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult result)
                    object[] paramList = new object[] { context, message, topLuisResult };
                    // Use reflection to find the methods with this attribute?
                    var methods = _callingObject.GetType().GetMethods();
                    foreach (var method in methods)
                    {
                        if (method.Name.ToLower() == _noneEventMethodName.ToLower())
                        {
                            Task tReturn = (Task)method.Invoke(_callingObject, paramList);
                        }
                    }
                }
                else
                {
                    // Return the generic strings?
                    throw new Exception("No conversation element, intent or none event found to deal with the query");
                }

            }
            catch (Exception eConvBusted)
            {
                string convRecord = "";
                foreach (string record in _conversationAudit.Last().progressList)
                    convRecord += record + "\r\n";
                Exception e = new Exception("Error occurred in ExecuteQuery", eConvBusted);

                throw e;
            }
            //return returnedLuisResponses;
        }

        #region Accessors
        /// <summary>
        /// Returns the greeting for the bot
        /// </summary>
        /// <returns></returns>
        public string GetGreetingMessage()
        {
            return _bot.conversation.mainTopic.greeting;
        }

        /// <summary>
        /// Returns a short, context-specific description of what you can do at any point
        /// </summary>
        /// <returns></returns>
        public string GetCurrentHelpString()
        {
            string helpText = "";
            Subconvelement[] elementsToIterate = _bot.conversation.mainTopic.subConvElements;

            if ((_currentConvElement != null) && (_currentConvElement.subConvElements != null))
                elementsToIterate = _currentConvElement.subConvElements;
            else
            {
                // Add top level help if needed
                if (!String.IsNullOrEmpty(_bot.conversation.mainTopic.topLevelHelp))
                    helpText += _bot.conversation.mainTopic.topLevelHelp;
            }
            helpText += "\n\n";
            if (!String.IsNullOrEmpty(_bot.conversation.mainTopic.alwaysThereHelp))
                helpText += _bot.conversation.mainTopic.alwaysThereHelp;

            string specificHelpText = "";
            foreach (Subconvelement subConv in elementsToIterate)
            {
                if ((!subConv.excludeFromHelpText) && (subConv.ShortDescription != null))
                    specificHelpText += (specificHelpText == "" ? "" : ", ") + subConv.ShortDescription;
            }

            return helpText + "\n\nFrom here, I can also: " + specificHelpText;

        }

        #endregion

        #region Debugging and Logging
        /// <summary>
        /// Generalised log function; calls logging on file system for now, to be moved to DB Later.
        /// </summary>
        /// <param name="logString"></param>
        public async void Log(string logString)
        {
            throw new NotImplementedException();
        }
        #endregion

        /// <summary>
        /// Sets the current conversation element if necessary. Returns null if not found at the top level
        /// </summary>
        /// <param name="intent">The intent we are trying to match</param>
        /// <returns></returns>
        private Subconvelement GetConversationElementFromIntent(LuisIntent intent)
        {
            // Run through each top-level intent and see if it is listed in the conversation
            foreach (var conv in _bot.conversation.mainTopic.subConvElements)
            {
                if ((conv.intent.ToLower() == intent.Name.ToLower()) && (!conv.dontUseAsIntentHome))
                    return conv;
            }
            return null;
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
#if DEBUG
                RequestURI += "&staging=true";
#endif
                //HttpResponseMessage msg = await client.GetAsync(RequestURI);
                Task<HttpResponseMessage> t = client.GetAsync(RequestURI);
                t.Wait();
                HttpResponseMessage msg = t.Result;

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


        private async Task<string> TryExecuteMethodWithAttributeName(Type type, string attributeName, object callingObject, IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult luisResult, Subconvelement convElement, LuisHelper.LuisIntent intent)
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
                            paramList = new object[] { context, message, luisResult, convElement, intent };
                        else
                            paramList = new object[] { context, message, luisResult, intent };

                        Task<string> tReturn = (Task<string>)method.Invoke(_callingObject, paramList);

                        await tReturn;
                        return tReturn.Result;
                    }
                }
            }

            return null;
        }

    }
}