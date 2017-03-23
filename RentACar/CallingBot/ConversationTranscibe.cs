using System;
using System.IO;
using System.Reflection;
using System.Text;

using System.Diagnostics;

namespace callbot
{
    public class ConversationTranscibe
    {
        private string DatetimeFormat;
        private string Filename;

        public ConversationTranscibe(bool append = false) {
            DatetimeFormat = "yyyy-MM-dd_HH:mm:ss.fff";
            Filename = "C:\\Users\\user\\Downloads\\BOT\\LOL.txt"; // + Assembly.GetExecutingAssembly().GetName().Name + ".log";
            Debug.WriteLine($"FILENAME: {Filename}");

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

        public void uploadToRS() {

            string user = "user";
            string private_key = "a8b9e532120b6b5ce491d4b4a102266740d285ca32c76b6ec2b5dd1158177d25";

            RSAPI test2 = new RSAPI(user, private_key);

            string fileTitle = "ConversationLog_" + DateTime.Now.ToString(DatetimeFormat);
            test2.UploadResource(Filename, fileTitle, "3");


        }

    }
}