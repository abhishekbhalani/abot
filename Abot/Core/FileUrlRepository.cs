using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Abot.Core
{
    public class FileUrlRepository : ICrawledUrlRepository, IDisposable
    {

        volatile bool creatingDirectory = false;
        static readonly object directoryLocker = new object();
        MemoryBloomUrlRepository memoryURLRepositoryCache = null;
        ConcurrentQueue<Uri> memoryURLRepositoryForWriting = new ConcurrentQueue<Uri>();
        Thread memoryFlusher = null;
        int threadSleep = 5000;
        bool useBloom = true;

        public FileUrlRepository(int watcherDelayInMS = 5000, bool useBloomFilterCache = true, double falsePositiveProbability = .0001, int expectedElements = 100000000)
        {
            if (useBloomFilterCache)
            {
                useBloom = true;
                memoryURLRepositoryCache = new MemoryBloomUrlRepository(falsePositiveProbability, expectedElements);
            }
            if (Directory.Exists("crawledURLS"))
            {
                Directory.Delete("crawledURLS", true);
            }
            Directory.CreateDirectory("crawledURLS");
            memoryFlusher = new Thread(new ThreadStart(monitorDisk));
            memoryFlusher.IsBackground = true;
            memoryFlusher.Start();

        }
        ~FileUrlRepository()
        {
            Dispose();
        }
        public virtual void Dispose()
        {
            memoryFlusher.Abort();
            if (Directory.Exists("crawledURLS"))
            {
                Directory.Delete("crawledURLS", true);
            }
        }
        public bool Contains(Uri uri)
        {
            if (useBloom)
            {
                if (memoryURLRepositoryCache.Contains(uri))
                {
                    if (memoryURLRepositoryForWriting.Contains(uri))
                    {
                        return true;
                    }
                    return Contains(filePath(uri));
                }
            }
            else
            {
                if (memoryURLRepositoryForWriting.Contains(uri))
                {
                    return true;
                }
                return Contains(filePath(uri));
            }
            return false;
        }
        protected bool Contains(string path)
        {
            while (creatingDirectory == true)
            {
                Thread.Sleep(100);
            }
            return Directory.Exists(path);
        }
        public bool AddIfNew(Uri uri)
        {
            if (Contains(uri))
            {
                return false;
            }
            else
            {
                memoryURLRepositoryCache.AddIfNew(uri);
                memoryURLRepositoryForWriting.Enqueue(uri);
                return true;
            }
        }
        protected void monitorDisk()
        {

            while (1 == 1)
            {
                try
                {
                    Uri cUri = null;
                    while (memoryURLRepositoryForWriting.TryDequeue(out cUri))
                    {
                        AddIfNewDisk(cUri);
                    }
                }
                catch (Exception e)
                {

                }
                Thread.Sleep(threadSleep);
            }
        }



        protected bool AddIfNewDisk(Uri uri)
        {
            var directoryName = filePath(uri);
            if (Contains(directoryName))
            {
                return false;
            }
            else
            {
                try
                {
                    creatingDirectory = true;
                    Directory.CreateDirectory(directoryName);

                }
                catch (Exception e)
                {
                }
                finally
                {
                    creatingDirectory = false;
                }
                return true;
            }

        }
        protected string filePath(Uri uri)
        {
            MurMur3.Murmur3 mm3 = new MurMur3.Murmur3();
            var directoryName = ToHex(mm3.ComputeHash(ASCIIEncoding.ASCII.GetBytes(uri.AbsoluteUri)));

            return "crawledURLS\\" + uri.Authority + "\\" + directoryName.Substring(0, 4) + "\\" + directoryName;
        }

        /// <summary>
        /// Hex string lookup table.
        /// </summary>
        private static readonly string[] HexStringTable = new string[]
{
    "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "0A", "0B", "0C", "0D", "0E", "0F",
    "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "1A", "1B", "1C", "1D", "1E", "1F",
    "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "2A", "2B", "2C", "2D", "2E", "2F",
    "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "3A", "3B", "3C", "3D", "3E", "3F",
    "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "4A", "4B", "4C", "4D", "4E", "4F",
    "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "5A", "5B", "5C", "5D", "5E", "5F",
    "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "6A", "6B", "6C", "6D", "6E", "6F",
    "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "7A", "7B", "7C", "7D", "7E", "7F",
    "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "8A", "8B", "8C", "8D", "8E", "8F",
    "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "9A", "9B", "9C", "9D", "9E", "9F",
    "A0", "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A9", "AA", "AB", "AC", "AD", "AE", "AF",
    "B0", "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8", "B9", "BA", "BB", "BC", "BD", "BE", "BF",
    "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "CA", "CB", "CC", "CD", "CE", "CF",
    "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "DA", "DB", "DC", "DD", "DE", "DF",
    "E0", "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9", "EA", "EB", "EC", "ED", "EE", "EF",
    "F0", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "FA", "FB", "FC", "FD", "FE", "FF"
};

        /// <summary>
        /// Returns a hex string representation of an array of bytes.
        /// </summary>
        /// <param name="value">The array of bytes.</param>
        /// <returns>A hex string representation of the array of bytes.</returns>
        public static string ToHex(byte[] value)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (value != null)
            {
                foreach (byte b in value)
                {
                    stringBuilder.Append(HexStringTable[b]);
                }
            }

            return stringBuilder.ToString();
        }

    }
}
