using Ads.LuceneIndexer.Models;
using System.Collections.Generic;

namespace Ads.LuceneIndexer.FilterBuilder
{
    public class ListResult<T>
    {
        public IList<Result<T>> Hits { get; set; }
        public int Count { get; set; }
        public float MaxScore { get; set; }
    }
    
    public class ListResult
    {
        public IList<Result<object>> Hits { get; set; }
        public int Count { get; set; }
        public float MaxScore { get; set; }
    }
}