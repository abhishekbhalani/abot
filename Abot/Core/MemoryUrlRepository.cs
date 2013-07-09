using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abot.Core
{
   
    public class MemoryUrlRepository : ICrawledUrlRepository, IDisposable
    {
        ConcurrentDictionary<string, object> _urlRepository = new ConcurrentDictionary<string, object>();

        ~MemoryUrlRepository()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            _urlRepository = null;
        }
        public bool Contains(Uri uri)
        {
            return _urlRepository.ContainsKey(uri.AbsoluteUri);
        }
        public bool AddIfNew(Uri uri)
        {
           return _urlRepository.TryAdd(uri.AbsoluteUri, null);
        }
    }
}
