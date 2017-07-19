using Microsoft.CognitiveServices.SpeechRecognition;
using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;
using System.Configuration;

namespace callbot
{
    public class BingSpeech
    {
        private DataRecognitionClient dataClient;
        private Action<string> _callback;
        private ConversationResult conversationResult;
        private Action<bool> _failedCallback;
        private Action<string> _bingresponse;

        public String responseJson;

        public BingSpeech(ConversationResult conversationResult, Action<string> callback, Action<bool> failedCallback, Action<string> bingresponse)
        {
            responseJson = "";

            this.conversationResult = conversationResult;
            _callback = callback;
            _failedCallback = failedCallback;
            _bingresponse = bingresponse;

        }

        public string DefaultLocale { get; } = "en-US";
        public string SubscriptionKey { get; } = ConfigurationManager.AppSettings["BingKey"];

        public void CreateDataRecoClient()
        {
            this.dataClient = SpeechRecognitionServiceFactory.CreateDataClient(
                SpeechRecognitionMode.ShortPhrase,
                this.DefaultLocale,
                this.SubscriptionKey);
            
            this.dataClient.OnResponseReceived += this.OnDataShortPhraseResponseReceivedHandler;
        }

        public void SendAudioHelper(Stream recordedStream)
        {
            // Note for wave files, we can just send data from the file right to the server.
            // In the case you are not an audio file in wave format, and instead you have just
            // raw data (for example audio coming over bluetooth), then before sending up any 
            // audio data, you must first send up an SpeechAudioFormat descriptor to describe 
            // the layout and format of your raw audio data via DataRecognitionClient's sendAudioFormat() method.
            int bytesRead = 0;
            byte[] buffer = new byte[1024];
            try
            {
                do
                {
                    // Get more Audio data to send into byte buffer.
                    bytesRead = recordedStream.Read(buffer, 0, buffer.Length);

                    // Send of audio data to service. 
                    this.dataClient.SendAudio(buffer, bytesRead);
                }
                while (bytesRead > 0);
            }
            catch (Exception ex)
            {
                WriteLine("Exception ------------ " + ex.Message);
            }
            finally
            {
                // We are done sending audio.  Final recognition results will arrive in OnResponseReceived event call.
                this.dataClient.EndAudio();
            }

        }

        private void OnDataShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {

            this.WriteLine("--- OnDataShortPhraseResponseReceivedHandler ---");
            this.WriteResponseResult(e);

            // we got the final result, so it we can end the mic reco.  No need to do this
            // for dataReco, since we already called endAudio() on it as soon as we were done
            // sending all the data.

            // Send to bot
            if (e.PhraseResponse.RecognitionStatus == RecognitionStatus.RecognitionSuccess)
            {
                _failedCallback(false);
                
            }
            else
            {
                _failedCallback(true);
            }
        }
        
        private void WriteResponseResult(SpeechResponseEventArgs e)
        {
            if (e.PhraseResponse.Results.Length == 0)
            {
                this.WriteLine("No phrase response is available.");
            }
            else
            {
                this.WriteLine("********* Final n-BEST Results *********");
                for (int i = 0; i < e.PhraseResponse.Results.Length; i++)
                {
                    this.WriteLine(
                        "[{0}] Confidence={1}, Text=\"{2}\"",
                        i,
                        e.PhraseResponse.Results[i].Confidence,
                        e.PhraseResponse.Results[i].DisplayText);
                }

                this.WriteLine(string.Empty);
            }

            // if bing does not return anything
            if (e.PhraseResponse.Results.Length > 0)
            {
                _bingresponse(e.PhraseResponse.Results[0].DisplayText);
            }
            else {
                _bingresponse("");
            }
        }

        private void WriteLine(string format, params object[] args)
        {
            var formattedStr = string.Format(format, args);
            Trace.WriteLine(formattedStr);
            Debug.WriteLine(formattedStr);
        }
    }
}