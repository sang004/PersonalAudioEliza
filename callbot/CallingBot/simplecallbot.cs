using Microsoft.Bot.Builder.Calling;
using Microsoft.Bot.Builder.Calling.Events;
using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Configuration;

using SSS = System.Speech.Synthesis;
using System.IO;
using System.Diagnostics;
using Microsoft.Bot.Connector;
using System.Web.Http;
using System.Net.Http;
using System.Linq;

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
        int noRecordTimes = 0;

        bool sttFailed = false;
        string bingresponse = "";
        //static ConversationTranscibe logger = new ConversationTranscibe(); // Will create a fresh new log file
        private Dialogs.ElizaDialog ED = new Dialogs.ElizaDialog();

        private string microsoftAppId { get; } = ConfigurationManager.AppSettings["MicrosoftAppId"];
        private string microsoftAppPassword { get; } = ConfigurationManager.AppSettings["MicrosoftAppPassword"];

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

        //private async void getUser( Participant p ) {

            
        //    // create the activity and retrieve
        //    StateClient stateClient = new StateClient(new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword));
        //    BotData userData = await stateClient.BotState.GetUserDataAsync("skype", p.Identity);
        //    var sentGreeting = userData.GetProperty<string>("Call");
            
        //}

        private Task OnIncomingCallReceived(IncomingCallEvent incomingCallEvent)
        {
            //get caller's information
            //var partipants = incomingCallEvent.IncomingCall.Participants;         
            //getUser(partipants.ElementAt(0));

            var id = Guid.NewGuid().ToString();
            incomingCallEvent.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    new Answer { OperationId = id },
                    GetRecordForText("Top of the day to you!", 2)
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
            var id = Guid.NewGuid().ToString();

            return new Record()
            {
                OperationId = id,
                PlayPrompt = prompt,
                MaxDurationInSeconds = 10,
                InitialSilenceTimeoutInSeconds = 5,
                MaxSilenceTimeoutInSeconds = 5,
                PlayBeep = false,
                RecordingFormat = RecordingFormat.Wav,
                StopTones = new List<char> { '#' }
            };
        }

        private async Task<Task> OnPlayPromptCompleted(PlayPromptOutcomeEvent playPromptOutcomeEvent)
        {
            var actionList = new List<ActionBase>();
            if (bingresponse != "")
            {

                // if its bye
                if (bingresponse.ToLower().Contains("bye"))
                {
                    playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetPromptForText("Anybody there? Bye.", 2),
                        new Hangup() { OperationId = Guid.NewGuid().ToString() }
                    };
                    playPromptOutcomeEvent.ResultingWorkflow.Links = null;
                    silenceTimes = 0;

                }
                else
                {

                    //else identify words
                    string output = await ED.Reply(bingresponse);
#if RELEASE
                    //use bot framework voice, mode -1
                    Debug.WriteLine($"Bing response: {output}");
                    actionList.Add(GetPromptForText(output, -1));


#else
                    //microsoft stt, mode 2
                    actionList.Add(GetPromptForText(output, 2));
#endif
                    actionList.Add(GetRecordForText(string.Empty, -1));
                    playPromptOutcomeEvent.ResultingWorkflow.Actions = actionList;
                }
            }
             
            else
            {
                if (sttFailed)
                {
                    playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetRecordForText("I didn't catch that, would you kindly repeat?",2)
                    };
                    sttFailed = false;
                    silenceTimes = 0;
                }
                else if (silenceTimes > 2)
                {
                    playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetPromptForText("Something went wrong. Call again later.",2),
                        new Hangup() { OperationId = Guid.NewGuid().ToString() }
                    };
                    playPromptOutcomeEvent.ResultingWorkflow.Links = null;
                    silenceTimes = 0;
                }
                else
                {
                    //last resort, listen longer
                    silenceTimes++;
                    playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetSilencePrompt()
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
                
                string replyAudioPath = "http://ec2-54-255-203-240.ap-southeast-1.compute.amazonaws.com/filestore/4_6243e7460bb03de/4_89e60a9e2072f2e.wav";

                var webClient = new WebClient();
                byte[] bytes = webClient.DownloadData(replyAudioPath);

                System.IO.Stream streams = new System.IO.MemoryStream(bytes);
                var record = streams;

                //TEST AUDIO END

#else
                var record = await recordOutcomeEvent.RecordedContent;
#endif

                BingSpeech bs = new BingSpeech(recordOutcomeEvent.ConversationResult, t => response.Add(t), s => sttFailed = s, b => bingresponse = b);
                bs.CreateDataRecoClient();
                bs.SendAudioHelper(record);

                recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    GetSilencePrompt()
                };

            }
            else if (recordOutcomeEvent.RecordOutcome.FailureReason == "CallTerminated")
            {
                //So if the caller hangs up, initiate hangout on bot
                new Hangup() { OperationId = Guid.NewGuid().ToString() };

            }
            else
            {
                if (silenceTimes > 1)
                {
                    recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetPromptForText("Bye bye!",2),
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
                        GetRecordForText("I didn't catch that, would you kindly repeat?",2)
                    };
                }
            }
            
        }

        private Task OnHangupCompleted(HangupOutcomeEvent hangupOutcomeEvent)
        {
            //logger.uploadToRS();
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
            //logger.WriteToText("BOT: ", text);
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
            else if (mode == 2)
            {
                // Configure the audio output. 
                MemoryStream ms = new MemoryStream();
                SSS.SpeechSynthesizer synth = new SSS.SpeechSynthesizer();
                audioMan am = new audioMan();
                
                string tempPath = Path.GetTempPath();
                synth.SetOutputToWaveStream(ms);
                synth.Speak(text);
                //now convert to mp3 using LameEncoder or shell out to audiograbber
                am.ConvertWavStreamToWav(ref ms, $"{tempPath}Rate.wav");

                uri = new System.Uri(am.azureUrl);

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
                //logger.WriteToText("BOT: ", txt);

                if (!string.IsNullOrEmpty(txt))
                    prompts.Add(new Prompt { Value = txt, Voice = VoiceGender.Female });
            }
            if (prompts.Count == 0)
                return GetSilencePrompt();
            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = prompts };
        }

        private static PlayPrompt GetSilencePrompt(uint silenceLengthInMilliseconds = 1500)
        {
            var prompt = new Prompt { Value = string.Empty, Voice = VoiceGender.Female, SilenceLengthInMilliseconds = silenceLengthInMilliseconds };
            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
        }
    }


}


