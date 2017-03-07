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
            await context.PostAsync("Microsoft, why is the documentation so unclear?");
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

        //[LuisIntent("CallChildren")]
        //public async Task Rent(IDialogContext context, LuisResult result)
        //{
        //    var entities = new List<EntityRecommendation>(result.Entities);
        //    foreach (var entity in result.Entities)
        //    {
        //        switch (entity.Type)
        //        {
        //            case PickLocationEntityType:
        //                entities.Add(new EntityRecommendation(type: nameof(MsgForm.PickLocation)) { Entity = entity.Entity });
        //                break;
        //            case PickDateEntityType:
        //                EntityRecommendation pickTime;
        //                result.TryFindEntity(PickTimeEntityType, out pickTime);
        //                var pickDateAndTime = entity.Entity + " " + pickTime?.Entity;
        //                if (!string.IsNullOrWhiteSpace(pickDateAndTime))
        //                    entities.Add(new EntityRecommendation(type: nameof(MsgForm.PickDateAndTime)) { Entity = pickDateAndTime });
        //                break;
        //            default:
        //                break;
        //        }
        //    }

        //    var MsgForm = new FormDialog<MsgForm>(new MsgForm(), MsgForm.BuildForm, FormOptions.PromptInStart, entities);
        //    context.Call(MsgForm, RentComplete);
        //}

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

        private async Task RentComplete(IDialogContext context, IAwaitable<MsgForm> result)
        {
            try
            {
                var form = await result;

                await context.PostAsync($"Your reservation is confirmed");

                context.Wait(MessageReceived);
            }
            catch (Exception e)
            {
                string reply;
                if (e.InnerException == null)
                {
                    reply = $"You quit --maybe you can finish next time!";
                }
                else
                {
                    reply = "Sorry, I've had a short circuit.  Please try again.";
                }
                await context.PostAsync(reply);
            }
        }
    }
}
