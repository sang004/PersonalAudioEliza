using Microsoft.Bot.Builder.Calling;
using Microsoft.Bot.Builder.Calling.Events;
using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Configuration;

using System.IO;
using System.Diagnostics;
using Microsoft.Bot.Connector;
using System.Linq;
using System.Threading;
using callbot.utility;

namespace callbot
{
    public class simplecallbot : IDisposable, ICallingBot
    {
        public ICallingBotService CallingBotService
        {
            get; private set;
        }

        private List<string> response = new List<string>();
        private string activeAcc = null;
        private string activeMode = null;
        private bool isSet;
        int askUserTime = 0;
        int silenceTimes = 0;

        static string serviceUrl = "https://smba.trafficmanager.net/apis/";
        static MicrosoftAppCredentials account = new MicrosoftAppCredentials(ConfigurationManager.AppSettings["MicrosoftAppId"], ConfigurationManager.AppSettings["MicrosoftAppPassword"]);
        ConnectorClient connector = new ConnectorClient(new Uri(serviceUrl), account);

        bool sttFailed = false;
        string bingresponse = "";

        IEnumerable<Participant> participant = null;
        private int recordNum = -1;
        private string recordPath = "";
        //static ConversationTranscibe logger = new ConversationTranscibe(); // Will create a fresh new log file
        private static Dialogs.ElizaDialog ED = new Dialogs.ElizaDialog();
        private static int clipNum = 5;//ED.response.Count;
        private static audioMan am = new audioMan();
        private SilenceTrim Trimmer = new SilenceTrim();
        private RSAPI RS = new RSAPI(ConfigurationManager.AppSettings["RSId"], ConfigurationManager.AppSettings["RSPassword"]);

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

        private Task<bool> OnIncomingCallReceived(IncomingCallEvent incomingCallEvent)
        {
            // reset all flags
            silenceTimes = 0;
            isSet = false;
            recordNum = -1;
            sttFailed = false;
            bingresponse = "";
            activeAcc = null;
            activeMode = null;
            askUserTime = 0;

            participant = incomingCallEvent.IncomingCall.Participants;
            var id = Guid.NewGuid().ToString();
            MicrosoftAppCredentials.TrustServiceUrl(serviceUrl, DateTime.Now.AddDays(7));

            //remove the persistent variables first
            int retries = 0;
            BotStateEdit.removeUserData(participant, "activeMode", ref retries);
            BotStateEdit.removeUserData(participant, "activeAcc", ref retries);
            incomingCallEvent.ResultingWorkflow.Actions = new List<ActionBase>
            {
                new Answer { OperationId = id },
                Replies.GetPromptForText("What would you like to do today? I just messaged you.")
            };
            return Task.FromResult(true);
        }

        private string GetActiveProperty(string property, string propertyName, int timeoutNum)
        {
            int retries = 0;
            for (int i = 0; i < timeoutNum; i++)
            {
                property = BotStateEdit.getUserData(participant, propertyName, ref retries);
                if (property == null)
                {
                    Thread.Sleep(30);
                }
                else
                {
                    break;
                }
            }
            return property;
        }

        private async Task<Task> OnPlayPromptCompleted(PlayPromptOutcomeEvent playPromptOutcomeEvent)
        {

            Debug.WriteLine("###################Onplayprompt");

            Debug.WriteLine($"#################CreateRichCard when isSet == {isSet}");
            // only run once when it first comes into this function
            if (isSet == false && askUserTime == 0)
            {
                await GetUserOption(playPromptOutcomeEvent);
            }

            if (isSet)
            {
                if (activeMode.Equals("record"))
                {
                    await RecordOnPlayPromptCompleted(playPromptOutcomeEvent);
                }
                else
                {
                    await CallOnPlayPromptCompleted(playPromptOutcomeEvent);
                }
            }
            return Task.CompletedTask;
        }

