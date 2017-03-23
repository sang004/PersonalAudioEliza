using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace callbot
{
    public class ConversationTranscibe
    {
        private string DatetimeFormat;
        private string Filename;

        public ConversationTranscibe(bool append = false) {
            DatetimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

            string logHeader = Filename + " is created.";
            if (!File.Exists(Filename))
            {
                WriteLine(DateTime.Now.ToString(DatetimeFormat) + " " + logHeader, false);
            }
            else
            {
                if (append == false)
                    WriteLine(DateTime.Now.ToString(DatetimeFormat) + " " + logHeader, false);
            }

        }

        public void WriteToText(string respondance, string text)
        {
            WriteLine(respondance + text);
        }

        private void WriteLine(string text, bool append = true)
        {
            try
            {
                using (StreamWriter Writer = new StreamWriter(Filename, append, Encoding.UTF8))
                {
                    if (text != "") Writer.WriteLine(text);
                }
            }
            catch
            {
                throw;
            }
        }
        

    }
}