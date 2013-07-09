using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Abot.Core
{
    public class FileUrlRepository : ICrawledUrlRepository, IDisposable
    {
        MD5 md5 = MD5.Create();
        volatile bool creatingDirectory = false;
        static readonly object directoryLocker = new object();
        ConcurrentQueue<Uri> memoryURLRepositoryForWriting = new ConcurrentQueue<Uri>();
        Thread memoryFlusher = null;
        int threadSleep = 5000;

        public FileUrlRepository(int watcherDelayInMS = 5000)
        {
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
            if (memoryURLRepositoryForWriting.Contains(uri))
            {
                return true;
            }
            return Contains(filePath(uri));
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
            var directoryName = BitConverter.ToString(md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(uri.AbsoluteUri))).Replace("-", string.Empty);

            return "crawledURLS\\" + uri.Authority + "\\" + directoryName.Substring(0, 4) + "\\" + directoryName;
        }

    }
}
