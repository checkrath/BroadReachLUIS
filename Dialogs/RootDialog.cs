using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Cognitive.LUIS;
using System.Net.Http;
using LuisBot.LuisHelper;
using LuisBot.Data;
using LuisBot;
using System.Configuration;
using LuisBot.Intents;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog
    {

        DateTime lastHello;
        public string outText;
        public bool isHandled;
        public string lastIntent = "";
        public string username;
        public string userId;
        public HappinessTracker happinessTracker;
        public bool getRating=false;

        // Bot Manager
        public BotManager _botManager;

        public async Task StartAsync(IDialogContext context)
        {
            //initial prompt for what's missing
            //await Greeting_Hello_Intent(context);
            context.Wait(MessageReceivedAsync); // State transition: wait for user to start conversation

            happinessTracker = new HappinessTracker();

            // Start the Bot Manager
            _botManager = new BotManager("BotConfiguration.json", this, "NoneIntent");

            // Extract the username and userid
            //Get the username
            if (context.Activity.From.Id != null)
            {
                userId = context.Activity.From.Id;
                username = context.Activity.From.Name;
            }
            else
            {
                userId = "Anonymous";
                username = "Unknown User";
            }


            // User Profile Information Management
            UserInfo us = new UserInfo();
            us.DefaultFacility = "";
            us.DefaultProgram = "All Programmes";
            EntitiesDBQuery db = new EntitiesDBQuery();
            UserInfo outInfo = db.GetUserInfo(userId, us);

        }


        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            //Get the user input
            string textIn = (await argument).Text;

            // Main Execution of query
            _botManager.ExecuteQuery(textIn, context, argument);

            ////If the user is unhappy and they haven't already asked for help, check on them
            //if (luisOutput.TopIntent.Name != "Help" && this.happinessTracker.RatingsCount > 3 
            //    && this.happinessTracker.AverageRating < 0 && this.happinessTracker.LastRating<0)
            //{
            //    await context.PostAsync("Seriously, feel free to ask for help");
            //    this.happinessTracker.ResetRatings();
            //}

            try
            {
                context.Wait(MessageReceivedAsync);
            }
            catch (Exception ex)
            {
                _botManager.Log($"Exception thrown in MessageReceivedAsync(): {ex.Message}");
                throw;
            }
            
        }
        

        #region Functional Intents
        [IntentAttribute("BestXThings")]
        public async Task<string> BestXThings_Intent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult result, LuisIntent intent)
        {
            this.lastIntent = "BestXThings";
            string outputAnswer = TopBottomThings(context, message, result, intent, true);
            return outputAnswer;

        }
        [IntentAttribute("WorstXThings")]
        public async Task<string> WorstXThings_Intent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult result, LuisIntent intent)
        {
            this.lastIntent = "WorstXThings";
            string outputAnswer = TopBottomThings(context, message, result, intent, false);
            return outputAnswer;
        }

        private string TopBottomThings(IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult result, LuisIntent intent, bool isTop)
        {

            // We need:
            // Which [3] [facilities] in [Ugu] are under-spending?
            // [Number] [EntityType] [District]/[Programme]/[Country]
            // Also, where are we currently?
            // Bit of a hack for now
            var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };

            string term = "this year";
            string topBottom = (isTop ? "highest" : "lowest");
            bool foundInLUIS;
            string inWhichDistrict = _botManager.Memory.GetFieldValueWithEntityUpdate("lastDistrict", "District", result, out foundInLUIS);
            string inWhichProgramme = _botManager.Memory.GetFieldValueWithEntityUpdate("lastProgramme", "Programme", result, out foundInLUIS);
            string inWhichFacility = _botManager.Memory.GetFieldValueWithEntityUpdate("lastFacility", "Facility", result, out foundInLUIS);
            string whichIndicator = _botManager.Memory.GetFieldValueWithEntityUpdate("lastIndicator", "Indicator", result, out foundInLUIS);
            bool numberEntitiesFoundInLUIS;
            string numberEntities = _botManager.Memory.GetFieldValueWithEntityUpdate("lastNumberEntities", "Number", result, out numberEntitiesFoundInLUIS);
            int numberElements = 1;
            string numberPlural = "";
            string numberIsAre = "is";


            // Here, we do a bit of checking. Because you could say "what about the RAD programme?", 
            // we check if the entity and the inWhich is the same, and if so, discaard the entity
            string thing = GetEntityValue("EntityType", "", result); // We don't want to persist it yet

            if (((thing == "District") && (inWhichDistrict != "")) || ((thing == "Programme") && (inWhichProgramme != "")) || ((thing == "Facility") && (inWhichFacility != "")))
                thing = "";

            if (thing == "")
                thing = _botManager.Memory.GetFieldValueWithEntityUpdate("lastEntityBeingDiscussed", "District", out foundInLUIS);
            else
                _botManager.Memory.SetFieldValue("lastEntityBeingDiscussed", thing);

            // Number of entities
            string number = "";
            numberElements = Convert.ToInt32(numberEntities);
            if (numberElements != 1)
            { 
                if (numberElements <= unitsMap.Length)
                    number = unitsMap[numberElements];
                if (numberElements > 1)
                { 
                    numberPlural = "s";
                    numberIsAre = "are";
                }
             }

            // Indicator

            string indicatorDescription = "";
            if (whichIndicator != "")
                indicatorDescription = $" on {whichIndicator} ";


            string outputAnswer = "";
            PerformanceDBQuery queryEngine = new PerformanceDBQuery();
            // Get the context
            switch (thing)
            {
                case "District":
                    // District, we need to know the Program
                    if (inWhichProgramme == "")
                        outputAnswer = $"To list the {topBottom} districts{numberPlural}, I need to know which program you are referring to? You can also ask for a list of programs.";
                    else
                    {
                        string listOf = queryEngine.GetBestWorstDistrictAsString(inWhichProgramme, true, isTop, numberElements,( whichIndicator.ToLower() == "all indicators"?"": whichIndicator));
                        outputAnswer = $"The {topBottom} {number} district{numberPlural} for {inWhichProgramme} {indicatorDescription}over {term} {numberIsAre} {listOf}";
                    }

                    break;
                case "Facility":
                    // Facility, we need to know the District
                    if (inWhichDistrict == "")
                        outputAnswer = "To show you this, I need you to narrow it down to a district. You can also ask me to list the districts. ";
                    else
                    {
                        outputAnswer = $"The {topBottom} {number} facilitie{numberPlural} {indicatorDescription}over {term} for {inWhichDistrict} {numberIsAre} X, Y, Z";
                    }


                    break;
                case "Indicator":

                    // Check again
                    if ((inWhichFacility == "") && (inWhichProgramme == "") && (inWhichDistrict == ""))
                        return $"To show you the {topBottom} indicator{numberPlural}, I need to know which facility, district or program you want to see the indicators for. You can also ask me to list the facilities, districts and indicators.";

                    if (inWhichProgramme != "")
                    {
                        // Indicators for a programme
                        string indicatorsForProgram = queryEngine.GetProgramPerformanceAsString(inWhichProgramme, true, true, whichIndicator, isTop, numberElements);
                        outputAnswer = $"The {topBottom} {number} indicator{numberPlural} {indicatorDescription}over {term} for {inWhichProgramme} {numberIsAre} {indicatorsForProgram}";
                        break;
                    }
                    if (inWhichDistrict != "")
                    {
                        // Indicators for a district
                        string indicatorsForDistrict = queryEngine.GetDistrictPerformanceAsString(inWhichDistrict, true, indicatorName: whichIndicator, best: isTop, n: numberElements);
                        outputAnswer = $"The {topBottom} {number} indicator{numberPlural} {indicatorDescription}over {term} for {inWhichDistrict} {numberIsAre} {indicatorsForDistrict}";
                        break;
                    }
                    if (inWhichFacility != "")
                    {
                        // Indicators for a facility
                        string indicatorsForFacility = "Facility X, Facility Y, Facility Z";// queryEngine.GetDistrictPerformanceAsString(inWhichDistrict, true, best: isTop, n: Convert.ToInt32(number));
                        outputAnswer = $"The {topBottom} {number} indicator{numberPlural} {indicatorDescription}over {term} for {inWhichFacility} {numberIsAre} {indicatorsForFacility}";
                        break;
                    }
                    break;
                case "Programme":throw new NotImplementedException();
            }
            return outputAnswer;
        }

        [IntentAttribute("ChangeParameter")]
        public async Task<string> ChangeParameter_Intent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult result, LuisIntent intent)
        {
            string output = "";
            //BestXThings, WorstXThings
            switch (lastIntent)
            {
                case ("PerformanceAgainstTarget"):
                    output = await PerformanceAgainstTarget_Intent(context, message, result, intent);
                    break;
                case ("WorstXThings"):
                     output = await WorstXThings_Intent(context, message, result, intent);
                    break;
                case ("BestXThings"):
                    output = await BestXThings_Intent(context, message, result, intent);
                    break;
                default:
                    output = await PerformanceAgainstTarget_Intent(context, message, result, intent);
                    break;
            }

            return output;

        }

        // To change parameter for the bestx or worstx
        [ConvElement("switchParameterBestX")]
        public async Task<string> ChangeParameterBestX_Conv(IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult result, BotManager.Subconvelement convelement, LuisIntent intent)
        {
            string output = TopBottomThings(context, message, result, intent, true);
            return output;
        }
        // To change parameter for the bestx or worstx
        [ConvElement("switchParameterWorstX")]
        public async Task<string> ChangeParameterWortX_Conv(IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult result, BotManager.Subconvelement convelement, LuisIntent intent)
        {
            string output = TopBottomThings(context, message, result, intent, false);
            return output;
        }

        [IntentAttribute("PerformanceAgainstTarget")]
        public async Task<string> PerformanceAgainstTarget_Intent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult result, LuisIntent intent)
        {
            //check if they passed params through
            //this.lastdate = GetEntityValue("builtin.datetimeV2.daterange", this.lastdate, result);
            bool foundInLUIS;
            string lastTermPerformance = _botManager.Memory.GetFieldValueWithEntityUpdate("lastTerm", "Term", result, out foundInLUIS);
            this.lastIntent = "PerformanceAgainstTarget";
            string lastIndicatorPerformance = _botManager.Memory.GetFieldValueWithEntityUpdate("lastIndicator", "Indicator", result, out foundInLUIS);
            string lastDistrictPerformance = _botManager.Memory.GetFieldValueWithEntityUpdate("lastDistrict", "District", result, out foundInLUIS);
            string lastProgrammePerformance = _botManager.Memory.GetFieldValueWithEntityUpdate("lastProgramme", "Programme", result, out foundInLUIS);

            //Build up a string describing performance:
            const string TOKEN_DISTRICT = "[district/prog]";
            string fullOutput = "The [term] [indicator]performance for **[district/prog]** is:\n\n[performance] [note]";

            //dummy output
            Random rnd = new Random();
            int target = rnd.Next(20, 120);
            int actual = rnd.Next(20, 120);
            int percentTarget = (int)(Math.Round(((double)actual / (double)target) * 100));

            //After [Mx_Past] months you should have spent $[LatestMonth_YTDTarget] [Indicator] but you have spent $[LatestMonth_YTDValue] which is [LatestMonth_AnnualTarget_perc]% of the annual target of $[LatestMonth_AnnualTarget]
            //if (this.lastProgramme == "All Programmes")

            //determine if it's a single indicator or a list
            bool allIndicators = (lastIndicatorPerformance.ToLower() == "all indicators");
            PerformanceType performanceType;

            //get the term
            fullOutput = fullOutput.Replace("[term]", _botManager.Memory["lastTerm"]);

            //get the indicator
            if (lastIndicatorPerformance == "All indicators")
                fullOutput = fullOutput.Replace("[indicator]", "");
            else
                fullOutput = fullOutput.Replace("[indicator]", "**" + lastIndicatorPerformance + "** ");

            //Get the district or program
            fullOutput = fullOutput.Replace(TOKEN_DISTRICT, GetDistrictProgramme(result));

            //generate the performance reposnse
            string performance;
            PerformanceDBQuery query = new PerformanceDBQuery();
            if (lastDistrictPerformance == "")
            {
                performanceType = PerformanceType.Program;
                //get performance for the program
                if (lastIndicatorPerformance.ToLower() == "all indicators")
                    performance = query.GetProgramPerformanceAsString(lastProgrammePerformance, (lastTermPerformance == "Annual"), true);
                else
                    performance = query.GetProgramPerformanceAsString(lastProgrammePerformance, (lastTermPerformance == "Annual"), true, lastIndicatorPerformance);

            }
            else
            {
                performanceType = PerformanceType.District;
                //get performance for the district
                if (lastIndicatorPerformance.ToLower() == "all indicators")
                    performance = query.GetDistrictPerformanceAsString(lastDistrictPerformance, (lastTermPerformance == "Annual"), true);
                else
                    performance = query.GetDistrictPerformanceAsString(lastDistrictPerformance, (lastTermPerformance == "Annual"), true, lastIndicatorPerformance);
                //performance = $"{actual} against a target of {target} which is {percentTarget}% of target";

            }
            fullOutput = fullOutput.Replace("[performance]", performance);



            //Add a note if required
            if (percentTarget > 100)
                fullOutput = fullOutput.Replace("[note]", "Great work!");
            else
                fullOutput = fullOutput.Replace("[note]", "");

            //Add link to RAD program. 
            if (lastProgrammePerformance.ToLower() == "rad")
                fullOutput += "\n\nFor more information on RAD, visit https://tinyurl.com/ya8sp8qu";

            //Display a card  todo: this is also calling 
            if (allIndicators && (ConfigurationManager.AppSettings["DisplayCard"] == "true"))
            {
                Intents.PerformanceIntentHandler perfHandler = new Intents.PerformanceIntentHandler();
                Attachment card = await perfHandler.ShowPerformanceCard(context, lastProgrammePerformance, lastDistrictPerformance, lastTermPerformance, performanceType);
                var reply = context.MakeMessage();
                reply.Attachments.Add(card);
                await context.PostAsync(reply);
                //await context.PostAsync("test");
                return "";
            }
            else
            {

                return fullOutput;
            }


        }

        [IntentAttribute("Listindicators")]
        public async Task<string> ListIndicators_Intent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult result, LuisIntent intent)
        {

            return $"I can give you the status of the HTS_TST, HTS_TST +, TX_CURR, TX_NEW, Testing Yield % or finance indicator performance. Alternatively, you can request the performance of all indicators.";

            //await context.PostAsync($"I can give you the status of the HTS_TST, HTS_TST +, TX_CURR, TX_NEW, Testing Yield % or finance indicator performance. Alternatively, you can request the performance of all indicators.");
            //return "";

        }

        [IntentAttribute("ListDistricts")]
        public async Task<string> ListDistricts_Intent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult result, LuisIntent intent)
        {
            //get a list of districts 
            EntitiesDBQuery query = new EntitiesDBQuery();
            string districts = query.GetListOfDistrictsAsString(_botManager.Memory["lastProgramme"]);

            return $"I can give you the performance of the {districts} districts. Alternatively, you can request the performance of all districts or a specific programme.";

            //await context.PostAsync($"I can give you the performance of the {districts} districts. Alternatively, you can request the performance of all districts or a specific programme.");
            //return "";

        }

        [IntentAttribute("ListPrograms")]
        public async Task<string> ListPrograms_Intent(IDialogContext context, IAwaitable<IMessageActivity> message,LuisFullResult result, LuisIntent intent)
        {

            return $"I can give you the status of the Comprehensive, RAD or IDeAS programmes. Alternatively, you can request the performance of all programmes.";

            //await context.PostAsync($"I can give you the status of the Comprehensive, RAD or IDeAS programmes. Alternatively, you can request the performance of all programmes.");
            //return "";

        }
        

        /// <summary>
        /// Get either the district or the program based on what the user passed through
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private string GetDistrictProgramme(LuisFullResult result)
        {
            //get the district/program
            //Rules: if program is explicitly set, reset district. 
            //if district is explicitly set, set program to specific parent program.
            //if both are set, ignore program for now. Later: tell user if inconsistent
            //Otherwise, use last district or last program if no last district 
            LuisEntity programEntity;
            bool foundProgram = result.TryFindEntity("Programme", out programEntity);
            LuisEntity districtEntity;
            bool foundDistrict = result.TryFindEntity("District", out districtEntity);

            if (foundDistrict)
            {
                _botManager.Memory["lastDistrict"] =  ((LuisStandardEntity)districtEntity).TrueValue;
                return _botManager.Memory["lastDistrict"];
            }
            else if (foundProgram)
            {
                _botManager.Memory["lastProgramme"] = ((LuisStandardEntity)programEntity).TrueValue;
                _botManager.Memory["lastDistrict"] = "";
                return _botManager.Memory["lastProgramme"];
            }
            else if (_botManager.Memory["lastDistrict"] == "")
            {
                //No district so program
                return _botManager.Memory["lastProgramme"];
            }
            else
                return _botManager.Memory["lastDistrict"];
            
        }

