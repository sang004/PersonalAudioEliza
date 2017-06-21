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
using System.Linq;
using System.Threading;

namespace callbot
{
    public class simplecallbot : ICallingBot
    {
        public ICallingBotService CallingBotService
        {
            get; private set;
        }

        private List<string> response = new List<string>();
        private string activeAcc = "";
        private string activeMode = "";
        private bool isSet;
        int silenceTimes = 0;

        private string microsoftAppId { get; } = ConfigurationManager.AppSettings["MicrosoftAppId"];
        private string microsoftAppPassword { get; } = ConfigurationManager.AppSettings["MicrosoftAppPassword"];
        
        string serviceUrl = "https://smba.trafficmanager.net/apis/";
        private MicrosoftAppCredentials account = new MicrosoftAppCredentials(ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);
        private StateClient stateClient;
        private BotData userData;

        bool sttFailed = false;
        string bingresponse = "";

        IEnumerable<Participant> participant = null;
        private int recordNum = -1;
        private string recordPath = "";
        //static ConversationTranscibe logger = new ConversationTranscibe(); // Will create a fresh new log file
        private static Dialogs.ElizaDialog ED = new Dialogs.ElizaDialog();
        private static int clipNum = ED.response.Count;
        private static audioMan am = new audioMan();
        private RSAPI RS = new RSAPI(ConfigurationManager.AppSettings["RSId"], ConfigurationManager.AppSettings["RSPassword"]);
        //RS. ConfigurationManager.AppSettings("RSId", ConfigurationManager.AppSettings("RSPassword");

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
            // reset all flags
            silenceTimes = 0;
            isSet = false;
            recordNum = -1;
            sttFailed = false;
            bingresponse = "";

            participant = incomingCallEvent.IncomingCall.Participants;
            var id = Guid.NewGuid().ToString();
            genResponseCard(participant);
            
            incomingCallEvent.ResultingWorkflow.Actions = new List<ActionBase>
            {
                new Answer { OperationId = id },
                GetPromptForText("Top of the day to you! What would you like to do today?",2)
            };
            
            return Task.FromResult(true);
        }


