using BloomFilterLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abot.Core
{

    public class MemoryBloomUrlRepository : ICrawledUrlRepository, IDisposable
    {
        BloomFilter _urlRepository = null;
        //ConcurrentDictionary<string, object> _urlRepository = new ConcurrentDictionary<string, object>();

        public MemoryBloomUrlRepository(double falsePositiveProbability = .0001, int expectedElements = 100000000)
        {
            _urlRepository = new BloomFilter(falsePositiveProbability, expectedElements); 
        }

        ~MemoryBloomUrlRepository()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            _urlRepository = null;
        }
        public bool Contains(Uri uri)
        {
            return _urlRepository.contains(uri.AbsoluteUri);
        }
        public bool AddIfNew(Uri uri)
        {
            if (!Contains(uri))
            {
                _urlRepository.add(uri.AbsoluteUri);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
