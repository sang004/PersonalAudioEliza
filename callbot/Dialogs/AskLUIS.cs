using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Microsoft.CognitiveServices.SpeechRecognition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using Autofac;
using System.Threading;
using static callbot.MessagesController;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;

using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System.Net.Http;

using System.Threading.Tasks;

using Newtonsoft.Json;
using System.Net.Http;
using System.Web;
using static System.Console;
using System.Diagnostics;

namespace callbot
{
    public class AskLUIS { 
        
        public AskLUIS() {
            
        }

        public String questionLUIS( String question )
        {
            LUISResponse luisResponse = new LUISResponse();
            
            string contextId = "";
            Task.Run(async () =>
            {
                luisResponse = await askLUIS(question, contextId);
                Debug.WriteLine(JsonConvert.SerializeObject(luisResponse));

            }).Wait();

            while (luisResponse?.dialog?.prompt?.Length > 0)
            {
                Debug.WriteLine(luisResponse.dialog.prompt + "  ");
                contextId = luisResponse.dialog.contextId;

                Task.Run(async () =>
                {
                    luisResponse = await askLUIS(question, contextId);
                    Debug.WriteLine(JsonConvert.SerializeObject(luisResponse));

                }).Wait();
            }
            Debug.WriteLine("dasdsasddsa:");
            Debug.WriteLine(JsonConvert.SerializeObject(luisResponse));

            return JsonConvert.SerializeObject(luisResponse);
        }

        static async Task<LUISResponse> askLUIS(string question, string contextId)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.projectoxford.ai");

                string id = "9f9431ae-4a39-4ac2-861a-b5ee265f5424";
                string subscriptionKey = "3368b24c1b4b488d8ee845e7f47a53cd";
                string requestUri = "";

                if (contextId == "")
                {
                    requestUri = $"/luis/v2.0/apps/{id}?subscription-key={subscriptionKey}&q={question}&timezoneOffset=8.0&verbose=true";
                }
                else
                {
                    requestUri = $"/luis/v2.0/apps/{id}?subscription-key={subscriptionKey}&q={question}&contextId={contextId}";
                }
                Debug.WriteLine(requestUri);
                HttpResponseMessage response = await client.GetAsync(requestUri);

                return JsonConvert.DeserializeObject<LUISResponse>(await response.Content.ReadAsStringAsync());
            }
        }

    }

}

