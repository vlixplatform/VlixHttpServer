using System.Collections.Concurrent;

namespace  Vlix.HttpServer
{
    public class CacheFiles : ConcurrentDictionary<string, HTTPCache>
    {
        public double TotalCacheInKB = 0;
        public new bool TryAdd(string key, HTTPCache hTTPCache)
        {
            if (base.TryAdd(key, hTTPCache))
            {
                TotalCacheInKB += hTTPCache.ContentLengthInKB;
                return true;
            }
            return false;
        }

        public new bool TryRemove(string key, out HTTPCache hTTPCache)
        {
            if (base.TryGetValue(key, out hTTPCache))
            {
                if (base.TryRemove(key, out _))
                {
                    TotalCacheInKB -= hTTPCache.ContentLengthInKB;
                    return true;
                }
            }
            hTTPCache = null;
            return false;
        }
    }
}
