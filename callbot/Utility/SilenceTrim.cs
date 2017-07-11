using System;
using System.Collections.Generic;
using System.Linq;

namespace callbot.utility
{
    /// <summary>
    /// The class is inspired by http://www.moscowcoffeereview.com/programming/programatically-trimming-intro-material-from-a-video-by-detecting-amplitude-changes-in-the-audio/.
    /// </summary>
    class SilenceTrim
    {
        private static int sampleRate;
        private static int channels;
        private static int bytesPerSingleChannelSample;

        /// <summary>
        /// Trim the silence at the end of the wav audio file
        /// </summary>
        /// <param name="inFilePath"></param>
        /// <param name="outPath"></param>
        public void TrimSilenceEnd(string inFilePath, string outFilePath)
        {
            clsWaveProcessor wain = new clsWaveProcessor();
            clsWaveProcessor waout = new clsWaveProcessor();

            byte[] arrfile = wain.GetWAVEData(inFilePath);
            wain.WaveHeaderIN(inFilePath);
            sampleRate = wain.SampleRate / 10;
            bytesPerSingleChannelSample = wain.BitsPerSample / 8;
            channels = wain.Channels;

            List<List<int>> resultList = MedianAmplitude(inFilePath, arrfile);
            List<int> ampList = resultList[0];
            List<int> idxList = resultList[1];
            List<int> resultList1 = StartEnd(ampList);

            int endpos = idxList[resultList1[1]];
            int startpos = idxList[resultList1[0]];
            byte[] newarr = new byte[(endpos - startpos) + 1];

            for (int ni = 0, m = startpos; m <= endpos; m++, ni++)
                newarr[ni] = arrfile[m];

            //write file
            waout.Length = wain.Length;
            waout.BitsPerSample = wain.BitsPerSample;
            waout.Channels = wain.Channels;
            waout.SampleRate = wain.SampleRate;
            waout.DataLength = newarr.Length;
            waout.WaveHeaderOUT(@outFilePath);
            waout.WriteWAVEData(outFilePath, ref newarr);
        }

        /// <summary>
        /// Detect the amplitude median of the sample of the wav file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<List<int>> MedianAmplitude(string filePath, byte[] data)
        {
            int head = 44; // The first 44 bytes have header info
            int sampleCount = 1;
            List<int> sampleBuffer = new List<int>();
            List<int> amplitudeMedian = new List<int>();
            List<int> byteIndex = new List<int>();
            List<List<int>> result = new List<List<int>>();

            while (head < (data.Length - 1) && head < 5000000) // Stop after reading 5 Megs of data - that is plenty
            {
                int sampleAmplitude = BitConverter.ToInt16(data, head);
                sampleAmplitude = Math.Abs(sampleAmplitude);
                sampleBuffer.Add(sampleAmplitude);
                // After enough samples are collected in the buffer, print out their average amplitude and then clear the buffer
                if (sampleBuffer.Count >= (sampleRate))
                {
                    byteIndex.Add(head + 44);
                    amplitudeMedian.Add(GetMedian(sampleBuffer));
                    sampleBuffer.Clear();
                }

                sampleCount++;
                // Advance the reading head to the next sample, skipping the second channel if it exists.
                // We only need to check the left channel of the stereo to simplify things
                head = head + (bytesPerSingleChannelSample * channels);
            }
            result.Add(amplitudeMedian);
            result.Add(byteIndex);
            return result;
        }

        private static int GetMedian(List<int> ints)
        {
            int[] temp = ints.ToArray();
            Array.Sort(temp);
            int middleIndex = Convert.ToInt32(Math.Floor((float)(temp.Length / 2)));
            return temp[middleIndex];
        }

        private static float SamplesToSeconds(int samples)
        {
            return samples / (sampleRate);
        }

        /// <summary>
        /// Detect the start and end position of the non-silence part of the wav file.
        /// </summary>
        /// <param name="inList">List of amplitude median of the sample of the wav file</param>
        /// <returns></returns>
        public List<int> StartEnd(List<int> inList)
        {
            List<int> result = new List<int>(new int[] {0, inList.Count-1});
            int product = 2;
            for (int idx = 0; idx < inList.Count - 4; idx++)
            {
                if (inList[idx + 1] > inList[idx] * product && inList[idx + 2] > inList[idx] * product && inList[idx + 3] > inList[idx] * product)
                {
                    result[0] = idx;
                    break;
                }
            }

            for (int idx = result[0] + 1; idx < inList.Count - 11; idx++)
            {
                int peakAvg = Convert.ToInt32(inList.Skip(result[0]).Take(idx - result[0]).Average());
                if (inList[idx+1] * product < peakAvg && inList[idx+2] * product < peakAvg && inList[idx+3] * product < peakAvg && inList[idx+4] * product < peakAvg && inList[idx+5] * product < peakAvg &&
                    inList[idx+6] * product < peakAvg && inList[idx+7] * product < peakAvg && inList[idx+8] * product < peakAvg && inList[idx+9] * product < peakAvg && inList[idx+10] * product < peakAvg)
                {
                    result[1] = idx;
                    break;
                }
            }

            // no trim for the beginning
            result[0] = 0;
            // extend the end of the recording for 30 ms
            if (result[1] < inList.Count - 6) { result[1] += 5; }
            else { result[1] = inList.Count - 1; };
            return result;
        }
    }
}
