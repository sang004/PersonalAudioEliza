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

        public List<string> target = new List<string>(new string[33] {
            @"I need(.*)", @"Why don'?t you(.*)", @"Why can'?t I(.*)", @"I can'?t(.*)", @"I am(.*)", @"Are you(.*)", @"What(.*)", @"How(.*)",
            @"Because(.*)", @"(.*)sorry(.*)", @"Hello(.*)", @"I think(.*)", @"(.*)friend(.*)", @"Yes(.*)", @"(.*)computer(.*)", @"Is it(.*)", @"It is(.*)",
            @"Can you(.*)", @"Can I(.*)", @"You are(.*)", @"I don'?t(.*)", @"I feel(.*)", @"I have(.*)", @"I would(.*)", @"Is there(.*)",
            @"My(.*)", @"You(.*)", @"Why(.*)", @"I want(.*)", @"(.*)mother(.*)", @"(.*)father(.*)", @"(.*)child(.*)", @"(.*)"});

        public List<string> response = new List<string>(new string[33] {
                                                                      "Why do you need that?",

                                                                      "Do you really think I don't?",

                                                                      "Do you think you should be able to?",

                                                                      "How do you know you can't?",

                                                                      "Did you come to me because of this?",

                                                                      "Why does it matter?",

                                                                      "Why do you ask?",

                                                                      "How do you suppose?",

                                                                       "Is that the real reason?",

                                                                       "There are many times when no apology is needed.",

                                                                       "Hello... I'm glad you could drop by today.",

                                                                       "Do you doubt it?",

                                                                       "Tell me more about your friends.",

                                                                       "You seem quite sure.",

                                                                       "Are you really talking about me?",

                                                                       "Do you think it is?",

                                                                       "You seem very certain.",

                                                                       "What makes you think I can't?",

                                                                       "Perhaps you don't want to.",

                                                                       "Why do you think I am?",

                                                                       "Don't you really?",

                                                                       "Good, tell me more about these feelings.",

                                                                       "Why do you tell me this?",

                                                                       "Could you explain why you would?",

                                                                       "Do you think there is?",

                                                                       "I see.",

                                                                       "We should be discussing you, not me.",

                                                                       "Why don't you tell me the reason why?",

                                                                       "What would it mean to you if you got it?",

                                                                       "Tell me more about your mother..",

                                                                       "Tell me more about your father.",

                                                                       "Did you have close friends as a child?",

                                                                       "Please tell me more.",
            });

        public ElizaDialog()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            BuildPsychobabble();
        }

        public void BuildPsychobabble()
        {

            List<string> response1 = new List<string>(new string[] {"Why do you need that?" });
            psychobabble.Add(@"I need(.*)", response1);

            List<string> response2 = new List<string>(new string[] {"Do you really think I don't?" });
            psychobabble.Add(@"Why don'?t you(.*)", response2);

            List<string> response3 = new List<string>(new string[] {"Do you think you should be able to?" });
            psychobabble.Add(@"Why can'?t I(.*)", response3);

            List<string> response4 = new List<string>(new string[] {"How do you know you can't?" });
            psychobabble.Add(@"I can'?t(.*)", response4);

            List<string> response5 = new List<string>(new string[] {"Did you come to me because of this?" });
            psychobabble.Add(@"I am(.*)", response5);

            List<string> response7 = new List<string>(new string[] {"Why does it matter?" });
            psychobabble.Add(@"Are you(.*)", response7);

            List<string> response8 = new List<string>(new string[] {"Why do you ask?" });
            psychobabble.Add(@"What(.*)", response8);

            List<string> response9 = new List<string>(new string[] {"How do you suppose?" });
            psychobabble.Add(@"How(.*)", response9);

            List<string> response10 = new List<string>(new string[] {"Is that the real reason?" });
            psychobabble.Add(@"Because(.*)", response10);

            List<string> response11 = new List<string>(new string[] {"There are many times when no apology is needed." });
            psychobabble.Add(@"(.*)sorry(.*)", response11);

            List<string> response12 = new List<string>(new string[] {"Hello... I'm glad you could drop by today." });
            psychobabble.Add(@"Hello(.*)", response12);

            List<string> response13 = new List<string>(new string[] {"Do you doubt it?" });
            psychobabble.Add(@"I think(.*)", response13);

            List<string> response14 = new List<string>(new string[] {"Tell me more about your friends." });
            psychobabble.Add(@"(.*)friend(.*)", response14);

            List<string> response15 = new List<string>(new string[] {"You seem quite sure." });
            psychobabble.Add(@"Yes(.*)", response15);

            List<string> response16 = new List<string>(new string[] {"Are you really talking about me?" });
            psychobabble.Add(@"(.*)computer(.*)", response16);

            List<string> response17 = new List<string>(new string[] {"Do you think it is?" });
            psychobabble.Add(@"Is it(.*)", response17);

            List<string> response18 = new List<string>(new string[] {"You seem very certain." });
            psychobabble.Add(@"It is(.*)", response18);

            List<string> response19 = new List<string>(new string[] {"What makes you think I can't?" });
            psychobabble.Add(@"Can you(.*)", response19);

            List<string> response20 = new List<string>(new string[] {"Perhaps you don't want to." });
            psychobabble.Add(@"Can I(.*)", response20);

            List<string> response21 = new List<string>(new string[] {"Why do you think I am?" });
            psychobabble.Add(@"You are(.*)", response21);

            List<string> response23 = new List<string>(new string[] {"Don't you really?" });
            psychobabble.Add(@"I don'?t(.*)", response23);

            List<string> response24 = new List<string>(new string[] {"Good, tell me more about these feelings." });
            psychobabble.Add(@"I feel(.*)", response24);

            List<string> response25 = new List<string>(new string[] {"Why do you tell me this?" });
            psychobabble.Add(@"I have(.*)", response25);

            List<string> response26 = new List<string>(new string[] {"Could you explain why you would?" });
            psychobabble.Add(@"I would(.*)", response26);

            List<string> response27 = new List<string>(new string[] {"Do you think there is?" });
            psychobabble.Add(@"Is there(.*)", response27);

            List<string> response28 = new List<string>(new string[] {"I see." });
            psychobabble.Add(@"My(.*)", response28);

            List<string> response29 = new List<string>(new string[] {"We should be discussing you, not me." });
            psychobabble.Add(@"You(.*)", response29);

            List<string> response30 = new List<string>(new string[] {"Why don't you tell me the reason why?" });
            psychobabble.Add(@"Why(.*)", response30);

            List<string> response31 = new List<string>(new string[] {"What would it mean to you if you got it?" });
            psychobabble.Add(@"I want(.*)", response31);

            List<string> response32 = new List<string>(new string[] {"Tell me more about your mother.." });
            psychobabble.Add(@"(.*)mother(.*)", response32);

            List<string> response33 = new List<string>(new string[] {"Tell me more about your father." });
            psychobabble.Add(@"(.*)father(.*)", response33);

            List<string> response34 = new List<string>(new string[] {"Did you have close friends as a child?" });
            psychobabble.Add(@"(.*)child(.*)", response34);

            List<string> response37 = new List<string>(new string[] {"Please tell me more." });
            psychobabble.Add(@"(.*)", response37);


        }

        static Random rnd = new Random();

        public int RandomResponse(string key)
        {
            List<string> response = psychobabble[key];
            int index = rnd.Next(response.Count);
            //return response[index];
            return index;
        }

        public Task<string> Reply(string text)
        {
            string reply = "";
            Debug.WriteLine($"Text ----- {text}");
            foreach (string pattern in target)
            {
                Debug.WriteLine($"Pattern ----- {pattern}");
                Debug.WriteLine($"Check ----- {Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase)}");
                if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
                {
                    reply = (RandomResponse(pattern)).ToString();
                    break;
                }
            }

            Debug.WriteLine($"Response ----- {reply}");
            return Task.FromResult(reply);
        }

    }
}