using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SP2.Cache
{
    public class WorkbookCache
    {
        private readonly ConcurrentDictionary<string, CacheElement> cache;
        private readonly int maxSize;
        private readonly TimeSpan ttl;
        private readonly Timer timer;
        private readonly Random rand;

        public WorkbookCache(int max, TimeSpan time)
        {
            cache = new ConcurrentDictionary<string, CacheElement>();
            maxSize = max;
            ttl = time;
            timer = new(_ => ClearOldValues(), null, 0, (int)TimeSpan.FromHours(1).TotalMilliseconds);
            rand = new();
        }

        public void AddOrUse(IWorkbook wb)
        {
            if (cache.Count >= maxSize)
            {
                cache.Remove(cache.Keys.ElementAt(rand.Next(cache.Count)), out _);
            }

            cache.AddOrUpdate(wb.GetSheetName(0), new CacheElement(wb),
                (string key, CacheElement el) => { el.LastUsed = DateTime.Now; return el; });
        }

        public IWorkbook? TryGet(string name)
        {
            cache.TryGetValue(name, out CacheElement? el);

            return el?.Workbook;
        }

        private void ClearOldValues()
        {
            DateTime cutoff = DateTime.Now - ttl;

            foreach (var kvp in cache)
            {
                if (kvp.Value.LastUsed < cutoff)
                {
                    cache.Remove(kvp.Key, out _);
                }
            }
        }
    }
}
