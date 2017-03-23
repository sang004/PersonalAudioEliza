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


using Newtonsoft.Json;

namespace callbot
{
    public class RSAPI
    {
        public const string siteIP = "bitnami-resourcespace-b0e4.cloudapp.net";
        public string siteAddress = string.Format("http://{0}/api", siteIP);
        public string user;
        public string privateKey;
        public string queryUrl;
        public Parameters parameter = new Parameters();


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
        /// <returns>The response of the API calling or "Request Failed" if the API calling fails.</returns>
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
        /// Create a resource, upload the file to RS(copy the original file to /filestore), attach it to a resource id,
        /// add the resource to a collection, update title and date for the uploaded file.
        /// </summary>
        /// <param name="filePath">Original file path on the server</param>
        /// <remark>The original file to be uploaded by this function has to be on the same server where ResourceSpace
        /// is installed.</remark>
        public async void UploadResource(string filePath, string title, string collectionId)
        {
            //Console.WriteLine("start!");
            string resourceId = await CallAPI("create_resource", parameter.CreateResource("4"));
            //Console.WriteLine("reID"+resourceId);
            string uploadSuccess = await CallAPI("upload_file", parameter.UploadFile(resourceId, filePath));
            //Console.WriteLine("upload"+uploadSuccess);
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
            string folerPath = "Empty";
            string modFullPath = "";
            string fullPath = await CallAPI("get_resource_path", parameter.GetResourcePath(resourceId, extension));
            if (!string.IsNullOrEmpty(fullPath))
            {
                //List<string> fullPathList = fullPath.Split('\\').ToList();
                // escape the double quote in the two ends of the string
                // escape the file name because it is the same as the folder name which is not correct
                //folerPath = string.Join("", fullPathList.Skip(1).Take(fullPathList.Count() - 2));
                
                // Process and extract http link from string
                modFullPath = fullPath.Replace("__", "_").Replace("\\", "").Replace("\"", "");
                //MatchCollection ms = Regex.Matches(modFullPath, @"(www.+|http.+)([\s]|$)");
                //modFullPath = ms[0].Value.ToString();
            }
            return modFullPath;//folerPath;
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

        public async Task<String> searchFile(string searchInput)
        {

            string jsonResponse = await CallAPI("do_search", parameter.DoSearch(searchInput));
            if (!string.IsNullOrEmpty(jsonResponse))
            {
                Debug.WriteLine(jsonResponse);
            }
            return jsonResponse;//folerPath;

        }

        /// <summary>
        /// An example to use the async method that returns values.
        /// </summary>
        public async Task<String> Call( string keyword )
        {
            //fetch collection
            
            string jsonResponse = await searchFile(keyword);
            List<searchResult> jsonList = JsonConvert.DeserializeObject<List<searchResult>>(jsonResponse);

            string extension = jsonList[0].file_extension;
            string resourceID = jsonList[0].@ref;
            string path = await GetResourceFolder(resourceID, extension);
            Debug.WriteLine("Path:" + path);

            
            return path;

            // Create a collection 
            //string collectionID = await CreateCollection("566");
            //Console.WriteLine("result" + collectionID);
        }

        //static void Main(string[] args)
        //{
        //    string user = "user";
        //    string private_key = "a8b9e532120b6b5ce491d4b4a102266740d285ca32c76b6ec2b5dd1158177d25";

        //    RSAPI test2 = new RSAPI(user, private_key);

        //    test2.UploadResource("/home/bitnami/test/MaidwiththeFlaxenHair.mp3", "testmusictitle", "3");
        //    //test2.DeleteCollection("2");
        //    //test2.DeleteResource("5");
        //    //test2.Call();

        //    //foreach (string collectionId1 in new string[] {"8", "9", "10", "11", "13", "14", "15"}){
        //    //    Console.WriteLine(collectionId1);
        //    //    test2.DeleteCollection(collectionId1);
        //    //}

        //    Console.WriteLine("Press any key to exit");
        //    Console.ReadKey();
        //}

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

        public string DoSearch( string searchStr ) {
            string resTypes = "";
            string orderby = "";
            string archive = "0";
            string fetchrows = "";
            string sort = "desc";

            parameters = String.Format("param1={0}",
                         searchStr);

            return parameters;
        }

        public string GetResourcePath(string resourceId, string extension)
        {
            string getFilePath = "false";  // to get url"true";
            string size = "";
            string generate = "";
            string page = "";
            string watermarked = "";
            string alternative = "";
            parameters = String.Format("param1={0}&param2={1}&param3={2}&param4={3}&param5={4}",
                         resourceId, getFilePath, size, generate, extension);
            return parameters;
        }

        public string CreateResource(string resourceType)
        {
            parameters = String.Format("param1={0}", resourceType);
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