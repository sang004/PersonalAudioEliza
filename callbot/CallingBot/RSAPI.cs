using System;
using System.Collections.Generic;
using System.Linq;

using System.Security.Cryptography;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;

using Renci.SshNet;
using System.IO;
using Newtonsoft.Json;
using System.Configuration;

namespace callbot
{
    public class RSAPI
    {
        public string siteIP { get; } = ConfigurationManager.AppSettings["RSBaseURI"];
        public string user;
        public string privateKey;
        public string queryUrl;
        public Parameters parameter = new Parameters();

        // TEST _ toggle random audio input from RS
        const string mode = "live";
        
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
            client.BaseAddress = new Uri(string.Format("http://{0}/api", siteIP));
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
        /// Create a resource id, attach a local file to the resource id,
        /// add the resource to a collection, update title and date for the uploaded file.
        /// </summary>
        /// <param name="filePath">Original file path</param>
        /// <remark>The file path on RS server</remark>
        public async void UploadResource(string filePath, string title, string extension)
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

            string resourceId = await CallAPI("create_resource", parameter.CreateResource(resourceType));
            Debug.WriteLine("............resourceId: " + resourceId);
            string uploadSuccess = await CallAPI("upload_file", parameter.UploadFile(resourceId, filePath));

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
            List<searchResult> jsonList;

            //fetch audio files based on keywords
            string jsonResponse = await searchFile(keyword, "4");
            try
            {
                jsonList = JsonConvert.DeserializeObject<List<searchResult>>(jsonResponse);
            }
            catch (JsonSerializationException)
            {
                Debug.WriteLine("======Could not find audio in RS");
                return "";
            }

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

        public async Task<int> isExist(string keyword)
        {
            List<searchResult> jsonList;

            //fetch audio files based on keywords
            string jsonResponse = await searchFile(keyword, "4");
            try
            {
                jsonList = JsonConvert.DeserializeObject<List<searchResult>>(jsonResponse);
            }
            catch (Exception)
            {
                Debug.WriteLine("======Could not find audio in RS");
                return 0;
            }

            if (jsonList == null) {
                return 0;
            }
            return jsonList.Count;
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
            string orderby = "relevance";
            string sort = "desc";

            parameters = String.Format("param1={0}&param2={1}&param3={2}&param6={3}",
                         searchStr, resTypes, orderby, sort);

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

    public class sftp
    {   
        private static string destinationPath = ConfigurationManager.AppSettings["RSSftpDestination"];
        private static string host = ConfigurationManager.AppSettings["RSHost"];
        private static string username = ConfigurationManager.AppSettings["RSSftpUsername"];
        private static string password = ConfigurationManager.AppSettings["RSSftpPassword"];
        private static int port = 22;
        
        /// <summary>
        /// Sftp upload the .wav file under the file folder to server
        /// </summary>
        /// <param name="sourceFileFolder"></param>
        public static bool UploadSFTPFile(string sourceFileFolder)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceFileFolder);

            try
            {
                using (SftpClient client = new SftpClient(host, port, username, password))
                {
                    client.Connect();
                    client.ChangeDirectory(@destinationPath);
                    foreach (var file in dir.GetFiles("*.wav"))
                    {
                        string sourcefile = sourceFileFolder + "\\" + file.ToString();
                        using (FileStream fs = new FileStream(sourcefile, FileMode.Open))
                        {
                            client.BufferSize = 40 * 1024;
                            Debug.WriteLine("........ file:" + sourcefile);
                            client.UploadFile(fs, Path.GetFileName(sourcefile));
                        }
                    }
                    client.Disconnect();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Sftp delete the .wav file under the file folder to server
        /// </summary>
        /// <param name="sourceFileFolder"></param>
        public static bool DeleteSFTPFile(string sourceFileFolder)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceFileFolder);

            try
            {
                using (SftpClient client = new SftpClient(host, port, username, password))
                {
                    client.Connect();
                    client.ChangeDirectory(destinationPath);
                    foreach (var file in dir.GetFiles("*.wav"))
                    {
                        string sourcefile = sourceFileFolder + "\\" + file.ToString();
                        client.BufferSize = 40 * 1024;
                        client.DeleteFile(Path.GetFileName(sourcefile));
                    }
                    client.Disconnect();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}

            