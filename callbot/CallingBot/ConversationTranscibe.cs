using System;
using System.IO;
using System.Reflection;
using System.Text;

using System.Diagnostics;

// Using statements for our libraries.
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.Configuration;
using System.Web;

namespace callbot
{
    public class ConversationTranscibe
    {
        private string DatetimeFormat;
        private string fileName;
        private CloudAppendBlob appBlob;

        public ConversationTranscibe(bool append = false)
        {

            // Let's set up our connection for the account and store the name and key in app.config.           
            string accName = ConfigurationManager.AppSettings["AzureStoreId"];
            string accKey = ConfigurationManager.AppSettings["AzureStorePassword"];

            // Implement the accout, set true for https for SSL.
            StorageCredentials creds = new StorageCredentials(accName, accKey);
            CloudStorageAccount strAcc = new CloudStorageAccount(creds, true);
            CloudBlobClient blobClient = strAcc.CreateCloudBlobClient();

            //Setup our container we are going to use and create it.
            CloudBlobContainer container = blobClient.GetContainerReference("logs");
            container.CreateIfNotExistsAsync();

            // Build my typical log file name.
            DateTime date = DateTime.Today;
            DateTime dateLogEntry = DateTime.Now;
            // This creates a reference to the append blob we are going to use.
            fileName = string.Format("{0}{1}", date.ToString("yyyyMMdd_HHmm"), ".txt");
            appBlob = container.GetAppendBlobReference(fileName);

            // Now we are going to check if todays file exists and if it doesn't we create it.
            if (!appBlob.Exists())
            {
                appBlob.CreateOrReplace();
            }
        }

        public void WriteToText(string respondance, string text)
        {
            try
            {
                appBlob.AppendText(respondance + text + "\n");
            }
            catch
            {
                throw;
            }
        }

        public void uploadToRS()
        {
            string localfile = saveAsLocal();

            string user = ConfigurationManager.AppSettings["RSId"];
            string private_key = ConfigurationManager.AppSettings["RSPassword"];
            
            RSAPI rs = new RSAPI(user, private_key);
            string fileTitle = "ConversationLog_" + appBlob.Name;

            rs.UploadResource(localfile, fileTitle, ".log");
        }

        public string saveAsLocal() {

            // save blob to local
            string tempPath = Path.GetTempPath();
            //init the file path  
            string filePath = tempPath + fileName;

            //if the path is exists,delete old file  
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Save blob contents to a file.
            using (var fileStream = System.IO.File.OpenWrite(filePath))
            {
                appBlob.DownloadToStream(fileStream);
            }

            return filePath;

        }

    }
}