using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using System.Text.RegularExpressions;

namespace callbot.Dialogs
{
    [Serializable]
    [LuisModel("9f9431ae-4a39-4ac2-861a-b5ee265f5424", "3368b24c1b4b488d8ee845e7f47a53cd")]
    public class LuisDialog: LuisDialog<object>
    {
        
        private const string PickDateEntityType = "builtin.datetime.date";
        private const string PickTimeEntityType = "builtin.datetime.time";
        private const string PickLocationEntityType = "builtin.geography.city";

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            //await context.PostAsync("Microsoft, why is the documentation so unclear?");
            await context.PostAsync("Meh");

            context.Wait(MessageReceived);            
        }

        [LuisIntent("AboutMe")]
        public async Task AboutMe(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(@"Hello, I am one");
            await context.PostAsync(@"I am none, I am all");
            
            context.Wait(MessageReceived);
        }

        [LuisIntent("CallChildren")]
        public async Task CallChildren(IDialogContext context, LuisResult result)
        {
            if (showMatch(result.Query, "call") == true) {
                await context.PostAsync(@"Caretaker is busy now...");
            }

            context.Wait(MessageReceived);
        }

        [LuisIntent("MedicHelp")]
        public async Task MedicHelp(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(@"Oh my, are you alright?");
            if (showMatch(result.Query, "dizzy") == true)
            {
                await context.PostAsync(@"Lie down and raise your legs up");
            }
            if (showMatch(result.Query, "pain") == true)
            {
                await context.PostAsync(@"Drink some water and a panadol");
            }

            context.Wait(MessageReceived);
        }

        private static bool showMatch(string text, string expr)
        {
            Console.WriteLine("The Expression: " + expr);
            MatchCollection mc = Regex.Matches(text, expr);
            if (mc.Count > 0)
            {
                return true;

            }
            else {
                return false;
            }
        }

    }
}
