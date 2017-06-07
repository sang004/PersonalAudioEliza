using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs;
using callbot.Dialogs;
using System.Configuration;

namespace callbot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        /// 
        private string microsoftAppId { get; } = ConfigurationManager.AppSettings["MicrosoftAppId"];
        private string microsoftAppPassword { get; } = ConfigurationManager.AppSettings["MicrosoftAppPassword"];


        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            
            if (activity.Type == ActivityTypes.Message)
            {

                StateClient stateClient = new StateClient(new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword));
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                //store data
                if (activity.Text.ToLower().Contains("call"))
                {
                    userData.SetProperty<string>("Call", activity.Text);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                }
                else if (activity.Text.ToLower().Contains("record"))
                {
                    var sentGreeting = userData.GetProperty<string>("Call");
                    Console.WriteLine(sentGreeting);
                }
                else {
                    var client = new ConnectorClient(new Uri(activity.ServiceUrl));
                    var outMessage = activity.CreateReply("I am not sure what you mean by: n Use: 'Call <name>' or 'Record <name>'");
                    await client.Conversations.SendToConversationAsync(outMessage);
                    
                }
                //await Conversation.SendAsync(activity, () => new LuisDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
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
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
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