using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class RatingDialog:IDialog<int>
    {
        public async Task StartAsync(IDialogContext context)
        {
            //initial prompt for what's missing
            //await context.PostAsync($"What's your date range? Is it still {this.DateRange}?");
            //context.Wait(MessageReceivedStartConversation); // State transition: wait for user to start conversation

            Activity activity = (Activity)context.Activity;
            GetRating(context, activity);
        }

        private async Task GetRating(IDialogContext context, Activity activity)
        {
            string textIn ="5";  //test
            int result;

            //if its a number, assign and return. Otherwise ask again
            if (int.TryParse(textIn, out result))
            {
                //await context.PostAsync($"Thanks for your date range of {DateRange}");
                //context.Wait(MessageReceivedRegistrationNumber); // State transition: wait for user to provide registration number
                context.Done<int>(int.Parse(textIn));
            }
            else
            {
                //context.Done<int>(-1);
                return;
            }
        }

        //public async Task MessageReceivedStartConversation(IDialogContext context, IAwaitable<IMessageActivity> argument)
        //{
        //    string textIn = (await argument).Text;
        //    int result;

        //    //if its a number, assign and return. Otherwise ask again
        //    if (int.TryParse(textIn, out result))
        //    {
        //        //await context.PostAsync($"Thanks for your date range of {DateRange}");
        //        //context.Wait(MessageReceivedRegistrationNumber); // State transition: wait for user to provide registration number
        //        context.Done<int>(int.Parse(textIn));
        //    }
        //    else
        //    {
        //        //context.Done<int>(-1);
        //        return;
        //    }
        //}
    }
}