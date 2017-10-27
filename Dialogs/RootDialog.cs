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

namespace Microsoft.Bot.Sample.LuisBot
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
        public string lastTerm = "annual";

        public async Task StartAsync(IDialogContext context)
        {
            //initial prompt for what's missing
            //await context.PostAsync($"Hello there. This bot does XYZ");
            context.Wait(MessageReceivedAsync); // State transition: wait for user to start conversation

        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            //Get the user input
            string textIn = (await argument).Text;
            //await context.PostAsync($"You said:{textIn}");
            //call LUIS for a reponse
            string output = await GetJsonFromLUIS(textIn);
            LuisFullResult luisOutput = new LuisFullResult(output);

            //call intent method based on LUIS intent
            await CallCorrectMethodFromLuisResponse(luisOutput, context, argument);

            context.Wait(MessageReceivedAsync);
            
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
                    break;
                case "PerformanceAgainstTarget":
                    await PerformanceAgainstTarget_Intent(context, luisOutput);
                    break;
                case "Greeting_Hello":
                    await Greeting_Hello_Intent(context, luisOutput);
                    break;
                case "Greeting_bye":
                    await Greeting_bye_Intent(context, luisOutput);
                    break;
                case "Human":
                    await Human_Intent(context, luisOutput);
                    break;
                case "Help":
                    await Help_Intent(context, luisOutput);
                    break;
                case "None":
                    await NoneIntent(context,argument, luisOutput);
                    break;
                default:
                    throw new Exception($"Unconfigured intent of: {luisOutput.TopIntent.Name}");

            }

        }

        #region functionalIntents
        private async Task ChangeParameter_Intent(IDialogContext context, LuisFullResult result)
        {
            if (lastIntent == "PerformanceAgainstTarget")
                await PerformanceAgainstTarget_Intent(context, result);
            
        }

        private async Task PerformanceAgainstTarget_Intent(IDialogContext context, LuisFullResult result)
        {
            //check if they passed params through
            //this.lastdate = GetEntityValue("builtin.datetimeV2.daterange", this.lastdate, result);
            this.lastTerm= GetEntityValue("Term", this.lastTerm, result);
            this.lastProgramme = GetEntityValue("Programme", this.lastProgramme, result);
            this.lastIntent = "PerformanceAgainstTarget";
            this.lastDistrict= GetEntityValue("District", this.lastDistrict, result);

            //Build up a string describing performance:
            string fullOutput = "The [term] [indicator]performance for [district/prog] is [performance]. [note]";

            //dummy output
            Random rnd = new Random();
            int target=rnd.Next(20,120);
            int actual = rnd.Next(20, 120); 
            int percentTarget = (int)(Math.Round( ((double)actual/(double)target)*100));

            //After [Mx_Past] months you should have spent $[LatestMonth_YTDTarget] [Indicator] but you have spent $[LatestMonth_YTDValue] which is [LatestMonth_AnnualTarget_perc]% of the annual target of $[LatestMonth_AnnualTarget]
            //if (this.lastProgramme == "All Programmes")

            //get the term
            fullOutput= fullOutput.Replace("[term]", lastTerm);

            //get the indicator
            if (lastIndicator== "All indicators")
                fullOutput = fullOutput.Replace("[indicator]", "");
            else
                fullOutput = fullOutput.Replace("[indicator]", lastIndicator + " ");

            //get the district/program
            if (lastDistrict=="")
            {
                //No district so program
                fullOutput = fullOutput.Replace("[district/prog]", lastProgramme);
            }
            else
                fullOutput = fullOutput.Replace("[district/prog]", lastDistrict);

            //generate the performance reposnse
            string performance = $"{actual} against a target of {target} which is {percentTarget}% of target";
            fullOutput = fullOutput.Replace("[performance]", performance);

            //Add a note if required
            if (percentTarget>100)
                fullOutput = fullOutput.Replace("[note]", "Great work!");
            else
                fullOutput = fullOutput.Replace("[note]", "");

            //output the reponse
            await context.PostAsync(fullOutput);

        }

#endregion

        #region NoneIntent

        private async Task NoneIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisFullResult result)
        {
            //Todo: try send it off to QnA maker at this point and only respond if I don't understand
            //await context.PostAsync($"Sorry, I don't understand: {result.Query}"); //
            //await context.PostAsync("Please try rephrase your question or type help");
            //outText = "hello outtext";
            //isHandled = false;

            var qnadialog = new BasicQnAMakerDialog();
            var messageToForward = await message;
            await context.Forward(qnadialog, AfterQnADialog, messageToForward, CancellationToken.None);
        }

        private async Task AfterQnADialog(IDialogContext context, IAwaitable<bool> result)
        {
            var answerFound = await result;

            // we might want to send a message or take some action if no answer was found (false returned)
            if (!answerFound)
            {
                await context.PostAsync("I’m not sure what you want.");
                //await context.PostAsync($"Sorry, I don't understand: {result.Query}"); //
                await context.PostAsync("Please try rephrase your question or type help");
            }

            //context.Wait(MessageReceived);
        }

        #endregion

        #region Generic intents

        private async Task Greeting_Hello_Intent(IDialogContext context, LuisFullResult result)
        {
            //Say hello back - joke if user said hello too recently
            if ((lastHello == null) || DateTime.Now.Subtract(lastHello).Minutes < 2)
                await context.PostAsync($"Hi again :)");
            else
                await context.PostAsync($"Hi. If you need some help, just ask for it.");

            lastHello = DateTime.Now;
        }

        private async Task Greeting_bye_Intent(IDialogContext context, LuisFullResult result)
        {
            await context.PostAsync($"Great chatting to you.. Let me know how I did on a scale from 1 to 10");
        }

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
            await context.PostAsync($"I can answer questions on your performance or business indicators.");
            await context.PostAsync($"I can also answer general questions about Broadreach and our offerings.");
            await context.PostAsync($"Try: \"What is my performance for 2017?\"");

        }

        #endregion

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