using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace callbot.Dialogs
{
    [Serializable]
    public class MessageDialog : IDialog<object>
    {

        protected string caller { get; set; };

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedStartConversation); // State transition: wait for user to start conversation
        }
        public async Task MessageReceivedStartConversation(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            await context.PostAsync("What's your registration number?");
            context.Wait(""); // State transition: wait for user to provide registration number
        }
    }

   
}