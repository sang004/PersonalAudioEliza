using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Configuration;
using System.IO;

namespace callbot
{ 
    public class audioMan
    {

        private CloudAppendBlob appBlob;
        public string fileName { get; set; }
        public string azureUrl { get; set; }

        private string output { get; set; }

        public audioMan() {
            
        }
        
        public void ConvertWavStreamToWav(ref MemoryStream ms, string savetofilename)
        {
            FileStream file = new FileStream(savetofilename, FileMode.Create, FileAccess.Write);
            ms.WriteTo(file);
            file.Close();
            ms.Close();

            //azureFunc(savetofilename);
        }

        public void deleteBlob( string fileName ) {
            // Let's set up our connection for the account and store the name and key in app.config.           
            string accName = ConfigurationManager.AppSettings["AzureStoreId"];
            string accKey = ConfigurationManager.AppSettings["AzureStorePassword"];

            // Implement the accout, set true for https for SSL.
            StorageCredentials creds = new StorageCredentials(accName, accKey);
            CloudStorageAccount strAcc = new CloudStorageAccount(creds, true);
            CloudBlobClient blobClient = strAcc.CreateCloudBlobClient();

            //Setup our container we are going to use and create it.
            CloudBlobContainer container = blobClient.GetContainerReference("logs");
            // Retrieve reference to a blob named "myblob.txt".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
        }

        public string azureFunc(string localPath)
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

            // Build file name
            // This creates a reference to the append blob we are going to use.
        
            // use GUID to generate a unique file name every time
            fileName = string.Format(@"{0}.wav", Guid.NewGuid()); 
            appBlob = container.GetAppendBlobReference(fileName);

            // Now we are going to check if todays file exists and if it doesn't we create it.
            if (!appBlob.Exists())
            {
                appBlob.CreateOrReplace();
            }

            // Save blob contents to a file.
            try
            {
                appBlob.UploadFromFile(localPath);
                Console.WriteLine(appBlob.Uri.AbsoluteUri);
                azureUrl = appBlob.Uri.AbsoluteUri;
                return azureUrl;
            }
            catch {
                Console.WriteLine("======Failed to save on Azure");
                return "";
            }

        }

    }
}