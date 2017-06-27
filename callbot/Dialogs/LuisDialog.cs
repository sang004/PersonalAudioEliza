using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace callbot.Dialogs
{
    [Serializable]
    [LuisModel("9f9431ae-4a39-4ac2-861a-b5ee265f5424", "3368b24c1b4b488d8ee845e7f47a53cd")]
    //[LuisModel("3091b2f3-d34b-4b8b-8a1e-96de8ab7baac", "b068f279cb5f4881a1a37bc1a13be595")]

    public class LuisDialog: LuisDialog<object>
    {
        
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            //await context.PostAsync("Microsoft, why is the documentation so unclear?");
            await context.PostAsync("None");

            context.Wait(MessageReceived);  
        }

        [LuisIntent("AboutMe")]
        public async Task AboutMe(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("What");

            //await context.PostAsync(@"Hello, I am one");
            //await context.PostAsync(@"I am none, I am all");

            context.Wait(MessageReceived);
        }

        [LuisIntent("CallChildren")]
        public async Task CallChildren(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Comfort");

            //if (showMatch(result.Query, "call") == true) {
            //    await context.PostAsync(@"Caretaker is busy now...");
            //}

            context.Wait(MessageReceived);
        }

        [LuisIntent("MedicHelp")]
        public async Task MedicHelp(IDialogContext context, LuisResult result)
        {

            var entities = new List<EntityRecommendation>(result.Entities);
            foreach (var entity in result.Entities)
            {
                switch (entity.Type)
                {
                    case "MedicDescription":
                        //entities.Add(new EntityRecommendation(type: nameof(RentForm.PickLocation)) { Entity = entity.Entity });
                        EntityRecommendation sick;
                        if (result.TryFindEntity("MedicDescription", out sick)) {
                            await context.PostAsync(@"mp3test");
                        }

                        break;
                    case "BodyPart":
                        EntityRecommendation bleed;
                        if (result.TryFindEntity("BodyPart", out bleed)) {
                            await context.PostAsync(@"mp3test");
                        }

                        break;
                    
                    // so if it cannot identify any useful entity type, use entity word itself to rephrase into a question.
                    case "Noun":
                        string word = entity.Entity;
                        await context.PostAsync($"What is {word}?");

                        break;

                    default:
                        break;
                }
            }
            await context.PostAsync(@"What is word?");



            //await context.PostAsync(@"Oh my, are you alright?");
            //if (showMatch(result.Query, "dizzy") == true)
            //{
            //    await context.PostAsync(@"Lie down and raise your legs up");
            //}
            //if (showMatch(result.Query, "pain") == true)
            //{
            //    await context.PostAsync(@"Drink some water and a panadol");
            //}

            context.Wait(MessageReceived);
        }

        [LuisIntent("Bored")]
        public async Task Bored(IDialogContext context, LuisResult result)
        {
            var entities = new List<EntityRecommendation>(result.Entities);
            foreach (var entity in result.Entities)
            {
                switch (entity.Type)
                {
                    case "MedicDescription":
                        //entities.Add(new EntityRecommendation(type: nameof(RentForm.PickLocation)) { Entity = entity.Entity });
                        EntityRecommendation painLevel;
                        result.TryFindEntity("BodyPart", out painLevel);
                        break;
                    case "BodyPart":
                        EntityRecommendation bleed;
                        result.TryFindEntity("BodyPart", out bleed);
                        break;
                    default:
                        break;
                }
            }


            await context.PostAsync("hum");

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
