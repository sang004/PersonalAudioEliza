using Microsoft.Bot.Builder.Calling;
using Microsoft.Bot.Builder.Calling.Events;
using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using SSS = System.Speech.Synthesis;
using System.IO;

namespace callbot
{
    public class simplecallbot : ICallingBot
    {
        public ICallingBotService CallingBotService
        {
            get; private set;
        }

        private List<string> response = new List<string>();
        int silenceTimes = 0;
        bool sttFailed = false;
        string bingresponse = "";
        static ConversationTranscibe logger = new ConversationTranscibe(); // Will create a fresh new log file

        public simplecallbot(ICallingBotService callingBotService)
        {

            if (callingBotService == null)
                throw new ArgumentNullException(nameof(callingBotService));

            this.CallingBotService = callingBotService;
            

            CallingBotService.OnIncomingCallReceived += OnIncomingCallReceived;
            CallingBotService.OnPlayPromptCompleted += OnPlayPromptCompleted;
            CallingBotService.OnRecordCompleted += OnRecordCompleted;
            CallingBotService.OnHangupCompleted += OnHangupCompleted;
        }

        private Task OnIncomingCallReceived(IncomingCallEvent incomingCallEvent)
        {
            var id = Guid.NewGuid().ToString();
            incomingCallEvent.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    new Answer { OperationId = id },
                    GetRecordForText("Hi hi!", -1)
                };

            return Task.FromResult(true);
        }

        private ActionBase GetRecordForText(string promptText, int mode)
        {
            PlayPrompt prompt;
            if (string.IsNullOrEmpty(promptText))
                prompt = null;
            else
                prompt = GetPromptForText(promptText, mode);
            //prompt = PlayAudioFile(promptText);
            var id = Guid.NewGuid().ToString();

            return new Record()
            {
                OperationId = id,
                PlayPrompt = prompt,
                MaxDurationInSeconds = 5,
                InitialSilenceTimeoutInSeconds = 5,
                MaxSilenceTimeoutInSeconds = 2,
                PlayBeep = false,
                RecordingFormat = RecordingFormat.Wav,
                StopTones = new List<char> { '#' }
            };
        }

