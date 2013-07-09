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
        volatile bool creatingDirectory = false;
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
            while (creatingDirectory == true)
            {
                Thread.Sleep(100);
            }
            return Directory.Exists(path);
        }
        public bool AddIfNew(Uri uri)
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
