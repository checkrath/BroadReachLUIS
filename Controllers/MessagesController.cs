using System;
using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Web.Http.Description;
using System.Net.Http;
using System.Diagnostics;

namespace Microsoft.Bot.Sample.LuisBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            // check if activity is of type message
            if (activity.GetActivityType() == ActivityTypes.Message)
            {
                //await Conversation.SendAsync(activity, () => new BasicQnAMakerDialog());
                //var luisDialog = new BasicLuisDialog();
                //await Conversation.SendAsync(activity, () => new BasicLuisDialog());
                await Conversation.SendAsync(activity, () => new RootDialog());
                //if (!luisDialog.isHandled)
                //{
                //    await Conversation.SendAsync(activity, () => new BasicQnAMakerDialog());
                //}
            }
            else
            {
                HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                if ( message.MembersAdded[0].Id== message.Recipient.Id)
                {
                    ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                    Activity reply = message.CreateReply("I am your service provider virtual assistant, How can I help you today? ");
                    //    await context.PostAsync("Welcome to the BroadReach bot.");
                    //    await context.PostAsync($"I can answer questions on your performance or business indicators for specific programmes or districts. Ensure your questions relate to districts, programs and indicators in our database.");
                    //    await context.PostAsync($"I can also answer general questions about Broadreach and our offerings.");
                    //    await context.PostAsync($"Try: \"What is the Ugu district performance for 2017?\"");

                    connector.Conversations.ReplyToActivityAsync(reply);
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}