        private async Task GetUserOption(PlayPromptOutcomeEvent playPromptOutcomeEvent)
        {
            // ask user for action (call / record)
            if (activeMode == null)
            {
                //get click input here
                Replies.GenResponseCard(participant, connector);
                activeMode = GetActiveProperty(activeMode, "activeMode", 50);
                Debug.WriteLine($"ACTION: {activeMode}");
            }

            //When user has choosen which mode (call / record), ask profile name
            if (activeMode != null)
            {
                askUserTime = 1;
                await SendRecordMessage($"Who would you like to {activeMode} as?");
                activeAcc = GetActiveProperty(activeAcc, "activeAcc", 50);
                Debug.WriteLine($"WHO: {activeAcc}");

                if (activeMode == "record" && activeAcc != null)
                {
                    int noOfClips = await RS.isExist(activeAcc);
                    if (noOfClips > 0)
                    {
                        int retries = 0;
                        BotStateEdit.removeUserData(participant, "activeAcc", ref retries);
                        await SendRecordMessage($"Profile already exist, please choose another");
                        activeAcc = null;
                        activeAcc = GetActiveProperty(activeAcc, "activeAcc", 50);
                    }
                }
            }

            isSet = activeAcc != null && activeMode != null;
            Debug.WriteLine($"isSET: {isSet}");

            if (!isSet)
            {
                Debug.WriteLine("No message recieved.");
                playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    await TextToAudio("Message reply not recieved, Bye"),
                    new Hangup() { OperationId = Guid.NewGuid().ToString() }
                };
                playPromptOutcomeEvent.ResultingWorkflow.Links = null;
                silenceTimes = 0;
            }
        }

        private Task RecordOnPlayPromptCompleted(PlayPromptOutcomeEvent playPromptOutcomeEvent)
        {
            if (recordNum == -1)
            {
                // create a directory in the appdata temp folder to temporary store audio on local
                recordPath = Path.GetTempPath() + activeAcc.ToString();
                Directory.CreateDirectory(recordPath);

                playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                { Replies.GetRecordForText("Recording. Please read out the sentence after you received the message and heared the beep.", silenceTimeout: 2) };
            }
            else
            {
                playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase> { Replies.GetRecordForMessage() };
            }
            return Task.CompletedTask;
        }

