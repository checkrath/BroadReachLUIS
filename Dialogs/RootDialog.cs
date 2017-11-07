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

namespace LuisBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {

        DateTime lastHello;
        public string outText;
        public bool isHandled;
        public string lastdate = "2017";
        public string lastProgramme = "All Programmes";
        public string lastCountry = "Worldwide";
        public string lastDistrict = "";
        public string lastIndicator = "All indicators";
        public string lastIntent = "";
        public string lastTerm = "Annual";
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
            _botManager = new BotManager("BotConfiguration.json",this);
        }

        //public async Task FirstReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        //{
        //    await context.PostAsync("Welcome to the BroadReach bot.");
        //    await context.PostAsync($"I can answer questions on your performance or business indicators for specific programmes or districts. Ensure your questions relate to districts, programs and indicators in our database.");
        //    await context.PostAsync($"I can also answer general questions about Broadreach and our offerings.");
        //    await context.PostAsync($"Try: \"What is the Ugu district performance for 2017?\"");

        //}

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            //exit if no input
            //if (context.Activity.GetActivityType() != ActivityTypes.Message) return;

            //Get the username
            if (context.Activity.From.Id != null)
            {
                userId = context.Activity.From.Id;
                username= context.Activity.From.Name;
            }
            else
            {
                userId = "Anonymous";
                username = "Unknown User";
            }



            //Get the user input
            string textIn = (await argument).Text;

            List<LuisFullResult> results = await _botManager.ExecuteQuery(textIn, context, argument);

            //await context.PostAsync($"You said:{textIn}");
            //call LUIS for a reponse
            string output = await GetJsonFromLUIS(textIn);
            LuisFullResult luisOutput = new LuisFullResult(output);

            //call get rating if expecting a rating
            //if (getRating) await GetRating(context, argument);

            //call intent method based on LUIS intent
            await CallCorrectMethodFromLuisResponse(luisOutput, context, argument);

            //If the user is unhappy and they haven't already asked for help, check on them
            if (luisOutput.TopIntent.Name != "Help" && this.happinessTracker.RatingsCount > 3 
                && this.happinessTracker.AverageRating < 0 && this.happinessTracker.LastRating<0)
            {
                await context.PostAsync("Seriously, feel free to ask for help");
                this.happinessTracker.ResetRatings();
            }


            try
            {
                context.Wait(MessageReceivedAsync);
            }
            catch (Exception ex)
            {
                //TODo: need to handle the above better! Maybe there's a way I can check onth status of the context
            }
            
        }


        /// <summary>
        /// Will call the appropriate method based on the LUIS intent. This is the primary flow of the solution
        /// </summary>
        /// <param name="luisOutput"></param>
        /// <returns></returns>
        private async Task CallCorrectMethodFromLuisResponse(LuisFullResult luisOutput, IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            switch (luisOutput.TopIntent.Name)
            {
                case "ChangeParameter":
                    await ChangeParameter_Intent(context, luisOutput);
                    this.happinessTracker.AddRating(5);
                    break;
                case "PerformanceAgainstTarget":
                    await PerformanceAgainstTarget_Intent(context, luisOutput);
                    this.happinessTracker.AddRating(5);
                    break;
                case "ListDistricts":
                    await ListDistricts_Intent(context, luisOutput);
                    this.happinessTracker.AddRating(0);
                    break;
                case "ListIndicators":
                    await ListIndicators_Intent(context, luisOutput);
                    this.happinessTracker.AddRating(0);
                    break;
                case "ListPrograms":
                    await ListPrograms_Intent(context, luisOutput);
                    this.happinessTracker.AddRating(0);
                    break;
                case "Greeting_Hello":
                    await Greeting_Hello_Intent(context);
                    this.happinessTracker.AddRating(0);
                    break;
                case "Greeting_bye":
                    await Greeting_bye_Intent(context, luisOutput);
                    this.happinessTracker.AddRating(-1);
                    break;
                case "Human":
                    await Human_Intent(context, luisOutput);
                    this.happinessTracker.AddRating(-3);
                    break;
                case "Help":
                    await Help_Intent(context, luisOutput);
                    this.happinessTracker.AddRating(-3);
                    break;
                case "None":
                    await NoneIntent(context,argument, luisOutput);
                    //happiness tracking in method
                    break;
                default:
                    throw new Exception($"Unconfigured intent of: {luisOutput.TopIntent.Name}");

            }

        }

        #region functionalIntents
        private async Task ChangeParameter_Intent(IDialogContext context, LuisFullResult result)
        {
            //if (lastIntent == "PerformanceAgainstTarget")
            //    await PerformanceAgainstTarget_Intent(context, result);
            //todo: fix this so it goes to the correct method
            await PerformanceAgainstTarget_Intent(context, result);

        }

        private async Task PerformanceAgainstTarget_Intent(IDialogContext context, LuisFullResult result)
        {
            //check if they passed params through
            //this.lastdate = GetEntityValue("builtin.datetimeV2.daterange", this.lastdate, result);
            this.lastTerm = GetEntityValue("Term", this.lastTerm, result);
            this.lastIntent = "PerformanceAgainstTarget";
            this.lastIndicator= GetEntityValue("Indicator", this.lastIndicator, result);

            //Build up a string describing performance:
            const string TOKEN_DISTRICT = "[district/prog]";
            string fullOutput = "The [term] [indicator]performance for [district/prog] is [performance]. [note]";

            //dummy output
            Random rnd = new Random();
            int target = rnd.Next(20, 120);
            int actual = rnd.Next(20, 120);
            int percentTarget = (int)(Math.Round(((double)actual / (double)target) * 100));

            //After [Mx_Past] months you should have spent $[LatestMonth_YTDTarget] [Indicator] but you have spent $[LatestMonth_YTDValue] which is [LatestMonth_AnnualTarget_perc]% of the annual target of $[LatestMonth_AnnualTarget]
            //if (this.lastProgramme == "All Programmes")

            //get the term
            fullOutput = fullOutput.Replace("[term]", lastTerm);

            //get the indicator
            if (lastIndicator == "All indicators")
                fullOutput = fullOutput.Replace("[indicator]", "");
            else
                fullOutput = fullOutput.Replace("[indicator]", lastIndicator + " ");

            //Get the district or program
            fullOutput = fullOutput.Replace(TOKEN_DISTRICT, GetDistrictProgramme(result));

            //generate the performance reposnse
            string performance;
            PerformanceDBQuery query = new PerformanceDBQuery();
            if (lastDistrict == "")
            {
                //get performance for the program
                if (lastIndicator == "All indicators")
                    performance = query.GetProgramPerformanceAsString(lastProgramme, (lastTerm == "Annual"), true);
                else
                    performance = query.GetProgramPerformanceAsString(lastProgramme, (lastTerm == "Annual"), true,lastIndicator);
                
            }
            else
            {
                //get performance for the district
                if (lastIndicator == "All indicators")
                    performance = query.GetDistrictPerformanceAsString(lastDistrict, (lastTerm == "Annual"), true);
                else
                    performance = query.GetDistrictPerformanceAsString(lastDistrict, (lastTerm == "Annual"), true, lastIndicator);
                //performance = $"{actual} against a target of {target} which is {percentTarget}% of target";

            }
            fullOutput = fullOutput.Replace("[performance]", performance);



            //Add a note if required
            if (percentTarget > 100)
                fullOutput = fullOutput.Replace("[note]", "Great work!");
            else
                fullOutput = fullOutput.Replace("[note]", "");

            //output the reponse
            await context.PostAsync(fullOutput);

        }

        private async Task ListIndicators_Intent(IDialogContext context, LuisFullResult result)
        {
            await context.PostAsync($"I can give you the status of the HTS_TST, HTS_TST +, TX_CURR, TX_NEW, Testing Yield % or finance indicator performance. Alternatively, you can request the performance of all indicators.");
        }

        private async Task ListDistricts_Intent(IDialogContext context, LuisFullResult result)
        {
            //get a list of districts 
            EntitiesDBQuery query = new EntitiesDBQuery();
            string districts = query.GetListOfDistrictsAsString(lastProgramme);
            await context.PostAsync($"I can give you the performance of the {districts} districts. Alternatively, you can request the performance of all districts or a specific programme.");
        }

        private async Task ListPrograms_Intent(IDialogContext context, LuisFullResult result)
        {
            await context.PostAsync($"I can give you the status of the Comprehensive, RAD or IDeAS programmes. Alternatively, you can request the performance of all programmes.");
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
                lastDistrict = ((LuisStandardEntity)districtEntity).TrueValue;
                return lastDistrict;
            }
            else if (foundProgram)
            {
                lastProgramme = ((LuisStandardEntity)programEntity).TrueValue;
                lastDistrict = "";
                return lastProgramme;
            }
            else if (lastDistrict == "")
            {
                //No district so program
                return lastProgramme;
            }
            else
                return lastDistrict;
            
        }

        #endregion

        #region NoneIntent

        private async Task NoneIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult result)
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

        #region Get rating
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

        #region Generic intents

        private async Task Greeting_Hello_Intent(IDialogContext context)
        {
            //Say hello back - joke if user said hello too recently
            if ((lastHello == null) || DateTime.Now.Subtract(lastHello).Minutes < 2)
                await context.PostAsync($"Hi again :)");
            else
                await context.PostAsync($"Hi {username}. If you need some help, just ask for it.");

            lastHello = DateTime.Now;
        }

        private async Task Greeting_bye_Intent(IDialogContext context, LuisFullResult result)
        {
            await context.PostAsync($"Great chatting to you.. Let me know how I did on a scale from 1 to 10");
            //Get the bot ready for a rating
            getRating = true;

            //PromptDialog.Number(context, AfterNumberDialog, "How did I do on a scale from 1 to 10?", attempts: 3);
            
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





        private async Task Human_Intent(IDialogContext context, LuisFullResult result)
        {
            //string department;
            LuisEntity entity;
            bool departmentSet = result.TryFindEntity("Department", out entity);
            LuisStandardEntity departmentEntity = (LuisStandardEntity)entity;
            if (departmentSet)
            {
                //string depName = entity.Value;
                await context.PostAsync($"Will pass your details onto someone in {departmentEntity.TrueValue}");
            }
            else
            {
                await context.PostAsync($"No problem. Will pass your details onto a human");
            }

        }

        private async Task Help_Intent(IDialogContext context, LuisFullResult result)
        {
            await context.PostAsync($"Can certainly help...");
            await context.PostAsync($"I can answer questions on your performance or business indicators for specific programmes or districts. Ensure your questions relate to districts, programs and indicators in our database.");
            await context.PostAsync($"I can also answer general questions about Broadreach and our offerings.");
            await context.PostAsync($"Try: \"What is the Ugu district performance for 2017?\"");

        }

        #endregion

        //// Test method
        //[ConvElement("firstHello")]
        //public async Task<string> DoSomething(BotManager.Subconvelement convElement, LuisIntent intent)
        //{
        //    return convElement.text;
        //}

        // Test method for intent
        [IntentAttribute("SwitchIntent")]
        public async Task<string> DoSomething(LuisFullResult result, LuisIntent intent)
        {
            return "...";
        }

        string GetEntityValue(string entityName, string defaultVal, LuisFullResult result)
        {
            LuisEntity entity;
            //get params
            bool foundIt = result.TryFindEntity(entityName, out entity);
            if (foundIt)
            {
                //get entity type and original value
                //if (entity.Type == "builtin.datetimeV2.daterange")
                //{
                //    foreach (var vals in entity.Resolution.Values)
                //    {
                //        System.Collections.Generic.List<Object> valsAsString = (List<Object>)vals;
                //        if (((Newtonsoft.Json.Linq.JArray)vals).First.SelectToken("type").ToString() == "daterange")
                //        {
                //            var start = (DateTime)((Newtonsoft.Json.Linq.JArray)vals).First.SelectToken("start");
                //            var end = (DateTime)((Newtonsoft.Json.Linq.JArray)vals).First.SelectToken("end");
                //        }
                //    }
                //}
                
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
                string RequestURI = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/05d130e4-3419-47de-8a50-f9ee960f02f3?subscription-key=664a0179dac5472b895eacc3f08ff58c&timezoneOffset=0&verbose=true&q=" + query;
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