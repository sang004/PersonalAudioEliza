using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Security.Cryptography;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;

using WinSCP;
using Newtonsoft.Json;

namespace callbot
{
    public class RSAPI
    {
        public const string siteIP = "ec2-52-77-210-245.ap-southeast-1.compute.amazonaws.com";
        public string siteAddress = string.Format("http://{0}/api", siteIP);
        public string user;
        public string privateKey;
        public string queryUrl;
        public Parameters parameter = new Parameters();

        // TEST _ toggle random audio input from RS
        const string mode = "demo";
        
        public RSAPI(string username, string userPrivateKey)
        {
            user = username;
            privateKey = userPrivateKey;
        }


        /// <summary>
        /// Form the parameters part of the API url. 
        /// </summary>
        /// <remark> The url consists of the query string and hashed query string together with the 
        /// private key.</remark>
        public void FormQuery(string functionName, string parameters)
        {
            string query = string.Format("user={0}&function={1}&{2}", user, functionName, parameters);

            //hash the query string
            SHA256 querySHA256 = SHA256Managed.Create();
            Encoding enc = Encoding.UTF8;
            Byte[] queryByte = querySHA256.ComputeHash(enc.GetBytes(privateKey + query));
            string sign = string.Concat(queryByte.Select(item => item.ToString("x2")));

            queryUrl = string.Format("?{0}&sign={1}", query, sign);
        }