#endregion

        #region None Intent

        public async Task NoneIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult result)
        {
            var qnadialog = new BasicQnAMakerDialog();
            var messageToForward = await message;
            await context.Forward(qnadialog, AfterQnADialog, messageToForward, CancellationToken.None);
            //context.Wait(MessageReceivedAsync);
        }

        private async Task AfterQnADialog(IDialogContext context, IAwaitable<bool> result)
        {
            var answerFound = await result;

            // we might want to send a message or take some action if no answer was found (false returned)
            if (!answerFound)
            {
                await context.PostAsync("I’m not sure what you want. Please try rephrase your question or type help");
                await context.PostAsync($"I can answer questions on your performance or business indicators for specific programmes or districts. Ensure your questions relate to districts, programs and indicators in our database.");
                await context.PostAsync($"For example: \"What is the Ugu district performance for 2017?\"");

                this.happinessTracker.AddRating(-5);
            }
            else this.happinessTracker.AddRating(0);


        }

#endregion

        #region Get Rating
        private async Task GetRating(IDialogContext context, IAwaitable<IMessageActivity> message)
        {
            //set the get rating flag back to false
            this.getRating = false;

            //Forward the context to the rating dialog
            var ratingdialog = new RatingDialog();
            var messageToForward = await message;
            await context.Forward(ratingdialog, RatingDialogResumeAfter,messageToForward,CancellationToken.None);
            //await context.Forward(qnadialog, AfterQnADialog, messageToForward, CancellationToken.None);
            await context.PostAsync("Here!");
        }

        private async Task RatingDialogResumeAfter(IDialogContext context, IAwaitable<int> result)
        {
            //var messageHandled = await result;
            //if (!messageHandled)
            //{
            //    await context.PostAsync("Sorry, I wasn't sure what you wanted.");
            //}

            //context.Wait(MessageReceived);
            int rating = await result;
            if (rating >= 0)
            {
                await context.PostAsync($"Thank for your rating of {rating.ToString()}");
                //context.Wait(MessageReceivedAsync);
 
            }

        }

