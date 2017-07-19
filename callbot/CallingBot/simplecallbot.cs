using Microsoft.Bot.Builder.Calling;
using Microsoft.Bot.Builder.Calling.Events;
using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Configuration;

namespace callbot
{
    public class simplecallbot : IDisposable ,ICallingBot
    {
        public ICallingBotService CallingBotService
        {
            get; private set;
        }

        private List<string> response = new List<string>();
        int silenceTimes = 0;

        bool sttFailed = false;
        string bingresponse = "";
        static ConversationTranscibe logger = new ConversationTranscibe(); // Will create a fresh new log file on Microsoft Azure storage
        private Dialogs.ElizaDialog ED = new Dialogs.ElizaDialog();
        
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

        // dispose garbage
        public void Dispose()
        {
            if (this.CallingBotService != null)
            {
                CallingBotService.OnIncomingCallReceived -= OnIncomingCallReceived;
                CallingBotService.OnPlayPromptCompleted -= OnPlayPromptCompleted;
                CallingBotService.OnRecordCompleted -= OnRecordCompleted;
                CallingBotService.OnHangupCompleted -= OnHangupCompleted;
            }
        }

        // initiated only once when a user calls the bot, bot decides whether to answer by "new Answer" action
        private Task OnIncomingCallReceived(IncomingCallEvent incomingCallEvent)
        {
            var id = Guid.NewGuid().ToString();
            incomingCallEvent.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    new Answer { OperationId = id },
                    GetRecordForText("Hello, good to see you today!")
                };

            return Task.FromResult(true);
        }

        // bot speaks and then waits for recording
        private ActionBase GetRecordForText(string promptText)
        {
            PlayPrompt prompt;
            if (string.IsNullOrEmpty(promptText))
                prompt = null;
            else
                prompt = GetPromptForText(promptText);
            var id = Guid.NewGuid().ToString();

            return new Record()
            {
                OperationId = id,
                PlayPrompt = prompt,
                MaxDurationInSeconds = 8,
                InitialSilenceTimeoutInSeconds = 3,
                MaxSilenceTimeoutInSeconds = 3,
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
                // log user response
                logger.WriteToText("USER: ", bingresponse);

                silenceTimes = 0;

                // if user says bye bye, end voice conversation
                if (bingresponse.ToLower().Contains("bye"))
                {
                    playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetPromptForText("This has been a great session! Bye bye."),
                        new Hangup() { OperationId = Guid.NewGuid().ToString() }
                    };
                    playPromptOutcomeEvent.ResultingWorkflow.Links = null;
                    silenceTimes = 0;

                }
                else
                {
                    //else identify words
                    string output = await ED.Reply(bingresponse);
                    
                    actionList.Add(GetRecordForText(output));
                    playPromptOutcomeEvent.ResultingWorkflow.Actions = actionList;
                }
                bingresponse = "";
            }
             
            else
            {
                if (sttFailed)
                {
                    playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetRecordForText("I didn't catch that, would you kindly repeat?")
                    };
                    sttFailed = false;
                    silenceTimes++;

                }
                else if (silenceTimes > 2)
                {
                    playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetPromptForText("Is anybody there? Bye."),
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
        
        // called once a record action is done
        private async Task OnRecordCompleted(RecordOutcomeEvent recordOutcomeEvent)
        {
            // When recording is done, send to BingSpeech to process
            if (recordOutcomeEvent.RecordOutcome.Outcome == Outcome.Success)
            {
#if DEBUG
                //TEST AUDIO START
                ///Retrieve audio from RS server to quick debugging          
                string user = ConfigurationManager.AppSettings["RSId"];
                string private_key = ConfigurationManager.AppSettings["RSPassword"];
                
                string replyAudioPath = "http://ec2-13-228-78-239.ap-southeast-1.compute.amazonaws.com/filestore/4_6243e7460bb03de/4_89e60a9e2072f2e.wav";

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
                        GetPromptForText("Bye bye!"),
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
                        GetRecordForText("I didn't catch that, would you kindly repeat?")
                    };
                }
            }
            
        }

        // method when "Hangup()" is called, terminates current voice conversation
        private Task OnHangupCompleted(HangupOutcomeEvent hangupOutcomeEvent)
        {

            hangupOutcomeEvent.ResultingWorkflow = null;
            return Task.FromResult(true);
        }

        private static PlayPrompt GetPromptForText(string text)
        {
            // log bot response
             logger.WriteToText("BOT: ", text);

             var prompt = new Prompt { Value = text, Voice = VoiceGender.Female };
             return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
        }


        // chains up a list of string to voice out synthetically
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
                return GetSilencePrompt();

            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = prompts };
        }

        // create a prompt that is silence, keeps data flow going
        private static PlayPrompt GetSilencePrompt(uint silenceLengthInMilliseconds = 800)
        {
            var prompt = new Prompt { Value = string.Empty, Voice = VoiceGender.Female, SilenceLengthInMilliseconds = silenceLengthInMilliseconds };
            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
        }
    }
}


