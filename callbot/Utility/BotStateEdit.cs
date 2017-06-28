using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;
using Microsoft.Bot.Connector;
using Microsoft.Rest;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;

namespace callbot.utility
{
    public static class BotStateEdit
    {
        private static string microsoftAppId { get; } = ConfigurationManager.AppSettings["MicrosoftAppId"];
        private static string microsoftAppPassword { get; } = ConfigurationManager.AppSettings["MicrosoftAppPassword"];

        private static int limit = 10;

        // gets user variables stored in BotState
        public static string getUserData(IEnumerable<Participant> p, string field, ref int count)
        {

            // create the and retrieve
            StateClient stateClient = new StateClient(new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword));
            BotData userData = stateClient.BotState.GetUserDataAsync("skype", p.ElementAt(0).Identity).Result;

            string output = "";
            try
            {
                output = userData.GetProperty<string>(field);

            }
            // if condition 412 appears, it will retry recursively until number of tries is more than limit
            catch (HttpOperationException)
            {
                if (count < limit)
                {
                    count++;
                    Thread.Sleep(300);
                    return getUserData(p, field, ref count);
                }
                else {
                    return "1";
                }
                
            }
            return output;
        }

        // overloaded getUserData function that uses activity from IMessage
        public static string getUserData(Activity activity, string field, ref int count)
        {

            // create the and retrieve
            StateClient stateClient = new StateClient(new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword));
            BotData userData = stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id).Result;

            string output = "";
            try
            {
                output = userData.GetProperty<string>(field);

            }
            catch (HttpOperationException)
            {
                if (count < limit)
                {
                    count++;
                    Thread.Sleep(300);
                    return getUserData(activity, field, ref count);
                }
                else
                {
                    return "1";
                }

            }
            return output;
        }

        // sets user variables stored in BotState
        public static string setUserData(IEnumerable<Participant> p, string field, string value, ref int count)
        {
            // create the and retrieve
            StateClient stateClient = new StateClient(new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword));
            BotData userData = stateClient.BotState.GetUserDataAsync("skype", p.ElementAt(0).Identity).Result;

            try
            {
                userData.SetProperty<string>(field, value);
                stateClient.BotState.SetUserDataAsync("skype", p.ElementAt(0).Identity, userData);
            }
            catch (HttpOperationException)
            {
                if (count < limit)
                {
                    count++;
                    Thread.Sleep(300);
                    return setUserData(p, field, value, ref count);

                }
            }
            return "1";
        }

        // overloaded setUserData function that uses activity from IMessage
        public static string setUserData(Activity activity, string field, string value, ref int count)
        {
            // create the and retrieve
            StateClient stateClient = new StateClient(new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword));
            BotData userData = stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id).Result;

            try
            {
                userData.SetProperty<string>(field, value);
                stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
            }
            catch (HttpOperationException)
            {
                if (count < limit)
                {
                    count++;
                    Thread.Sleep(300);
                    return setUserData(activity, field, value, ref count);

                }
            }
            return "1";
        }

        // removes variable in user data stored in BotState
        public static string removeUserData(IEnumerable<Participant> p, string field, ref int count)
        {
            // create the and retrieve
            StateClient stateClient = new StateClient(new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword));
            BotData userData = stateClient.BotState.GetUserDataAsync("skype", p.ElementAt(0).Identity).Result;

            try
            {
                userData.RemoveProperty(field);
                stateClient.BotState.SetUserDataAsync("skype", p.ElementAt(0).Identity, userData);
            }
            catch (HttpOperationException)
            {
                if (count < limit)
                {
                    count++;
                    Thread.Sleep(300);
                    return removeUserData(p, field, ref count);

                }
            }
            return "1";
        }
    }
}