#endregion

        #region Generic Intents

        [IntentAttribute("Greeting_Hello")]
        public async Task<string> Greeting_Hello_Intent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult result, LuisIntent intent)
        {
            string output = "";
            //Say hello back - joke if user said hello too recently
            if ((lastHello == null) || DateTime.Now.Subtract(lastHello).Minutes < 2)
                output = $"Hi again :)";
            else
            {
                string userNameDisplay = "";
                if (!String.IsNullOrEmpty(username))
                    userNameDisplay = $" {username}";
                output = $"Hi{userNameDisplay}. If you need some help, just ask for it.";
            }

            lastHello = DateTime.Now;

            return output;

        }

        [IntentAttribute("Greeting_bye")]
        public async Task<string> Greeting_bye_Intent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult result, LuisIntent intent)
        {
            string output = "$Great chatting to you.. Let me know how I did on a scale from 1 to 10";

            //Get the bot ready for a rating
            getRating = true;

            //PromptDialog.Number(context, AfterNumberDialog, "How did I do on a scale from 1 to 10?", attempts: 3);

            return output;

        }


        [IntentAttribute("Thanks")]
        public async Task<string> Thanks_Intent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult result, LuisIntent intent)
        {
            string output = $"Glad to be of service";
            return output;
        }


        //private async Task AfterNumberDialog(IDialogContext context, IAwaitable<double> result)
        //{
        //    try
        //    {
        //        double rating = await result;
        //        await context.PostAsync($"Thanks. Will report back your rating of {rating}");

        //    }
        //    catch (Exception ex)
        //    {
        //        await context.PostAsync("Will ignore that rating");
        //    }
        //}




        [IntentAttribute("Human")]
        public async Task<string> Human_Intent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult result, LuisIntent intent)
        {
            string output = "";
            //string department;
            LuisEntity entity;
            bool departmentSet = result.TryFindEntity("Department", out entity);
            LuisStandardEntity departmentEntity = (LuisStandardEntity)entity;
            if (departmentSet)
            {
                //string depName = entity.Value;
               output = $"Will pass your details onto someone in {departmentEntity.TrueValue}";
            }
            else
            {
                output = $"No problem. Will pass your details onto a human";
            }

            return output;
        }

        [IntentAttribute("Help")]
        public async Task<string> Help_Intent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult result, LuisIntent intent)
        {
            string output = "";
            output += $"Can certainly help...\n\n\n\n";



            output += _botManager.GetCurrentHelpString();
            return output;

            //output += "Welcome to the BroadReach bot.I can answer questions on your performance or business indicators for specific programmes or districts.Ensure your questions relate to districts, programs and indicators in our database.\n\n I can also answer general questions about Broadreach and our offerings.\n\n Try: 'What is the Ugu district performance for 2017?'\n\n";
            //await context.PostAsync(output);
            //return "";

        }

        #endregion



        string GetEntityValue(string entityName, string defaultVal, LuisFullResult result)
        {
            LuisEntity entity;
            //get params
            bool foundIt = result.TryFindEntity(entityName, out entity);
            if (foundIt)
            {
               
                //check on the type and return appropriate value
                if (entity is LuisStandardEntity)
                {
                    return ((LuisStandardEntity)entity).TrueValue;
                }
                else
                    return entity.Value;
            }
            else
                return defaultVal;
        }

        /// <summary>
        /// Call the LUIS service
        /// </summary>
        /// <param name="userQuery"></param>
        /// <returns></returns>
        private async Task<string> GetJsonFromLUIS(string userQuery)
        {
            //todo: put this into a separate class that used the app object to initialize

            string query = Uri.EscapeDataString(userQuery);

            using (HttpClient client = new HttpClient())
            {
                //https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/05d130e4-3419-47de-8a50-f9ee960f02f3?subscription-key=664a0179dac5472b895eacc3f08ff58c&timezoneOffset=0&verbose=true&q=
                string RequestURI = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/05d130e4-3419-47de-8a50-f9ee960f02f3?subscription-key=a64477db395d4458855e42186eed40d2&timezoneOffset=0&verbose=true&q=" + query;
                HttpResponseMessage msg = await client.GetAsync(RequestURI);
                if (msg.IsSuccessStatusCode)
                {
                    string JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    //Data = JsonConvert.DeserializeObject<StockLUIS>(JsonDataResponse);
                    return JsonDataResponse;
                }
                else
                {
                    throw new Exception("Invalid respose from LUIS service");
                }
            }
        }

            
    }
}