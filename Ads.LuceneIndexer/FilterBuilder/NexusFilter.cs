using Lucene.Net.Search;

namespace Ads.LuceneIndexer.FilterBuilder
{
    public class NexusFilter
    {
        public Query? Query { get; set; }
        public Occur OccurType { get; set; }
    }
}