        private async Task CallOnPlayPromptCompleted(PlayPromptOutcomeEvent playPromptOutcomeEvent)
        {
            var actionList = new List<ActionBase>();
            if (recordNum == -1)
            {
                Debug.WriteLine("^^^^^^Battle prompt");
                playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    Replies.GetPromptForText($"Transferring call to {activeAcc}!"),
                    await TextToAudio("Hi!"),
                    Replies.GetRecordForText(string.Empty)

                };
                silenceTimes = 0;
                recordNum++;
            }
            else
            {
                Debug.WriteLine("^^^^^^prompt completed, bing result: ", bingresponse);
                if (bingresponse != "")
                {
                    silenceTimes = 0;

                    // if its bye
                    if (bingresponse.ToLower().Contains("bye"))
                    {
                        playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                        {
                            await TextToAudio("Bye bye"),
                            new Hangup() { OperationId = Guid.NewGuid().ToString() }
                        };
                        playPromptOutcomeEvent.ResultingWorkflow.Links = null;

                    }
                    else
                    {
                        actionList.Add(await BingToAudio(bingresponse));

                        actionList.Add(Replies.GetRecordForText(string.Empty));
                        playPromptOutcomeEvent.ResultingWorkflow.Actions = actionList;
                    }
                    bingresponse = "";
                }
                else
                {
                    if (sttFailed)
                    {
                        Debug.WriteLine("^^^^^^^^^^^Bing not captured");
                        playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                        {
                            await TextToAudio("I didn't catch that, would you kindly repeat?"),
                            Replies.GetRecordForText(string.Empty)
                        };
                        sttFailed = false;
                        silenceTimes++;

                    }
                    else if (silenceTimes > 5)
                    {
                        playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                        {
                            await TextToAudio("I think the call is breaking up... Bye"),
                            new Hangup() { OperationId = Guid.NewGuid().ToString() }
                        };
                        playPromptOutcomeEvent.ResultingWorkflow.Links = null;
                    }
                    else
                    {
                        Debug.WriteLine("^^^^^^^^^last resort");
                        //last resort, listen longer
                        silenceTimes++;
                        playPromptOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                        {
                           Replies.GetSilencePrompt()

                        };

                    }
                }
            }
        }

        private Task OnHangupCompleted(HangupOutcomeEvent hangupOutcomeEvent)
        {
            //logger.uploadToRS();
            Debug.WriteLine("###################Hanging up");

            //remove the persistent variables first
            int retries = 0;
            BotStateEdit.removeUserData(participant, "activeMode", ref retries);
            BotStateEdit.removeUserData(participant, "activeAcc", ref retries);

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

        private async Task RecordOnRecordCompleted(RecordOutcomeEvent recordOutcomeEvent)
        {
            Debug.WriteLine("00000");
            Debug.WriteLine(recordOutcomeEvent.RecordOutcome.Outcome == Outcome.Success);

            if (recordOutcomeEvent.RecordOutcome.Outcome == Outcome.Success && recordNum >= 0)
            {
                var record = await recordOutcomeEvent.RecordedContent;
                silenceTimes = 0;

                #region RECORD_SAVE
                if (recordNum < clipNum)
                {
                    Debug.WriteLine("#########################SAVING RECORD AT: " + recordPath);
                    MemoryStream ms = new MemoryStream();
                    record.CopyTo(ms);
                    string filePath = string.Format("{0}\\{1}_{2}.wav", recordPath, recordNum, activeAcc);
                    am.ConvertWavStreamToWav(ref ms, filePath);
                    recordNum++;
                    // send message for the next clip
                    if (recordNum < clipNum)
                    {
                        await SendRecordMessage();
                        recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                        {
                            Replies.GetRecordForMessage()
                        };
                    }
                    // all record done
                    else
                    {
                        recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                        {
                            Replies.GetRecordForText("Record completed, uploading recording!Please do not hangup!")
                        };
                    }
                }
                else if (recordNum == clipNum)
                {
                    recordNum++;
                    recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        await UploadToRs(recordPath),
                        new Hangup() { OperationId = Guid.NewGuid().ToString() }
                    };
                    recordOutcomeEvent.ResultingWorkflow.Links = null;
                }
                #endregion
            }
            else if (recordOutcomeEvent.RecordOutcome.FailureReason == "CallTerminated")
            {
                //So if the caller hangs up, initiate hangout on bot
                new Hangup() { OperationId = Guid.NewGuid().ToString() };
                recordOutcomeEvent.ResultingWorkflow.Links = null;
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
                        Replies.GetRecordForMessage()
                    };
                    silenceTimes++;
                }
                else
                {
                    if (silenceTimes < 5)
                    {
                        silenceTimes++;
                        recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                        {
                            await TextToAudio("I didn't catch that, would you kindly repeat?"),
                            Replies.GetRecordForText(string.Empty)
                        };
                    }
                    else
                    {
                        recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                        {
                            await TextToAudio("I didn't catch that, record terminated!"),
                            new Hangup() { OperationId = Guid.NewGuid().ToString() }
                        };
                        recordOutcomeEvent.ResultingWorkflow.Links = null;
                        silenceTimes = 0;
                    }
                }
            }
        }

        private async Task CallOnRecordCompleted(RecordOutcomeEvent recordOutcomeEvent)
        {
            Debug.WriteLine("00000");
            Debug.WriteLine(recordOutcomeEvent.RecordOutcome.Outcome == Outcome.Success);
            // When recording is done, send to BingSpeech to process
            if (recordOutcomeEvent.RecordOutcome.Outcome == Outcome.Success)
            {
#if RELEASE
                //TEST AUDIO START
                ///Retrieve random audio            
                string replyAudioPath = "http://ec2-13-228-78-239.ap-southeast-1.compute.amazonaws.com/filestore/4_6243e7460bb03de/4_89e60a9e2072f2e.wav";

                var webClient = new WebClient();
                byte[] bytes = webClient.DownloadData(replyAudioPath);

                System.IO.Stream streams = new System.IO.MemoryStream(bytes);
                var record = streams;

                //TEST AUDIO END

#else
                var record = await recordOutcomeEvent.RecordedContent;

#endif

                Debug.WriteLine("^^^^^^^^^Record succeed, bing process");
                BingSpeech bs = new BingSpeech(recordOutcomeEvent.ConversationResult, t => response.Add(t), s => sttFailed = s, b => bingresponse = b);
                ;
                bs.CreateDataRecoClient();
                bs.SendAudioHelper(record);
                recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    Replies.GetSilencePrompt()
                };
            }
            else if (recordOutcomeEvent.RecordOutcome.FailureReason == "CallTerminated")
            {
                //So if the caller hangs up, initiate hangout on bot
                new Hangup() { OperationId = Guid.NewGuid().ToString() };
                recordOutcomeEvent.ResultingWorkflow.Links = null;
            }
            else
            {
                if (silenceTimes > 5)
                {
                    recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        await TextToAudio("Bye bye"),
                        new Hangup() { OperationId = Guid.NewGuid().ToString() }
                    };
                    recordOutcomeEvent.ResultingWorkflow.Links = null;
                    silenceTimes = 0;
                }
                else
                {
                    Debug.WriteLine("^^^^^^^^^^^^RECORD FAILED");
                    silenceTimes++;
                    recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        await TextToAudio("I didn't catch that, would you kindly repeat?"),
                        Replies.GetRecordForText(string.Empty)
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

            // if param msg has words, just reply using msg. Else use Eliza to determine response
            if (! msg.Equals(""))
            {
                finalMsg = msg;
            }
            else
            {
                string text = ED.response[recordNum];
                finalMsg = string.Format("{0}/{1}: {2}\n", recordNum + 1, clipNum, text);
            }

            IMessageActivity newMessage = Activity.CreateMessageActivity();
            newMessage.Type = ActivityTypes.Message;
            newMessage.From = new ChannelAccount(botId, ConfigurationManager.AppSettings["BotId"]);
            newMessage.Conversation = new ConversationAccount(false, recipientId);
            newMessage.Recipient = new ChannelAccount(recipientId);
            newMessage.Text = finalMsg;

            await connector.Conversations.SendToConversationAsync((Activity)newMessage);
        }

        private async Task<PlayPrompt> BingToAudio(string keyword)
        {
            // create a reflection of user speech
            string output = await ED.Reply(keyword);
            int ElizaIdx = ED.response.IndexOf(output);

            string audioKeyword = ElizaIdx + "_" + activeAcc;

            string path = RS.Call(audioKeyword).Result;
            if (!path.Equals(""))
            {
                return Replies.PlayAudioFile(path);
            }
            // microsoft stt
            else
            {
                return Replies.GetPromptForText(output);
            }
        }

        private async Task<PlayPrompt> TextToAudio(string text)
        {
            int ElizaIdx = ED.response.IndexOf(text);
            if (ElizaIdx == -1)
            {
                return Replies.GetPromptForText(text);
            }
            string audioKeyword = ElizaIdx + "_" + activeAcc;

            string path = RS.Call(audioKeyword).Result;
            if (!path.Equals(""))
            {
                return Replies.PlayAudioFile(path);
            }
            // microsoft stt
            else
            {
                return Replies.GetPromptForText(text);
            }
        }

        /// <summary>
        /// Upload clips saved on local to ResourceSpace.
        /// Firstly trim the silence, then sftp transfer the file to RS, add the clips to RS database.
        /// Finally delete the clips on local and the sftp transfer result.
        /// </summary>
        /// <param name="recordPath"></param>
        /// <returns></returns>
        private Task<PlayPrompt> UploadToRs(string recordPath)
        {
            DirectoryInfo dir = new DirectoryInfo(recordPath);

            if (dir.GetFiles("*.wav").Length != clipNum)
            {
                System.IO.Directory.Delete(recordPath, true);
                Debug.WriteLine(string.Format("Number of recorded clips {0} less than {1}", dir.GetFiles("*.wav").Length.ToString(), "3"));
                return TextToAudio("Upload failed, bye!");
            }
            else
            {   
                //trim silence at the end of the recording
                foreach (var file in dir.GetFiles("*.wav"))
                {
                    string filePath = recordPath + "//" + file.ToString();
                    Trimmer.TrimSilenceEnd(filePath, filePath);
                }

                // sftp upload recording to RS server
                if (!sftp.UploadSFTPFile(recordPath))
                {
                    return TextToAudio("Upload failed, bye!");
                }

                foreach (var file in dir.GetFiles("*.wav"))
                {
                    Debug.WriteLine(file.ToString());
                    string filePath = "/home/upload/" + file.ToString();
                    RS.UploadResource(filePath, file.ToString(), ".wav");
                }
                // Delete the folder contains the clips
                sftp.DeleteSFTPFile(recordPath);
                System.IO.Directory.Delete(recordPath, true);
                return TextToAudio("Upload completeted, bye!");
            }
        }
    }
}
