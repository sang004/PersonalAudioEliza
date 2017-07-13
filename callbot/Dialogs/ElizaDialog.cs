using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace callbot.Dialogs
{
    [Serializable]
    public class ElizaDialog
    {

        public static Dictionary<string, string> reflection = new Dictionary<string, string>();
        public static Dictionary<string, List<string>> psychobabble = new Dictionary<string, List<string>>();

        public List<string> target = new List<string>(new string[28] {
            @"I need(.*)", @"Why don'?t you(.*)", @"Why can'?t I(.*)", @"I can'?t(.*)", @"I am(.*)", @"Are you(.*)", @"What(.*)", @"How(.*)",
            @"Because(.*)", @"(.*)sorry(.*)", @"Hello(.*)", @"I think(.*)", @"Yes(.*)", @"Is it(.*)", @"It is(.*)",
            @"Can you(.*)", @"Can I(.*)", @"You are(.*)", @"I don'?t(.*)", @"I feel(.*)", @"I have(.*)", @"I would(.*)", @"Is there(.*)",
            @"My(.*)", @"You(.*)", @"Why(.*)", @"I want(.*)", @"(.*)"});

        public List<string> response = new List<string>(new string[36] {
                                                                      "Why do you need that?",

                                                                      "Don't temp me",

                                                                      "Do you think you can?",

                                                                      "How do you know you can't?",

                                                                      "Is this why you called me?",

                                                                      "Why does it matter?",

                                                                      "Why do you ask?",

                                                                       "Is that the real reason?",

                                                                       "No worries.",

                                                                       "Hi!",

                                                                       "Do you really think so?",

                                                                       "Sure?",

                                                                       "Do you think it is?",

                                                                       "Yes you are right",

                                                                       "What makes you think I can't?",

                                                                       "For sure you can! Go on.",

                                                                       "Why do you think so?",

                                                                       "Don't you really?",

                                                                       "Why do you feel that?",

                                                                       "Why do you tell me this?",

                                                                       "Why would you?",

                                                                       "I am not sure, google it.",

                                                                       "see~ see~",

                                                                       "We should be talking about you, not me",

                                                                       "Why dont't you tell me the reason",

                                                                       "What would it mean to you if you got it?",

                                                                       "Please tell me more.",

                                                                       "Let's talk about something else",

                                                                       "Can you elaborate on that?",

                                                                       "Why do you say that?",

                                                                       "I see.",

                                                                       "I see. And what does that tell you?",

                                                                       "How do you feel?",

                                                                       "I didn't catch, can you repeat yourself?",

                                                                       "I think the call is breaking up... Bye",

                                                                       "Bye bye",
            });

        public ElizaDialog()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            BuildPsychobabble();
        }

        public void BuildPsychobabble()
        {

            List<string> response1 = new List<string>(new string[] { "Why do you need that?" });
            psychobabble.Add(@"I need(.*)", response1);

            List<string> response2 = new List<string>(new string[] { "Don't temp me" });
            psychobabble.Add(@"Why don'?t you(.*)", response2);

            List<string> response3 = new List<string>(new string[] { "Do you think you can?" });
            psychobabble.Add(@"Why can'?t I(.*)", response3);

            List<string> response4 = new List<string>(new string[] { "How do you know you can't?" });
            psychobabble.Add(@"I can'?t(.*)", response4);

            List<string> response5 = new List<string>(new string[] { "Is this why you called me?" });
            psychobabble.Add(@"I am(.*)", response5);

            List<string> response6 = new List<string>(new string[] { "Why does it matter?" });
            psychobabble.Add(@"Are you(.*)", response6);

            List<string> response7 = new List<string>(new string[] { "Why do you ask?" });
            psychobabble.Add(@"What(.*)", response7);

            List<string> response8 = new List<string>(new string[] { "Why does it matter?" });
            psychobabble.Add(@"How(.*)", response8);

            List<string> response9 = new List<string>(new string[] { "Is that the real reason?" });
            psychobabble.Add(@"Because(.*)", response9);

            List<string> response10 = new List<string>(new string[] { "No worries." });
            psychobabble.Add(@"(.*)sorry(.*)", response10);

            List<string> response11 = new List<string>(new string[] { "Hi!" });
            psychobabble.Add(@"Hello(.*)", response11);

            List<string> response12 = new List<string>(new string[] { "Do you really think so?" });
            psychobabble.Add(@"I think(.*)", response12);

            List<string> response13 = new List<string>(new string[] { "Sure?" });
            psychobabble.Add(@"Yes(.*)", response13);

            List<string> response14 = new List<string>(new string[] { "Do you think it is?" });
            psychobabble.Add(@"Is it(.*)", response14);

            List<string> response15 = new List<string>(new string[] { "Yes you are right" });
            psychobabble.Add(@"It is(.*)", response15);

            List<string> response16 = new List<string>(new string[] { "What makes you think I can't?" });
            psychobabble.Add(@"Can you(.*)", response16);

            List<string> response17 = new List<string>(new string[] { "For sure you can! Go on." });
            psychobabble.Add(@"Can I(.*)", response17);

            List<string> response18 = new List<string>(new string[] { "Why do you think so?" });
            psychobabble.Add(@"You are(.*)", response18);

            List<string> response19 = new List<string>(new string[] { "Don't you really?" });
            psychobabble.Add(@"I don'?t(.*)", response19);

            List<string> response20 = new List<string>(new string[] { "Why do you feel that?" });
            psychobabble.Add(@"I feel(.*)", response20);

            List<string> response21 = new List<string>(new string[] { "Why do you tell me this?" });
            psychobabble.Add(@"I have(.*)", response21);

            List<string> response22 = new List<string>(new string[] { "Why would you?" });
            psychobabble.Add(@"I would(.*)", response22);

            List<string> response23 = new List<string>(new string[] { "I am not sure, google it." });
            psychobabble.Add(@"Is there(.*)", response23);

            List<string> response24 = new List<string>(new string[] { "see~ see~" });
            psychobabble.Add(@"My(.*)", response24);

            List<string> response25 = new List<string>(new string[] { "We should be talking about you, not me" });
            psychobabble.Add(@"You(.*)", response25);

            List<string> response26 = new List<string>(new string[] { "Why dont't you tell me the reason" });
            psychobabble.Add(@"Why(.*)", response26);

            List<string> response27 = new List<string>(new string[] { "What would it mean to you if you got it?" });
            psychobabble.Add(@"I want(.*)", response27);

            List<string> response28 = new List<string>(new string[] {"Please tell me more.",
                                                                     "Let's talk about something else",
                                                                     "Can you elaborate on that?",
                                                                     "Why do you say that?",
                                                                     "I see.",
                                                                     "I see. And what does that tell you?",
                                                                     "How do you feel?"});
            psychobabble.Add(@"(.*)", response28);


        }

        static Random rnd = new Random();

        public string RandomResponse(string key)
        {
            List<string> response = psychobabble[key];
            int index = rnd.Next(response.Count);
            return response[index];
        }

        public Task<string> Reply(string text)
        {
            string reply = "";
            Debug.WriteLine($"Text ----- {text}");
            foreach (string pattern in target)
            {
                //Debug.WriteLine($"Pattern ----- {pattern}");
                //Debug.WriteLine($"Check ----- {Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase)}");
                if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
                {
                    reply = (RandomResponse(pattern)).ToString();
                    Debug.WriteLine($"&&&Response ----- {reply}");
                    break;
                }
            }

            Debug.WriteLine($"Response ----- {reply}");
            return Task.FromResult(reply);
        }

    }
}