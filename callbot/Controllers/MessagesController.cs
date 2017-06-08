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
using System.Text.RegularExpressions;
using System.Diagnostics;

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
            if (activity.Type == ActivityTypes.Message)
            {
                SurveyDialog sd = new SurveyDialog();

                //store data
                if (activity.Text.ToLower().Contains("call"))
                {
                    // if call, extract name in user typed sentence and pass it to botstate userData update currentAction too
                    // "call" as key
                    var matches = Regex.Match(activity.Text.ToLower(), @"call\s?(.*).?");
                    sd.setData(activity, "call", matches.Groups[1].Value);
                    sd.setData(activity, "currAction", "call");

                    Debug.WriteLine(await sd.getData(activity, "call"));
                    Debug.WriteLine(await sd.getData(activity, "currentAction"));

                    postReply(activity, $"Okay, done setting up {activity.Text}, please call in to continue :)");
                }
                else if (activity.Text.ToLower().Contains("record"))
                {
                    // if call, extract name in user typed sentence and pass it to botstate userData update currentAction too
                    // "record" as key
                    var matches = Regex.Match(activity.Text.ToLower(), @"record\s?(.*).?");
                    sd.setData(activity, "record", matches.Groups[1].Value);
                    sd.setData(activity, "currAction", "record");

                    Debug.WriteLine(await sd.getData(activity, "record"));
                    Debug.WriteLine(await sd.getData(activity, "currentAction"));

                    postReply(activity, $"Okay, done setting up {activity.Text}, please call in to continue :)");
                }
                else
                {
                    //else reply with prompt in text
                    postReply(activity, $"I am not sure what you mean by {activity.Text}. Use: Call ***** or Record *****");
                   
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

        private async void postReply(Activity activity, string msg)
        {
            // creates a reply and post to user
            var client = new ConnectorClient(new Uri(activity.ServiceUrl));
            var outMessage = activity.CreateReply(msg);
            await client.Conversations.SendToConversationAsync(outMessage);
        }
    }
}