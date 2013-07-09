using System;
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
        volatile bool creatingFile = false;
        static readonly object directoryLocker = new object();
        public FileUrlRepository()
        {
            if (Directory.Exists("crawledURLS"))
            {
                Directory.Delete("crawledURLS", true);
            }
            Directory.CreateDirectory("crawledURLS");
        }
        ~FileUrlRepository()
        {
            Dispose();
        }
        public virtual void Dispose()
        {
            if (Directory.Exists("crawledURLS"))
            {
                Directory.Delete("crawledURLS", true);
            }
        }
        public bool Contains(Uri uri)
        {
            return Contains(filePath(uri));
        }
        protected bool Contains(string path)
        {
            while (creatingFile == true)
            {
                Thread.Sleep(100);
            }
            return File.Exists(path);
        }
        public bool AddIfNew(Uri uri)
        {
            var fileName = filePath(uri);
            if (Contains(fileName))
            {
                return false;
            }
            else
            {
                try
                {
                    creatingFile = true;
                    File.Create(fileName);

                }
                catch (Exception e)
                {
                }
                finally
                {
                    creatingFile = false;
                }
                return true;
            }

        }
        protected string filePath(Uri uri)
        {
            var fileName = BitConverter.ToString(md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(uri.AbsoluteUri))).Replace("-", string.Empty);

            Directory.CreateDirectory("crawledURLS\\" + uri.Authority + "\\" + fileName.Substring(0, 4) + "\\");
            return "crawledURLS\\" + uri.Authority + "\\" + fileName.Substring(0, 4) + "\\" + fileName;
        }

    }
}
