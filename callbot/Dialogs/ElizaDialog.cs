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

        public List<string> target = new List<string>(new string[34] {
            @"I need (.*)", @"Why don'?t you (.*)", @"Why can'?t I (.*)", @"I can'?t (.*)", @"I am (.*)", @"Are you (.*)", @"What (.*)", @"How (.*)",
            @"Because (.*)", @"(.*) sorry (.*)", @"Hello (.*)", @"I think (.*)", @"(.*) friend (.*)", @"Yes", @"(.*) computer (.*)", @"Is it (.*)", @"It is (.*)",
            @"Can you (.*)", @"Can I (.*)", @"You are (.*)", @"I don'?t (.*)", @"I feel (.*)", @"I have (.*)", @"I would (.*)", @"Is there (.*)",
            @"My (.*)", @"You (.*)", @"Why (.*)", @"I want (.*)", @"(.*) mother (.*)", @"(.*) father (.*)", @"(.*) child (.*)", @"bye", @"(.*)"});

        public List<string> response = new List<string>(new string[] {
                                                                      "Why do you need that?",
                                                                      "Would it really help you to get that?",
                                                                      "Are you sure you need that?",

                                                                      "Do you really think I don't?",
                                                                      "Perhaps eventually I will.",
                                                                      "Do you really want me to?",

                                                                      "Do you think you should be able to?",
                                                                      "If you could, what would you do?",
                                                                      "Have you really tried?",
                                                                      "I don't know, why can't you just do it?",

                                                                      "How do you know you can't?",
                                                                      "Perhaps you could, if you tried.",
                                                                      "What would it take for you to do it?",

                                                                      "Did you come to me because of this?",
                                                                      "How long have you been this way?",
                                                                      "How do you feel about it?",


                                                                      "Why does it matter?",
                                                                      "Would you prefer it if I were not?",
                                                                      "Perhaps you believe I am.",
                                                                      "I may be - what do you think?",

                                                                      "Why do you ask?",
                                                                      "How would an answer to that help you?",
                                                                      "What do you think?",

                                                                      "How do you suppose?",
                                                                      "Perhaps you can answer your own question.",
                                                                      "What is it you're really asking?" ,

                                                                       "Is that the real reason?",
                                                                       "What other reasons come to mind?",
                                                                       "Does that reason apply to anything else?",
                                                                       "If that's true, what else must be true?" ,

                                                                       "There are many times when no apology is needed.",
                                                                       "What feelings do you have when you apologize?" ,

                                                                       "Hello... I'm glad you could drop by today.",
                                                                       "Hi there... how are you today?",
                                                                       "Hello, how are you feeling today?" ,

                                                                       "Do you doubt it?",
                                                                       "Do you really think so?",
                                                                       "But you're not sure?" ,

                                                                       "Tell me more about your friends.",
                                                                       "When you think of a friend, what comes to mind?",
                                                                       "Why don't you tell me about a childhood friend?" ,

                                                                       "You seem quite sure.",
                                                                       "OK, but can you elaborate a bit?" ,

                                                                       "Are you really talking about me?",
                                                                       "Does it seem strange to talk to a computer?",
                                                                       "How do computers make you feel?",
                                                                       "Do you feel threatened by computers?" ,

                                                                       "Do you think it is?",
                                                                       "Perhaps it is -- what do you think?",
                                                                       "If it were so, what would you do?",
                                                                       "It could well be that." ,

                                                                       "You seem very certain.",
                                                                       "If I told you that it probably isn't, what would you feel?" ,

                                                                       "What makes you think I can't?",
                                                                       "If I could, then what?",
                                                                       "Why do you ask?" ,

                                                                       "Perhaps you don't want to.",
                                                                       "Do you want to be able to?",
                                                                       "If you could, would you?" ,

                                                                       "Why do you think I am?",
                                                                       "Does it please you to think that I am?",
                                                                       "Perhaps you would like me to be.",
                                                                       "Perhaps you're really talking about yourself?" ,


                                                                       "Don't you really?",
                                                                       "Why don't you?",
                                                                       "Do you want to?" ,

                                                                       "Good, tell me more about these feelings.",
                                                                       "Do you often feel this way?",
                                                                       "When do you usually feel this way?",
                                                                       "When you feel this way, what do you do?" ,

                                                                       "Why do you tell me this?",
                                                                       "Have you really?",
                                                                       "Now that you have, what will you do next?" ,

                                                                       "Could you explain why you would?",
                                                                       "Why would you?",
                                                                       "Who else knows that?" ,

                                                                       "Do you think there is?",
                                                                       "It's likely that there is.",
                                                                       "Would you like there to be?" ,

                                                                       "I see.",
                                                                       "Why do you say that?",
                                                                       "How do you feel?" ,

                                                                       "We should be discussing you, not me.",
                                                                       "Why do you say that about me?",
                                                                       "Why do you care?" ,


                                                                       "Why don't you tell me the reason why?",
                                                                       "Why do you think?" ,

                                                                       "What would it mean to you if you got it?",
                                                                       "Why?",
                                                                       "What would you do if you got it?",
                                                                       "All right, if you got this, then what would you do?" ,

                                                                       "Tell me more about your mother..",
                                                                       "What was your relationship with your mother like?",
                                                                       "How do you feel about your mother?",
                                                                       "How does this relate to your feelings today?",
                                                                       "Good family relations are important." ,

                                                                       "Tell me more about your father.",
                                                                       "How did your father make you feel?",
                                                                       "How do you feel about your father?",
                                                                       "Does your relationship with your father relate to your feelings today?",
                                                                       "Do you have trouble showing affection with your family?" ,

                                                                       "Did you have close friends as a child?",
                                                                       "What is your favorite childhood memory?",
                                                                       "Do you remember any dreams or nightmares from childhood?",
                                                                       "Did the other children sometimes tease you?",
                                                                       "How do you think your childhood experiences relate to your feelings today?" ,

                                                                       "Thank you for talking with me.",
                                                                       "Good-bye." ,

                                                                       "Please tell me more.",
                                                                       "Let's change focus a bit... Tell me about something else.",
                                                                       "Can you elaborate on that?",
                                                                       "Why do you say that?",
                                                                       "I see.",
                                                                       "Very interesting.",
                                                                       "I see.  And what does that tell you?",
                                                                       "How does that make you feel?",
                                                                       "How do you feel when you say that?" ,
            });

        public ElizaDialog()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            BuildPsychobabble();
        }

        public void BuildPsychobabble()
        {

            List<string> response1 = new List<string>(new string[3] {"Why do you need that?",
                                                                      "Would it really hep you to get that?",
                                                                      "Are you sure you need that?" });
            psychobabble.Add(@"I need (.*)", response1);

            List<string> response2 = new List<string>(new string[3] {"Do you really think I don't?",
                                                                      "Perhaps eventually I will.",
                                                                      "Do you really want me to?" });
            psychobabble.Add(@"Why don'?t you (.*)", response2);

            List<string> response3 = new List<string>(new string[4] {"Do you think you should be able to?",
                                                                      "If you could, what would you do?",
                                                                      "Have you really tried?",
                                                                      "I don't know, why can't you just do it?"});
            psychobabble.Add(@"Why can'?t I (.*)", response3);

            List<string> response4 = new List<string>(new string[3] {"How do you know you can't?",
                                                                      "Perhaps you could, if you tried.",
                                                                      "What would it take for you to do it?" });
            psychobabble.Add(@"I can'?t (.*)", response4);

            List<string> response5 = new List<string>(new string[3] {"Did you come to me because of this?",
                                                                      "How long have you been this way?",
                                                                      "How do you feel about it?"});
            psychobabble.Add(@"I am (.*)", response5);


            List<string> response7 = new List<string>(new string[4] {"Why does it matter?",
                                                                      "Would you prefer it if I were not?",
                                                                      "Perhaps you believe I am.",
                                                                      "I may be - what do you think?" });
            psychobabble.Add(@"Are you (.*)", response7);

            List<string> response8 = new List<string>(new string[3] {"Why do you ask?",
                                                                      "How would an answer to that help you?",
                                                                      "What do you think?" });
            psychobabble.Add(@"What (.*)", response8);

            List<string> response9 = new List<string>(new string[3] {"How do you suppose?",
                                                                      "Perhaps you can answer your own question.",
                                                                      "What is it you're really asking?" });
            psychobabble.Add(@"How (.*)", response9);

            List<string> response10 = new List<string>(new string[4] {"Is that the real reason?",
                                                                       "What other reasons come to mind?",
                                                                       "Does that reason apply to anything else?",
                                                                       "If that's true, what else must be true?" });
            psychobabble.Add(@"Because (.*)", response10);

            List<string> response11 = new List<string>(new string[2] {"There are many times when no apology is needed.",
                                                                       "What feelings do you have when you apologize?" });
            psychobabble.Add(@"(.*) sorry (.*)", response11);

            List<string> response12 = new List<string>(new string[3] {"Hello... I'm glad you could drop by today.",
                                                                       "Hi there... how are you today?",
                                                                       "Hello, how are you feeling today?" });
            psychobabble.Add(@"Hello (.*)", response12);

            List<string> response13 = new List<string>(new string[3] {"Do you doubt it?",
                                                                       "Do you really think so?",
                                                                       "But you're not sure?" });
            psychobabble.Add(@"I think (.*)", response13);

            List<string> response14 = new List<string>(new string[3] {"Tell me more about your friends.",
                                                                       "When you think of a friend, what comes to mind?",
                                                                       "Why don't you tell me about a childhood friend?" });
            psychobabble.Add(@"(.*) friend (.*)", response14);

            List<string> response15 = new List<string>(new string[2] {"You seem quite sure.",
                                                                       "OK, but can you elaborate a bit?" });
            psychobabble.Add(@"Yes", response15);

            List<string> response16 = new List<string>(new string[4] {"Are you really talking about me?",
                                                                       "Does it seem strange to talk to a computer?",
                                                                       "How do computers make you feel?",
                                                                       "Do you feel threatened by computers?" });
            psychobabble.Add(@"(.*) computer (.*)", response16);

            List<string> response17 = new List<string>(new string[4] {"Do you think it is?",
                                                                       "Perhaps it is -- what do you think?",
                                                                       "If it were so, what would you do?",
                                                                       "It could well be that." });
            psychobabble.Add(@"Is it (.*)", response17);

            List<string> response18 = new List<string>(new string[2] {"You seem very certain.",
                                                                       "If I told you that it probably isn't, what would you feel?" });
            psychobabble.Add(@"It is (.*)", response18);

            List<string> response19 = new List<string>(new string[3] {"What makes you think I can't?",
                                                                       "If I could, then what?",
                                                                       "Why do you ask?" });
            psychobabble.Add(@"Can you (.*)", response19);

            List<string> response20 = new List<string>(new string[3] {"Perhaps you don't want to.",
                                                                       "Do you want to be able to?",
                                                                       "If you could, would you?" });
            psychobabble.Add(@"Can I (.*)", response20);

            List<string> response21 = new List<string>(new string[4] {"Why do you think I am?",
                                                                       "Does it please you to think that I am?",
                                                                       "Perhaps you would like me to be.",
                                                                       "Perhaps you're really talking about yourself?" });
            psychobabble.Add(@"You are (.*)", response21);


            List<string> response23 = new List<string>(new string[3] {"Don't you really?",
                                                                       "Why don't you?",
                                                                       "Do you want to?" });
            psychobabble.Add(@"I don'?t (.*)", response23);

            List<string> response24 = new List<string>(new string[4] {"Good, tell me more about these feelings.",
                                                                       "Do you often feel this way?",
                                                                       "When do you usually feel this way?",
                                                                       "When you feel this way, what do you do?" });
            psychobabble.Add(@"I feel (.*)", response24);

            List<string> response25 = new List<string>(new string[3] {"Why do you tell me this?",
                                                                       "Have you really?",
                                                                       "Now that you have, what will you do next?" });
            psychobabble.Add(@"I have (.*)", response25);

            List<string> response26 = new List<string>(new string[3] {"Could you explain why you would?",
                                                                       "Why would you?",
                                                                       "Who else knows that?" });
            psychobabble.Add(@"I would (.*)", response26);

            List<string> response27 = new List<string>(new string[3] {"Do you think there is?",
                                                                       "It's likely that there is.",
                                                                       "Would you like there to be?" });
            psychobabble.Add(@"Is there (.*)", response27);

            List<string> response28 = new List<string>(new string[3] {"I see.",
                                                                       "Why do you say that?",
                                                                       "How do you feel?" });
            psychobabble.Add(@"My (.*)", response28);

            List<string> response29 = new List<string>(new string[3] {"We should be discussing you, not me.",
                                                                       "Why do you say that about me?",
                                                                       "Why do you care?" });
            psychobabble.Add(@"You (.*)", response29);


            List<string> response30 = new List<string>(new string[2] {"Why don't you tell me the reason why?",
                                                                       "Why do you think?" });
            psychobabble.Add(@"Why (.*)", response30);

            List<string> response31 = new List<string>(new string[4] {"What would it mean to you if you got it?",
                                                                       "Why?",
                                                                       "What would you do if you got it?",
                                                                       "All right, if you got this, then what would you do?" });
            psychobabble.Add(@"I want (.*)", response31);

            List<string> response32 = new List<string>(new string[5] {"Tell me more about your mother..",
                                                                       "What was your relationship with your mother like?",
                                                                       "How do you feel about your mother?",
                                                                       "How does this relate to your feelings today?",
                                                                       "Good family relations are important." });
            psychobabble.Add(@"(.*) mother (.*)", response32);

            List<string> response33 = new List<string>(new string[5] {"Tell me more about your father.",
                                                                       "How did your father make you feel?",
                                                                       "How do you feel about your father?",
                                                                       "Does your relationship with your father relate to your feelings today?",
                                                                       "Do you have trouble showing affection with your family?" });
            psychobabble.Add(@"(.*) father (.*)", response33);

            List<string> response34 = new List<string>(new string[5] {"Did you have close friends as a child?",
                                                                       "What is your favorite childhood memory?",
                                                                       "Do you remember any dreams or nightmares from childhood?",
                                                                       "Did the other children sometimes tease you?",
                                                                       "How do you think your childhood experiences relate to your feelings today?" });
            psychobabble.Add(@"(.*) child (.*)", response34);

            List<string> response36 = new List<string>(new string[2] {"Thank you for talking with me.",
                                                                       "Good-bye." });
            psychobabble.Add(@"bye", response36);

            List<string> response37 = new List<string>(new string[9] {"Please tell me more.",
                                                                       "Let's change focus a bit... Tell me about something else.",
                                                                       "Can you elaborate on that?",
                                                                       "Why do you say that?",
                                                                       "I see.",
                                                                       "Very interesting.",
                                                                       "I see.  And what does that tell you?",
                                                                       "How does that make you feel?",
                                                                       "How do you feel when you say that?" });
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

            foreach (string pattern in target)
            {
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