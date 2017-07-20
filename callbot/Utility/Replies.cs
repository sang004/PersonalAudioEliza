using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace callbot.utility
{
    public static class Replies
    {

        // audio playback with url
        public static PlayPrompt PlayAudioFile(string audioPath)
        {

            // example will be "https://callbotstorage.blob.core.windows.net/blobtest/graham_any_nation.wav"
            System.Uri uri = new System.Uri(audioPath);

            var prompt = new Prompt { FileUri = uri };
            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
        }

        public static PlayPrompt GetPromptForText(string text)
        {
            var prompt = new Prompt { Value = text, Voice = VoiceGender.Female };
            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
        }

        // generate a Hero card with call or record button
        public static void GenResponseCard(IEnumerable<Participant> participant, ConnectorClient connector)
        {


            string recipientId = participant.ElementAt(0).Identity;
            string botId = participant.ElementAt(1).Identity;

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

        // create a silent moment in ms
        public static PlayPrompt GetSilencePrompt(uint silenceLengthInMilliseconds = 500)
        {
            var prompt = new Prompt { Value = string.Empty, Voice = VoiceGender.Female, SilenceLengthInMilliseconds = silenceLengthInMilliseconds };
            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
        }

        public static Record SetRecord(string id, PlayPrompt prompt, bool playBeep, int maxSilenceTimeout)
        {
            return new Record()
            {
                MaxSilenceTimeoutInSeconds = maxSilenceTimeout,
                OperationId = id,
                PlayPrompt = prompt,
                MaxDurationInSeconds = 8,
                InitialSilenceTimeoutInSeconds = 3,
                PlayBeep = playBeep,
                RecordingFormat = RecordingFormat.Wav,
                StopTones = new List<char> { '#' }
            };
        }

        public static Record SetRecord(string id, bool playBeep, int maxSilenceTimeout)
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

        public static ActionBase GetRecordForText(string promptText, bool playbeep = false, int silenceTimeout = 3)
        {
            PlayPrompt prompt;
            if (string.IsNullOrEmpty(promptText))
                prompt = null;
            else
                prompt = Replies.GetPromptForText(promptText);

            var id = Guid.NewGuid().ToString();

            return SetRecord(id, prompt, playbeep, silenceTimeout);
        }

        public static ActionBase GetRecordForMessage()
        {
            string audioPath = "https://callbotstorage.blob.core.windows.net/built-in/Electronic_Chime.wav";
            System.Uri uri = new System.Uri(audioPath);
            var prompt = new Prompt { FileUri = uri };
            var playPrompt = new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
            var id = Guid.NewGuid().ToString();

            return SetRecord(id, playPrompt, false, 2);
        }

    }
}