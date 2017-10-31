using System;
using System.Configuration;

//using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using QnAMakerDialog;
//using Microsoft.Bot.Builder.CognitiveServices.QnAMaker.Resource;

namespace LuisBot.Dialogs
{
    [Serializable]
    [QnAMakerService("19223c62460043aa895263dbe0a820d5", "099a4406-09bd-429c-9322-47077a46ad2d")]
    public class BasicQnAMakerDialog : QnAMakerDialog<bool>
    {
        // Go to https://qnamaker.ai and feed data, train & publish your QnA Knowledgebase.
        //public BasicQnAMakerDialog() : base(new QnAMakerService(new QnAMakerAttribute(ConfigurationManager.AppSettings["QnASubscriptionKey"], ConfigurationManager.AppSettings["QnAKnowledgebaseId"])))  // If you're running this bot locally, make sure you have these appSettings in youe web.config
        //{
        //    //todo add keys to web.config
        //}

        public override async Task NoMatchHandler(IDialogContext context, string originalQueryText)
        {
            //await context.PostAsync($"Sorry, I couldn't find an answer for '{originalQueryText}'.");
            context.Done(false);
        }

        public override async Task DefaultMatchHandler(IDialogContext context, string originalQueryText, QnAMakerResult result)
        {
            await context.PostAsync($"{result.Answer}.");
            context.Done(true);
        }

        [QnAMakerResponseHandler(50)]
        public async Task LowScoreHandler(IDialogContext context, string originalQueryText, QnAMakerResult result)
        {
            await context.PostAsync($"I'm not sure but here's an answer that might help...{result.Answer}.");
            context.Done(true);
        }

        //[QnAMakerResponseHandler(50)]
        //public async Task LowScoreHandler(IDialogContext context, string originalQueryText, QnAMakerResult result)
        //{
        //    await context.PostAsync($"I found an answer that might help...{result.Answer}.");
        //    context.Wait(MessageReceived);
        //}

        /// <summary>
        /// This is an override to catch the QnA answer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        //public override async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        //{
        //    var message = await result;

        //    //if (sendDefaultMessageAndWait)
        //    //{
        //    //    // The following line should be removed if you don't want that the QnADialog replies if no answer found
        //    //    await context.PostAsync(qnaMakerResults.ServiceCfg.DefaultMessage);
        //    //    await this.DefaultWaitNextMessageAsync(context, message, qnaMakerResults);
        //    //}
        //}


    }
}