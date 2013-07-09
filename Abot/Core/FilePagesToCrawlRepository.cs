using Abot.Poco;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
namespace Abot.Core
{
    public class FilePagesToCrawlRepository : IPagesToCrawlRepository, IDisposable
    {
        string filePath = "urlRepository";
        int totalFiles = 0;
        static readonly object filelocker = new object();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        MemoryPageToCrawlRepository pagesToCrawlMemoryRepositroy = new MemoryPageToCrawlRepository();
        MemoryPageToCrawlRepository pagesToCrawlMemoryRepositroyForWriting = new MemoryPageToCrawlRepository();
        int maxObjectsForMemory = 5000;
        Thread memoryLoader = null;
        Thread memoryFlusher = null;
        bool initialFilled = false;
        bool ensureFifo = true;
        int threadSleep = 5000;
        public FilePagesToCrawlRepository(int maxPagesForMemory = 5000, int watcherDelayInMS = 5000, bool Fifo = true)
        {
            ensureFifo = Fifo;
            maxObjectsForMemory = maxPagesForMemory;
            threadSleep = watcherDelayInMS;
            filePath += "\\" + Guid.NewGuid().ToString("N").Substring(0, 6) + "\\";
            Clear();
            memoryLoader = new Thread(new ThreadStart(monitorDisk));
            memoryLoader.IsBackground = true;
            memoryLoader.Start();
            memoryFlusher = new Thread(new ThreadStart(monitorMemory));
            memoryFlusher.IsBackground = true;
            memoryFlusher.Start();
        }
        ~FilePagesToCrawlRepository()
        {
            Dispose();
        }
        public virtual void Dispose()
        {
            memoryLoader.Abort();
            pagesToCrawlMemoryRepositroy = null;
            pagesToCrawlMemoryRepositroyForWriting = null;
            Clear();
        }
        public void Add(PageToCrawl page)
        {

            if (pagesToCrawlMemoryRepositroy.Count() < maxObjectsForMemory && initialFilled == false)
            {
                pagesToCrawlMemoryRepositroy.Add(page);
            }
            else
            {
                initialFilled = true;
                pagesToCrawlMemoryRepositroyForWriting.Add(page);
            }
        }
        protected void monitorMemory()
        {

            while (1 == 1)
            {
                try
                {
                    for (int x = 0; x < pagesToCrawlMemoryRepositroyForWriting.Count(); x++)
                    {
                        var page = pagesToCrawlMemoryRepositroyForWriting.GetNext();
                        if (page != null)
                        {
                            Interlocked.Increment(ref totalFiles);

                            var json = serializer.Serialize(page);
                            //Filenames use ticks to be able to be sorted and combine with guid at end to ensure uniqueness if written at same tick.
                            lock (filelocker)
                            {
                                using (StreamWriter file = new StreamWriter(filePath + DateTime.Now.Ticks + Guid.NewGuid().ToString("N").Substring(0, 12)))
                                {
                                    file.WriteLine(json);
                                    file.Close();
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {

                }
                Thread.Sleep(threadSleep);
            }
        }
        protected void monitorDisk()
        {

            while (1 == 1)
            {
                Console.WriteLine(pagesToCrawlMemoryRepositroy.Count() + " : " + totalFiles + " : " + pagesToCrawlMemoryRepositroyForWriting.Count());
                try
                {
                    int loopAmt = maxObjectsForMemory - pagesToCrawlMemoryRepositroy.Count();
                    if (ensureFifo)
                    {
                        if (totalFiles < loopAmt)
                        {
                            loopAmt = totalFiles;
                        }
                        FillMemoryFromDisk(loopAmt);
                    }
                    else
                    {
                        var memWriteAmt = pagesToCrawlMemoryRepositroyForWriting.Count();
                        if (memWriteAmt < loopAmt)
                        {
                            loopAmt = memWriteAmt;
                        }
                        for (int x = 0; x < loopAmt; x++)
                        {
                            var page = pagesToCrawlMemoryRepositroyForWriting.GetNext();
                            if (page != null)
                            {
                                pagesToCrawlMemoryRepositroy.Add(page);
                            }
                        }
                        loopAmt = maxObjectsForMemory - pagesToCrawlMemoryRepositroy.Count();
                        if (totalFiles < loopAmt)
                        {
                            loopAmt = totalFiles;
                        }
                        FillMemoryFromDisk(loopAmt);
                    }
                }
                catch (Exception e)
                {

                }
                Thread.Sleep(threadSleep);
            }
        }
        public PageToCrawl GetNext()
        {
            PageToCrawl rPage = null;
            if (maxObjectsForMemory > 0)
            {
                rPage = pagesToCrawlMemoryRepositroy.GetNext();
                if (rPage != null)
                {
                    return rPage;
                }
            }
            if (ensureFifo)
            {
                rPage = GetNextDisk();
                if (rPage == null)
                {
                    rPage = pagesToCrawlMemoryRepositroyForWriting.GetNext();
                }
            }
            else
            {
                rPage = pagesToCrawlMemoryRepositroyForWriting.GetNext();
                if (rPage == null)
                {
                    rPage = GetNextDisk();
                }
            }

            return rPage;

        }
        protected void FillMemoryFromDisk(int numberToFill)
        {
            lock (filelocker)
            {
                var fNames = (from f in Directory.EnumerateFiles(filePath, "*.*", SearchOption.TopDirectoryOnly) orderby f select f).ToList();

                if (fNames.Count() < numberToFill)
                {
                    numberToFill = fNames.Count();
                }
                for (int x = 0; x < numberToFill; x++)
                {
                    try
                    {
                        using (StreamReader file = new StreamReader(fNames[x]))
                        {
                            pagesToCrawlMemoryRepositroy.Add(serializer.Deserialize<PageToCrawl>(file.ReadToEnd()));
                        }
                        File.Delete(fNames[x]);
                        Interlocked.Decrement(ref totalFiles);
                    }
                    catch
                    {
                    }
                }
            }
        }
        protected PageToCrawl GetNextDisk()
        {
            PageToCrawl page = null;
            lock (filelocker)
            {
                string fName = (from f in Directory.EnumerateFiles(filePath, "*.*", SearchOption.TopDirectoryOnly) orderby f select f).FirstOrDefault();
                if (fName != null && fName != "")
                {
                    using (StreamReader file = new StreamReader(fName))
                    {
                        page = serializer.Deserialize<PageToCrawl>(file.ReadToEnd());
                    }
                    File.Delete(fName);
                    Interlocked.Decrement(ref totalFiles);
                }
            }
            return page;
        }

        public void Clear()
        {
            if (Directory.Exists(filePath))
            {
                Directory.Delete(filePath, true);
            }
            Directory.CreateDirectory(filePath);
        }

        public int Count()
        {
            return totalFiles + pagesToCrawlMemoryRepositroy.Count() + pagesToCrawlMemoryRepositroyForWriting.Count();
        }

        protected int FullCount()
        {
            return Directory.GetFiles(filePath, "*.*", SearchOption.TopDirectoryOnly).Length;
        }
    }
}
