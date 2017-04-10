using Microsoft.IdentityModel.Protocols;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace callbot
{ 
    public class audioMan
    {

        private CloudAppendBlob appBlob;
        public string fileName { get; set; }
        public string azureUrl { get; set; }

        private string output;

        public audioMan(List<string> audioPaths) {

            ConcatenateAudio(audioPaths);
            azureFunc(output);
        }


        private void azureFunc(string localPath)
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
            fileName = "test.wav";
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
            }
            catch {
                Console.WriteLine("GG");
            }

        }

        private void ConcatenateAudio(IEnumerable<string> sourceFiles)
        {
            byte[] buffer = new byte[1024];
            WaveFileWriter waveFileWriter = null;

            //get temp directory path  
            string tempPath = Path.GetTempPath();
            output = $"{tempPath}b.wav";

            try
            {
                foreach (string sourceFile in sourceFiles)
                {
                    string realPath = $"{tempPath}a.wav";
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(sourceFile, realPath);
                    }

                    using (WaveFileReader reader = new WaveFileReader(realPath))
                    {
                        if (waveFileWriter == null)
                        {
                            // first time in create new Writer
                            waveFileWriter = new WaveFileWriter(output, reader.WaveFormat);
                        }
                        else
                        {
                            if (!reader.WaveFormat.Equals(waveFileWriter.WaveFormat))
                            {
                                throw new InvalidOperationException("Can't concatenate WAV Files that don't share the same format");
                            }
                        }

                        int read;
                        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            waveFileWriter.Write(buffer, 0, read);
                        }
                    }
                    //remove file from temp storage after use
                    File.Delete(realPath);

                }
            }
            catch
            {
                Console.WriteLine("ERROR LOH");

            }
            finally
            {
                if (waveFileWriter != null)
                {
                    waveFileWriter.Dispose();
                }
            }
            Console.WriteLine("done");
            
        }
     

    }
}