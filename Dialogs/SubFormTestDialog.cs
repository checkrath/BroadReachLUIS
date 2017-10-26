using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;


namespace Microsoft.Bot.Sample.LuisBot
{
    [Serializable]
    public class SubFormTestDialog : IDialog<string>
    {
        //TODO: I still need to call LUIS here sometimes to check if the user is asking some basic questions like for help etc.
        //todo: do this as a standard service call - let's rather do this in future
        public string DateRange;

        public SubFormTestDialog(string passedDateRange) : base()
        {
            this.DateRange = passedDateRange;
        }

        public async Task StartAsync(IDialogContext context)
        {
            //initial prompt for what's missing
            await context.PostAsync($"What's your date range? Is it still {this.DateRange}?");
            context.Wait(MessageReceivedStartConversation); // State transition: wait for user to start conversation
        }

        public async Task MessageReceivedStartConversation(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            string textIn = (await argument).Text;
            int result;

            //if its a number, assign and return. Otherwise ask again
            if (int.TryParse(textIn, out result))
            {
                this.DateRange = textIn;

                //await context.PostAsync($"Thanks for your date range of {DateRange}");
                //context.Wait(MessageReceivedRegistrationNumber); // State transition: wait for user to provide registration number
                context.Done<string>(this.DateRange);
            }
            else
            {
                await context.PostAsync($"{textIn} is not a valid year. PLease entre a valid year.");
                context.Wait(MessageReceivedStartConversation);
            }
        }
    }
}