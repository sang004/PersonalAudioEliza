using Microsoft.Bot.Connector;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace callbot.Dialogs
{
    public class SurveyDialog
    {
        // get variables from web.config
        private string microsoftAppId { get; } = ConfigurationManager.AppSettings["MicrosoftAppId"];
        private string microsoftAppPassword { get; } = ConfigurationManager.AppSettings["MicrosoftAppPassword"];
        
        public async Task<string> getData(Activity activity, string key) {
            string r = "";
            bool succeeded = false;
            int tries = 2;

            //retry 2 times or break if set data succeeded.
            do
            {
                //try catch handles lock in simultaneous access to userData
                try
                {
                    //open user data from stateclient that connects to the bot itself on bot framework
                    StateClient stateClient = new StateClient(new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword));
                    BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                    r = userData.GetProperty<string>(key);
                    succeeded = true;

                }
                catch (HttpOperationException err)
                {
                    tries--;
                }
                
            }
            while (!succeeded && tries > 0);

            return r;
        }

        public async void setData(Activity activity, string key, string value)
        {
            bool succeeded = false;
            int tries = 2;

            do { 
                try
                {
                    StateClient stateClient = new StateClient(new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword));
                    BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                    
                    // different here that it sets data instead of get
                    userData.SetProperty<string>(key, value);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    succeeded = true;
                }
                catch (HttpOperationException err)
                {
                    tries--;
                }
            }
            while (!succeeded && tries > 0);
        }
        
    }
}