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

using System.Speech.AudioFormat;
using NAudio.Lame;
using NAudio.Wave;
using System.Diagnostics;

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

        public void callCombine(List<string> audioPaths) {
            string tempPath = Path.GetTempPath();

            output = $"{tempPath}output.mp3";

            // check if file is already there, if yes, delete
            if (File.Exists(output))
            {
                File.Delete(output);
            }

            // for wav
            //ConcatenateAudio(audioPaths, output);

            //for mp3
            ConcatenateAudio_mp3(audioPaths, output);

            azureFunc(output);

        }

        public static void CheckAddBinPath()
        {
            // find path to 'bin' folder
            var binPath = Path.Combine(new string[] { AppDomain.CurrentDomain.BaseDirectory, "bin" });
            // get current search path from environment
            var path = Environment.GetEnvironmentVariable("PATH") ?? "";

            // add 'bin' folder to search path if not already present
            if (!path.Split(Path.PathSeparator).Contains(binPath, StringComparer.CurrentCultureIgnoreCase))
            {
                path = string.Join(Path.PathSeparator.ToString(), new string[] { path, binPath });
                Environment.SetEnvironmentVariable("PATH", path);
            }
        }

        public void ConvertWavStreamToMp3File( ref MemoryStream ms, string savetofilename)
        {
            try
            {
                CheckAddBinPath();
                //rewind to beginning of stream
                ms.Seek(0, SeekOrigin.Begin);

                using (var retMs = new MemoryStream())
                using (var rdr = new WaveFileReader(ms))
                using (var wtr = new LameMP3FileWriter(savetofilename, rdr.WaveFormat, LAMEPreset.VBR_90))
                {
                    rdr.CopyTo(wtr);
                }
            }
            catch
            {
                Debug.WriteLine("GG");

            }
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

        private void ConcatenateAudio_wav(IEnumerable<string> sourceFiles, string outputPath)
        {
            byte[] buffer = new byte[1024];
            WaveFileWriter waveFileWriter = null;

            //get temp directory path  
            string tempPath = Path.GetTempPath();

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
                            waveFileWriter = new WaveFileWriter(outputPath, reader.WaveFormat);
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

        private void ConcatenateAudio_mp3(List<string> inputFiles, string outputPath)
        {
            System.IO.Stream output = new System.IO.MemoryStream();
            
            //get temp directory path  
            string tempPath = Path.GetTempPath();
            string realPath = "";

            foreach (string file in inputFiles)
            {
                if (file.Contains("http"))
                {
                    realPath = $"{tempPath}temp.mp3";
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(file, realPath);
                    }
                }
                else {
                    realPath = file;
                }
                

                Mp3FileReader reader = new Mp3FileReader(realPath);
                if ((output.Position == 0) && (reader.Id3v2Tag != null))
                {
                    output.Write(reader.Id3v2Tag.RawData, 0, reader.Id3v2Tag.RawData.Length);
                }
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                {
                    output.Write(frame.RawData, 0, frame.RawData.Length);
                }

                reader.Close();

                File.Delete(realPath);
            }
            
            using (var fileStream = new FileStream(outputPath, FileMode.CreateNew, FileAccess.ReadWrite))
            {
                output.Position = 0;
                output.CopyTo(fileStream); // fileStream is not populated
            }

        }
        


    }
}