using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using callbot.Dialogs;

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
        
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            SurveyDialog sd = new SurveyDialog();

            //get current botstate userdata and see if the settings required configuration, else give generic response
            string currMode = await sd.getData(activity, "activeMode");
            string currAcc = await sd.getData(activity, "activeAcc");
            
            if (currMode == "None" || currAcc == "None")
            {
                if (activity.Text.ToLower().Contains("call") || activity.Text.ToLower().Contains("record"))
                {
                    sd.setData(activity, "activeMode", activity.Text.ToLower());
                }
                else if (activity.Text.ToLower().Contains("as"))
                {
                    sd.setData(activity, "activeAcc", activity.Text.ToLower());
                }
            }
            else
            {
                postReply(activity, "I am ignoring you");
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

        private async void postReply(Activity activity, string msg)
        {
            // creates a reply and post to user
            var client = new ConnectorClient(new Uri(activity.ServiceUrl));
            var outMessage = activity.CreateReply(msg);
            await client.Conversations.SendToConversationAsync(outMessage);
        }
    }
}