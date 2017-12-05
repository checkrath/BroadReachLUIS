using System;
using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Web.Http.Description;
using System.Net.Http;
using System.Diagnostics;
using LuisBot.Dialogs;

namespace Microsoft.Bot.Sample.LuisBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("SleepyCore");

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
                //RootDialog rootDialog = new RootDialog();
                log.Info("about to send message" + DateTime.Now.ToShortTimeString());
                if (activity.From.Id!= activity.Recipient.Id)
                    await Conversation.SendAsync(activity, () => new RootDialog());
                log.Debug("Out of root dialog");
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
                    //ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                    //string replyText = "Welcome to the BroadReach bot. \r\n" +
                    //    "I can answer questions on your performance or business indicators for specific programmes or districts. Ensure your questions relate to districts, programs and indicators in our database.\r\n" +
                    //    "I can also answer general questions about Broadreach and our offerings.\r\n" +
                    //    "Try: \"What is the Ugu district performance for 2017?\"";

                    //Activity reply = message.CreateReply(replyText);
                  
                    //connector.Conversations.ReplyToActivityAsync(reply);
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