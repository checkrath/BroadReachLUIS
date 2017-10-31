using System;
using System.Configuration;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System.Threading;
using System.Collections.Generic;

namespace LuisBot.Dialogs
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        DateTime lastHello;
        public string outText;
        public bool isHandled;
        public string lastdate="2017";
        public string lastProgramme = "All Programmes";
        public string lastCountry = "Worldwide";
        public string lastIntent = "";

        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["LuisAppId"], ConfigurationManager.AppSettings["LuisAPIKey"])))
        {
            isHandled = true;

        }

        public async Task StartAsync(IDialogContext context)
        {
            
            context.Wait(MessageReceived);
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
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

            context.Wait(MessageReceived);
        }

        [LuisIntent("ChangeParameter")]
        public async Task ChangeParameter_Intent(IDialogContext context, LuisResult result)
        {
            if (lastIntent == "PerformancePersonal")
                PerformancePersonal_Intent(context, result);
            else if (lastIntent == "PerformanceIndicators")
                PerformanceIndicators_Intent(context, result);
            else
                context.Wait(MessageReceived);
        }

        /// <summary>
        /// Return thre personal perfomanc of the user
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [LuisIntent("PerformancePersonal")]
        public async Task PerformancePersonal_Intent(IDialogContext context, LuisResult result)
        {
            //check if they passed params through
            this.lastdate = GetEntityValue("builtin.datetimeV2.daterange", this.lastdate,result);
            this.lastProgramme = GetEntityValue("Programme", this.lastProgramme, result);
            this.lastIntent = "PerformancePersonal";

            if (this.lastProgramme == "All Programmes")
            {
                await context.PostAsync($"Your {lastdate} personal performance is 78% of your target R20.3M");
            }
            else
            {
                await context.PostAsync($"Your {lastdate} personal performance is 78% of your target R20.3M for {lastProgramme}");
            }

            //Calling another dialog for more clarity. 
            //if (FoundYear)
            //{
            //    var subDialog = new SubFormTestDialog(entity.Entity.ToString());
            //    //var messageToForward = await context.MakeMessage;
            //    context.Call(subDialog, AfterSubFormTestDialog);
            //}
            //else
            //{
            //    //await context.PostAsync($"Your personal performance is 78% of your target R20.3M for "); //
            //    //context.Wait(MessageReceived);
            //    var subDialog = new SubFormTestDialog("2017");
            //    //var messageToForward = await context.MakeMessage;
            //    context.Call(subDialog, AfterSubFormTestDialog);
            //}

        }

        private async Task AfterSubFormTestDialog(IDialogContext context, IAwaitable<string> result)
        {
            var year = await result;

            await context.PostAsync($"Your personal performance is 78% of your target R20.3M for {year}");



            context.Wait(MessageReceived);
        }

        [LuisIntent("PerformingYTDTarget")]
        public async Task PerformingYTDTarget_Intent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Your division is at 93% of the YTD target R81.7M"); //
            context.Wait(MessageReceived);
        }

        [LuisIntent("PerformanceIndicators")]
        public async Task PerformanceIndicators_Intent(IDialogContext context, LuisResult result)
        {
            //check if they passed params through
            this.lastdate = GetEntityValue("builtin.datetimeV2.daterange", this.lastdate, result);
            this.lastProgramme = GetEntityValue("Programme", this.lastProgramme, result);
            this.lastCountry = GetEntityValue("Country", this.lastCountry, result);
            this.lastIntent = "PerformanceIndicators";

            //todo: specific text
            await context.PostAsync($"{lastCountry} indicators for {lastProgramme} over {lastdate} are as follows: x% for indicator A and y%for indicator B"); //
            context.Wait(MessageReceived);
        }

        [LuisIntent("PerfromanceAnnualTarget")]
        public async Task PerfromanceAnnualTarget_Intent(IDialogContext context, LuisResult result)
        {
            //todo: specific text
            await context.PostAsync($"Your division is at 93% of the YTD target R81.7M"); //
            context.Wait(MessageReceived);
        }



        #region Generic intents

        [LuisIntent("Greeting_Hello")]
        public async Task Greeting_Hello_Intent(IDialogContext context, LuisResult result)
        {
            //Say hello back - joke if user said hello too recently
            if ((lastHello == null) || DateTime.Now.Subtract(lastHello).Minutes < 2)
                await context.PostAsync($"Hi again :)");
            else
                await context.PostAsync($"Hi. If you need some help, just ask for it.");

            lastHello = DateTime.Now;
            context.Wait(MessageReceived);
        }

        [LuisIntent("Greeting_bye")]
        public async Task Greeting_bye_Intent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Great chatting to you.. Let me know how I did on a scale from 1 to 10");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Human")]
        public async Task Human_Intent(IDialogContext context, LuisResult result)
        {
            //string department;
            EntityRecommendation entity;
            bool departmentSet = result.TryFindEntity("Department", out entity);
            if (departmentSet)
            {
                string depName = entity.Entity;
                await context.PostAsync($"Will pass your details onto someone in {entity.Entity}");
            }
            else
            {
                await context.PostAsync($"No problem. Will pass your details onto a human");
            }
            
            context.Wait(MessageReceived);
        }

        [LuisIntent("Help")]
        public async Task Help_Intent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Can certainly help...");
            await context.PostAsync($"I can answer questions on your performance or business indicators.");
            await context.PostAsync($"I can also answer general questions about Broadreach and our offerings.");
            await context.PostAsync($"Try: \"What is my performance for 2017?\"");

            context.Wait(MessageReceived);
        }

        #endregion

        string GetEntityValue(string entityName, string defaultVal, LuisResult result)
        {
            EntityRecommendation entity;
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
                return entity.Entity;
            }
            else
                return defaultVal;
        }







    }
}