        private async Task<Task> OnPlayPromptCompleted(PlayPromptOutcomeEvent playPromptOutcomeEvent)
        {
            //string user = ConfigurationManager.AppSettings["RSId"];
            //string private_key = ConfigurationManager.AppSettings["RSPassword"];
            //RSAPI rsapi = new RSAPI(user, private_key);

            //List<string> audioArr = new List<string>();
            //audioMan am = new audioMan();

            // get response from LUIS in text form
            //if (response.Count > 0)
            //{
            //    silenceTimes = 0;
            //    var actionList = new List<ActionBase>();

            //    // there might be multiple replies from LUIS
            //    foreach (var res in response)
            //    {
            //        logger.WriteToText("USER: ", res);
            //        Debug.WriteLine($"Response ----- {res}");

            //        // if LUIS does not return a topic but a sentence with question mark, use text to speech
            //        //bool isEcho = res.Contains("?");
            //        //if (isEcho) {
            //        //SSS.SpeechSynthesizer synth = new SSS.SpeechSynthesizer();
            //        //// Configure the audio output. 
            //        //MemoryStream ms = new MemoryStream();

            //        //string tempPath = Path.GetTempPath();
            //        //synth.SetOutputToWaveStream(ms);
            //        //synth.Speak(res);
            //        ////now convert to mp3 using LameEncoder or shell out to audiograbber
            //        //am.ConvertWavStreamToMp3File(ref ms, $"{tempPath}Rate.mp3");

            //        //audioArr.Add($"{tempPath}Rate.mp3");

            //        //}
            //        //else
            //        //{
            //        //use rs object to fetch appropriate url for audio based on each result given
            //        //    audioArr.Add(rsapi.Call(res).Result);
            //        //}
            //    }
            // if there is only 1 file, no point creating an object to combine the audio files, just use RS link


            //am.callCombine(audioArr);
            //actionList.Add(PlayAudioFile(am.azureUrl));


            //actionList.Add(GetPromptForText(res, -1));
            var actionList = new List<ActionBase>();
            if(bingresponse != "") { 
                Dialogs.ElizaDialog ED = new Dialogs.ElizaDialog();
                string output = ED.Reply(bingresponse);
                actionList.Add(GetRecordForText(output,-1));
                playPromptOutcomeEvent.ResultingWorkflow.Actions = actionList;
                response.Clear();
            }
            else
            {
                if (sttFailed)
                {
                    playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetRecordForText("I didn't catch that, would you kindly repeat?",-1)
                    };
                    sttFailed = false;
                    silenceTimes = 0;
                }
                else if (silenceTimes > 2)
                {
                    playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetPromptForText("Something went wrong. Call again later.",-1),
                        new Hangup() { OperationId = Guid.NewGuid().ToString() }
                    };
                    playPromptOutcomeEvent.ResultingWorkflow.Links = null;
                    silenceTimes = 0;
                }
                else
                {
                    silenceTimes++;
                    playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetSilencePrompt(2000)
                    };
                }
            }
            return Task.CompletedTask;
        }



        private async Task OnRecordCompleted(RecordOutcomeEvent recordOutcomeEvent)
        {
            // When recording is done, send to BingSpeech to process
            if (recordOutcomeEvent.RecordOutcome.Outcome == Outcome.Success)
            {

#if RELEASE
                //TEST AUDIO START
                ///Retrieve random audio            
                string user = ConfigurationManager.AppSettings["RSId"];
                string private_key = ConfigurationManager.AppSettings["RSPassword"];

                //RSAPI test2 = new RSAPI(user, private_key);
                //string replyAudioPath = test2.Call("sample").Result;


                string replyAudioPath = "http://ec2-52-221-215-199.ap-southeast-1.compute.amazonaws.com/filestore/4_6243e7460bb03de/4_89e60a9e2072f2e.wav";
                
                var webClient = new WebClient();
                byte[] bytes = webClient.DownloadData(replyAudioPath);

                System.IO.Stream streams = new System.IO.MemoryStream(bytes);
                var record = streams;

                //TEST AUDIO END

#else
                var record = await recordOutcomeEvent.RecordedContent;
#endif

                BingSpeech bs = new BingSpeech(recordOutcomeEvent.ConversationResult, t => response.Add(t), s => sttFailed = s, b => bingresponse=b);
                bs.CreateDataRecoClient();
                bs.SendAudioHelper(record);
                
                //AskLUIS test = new AskLUIS();
                //String response = test.questionLUIS(activity.Text);
                recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    GetSilencePrompt()
                };

            } else if (recordOutcomeEvent.RecordOutcome.FailureReason == "CallTerminated") {
                //So if the caller hangs up, initiate hangout on bot
                new Hangup() { OperationId = Guid.NewGuid().ToString() };

            }
            else
            {
                if (silenceTimes > 1)
                {
                    recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetPromptForText("Bye bye!",-1),
                        new Hangup() { OperationId = Guid.NewGuid().ToString() }
                    };
                    recordOutcomeEvent.ResultingWorkflow.Links = null;
                    silenceTimes = 0;
                }
                else
                {
                    silenceTimes++;
                    recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetRecordForText("I didn't catch that, would you kindly repeat?",-1)
                    };
                }
            }


        }

        private Task OnHangupCompleted(HangupOutcomeEvent hangupOutcomeEvent)
        {
            logger.uploadToRS();
            hangupOutcomeEvent.ResultingWorkflow = null;
            return Task.FromResult(true);
        }

        // TEST playback
        private static PlayPrompt PlayAudioFile(string audioPath)
        {

            //System.Uri uri = new System.Uri("https://callbotstorage.blob.core.windows.net/blobtest/graham_any_nation.wav");
            System.Uri uri = new System.Uri(audioPath);
            
            var prompt = new Prompt { FileUri = uri };
            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
        }

        private static PlayPrompt GetPromptForText(string text, int mode)
        {

            System.Uri uri;
            logger.WriteToText("BOT: ", text);
            if (mode == 1)
            {
                uri = new System.Uri("http://bitnami-resourcespace-b0e4.cloudapp.net/filestore/8/7_36cabf597b6f9db/87_4f2bd3c2b2825fc.wav");

                var prompt = new Prompt { FileUri = uri };
                return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };


            }
            else if (mode == 0)
            {
                uri = new System.Uri("http://bitnami-resourcespace-b0e4.cloudapp.net/filestore/8/5_1b312f7bcb5fbc6/85_0edca2f3cccad42.wav");

                var prompt = new Prompt { FileUri = uri };
                return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };


            }
            else {
                var prompt = new Prompt { Value = text, Voice = VoiceGender.Female };
                return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };

            }

        }

        private static PlayPrompt GetPromptForText(List<string> text)
        {
            var prompts = new List<Prompt>();
            foreach (var txt in text)
            {
                logger.WriteToText("BOT: ", txt);

                if (!string.IsNullOrEmpty(txt))
                    prompts.Add(new Prompt { Value = txt, Voice = VoiceGender.Female });
            }
            if (prompts.Count == 0)
                return GetSilencePrompt(1000);
            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = prompts };
        }

        private static PlayPrompt GetSilencePrompt(uint silenceLengthInMilliseconds = 3000)
        {
            var prompt = new Prompt { Value = string.Empty, Voice = VoiceGender.Female, SilenceLengthInMilliseconds = silenceLengthInMilliseconds };
            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
        }
    }


}


