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
        public async Task StartAsync(IDialogContext context)
        {
            //initial prompt for what's missing
            await context.PostAsync($"Hello there. This bot does XYZ");
            context.Wait(MessageReceivedAsync); // State transition: wait for user to start conversation

        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            string textIn = (await argument).Text;
            await context.PostAsync($"You said:{textIn}");
            string output = await GetJsonFromLUIS(textIn);
            LuisResultFromJson luisOutput = new LuisResultFromJson(output);
            context.Wait(MessageReceivedAsync);
            //int result;

            ////if its a number, assign and return. Otherwise ask again
            //if (int.TryParse(textIn, out result))
            //{
            //    this.DateRange = textIn;

            //    //await context.PostAsync($"Thanks for your date range of {DateRange}");
            //    //context.Wait(MessageReceivedRegistrationNumber); // State transition: wait for user to provide registration number
            //    context.Done<string>(this.DateRange);
            //}
            //else
            //{
            //    await context.PostAsync($"{textIn} is not a valid year. PLease entre a valid year.");
            //    context.Wait(MessageReceivedStartConversation);
            //}
        }

        /// <summary>
        /// Call the LUIS service
        /// </summary>
        /// <param name="userQuery"></param>
        /// <returns></returns>
        private async Task<string> GetJsonFromLUIS(string userQuery)
        {
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