        private async Task<Task> OnPlayPromptCompleted(PlayPromptOutcomeEvent playPromptOutcomeEvent)
        {
            Debug.WriteLine("###################Onplayprompt");
            var actionList = new List<ActionBase>();

            Debug.WriteLine($"#################CreateRichCard when isSet == {isSet}");
            // only run once when it first comes into this function
            if (isSet == false)
            {
                //get click input here

                // reset all settings at start of call
                await setUserData(participant, "activeMode", "None");
                await setUserData(participant, "activeAcc", "None");

                //loop and get user persistant data
                DateTime curTime = DateTime.UtcNow;
                do { activeMode = await getUserData(participant, "activeMode");} while (activeMode.Equals("None") && DateTime.UtcNow - curTime < new TimeSpan(0, 3, 00) );
                Debug.WriteLine($"ACTION: {activeMode}");
                if (!activeMode.Equals("None"))
                {
                    await SendRecordMessage($"Who would you like to {activeMode} as? 'as ::name::'");
                    curTime = DateTime.UtcNow;
                    do { activeAcc = await getUserData(participant, "activeAcc"); } while (activeAcc.Equals("None") && DateTime.UtcNow - curTime < new TimeSpan(0, 0, 30));
                }
                if (!activeAcc.Equals("None") && !activeMode.Equals("None"))
                {
                    isSet = true;
                }
            }

            if (isSet)
            {
                if (activeMode.Equals("record"))
                {
                    if (recordNum == -1)
                    {
                        recordPath = string.Format("C:\\Users\\Joyce\\audio_records\\{0}", activeAcc.ToString());
                        Directory.CreateDirectory(recordPath);
                        playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase> { GetRecordForText("Recording. Please read out the sentence after you received the message and heared the beep.", silenceTimeout: 2) };
                    }
                    else
                    {
                        playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase> { GetRecordForMessage() };
                    }
                }
                else
                {
                    if (recordNum == -1)
                    {
                        playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase> { GetRecordForText("Lets start a verbal battle!") };
                        recordNum++;
                    }
                    else
                    {
                        if (bingresponse != "")
                        {
                            silenceTimes = 0;

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
                                int outputIndex = ED.response.IndexOf(output);
                                string audioKeyword = outputIndex + "_" + activeAcc;
#if RELEASE
                        //use bot framework voice, mode -1
                        Debug.WriteLine($"Bing response: {output}");
                        actionList.Add(GetPromptForText(output, -1));
                            
#else
                                //microsoft stt, mode 2
                                string user = ConfigurationManager.AppSettings["RSId"];
                                string private_key = ConfigurationManager.AppSettings["RSPassword"];
                                RSAPI rsapi = new RSAPI(user, private_key);

                                string path = rsapi.Call(audioKeyword).Result;
                                if (!path.Equals(""))
                                {
                                    actionList.Add(PlayAudioFile(path));
                                }
                                else
                                {
                                    actionList.Add(GetPromptForText(output, 2));
                                }
#endif
                                actionList.Add(GetRecordForText(string.Empty, mode: -1));
                                playPromptOutcomeEvent.ResultingWorkflow.Actions = actionList;
                            }
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
                                    GetPromptForText("Is anybody there? Bye.",2),
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
                                    GetRecordForText("I didn't catch that")
                                };

                            }
                        }
                    }
                }
            }
            else
            {
                playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    GetPromptForText("Message reply not recieved, Bye.", 2),
                    //new Hangup() { OperationId = Guid.NewGuid().ToString() }
                };
                playPromptOutcomeEvent.ResultingWorkflow.Links = null;
            }
            return Task.CompletedTask;
        }

        private Task OnHangupCompleted(HangupOutcomeEvent hangupOutcomeEvent)
        {
            //logger.uploadToRS();
            hangupOutcomeEvent.ResultingWorkflow = null;
            return Task.FromResult(true);
        }

        private async Task OnRecordCompleted(RecordOutcomeEvent recordOutcomeEvent)
        {
            if (activeMode == "record")
            {
                await RecordOnRecordCompleted(recordOutcomeEvent);
            }
            else
            {
                await CallOnRecordCompleted(recordOutcomeEvent);
            }
        }

        private Record SetRecord(string id, PlayPrompt prompt, bool playBeep, int maxSilenceTimeout)
        {
            return new Record()
            {
                OperationId = id,
                PlayPrompt = prompt,
                MaxDurationInSeconds = 8,
                InitialSilenceTimeoutInSeconds = 3,
                MaxSilenceTimeoutInSeconds = maxSilenceTimeout,
                PlayBeep = playBeep,
                RecordingFormat = RecordingFormat.Wav,
                StopTones = new List<char> { '#' }
            };
        }

        private Record SetRecord(string id, bool playBeep, int maxSilenceTimeout)
        {
            return new Record()
            {
                OperationId = id,
                MaxDurationInSeconds = 8,
                InitialSilenceTimeoutInSeconds = 3,
                MaxSilenceTimeoutInSeconds = maxSilenceTimeout,
                PlayBeep = playBeep,
                RecordingFormat = RecordingFormat.Wav,
                StopTones = new List<char> { '#' }
            };
        }

        private ActionBase GetRecordForText(string promptText, bool playbeep = false, int mode = 2, int silenceTimeout = 3)
        {
            PlayPrompt prompt;
            if (string.IsNullOrEmpty(promptText))
                prompt = null;
            else
                prompt = GetPromptForText(promptText, mode);
            var id = Guid.NewGuid().ToString();

            return SetRecord(id, prompt, playbeep, silenceTimeout);
        }

        private ActionBase GetRecordForMessage()
        {
            var id = Guid.NewGuid().ToString();

            return SetRecord(id, true, 2);
        }

        private async Task RecordOnRecordCompleted(RecordOutcomeEvent recordOutcomeEvent)
        {
            Debug.WriteLine("00000");
            Debug.WriteLine(recordOutcomeEvent.RecordOutcome.Outcome == Outcome.Success);
            
            if (recordOutcomeEvent.RecordOutcome.Outcome == Outcome.Success && recordNum >= 0)
            {
                var record = await recordOutcomeEvent.RecordedContent;
                silenceTimes = 0;
                if (recordNum < clipNum)//ED.response.Count() + 1)
                {
                    Debug.WriteLine("#########################SAVING RECORD");
                    MemoryStream ms = new MemoryStream();
                    record.CopyTo(ms);
                    string filePath = string.Format("{0}\\{1}_{2}.wav", recordPath, recordNum, activeAcc);
                    am.ConvertWavStreamToWav(ref ms, filePath);
                    recordNum++;
                    if (recordNum < clipNum)
                    {
                        await SendRecordMessage();
                        recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                        {
                            GetRecordForMessage()
                        };
                    }
                    // all record done
                    else
                    {
                        recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                        {
                            GetRecordForText("Record completed, uploading recording!")
                        };
                    }
                }
                else if (recordNum == clipNum)
                {
                    DirectoryInfo dir = new DirectoryInfo(recordPath);
    
                    if (dir.GetFiles("*.wav").Length == clipNum)
                    {
                        foreach (var file in dir.GetFiles("*.wav"))
                        {
                            Debug.WriteLine(file.ToString());
                            string azureUrl = am.azureFunc(recordPath + "\\" + file.ToString());
                            Debug.WriteLine("*********" + azureUrl);
                            if (azureUrl != "")
                            {
                                RS.UploadResource(azureUrl, file.ToString(), ".wav");
                            }
                            else
                            {
                                Debug.WriteLine("Upload to azure failed" + file.ToString());
                            }
                        }
                        // Delete the folder contains the clips
                        System.IO.Directory.Delete(recordPath, true);
                        recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                        {
                            GetRecordForText("Upload completeted, bye!", silenceTimeout: 2),
                            new Hangup() { OperationId = Guid.NewGuid().ToString() }
                        };
                    }
                    else
                    {
                        System.IO.Directory.Delete(recordPath, true);
                        Debug.WriteLine(string.Format("Number of recorded clips {0} less than {1}", dir.GetFiles("*.wav").Length.ToString(), "3"));
                        recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                        {
                            GetRecordForText("Record failed, bye!", silenceTimeout: 2),
                            new Hangup() { OperationId = Guid.NewGuid().ToString() }
                        };
                    }

                    recordOutcomeEvent.ResultingWorkflow.Links = null;
                }
            }
            else if (recordOutcomeEvent.RecordOutcome.FailureReason == "CallTerminated")
            {
                //So if the caller hangs up, initiate hangout on bot
                new Hangup() { OperationId = Guid.NewGuid().ToString() };
            }
            // silence from the user
            else
            {
                if (recordNum == -1)
                {
                    recordNum++;
                    await SendRecordMessage();
                    recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetRecordForMessage()
                    };
                    silenceTimes++;
                }
                else
                {
                    if (silenceTimes < 3)
                    {
                        silenceTimes++;
                        recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                        {
                            GetRecordForText("I didn't catch that, would you kindly repeat?", playbeep: true, silenceTimeout: 2)
                        };
                    }
                    else
                    {
                        recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                        {
                            GetRecordForText("I didn't catch that, record terminated!", silenceTimeout: 2),
                            new Hangup() { OperationId = Guid.NewGuid().ToString() }
                        };
                        recordOutcomeEvent.ResultingWorkflow.Links = null;
                        silenceTimes = 0;
                    }
                }
            }
        }

        private async Task<string> getUserData(IEnumerable<Participant> p, string field)
        {
            // create the activity and retrieve
            stateClient = new StateClient(new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword));
            userData = await stateClient.BotState.GetUserDataAsync("skype", p.ElementAt(0).Identity);

            return userData.GetProperty<string>(field);
            
        }
        private async Task setUserData(IEnumerable<Participant> p, string field, string value)
        {
            // create the activity and retrieve
            stateClient = new StateClient(new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword));
            userData = await stateClient.BotState.GetUserDataAsync("skype", p.ElementAt(0).Identity);
            userData.SetProperty<string>(field, value);
            
            await stateClient.BotState.SetUserDataAsync("skype", p.ElementAt(0).Identity, userData);
        }
        private async Task CallOnRecordCompleted(RecordOutcomeEvent recordOutcomeEvent)
        {
            Debug.WriteLine("00000");
            Debug.WriteLine(recordOutcomeEvent.RecordOutcome.Outcome == Outcome.Success);
            // When recording is done, send to BingSpeech to process
            if (recordOutcomeEvent.RecordOutcome.Outcome == Outcome.Success)
            {
#if DEBUG
                //TEST AUDIO START
                ///Retrieve random audio            
                string user = ConfigurationManager.AppSettings["RSId"];
                string private_key = ConfigurationManager.AppSettings["RSPassword"];
                
                string replyAudioPath = "http://ec2-52-77-210-245.ap-southeast-1.compute.amazonaws.com/filestore/4_6243e7460bb03de/4_89e60a9e2072f2e.wav";

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
                        GetRecordForText("I didn't catch, would you kindly repeat?")
                    };
                }
            }

        }


        private async Task SendRecordMessage( string msg = "" )
        {
            Debug.WriteLine("#################SEND MESSAGE  Record Number:" + recordNum + "silence time: " + silenceTimes);
            string finalMsg = "";

            string recipientId = participant.ElementAt(0).Identity;
            string botId = participant.ElementAt(1).Identity;
            MicrosoftAppCredentials.TrustServiceUrl(serviceUrl, DateTime.Now.AddDays(7));
            ConnectorClient connector = new ConnectorClient(new Uri(serviceUrl), account);

            if (! msg.Equals(""))
            {
                finalMsg = msg;
            }
            else
            {
                string text = ED.response[recordNum];
                finalMsg = "Please record the sentence:\n" + text;
            }

            IMessageActivity newMessage = Activity.CreateMessageActivity();
            newMessage.Type = ActivityTypes.Message;
            newMessage.From = new ChannelAccount(botId, ConfigurationManager.AppSettings["BotId"]);
            newMessage.Conversation = new ConversationAccount(false, recipientId);
            newMessage.Recipient = new ChannelAccount(recipientId);
            newMessage.Text = finalMsg;

            await connector.Conversations.SendToConversationAsync((Activity)newMessage);
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
            else if (mode == -2)
            {
                // Configure the audio output. 
                MemoryStream ms = new MemoryStream();
                SSS.SpeechSynthesizer synth = new SSS.SpeechSynthesizer();

                string tempPath = Path.GetTempPath();
                synth.SetOutputToWaveStream(ms);
                synth.Speak(text);

                ////now convert to mp3 using LameEncoder or shell out to audiograbber
                am.ConvertWavStreamToWav(ref ms, $"{tempPath}Rate.wav");

                uri = new System.Uri(am.azureUrl);

                var prompt = new Prompt { FileUri = uri };
                return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };


            }
            else
            {
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

        private void genResponseCard(IEnumerable<Participant> participant)
        {

            string serviceUrl = "https://smba.trafficmanager.net/apis/";
            MicrosoftAppCredentials account = new MicrosoftAppCredentials(ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);

            string recipientId = participant.ElementAt(0).Identity;
            string botId = participant.ElementAt(1).Identity;
            MicrosoftAppCredentials.TrustServiceUrl(serviceUrl, DateTime.Now.AddDays(7));
            ConnectorClient connector = new ConnectorClient(new Uri(serviceUrl), account);

            List<CardAction> cardButtons = new List<CardAction>();

            CardAction plButton1 = new CardAction()
            {
                Value = "call",
                Type = ActionTypes.PostBack,
                Title = "call"
            };
            CardAction plButton2 = new CardAction()
            {
                Value = "record",
                Type = ActionTypes.PostBack,
                Title = "record"
            };

            cardButtons.Add(plButton1);
            cardButtons.Add(plButton2);

            var heroCard = new HeroCard()
            {
                Text = "Choose your destiny!",
                Buttons = cardButtons
            };

            IMessageActivity newMessage = Activity.CreateMessageActivity();
            newMessage.Type = ActivityTypes.Message;
            newMessage.From = new ChannelAccount(botId, ConfigurationManager.AppSettings["BotId"]);
            newMessage.Conversation = new ConversationAccount(false, recipientId);
            newMessage.Recipient = new ChannelAccount(recipientId);

            newMessage.Attachments = new List<Attachment> {
                heroCard.ToAttachment()
            };

            var response = connector.Conversations.SendToConversation((Activity)newMessage);
            
            //logger.WriteToText("BOT: ", txt);
        }

        private static PlayPrompt GetSilencePrompt(uint silenceLengthInMilliseconds = 300)
        {
            var prompt = new Prompt { Value = string.Empty, Voice = VoiceGender.Female, SilenceLengthInMilliseconds = silenceLengthInMilliseconds };
            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
        }
    }


}


