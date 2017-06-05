using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace callbot.Dialogs
{
    [Serializable]
    public class ElizaDialog //: IDialog<Object>
    {
        public Dictionary<string, string> reflection = new Dictionary<string, string>();
        public Dictionary<string, List<string>> psychobabble = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> psychobabble_ori = new Dictionary<string, List<string>>();

        public List<string> targetori = new List<string>(new string[34] {
            @"I need (.*)", @"Why don'?t you (.*)", @"Why can'?t I (.*)", @"I can'?t (.*)", @"I am (.*)", @"Are you (.*)", @"What (.*)", @"How (.*)",
            @"Because (.*)", @"(.*) sorry (.*)", @"Hello (.*)", @"I think (.*)", @"(.*) friend (.*)", @"Yes", @"(.*) computer (.*)", @"Is it (.*)", @"It is (.*)",
            @"Can you (.*)", @"Can I (.*)", @"You are (.*)", @"I don'?t (.*)", @"I feel (.*)", @"I have (.*)", @"I would (.*)", @"Is there (.*)",
            @"My (.*)", @"You (.*)", @"Why (.*)", @"I want (.*)", @"(.*) mother (.*)", @"(.*) father (.*)", @"(.*) child (.*)", @"bye", @"(.*)"});

        public List<string> target = new List<string>(new string[34] {
            @"I need", @"Why don't you", @"Why can't I", @"I can't", @"I am", @"Are you", @"What", @"How",
            @"Because", @" sorry", @"Hello", @"I think", @" friend", @"Yes", @" computer", @"Is it", @"It is",
            @"Can you", @"Can I", @"You are", @"I don't", @"I feel", @"I have", @"I would", @"Is there",
            @"My", @"You", @"Why", @"I want", @" mother", @" father", @" child", @"bye", @""});

        public ElizaDialog()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            BuildPsychobabble();
            BuildPsychobabble_ori();
            BuildReflection();
        }

        public void BuildReflection()
        {
            reflection.Add("am", "are");
            reflection.Add("was", "were");
            reflection.Add("i", "you");
            reflection.Add("i'd", "you would");
            reflection.Add("i've", "you have");
            reflection.Add("i'll", "you will");
            reflection.Add("my", "your");
            reflection.Add("are", "am");
            reflection.Add("you've", "I have");
            reflection.Add("you'll", "I will");
            reflection.Add("your", "my");
            reflection.Add("yours", "mine");
            reflection.Add("you", "me");
            reflection.Add("me", "you");
        }

        public void BuildPsychobabble_ori()
        {

            List<string> response1 = new List<string>(new string[3] {"Why do you need {0}?",
                                                                      "Would it really help you to get {0}?",
                                                                      "Are you sure you need {0}?" });
            psychobabble_ori.Add(@"I need (.*)", response1);

            List<string> response2 = new List<string>(new string[3] {"Do you really think I don't {0}?",
                                                                      "Perhaps eventually I will {0}.",
                                                                      "Do you really want me to {0}?" });
            psychobabble_ori.Add(@"Why don'?t you (.*)", response2);

            List<string> response3 = new List<string>(new string[4] {"Do you think you should be able to {0}?",
                                                                      "If you could {0}, what would you do?",
                                                                      "Have you really tried?",
                                                                      "I don't know, why can't you {0}?"});
            psychobabble_ori.Add(@"Why can'?t I (.*)", response3);

            List<string> response4 = new List<string>(new string[3] {"How do you know you can't {0}?",
                                                                      "Perhaps you could {0}, if you tried.",
                                                                      "What would it take for you to {0}?" });
            psychobabble_ori.Add(@"I can'?t (.*)", response4);

            List<string> response5 = new List<string>(new string[3] {"Did you come to me because of {0} this?",
                                                                      "How long have you been {0}?",
                                                                      "How do you feel about been {0}?"});
            psychobabble_ori.Add(@"I am (.*)", response5);


            List<string> response7 = new List<string>(new string[4] {"Why does it matter whether I am {0}?",
                                                                      "Would you prefer it if I were not {0}?",
                                                                      "Perhaps you believe I am {0}.",
                                                                      "I may be {0} - what do you think?" });
            psychobabble_ori.Add(@"Are you (.*)", response7);

            List<string> response8 = new List<string>(new string[3] {"Why do you ask?",
                                                                      "How would an answer to that help you?",
                                                                      "What do you think?" });
            psychobabble_ori.Add(@"What (.*)", response8);

            List<string> response9 = new List<string>(new string[3] {"How do you suppose?",
                                                                      "Perhaps you can answer your own question.",
                                                                      "What is it you're really asking?" });
            psychobabble_ori.Add(@"How (.*)", response9);

            List<string> response10 = new List<string>(new string[4] {"Is that the real reason?",
                                                                       "What other reasons come to mind?",
                                                                       "Does that reason apply to anything else?",
                                                                       "If {0}, what else must be true?" });
            psychobabble_ori.Add(@"Because (.*)", response10);

            List<string> response11 = new List<string>(new string[2] {"There are many times when no apology is needed.",
                                                                       "What feelings do you have when you apologize?" });
            psychobabble_ori.Add(@"(.*) sorry (.*)", response11);

            List<string> response12 = new List<string>(new string[3] {"Hello... I'm glad you could drop by today.",
                                                                       "Hi there... how are you today?",
                                                                       "Hello, how are you feeling today?" });
            psychobabble_ori.Add(@"Hello (.*)", response12);

            List<string> response13 = new List<string>(new string[3] {"Do you doubt {0}?",
                                                                       "Do you really think so?",
                                                                       "But you're not sure {0}?" });
            psychobabble_ori.Add(@"I think (.*)", response13);

            List<string> response14 = new List<string>(new string[3] {"Tell me more about your friends.",
                                                                       "When you think of a friend, what comes to mind?",
                                                                       "Why don't you tell me about a childhood friend?" });
            psychobabble_ori.Add(@"(.*) friend (.*)", response14);

            List<string> response15 = new List<string>(new string[2] {"You seem quite sure.",
                                                                       "OK, but can you elaborate a bit?" });
            psychobabble_ori.Add(@"Yes", response15);

            List<string> response16 = new List<string>(new string[4] {"Are you really talking about me?",
                                                                       "Does it seem strange to talk to a computer?",
                                                                       "How do computers make you feel?",
                                                                       "Do you feel threatened by computers?" });
            psychobabble_ori.Add(@"(.*) computer (.*)", response16);

            List<string> response17 = new List<string>(new string[4] {"Do you think it is {0}?",
                                                                       "Perhaps it is {0} -- what do you think?",
                                                                       "If it were {0}, what would you do?",
                                                                       "It could well be that {0}." });
            psychobabble_ori.Add(@"Is it (.*)", response17);

            List<string> response18 = new List<string>(new string[2] {"You seem very certain.",
                                                                       "If I told you that it probably isn't, what would you feel?" });
            psychobabble_ori.Add(@"It is (.*)", response18);

            List<string> response19 = new List<string>(new string[3] {"What makes you think I can't {0}?",
                                                                       "If I could {0}, then what?",
                                                                       "Why do you ask if I can {0}?" });
            psychobabble_ori.Add(@"Can you (.*)", response19);

            List<string> response20 = new List<string>(new string[3] {"Perhaps you don't want to {0}.",
                                                                       "Do you want to be able to {0}?",
                                                                       "If you could {0}, would you?" });
            psychobabble_ori.Add(@"Can I (.*)", response20);

            List<string> response21 = new List<string>(new string[4] {"Why do you think I am {0}?",
                                                                       "Does it please you to think that I am {0}?",
                                                                       "Perhaps you would like me to be {0}?",
                                                                       "Perhaps you're really talking about yourself?" });
            psychobabble_ori.Add(@"You are (.*)", response21);


            List<string> response23 = new List<string>(new string[3] {"Don't you really {0}?",
                                                                       "Why don't you {0}?",
                                                                       "Do you want to {0}?" });
            psychobabble_ori.Add(@"I don'?t (.*)", response23);

            List<string> response24 = new List<string>(new string[4] {"Good, tell me more about these feelings.",
                                                                       "Do you often feel {0}?",
                                                                       "When do you usually feel {0}?",
                                                                       "When you feel {0}, what do you do?" });
            psychobabble_ori.Add(@"I feel (.*)", response24);

            List<string> response25 = new List<string>(new string[3] {"Why do you tell me that you've {0}?",
                                                                       "Have you really {0}?",
                                                                       "Now that you have {0}, what will you do next?" });
            psychobabble_ori.Add(@"I have (.*)", response25);

            List<string> response26 = new List<string>(new string[3] {"Could you explain why you would {0}?",
                                                                       "Why would you {0}?",
                                                                       "Who else knows that you would {0}?" });
            psychobabble_ori.Add(@"I would (.*)", response26);

            List<string> response27 = new List<string>(new string[3] {"Do you think there is {0}?",
                                                                       "It's likely that there is {0}.",
                                                                       "Would you like there to be {0}?" });
            psychobabble_ori.Add(@"Is there (.*)", response27);

            List<string> response28 = new List<string>(new string[3] {"I see, your {0}.",
                                                                       "Why do you say that your {0}?",
                                                                       "When your {0}, how do you feel?" });
            psychobabble_ori.Add(@"My (.*)", response28);

            List<string> response29 = new List<string>(new string[3] {"We should be discussing you, not me.",
                                                                       "Why do you say that about me?",
                                                                       "Why do you care whether I {0}?" });
            psychobabble_ori.Add(@"You (.*)", response29);


            List<string> response30 = new List<string>(new string[2] {"Why don't you tell me the reason why {0}?",
                                                                       "Why do you think {0}?" });
            psychobabble_ori.Add(@"Why (.*)", response30);

            List<string> response31 = new List<string>(new string[4] {"What would it mean to you if you got {0}?",
                                                                       "Why do you want {0}?",
                                                                       "What would you do if you got {0}?",
                                                                       "All right, if you got {0}, then what would you do?" });
            psychobabble_ori.Add(@"I want (.*)", response31);

            List<string> response32 = new List<string>(new string[5] {"Tell me more about your mother..",
                                                                       "What was your relationship with your mother like?",
                                                                       "How do you feel about your mother?",
                                                                       "How does this relate to your feelings today?",
                                                                       "Good family relations are important." });
            psychobabble_ori.Add(@"(.*) mother (.*)", response32);

            List<string> response33 = new List<string>(new string[5] {"Tell me more about your father.",
                                                                       "How did your father make you feel?",
                                                                       "How do you feel about your father?",
                                                                       "Does your relationship with your father relate to your feelings today?",
                                                                       "Do you have trouble showing affection with your family?" });
            psychobabble_ori.Add(@"(.*) father (.*)", response33);

            List<string> response34 = new List<string>(new string[5] {"Did you have close friends as a child?",
                                                                       "What is your favorite childhood memory?",
                                                                       "Do you remember any dreams or nightmares from childhood?",
                                                                       "Did the other children sometimes tease you?",
                                                                       "How do you think your childhood experiences relate to your feelings today?" });
            psychobabble_ori.Add(@"(.*) child (.*)", response34);


            List<string> response36 = new List<string>(new string[2] {"Thank you for talking with me.",
                                                                       "Good-bye." });
            psychobabble_ori.Add(@"bye", response36);

            List<string> response37 = new List<string>(new string[10] {"Please tell me more.",
                                                                       "Let's change focus a bit... Tell me about something else.",
                                                                       "Can you elaborate on that?",
                                                                       "Why do you say that {0}?",
                                                                       "I see.",
                                                                       "Very interesting.",
                                                                       "{0}",
                                                                       "I see.  And what does that tell you?",
                                                                       "How does that make you feel?",
                                                                       "How do you feel when you say that?" }); 
            psychobabble_ori.Add(@"(.*)", response37);

        }

        public void BuildPsychobabble()
        {

            List<string> response1 = new List<string>(new string[3] {"Why do you need {0}?",
                                                                      "Would it really help you to get {0}?",
                                                                      "Are you sure you need {0}?" });
            psychobabble.Add(@"I need", response1);

            List<string> response2 = new List<string>(new string[3] {"Do you really think I don't {0}?",
                                                                      "Perhaps eventually I will {0}.",
                                                                      "Do you really want me to {0}?" });
            psychobabble.Add(@"Why don't you", response2);

            List<string> response3 = new List<string>(new string[4] {"Do you think you should be able to {0}?",
                                                                      "If you could {0}, what would you do?",
                                                                      "Have you really tried?",
                                                                      "I don't know, why can't you {0}?"});
            psychobabble.Add(@"Why can't I", response3);

            List<string> response4 = new List<string>(new string[3] {"How do you know you can't {0}?",
                                                                      "Perhaps you could {0}, if you tried.",
                                                                      "What would it take for you to {0}?" });
            psychobabble.Add(@"I can't", response4);

            List<string> response5 = new List<string>(new string[3] {"Did you come to me because of {0} this?",
                                                                      "How long have you been {0}?",
                                                                      "How do you feel about been {0}?"});
            psychobabble.Add(@"I am", response5);


            List<string> response7 = new List<string>(new string[4] {"Why does it matter whether I am {0}?",
                                                                      "Would you prefer it if I were not {0}?",
                                                                      "Perhaps you believe I am {0}.",
                                                                      "I may be {0} - what do you think?" });
            psychobabble.Add(@"Are you", response7);

            List<string> response8 = new List<string>(new string[3] {"Why do you ask?",
                                                                      "How would an answer to that help you?",
                                                                      "What do you think?" });
            psychobabble.Add(@"What", response8);

            List<string> response9 = new List<string>(new string[3] {"How do you suppose?",
                                                                      "Perhaps you can answer your own question.",
                                                                      "What is it you're really asking?" });
            psychobabble.Add(@"How", response9);

            List<string> response10 = new List<string>(new string[4] {"Is that the real reason?",
                                                                       "What other reasons come to mind?",
                                                                       "Does that reason apply to anything else?",
                                                                       "If {0}, what else must be true?" });
            psychobabble.Add(@"Because", response10);

            List<string> response11 = new List<string>(new string[2] {"There are many times when no apology is needed.",
                                                                       "What feelings do you have when you apologize?" });
            psychobabble.Add(@" sorry ", response11);

            List<string> response12 = new List<string>(new string[3] {"Hello... I'm glad you could drop by today.",
                                                                       "Hi there... how are you today?",
                                                                       "Hello, how are you feeling today?" });
            psychobabble.Add(@"Hello", response12);

            List<string> response13 = new List<string>(new string[3] {"Do you doubt {0}?",
                                                                       "Do you really think so?",
                                                                       "But you're not sure {0}?" });
            psychobabble.Add(@"I think", response13);

            List<string> response14 = new List<string>(new string[3] {"Tell me more about your friends.",
                                                                       "When you think of a friend, what comes to mind?",
                                                                       "Why don't you tell me about a childhood friend?" });
            psychobabble.Add(@"friend", response14);

            List<string> response15 = new List<string>(new string[2] {"You seem quite sure.",
                                                                       "OK, but can you elaborate a bit?" });
            psychobabble.Add(@"Yes", response15);

            List<string> response16 = new List<string>(new string[4] {"Are you really talking about me?",
                                                                       "Does it seem strange to talk to a computer?",
                                                                       "How do computers make you feel?",
                                                                       "Do you feel threatened by computers?" });
            psychobabble.Add(@"computer", response16);

            List<string> response17 = new List<string>(new string[4] {"Do you think it is {0}?",
                                                                       "Perhaps it is {0} -- what do you think?",
                                                                       "If it were {0}, what would you do?",
                                                                       "It could well be that {0}." });
            psychobabble.Add(@"Is it", response17);

            List<string> response18 = new List<string>(new string[2] {"You seem very certain.",
                                                                       "If I told you that it probably isn't, what would you feel?" });
            psychobabble.Add(@"It is", response18);

            List<string> response19 = new List<string>(new string[3] {"What makes you think I can't {0}?",
                                                                       "If I could {0}, then what?",
                                                                       "Why do you ask if I can {0}?" });
            psychobabble.Add(@"Can you", response19);

            List<string> response20 = new List<string>(new string[3] {"Perhaps you don't want to {0}.",
                                                                       "Do you want to be able to {0}?",
                                                                       "If you could {0}, would you?" });
            psychobabble.Add(@"Can I", response20);

            List<string> response21 = new List<string>(new string[4] {"Why do you think I am {0}?",
                                                                       "Does it please you to think that I am {0}?",
                                                                       "Perhaps you would like me to be {0}?",
                                                                       "Perhaps you're really talking about yourself?" });
            psychobabble.Add(@"You are", response21);


            List<string> response23 = new List<string>(new string[3] {"Don't you really {0}?",
                                                                       "Why don't you {0}?",
                                                                       "Do you want to {0}?" });
            psychobabble.Add(@"I don't", response23);

            List<string> response24 = new List<string>(new string[4] {"Good, tell me more about these feelings.",
                                                                       "Do you often feel {0}?",
                                                                       "When do you usually feel {0}?",
                                                                       "When you feel {0}, what do you do?" });
            psychobabble.Add(@"I feel", response24);

            List<string> response25 = new List<string>(new string[3] {"Why do you tell me that you've {0}?",
                                                                       "Have you really {0}?",
                                                                       "Now that you have {0}, what will you do next?" });
            psychobabble.Add(@"I have", response25);

            List<string> response26 = new List<string>(new string[3] {"Could you explain why you would {0}?",
                                                                       "Why would you {0}?",
                                                                       "Who else knows that you would {0}?" });
            psychobabble.Add(@"I would", response26);

            List<string> response27 = new List<string>(new string[3] {"Do you think there is {0}?",
                                                                       "It's likely that there is {0}.",
                                                                       "Would you like there to be {0}?" });
            psychobabble.Add(@"Is there", response27);

            List<string> response28 = new List<string>(new string[3] {"I see, your {0}.",
                                                                       "Why do you say that your {0}?",
                                                                       "When your {0}, how do you feel?" });
            psychobabble.Add(@"My", response28);

            List<string> response29 = new List<string>(new string[3] {"We should be discussing you, not me.",
                                                                       "Why do you say that about me?",
                                                                       "Why do you care whether I {0}?" });
            psychobabble.Add(@"You", response29);


            List<string> response30 = new List<string>(new string[2] {"Why don't you tell me the reason why {0}?",
                                                                       "Why do you think {0}?" });
            psychobabble.Add(@"Why", response30);

            List<string> response31 = new List<string>(new string[4] {"What would it mean to you if you got {0}?",
                                                                       "Why do you want {0}?",
                                                                       "What would you do if you got {0}?",
                                                                       "All right, if you got {0}, then what would you do?" });
            psychobabble.Add(@"I want", response31);

            List<string> response32 = new List<string>(new string[5] {"Tell me more about your mother..",
                                                                       "What was your relationship with your mother like?",
                                                                       "How do you feel about your mother?",
                                                                       "How does this relate to your feelings today?",
                                                                       "Good family relations are important." });
            psychobabble.Add(@"mother", response32);

            List<string> response33 = new List<string>(new string[5] {"Tell me more about your father.",
                                                                       "How did your father make you feel?",
                                                                       "How do you feel about your father?",
                                                                       "Does your relationship with your father relate to your feelings today?",
                                                                       "Do you have trouble showing affection with your family?" });
            psychobabble.Add(@"father", response33);

            List<string> response34 = new List<string>(new string[5] {"Did you have close friends as a child?",
                                                                       "What is your favorite childhood memory?",
                                                                       "Do you remember any dreams or nightmares from childhood?",
                                                                       "Did the other children sometimes tease you?",
                                                                       "How do you think your childhood experiences relate to your feelings today?" });
            psychobabble.Add(@"child", response34);


            List<string> response36 = new List<string>(new string[2] {"Thank you for talking with me.",
                                                                       "Good-bye." });
            psychobabble.Add(@"bye", response36);

            List<string> response37 = new List<string>(new string[10] {"Please tell me more.",
                                                                       "Let's change focus a bit... Tell me about something else.",
                                                                       "Can you elaborate on that?",
                                                                       "Why do you say that {0}?",
                                                                       "I see.",
                                                                       "Very interesting.",
                                                                       "{0}",
                                                                       "I see.  And what does that tell you?",
                                                                       "How does that make you feel?",
                                                                       "How do you feel when you say that?" });
            psychobabble.Add(@"", response37);

        }

        static Random rnd = new Random();

        public string RandomResponse_ori(string key)
        {
            List<string> response = psychobabble_ori[key];
            int index = rnd.Next(response.Count);
            return response[index];
        }

        public string RandomResponse(string key)
        {
            List<string> response = psychobabble[key];
            int index = rnd.Next(response.Count);
            return response[index];
        }

        public string Reflect(string match)
        {
            string[] tokens = match.ToLower().Split();
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                if (reflection.ContainsKey(token))
                {
                    tokens[i] = reflection[token];
                }
            }
            return string.Join(" ", tokens);
        }

        public string Reply(string text)
        {
            string response = "";
            List<string> result = new List<string>();
            List<string> result_ori = new List<string>();

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            //var matches = target.AsParallel().Where(s=>text.Contains(s)).WithCancellation(tokenSource.Token).Select(s=>text.Contains(s)).ToList();
            //var matches = target.AsParallel().Where(s => text.Contains(s)).WithCancellation(tokenSource.Token).ToList();
            //Parallel.ForEach(matches, m =>
            //{
            //    result.Add(RandomResponse(m));
            //    string entity = text.Replace(m, "");
            //    result.Add(Reflect(entity));

            //});

            Parallel.ForEach(targetori, (pattern, state) =>
            {
                if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
                {
                    response = RandomResponse_ori(pattern);
                    result_ori.Add(response);
                    foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
                    {
                        result_ori.Add(Reflect(match.Groups[1].Value));
                    }
                    state.Break();

                   
                }
            });

            string retVal_ori = string.Format(result_ori[0], result_ori[1]);
            //string retVal = string.Format(result[0], result[1]);

            Debug.WriteLine($"Response ----- {retVal_ori}");
            return retVal_ori;
        }


        //public virtual async Task reply(IDialogContext context, IAwaitable<IMessageActivity> argument)
        //{
        //    string response = "";
        //    List<string> result = new List<string>();
        //    var message = await argument;
        //    foreach (string pattern in target)
        //    {

        //        if (Regex.IsMatch(message.Text, pattern, RegexOptions.IgnoreCase))
        //        {
        //            response = RandomResponse(pattern);
        //            result.Add(response);
        //            foreach (Match match in Regex.Matches(message.Text, pattern, RegexOptions.IgnoreCase))
        //            {
        //                result.Add(Reflect(match.Groups[1].Value));
        //            }
        //            break;
        //        }
        //    }
        //    string retVal = string.Format(result[0], result[1]);
        //    Debug.WriteLine($"Response ----- {retVal}");

        //    await context.PostAsync(retVal);
        //}

        //public async Task StartAsync(IDialogContext context)
        //{
        //    context.Wait(reply);
        //}
    }
}