using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Resource;
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
    [Serializable]

    public class SurveyDialog : IDialog<object>
    {
        // get variables from web.config
        private string microsoftAppId { get; } = ConfigurationManager.AppSettings["MicrosoftAppId"];
        private string microsoftAppPassword { get; } = ConfigurationManager.AppSettings["MicrosoftAppPassword"];

        public async Task<string> getData(Activity activity, string key)
        {
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
            while (!succeeded);

            return r;
        }

        public async void setData(Activity activity, string key, string value)
        {
            bool succeeded = false;
            int tries = 2;

            do
            {
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
            while (!succeeded);
        }

        public async void createHeroCard(ConnectorClient connector, string recipientId, string botId)
        {

            Dictionary<string, string> cardContentList = new Dictionary<string, string>();
            cardContentList.Add("PigLatin", "https://<ImageUrl1>");
            cardContentList.Add("Bacon", "https://upload.wikimedia.org/wikipedia/commons/3/31/Made20bacon.png");

            string serviceUrl = "https://smba.trafficmanager.net/apis/";
            MicrosoftAppCredentials account = new MicrosoftAppCredentials(ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);

            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(url: cardContentList["Bacon"]));

            List<CardAction> cardButtons = new List<CardAction>();
            var heroCard = new HeroCard()
            {
                Text = "Booo - no *markdown* supported here",
                Buttons = cardButtons
            };

            IMessageActivity newMessage = Activity.CreateMessageActivity();
            newMessage.Type = ActivityTypes.Message;
            newMessage.From = new ChannelAccount(botId, ConfigurationManager.AppSettings["BotId"]);
            newMessage.Conversation = new ConversationAccount(false, recipientId);
            newMessage.Recipient = new ChannelAccount(recipientId);
            newMessage.Text = "Please record the sentence:\n";
            newMessage.Attachments = new List<Attachment> {
                heroCard.ToAttachment()
            };

            await connector.Conversations.SendToConversationAsync((Activity)newMessage);

        }

        public async Task createHeroCardc(IDialogContext context)
        {

            var reply = context.MakeMessage();
            List<CardAction> cardButtons = new List<CardAction>();
            var heroCard = new HeroCard()
            {
                Text = "Booo - no *markdown* supported here",
                Buttons = cardButtons
            };
            reply.Attachments = new List<Attachment> {
                heroCard.ToAttachment()
            };

            await context.PostAsync(reply);

            context.Wait(this.OnOptionSelected);

        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            await this.StartOverAsync(context, message);
        }

        private async Task StartOverAsync(IDialogContext context, string text)
        {
            var message = context.MakeMessage();
            message.Text = text;
            await this.StartOverAsync(context, message);
        }

        private async Task StartOverAsync(IDialogContext context, IMessageActivity message)
        {
            await context.PostAsync(message);
            await this.WelcomeMessageAsync(context);
        }


        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            await this.WelcomeMessageAsync(context);
        }

        private async Task WelcomeMessageAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();
            List<CardAction> cardButtons = new List<CardAction>();
            var heroCard = new HeroCard()
            {
                Text = "Booo - no *markdown* supported here",
                Buttons = cardButtons
            };
            reply.Attachments = new List<Attachment> {
                heroCard.ToAttachment()
            };

            await context.PostAsync(reply);

            context.Wait(this.OnOptionSelected);
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }
    }
}