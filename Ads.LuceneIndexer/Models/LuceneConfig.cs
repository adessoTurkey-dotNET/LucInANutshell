using Lucene.Net.Util;

namespace Ads.LuceneIndexer.Models
{
    public class LuceneConfig
    {
        public LuceneVersion Version { get; set; }

        public int BatchSize { get; set; }
    }
}