        /// <summary>
        /// Call the API with the url.
        /// </summary>
        /// <returns>The response of the API calling or "Request Failed"bitnami  if the API calling fails.</returns>
        public async Task<string> CallAPI(string functionName, string parameters)
        {
            FormQuery(functionName, parameters);

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(siteAddress);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.GetAsync(queryUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
            else
            {
                return "Request Failed";
            }
        }

        /// <summary>
        /// Create a resource, upload the file to RS (with url of the file), attach it to a resource id,
        /// add the resource to a collection, update title and date for the uploaded file.
        /// </summary>
        /// <param name="fileUrl">Original file url</param>
        /// <remark>The original file to be uploaded by this function has to be on the same server where ResourceSpace
        /// is installed.</remark>
        public async void UploadResource(string fileUrl, string title, string extension)
        {
            string resourceType = "";
            string collectionId = "";

            if (extension == ".mp3" || extension == ".wav")
            {
                resourceType = "4";
                collectionId = "6";
                Console.WriteLine(extension);
            }
            else{
                resourceType = "2";
                collectionId = "5";
            }

            // get sftp path on resource space server
            //string rsPath = sstpProtocol(filePath);

            Debug.WriteLine("............extension: " + extension + " resourceType: " + resourceType);
            string resourceId = await CallAPI("create_resource", parameter.CreateResource(resourceType));
            Debug.WriteLine("............resourceId: " + resourceId);
            string uploadSuccess = await CallAPI("upload_file_by_url", parameter.UploadFile(resourceId, fileUrl));

            if (uploadSuccess.Equals("true"))
            {
                await CallAPI("update_field", parameter.UpdateField(resourceId, "8", title));
                await CallAPI("update_field", parameter.UpdateField(resourceId, "12"));
                await CallAPI("add_resource_to_collection", parameter.AddResourceToCollection(resourceId, collectionId));
                //Console.WriteLine("added");
            }
        }


        /// <returns>The server path to the folder that contains the resource file</returns>
        public async Task<string> GetResourceFolder(string resourceId, string extension)
        {
            string modFullPath = "";
            string fullPath = await CallAPI("get_resource_path", parameter.GetResourcePath(resourceId, extension));
            if (!string.IsNullOrEmpty(fullPath))
            {
                // Edit url to a correct http format
                modFullPath = fullPath.Replace("__", "_").Replace("\\", "").Replace("\"", "");
            }
            return modFullPath;
        }

        public async Task<string> CreateCollection(string collectionName)
        {
            string collectionId = await CallAPI("create_collection", parameter.CreateCollection(collectionName));
            return collectionId;
        }

        /// <remark>The API return "null" either the collection to be deleted is existed or not.</remark>
        public async void DeleteCollection(string collectionId)
        {
            await CallAPI("delete_collection", parameter.DeleteCollection(collectionId));
            //string response = await CallAPI("delete_collection", parameter.DeleteCollection(collectionId));
            //Console.WriteLine("response: "+response);
        }

        /// <summary>
        /// Delete the resource in the website, database and under the folder /filestore.
        /// </summary>
        /// <remark>One time API calling cannot really work. Keep query until API return "false" to make sure the
        /// resource is deleted. Restrict to 3 iteration of the API calling.</remark>
        public async void DeleteResource(string resourceId)
        {
            string deleteResponse;
            for (int i = 0; i < 3; i++)
            {
                deleteResponse = await CallAPI("delete_resource", parameter.DeleteResource(resourceId));
                //Console.WriteLine("Response" + deleteResponse);
                if (deleteResponse.Equals("false")) { break; }
            }
        }

        public async Task<String> searchFile(string searchInput, string resTypes)
        {
            string jsonResponse = "";
            // use do_search api call to search using string and return json
            jsonResponse = await CallAPI("do_search", parameter.DoSearch(searchInput, resTypes));
            if (!string.IsNullOrEmpty(jsonResponse))
            {
                Debug.WriteLine(jsonResponse);
            }
            return jsonResponse;

        }

        /// <summary>
        /// An example to use the async method that returns values.
        /// </summary>
        public async Task<String> Call( string keyword )
        {
            string extension = "";
            string resourceID = "";
            int eleIdx = 0;

            //fetch audio files based on keywords
            string jsonResponse = await searchFile(keyword, "4");
            if (jsonResponse.Equals(""))
            {
                return "";
            }
            List<searchResult> jsonList = JsonConvert.DeserializeObject<List<searchResult>>(jsonResponse);

            if (mode == "demo")
            {
                Random rnd = new Random();
                eleIdx = rnd.Next(jsonList.Count);

            }

            extension = jsonList[eleIdx].file_extension;
            resourceID = jsonList[eleIdx].@ref;
            string path = await GetResourceFolder(resourceID, extension);
            Debug.WriteLine("Path:" + path);

            return path;

            // Create a collection 
            //string collectionID = await CreateCollection("566");
            //Console.WriteLine("result" + collectionID);
        }

        public string sstpProtocol( string local_filePath )
        {
            string fileExtension = local_filePath.Split('.').Last();
            string remote_fileName = "";
            string remote_filePath = "";
            string DatetimeFormat;

            DatetimeFormat = "yyyy-MM-dd_HH-mm";
            remote_fileName = "ChatLog_" + DateTime.Now.ToString(DatetimeFormat) + "." + fileExtension;
            remote_filePath = $"/home/bitnami/test/{remote_fileName}";

            try
            {
                // Setup session options
                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Sftp,
                    HostName = "52.187.186.3",
                    UserName = "bitnami",
                    Password = "8dyZ!B)-4JM}",
                    SshHostKeyFingerprint = "ssh-rsa 2048 c4:0b:99:e9:53:a4:1a:af:24:e1:8c:f5:44:a7:13:ff"
                };

                using (Session session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);

                    // Upload files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;

                    TransferOperationResult transferResult;
                    transferResult = session.PutFiles(local_filePath, remote_filePath, false, transferOptions);

                    // Throw on any error
                    transferResult.Check();

                    // Print results
                    foreach (TransferEventArgs transfer in transferResult.Transfers)
                    {
                        Console.WriteLine("Upload of {0} succeeded", transfer.FileName);
                    }
                }

                return remote_filePath;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
                return "Error" ;
            }

        }

    }

    

    /// <summary>
    /// Form the parameters string.
    /// </summary>
    public class Parameters
    {
        public string parameters;

        public Parameters()
        {
            parameters = "";
        }


        public string DoSearch( string searchStr, string resTypes ) {
            string orderby = "title";
            string archive = "0";
            string fetchrows = "";
            string sort = "desc";

            parameters = String.Format("param1={0}&param2={1}&param3={2}",
                         searchStr, resTypes, orderby);

            return parameters;
        }

        public string GetResourcePath(string resourceId, string extension)
        {
            string getFilePath = "false";  // to get url"true";
            string size = "";
            string generate = "";

            parameters = String.Format("param1={0}&param2={1}&param3={2}&param4={3}&param5={4}",
                         resourceId, getFilePath, size, generate, extension);
            return parameters;
        }

        public string CreateResource(string resourceType)
        {
            string archive = "2";
            string revert = "";

            parameters = String.Format("param1={0}&prarm2={1}&param5={2}",
                resourceType, archive, revert);
            return parameters;
        }

        public string UploadFile(string resourceId, string filePath)
        {
            string noExif = "1";
            string revert = "";
            string autorotate = "1";
            parameters = String.Format("param1={0}&param2={1}&param3={2}&param4={3}&param5={4}",
                         resourceId, noExif, revert, autorotate, filePath);
            return parameters;
        }

        public string UpdateField(string resourceId, string fieldID, string value = null)
        {
            // get the current date when update the field "Date"
            if (fieldID == "12")
            {
                value = DateTime.Now.ToString("yyyy-MM-dd");
            }
            parameters = String.Format("param1={0}&param2={1}&param3={2}",
                         resourceId, fieldID, value);
            return parameters;
        }

        public string AddResourceToCollection(string resourceId, string collectionId)
        {
            parameters = String.Format("param1={0}&param2={1}", resourceId, collectionId);
            return parameters;
        }

        public string CreateCollection(string collectionName)
        {
            parameters = String.Format("param1={0}", collectionName);
            return parameters;
        }

        public string DeleteCollection(string collectionId)
        {
            parameters = String.Format("param1={0}", collectionId);
            return parameters;
        }

        public string DeleteResource(string resourceId)
        {
            parameters = String.Format("param1={0}", resourceId);
            return parameters;
        }

    }

    public class searchResult
    {
        public string @ref { get; set; }
        public string resource_type { get; set; }
        public string file_extension { get; set; }
        public string file_path { get; set; }
    }
}