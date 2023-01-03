using Ads.LuceneIndexer.Interfaces;
using Ads.LuceneIndexer.Models;
using Lucene.Net.Search;

namespace Ads.LuceneIndexer.FilterBuilder
{
    public class FilterBuilder
    {
        private readonly IIndexProvider _indexProvider;

        private readonly List<NexusFilter> _filters = new List<NexusFilter>();
        private readonly List<NexusSort> _sorts = new List<NexusSort>();

        public FilterBuilder(IIndexProvider indexProvider)
        {
            _indexProvider = indexProvider;
        }

        public FilterBuilder Must(Func<Query> getQueryAction)
        {
            _filters.Add(new NexusFilter
            {
                OccurType = Occur.MUST,
                Query = getQueryAction()
            });

            return this;
        }

        public FilterBuilder Should(Func<Query> getQueryAction)
        {
            _filters.Add(new NexusFilter
            {
                OccurType = Occur.SHOULD,
                Query = getQueryAction()
            });

            return this;
        }

        public FilterBuilder MustNot(Func<Query> getQueryAction)
        {
            _filters.Add(new NexusFilter
            {
                OccurType = Occur.MUST_NOT,
                Query = getQueryAction()
            });

            return this;
        }

        public FilterBuilder Sort(Func<SortField> getSortFunc)
        {
            _sorts.Add(new NexusSort()
            {
                SortField = getSortFunc()
            });

            return this;
        }

        public async Task<ListResult<T>> ListResult<T>()
        {
            return await _indexProvider.GetByFilters<T>(_filters, _sorts);
        }

        public async Task<Result<T>> SingleResult<T>()
        {
            var results = await _indexProvider.GetByFilters<T>(_filters, _sorts);
            return results.Hits.FirstOrDefault();
        }

        public async Task<Result<object>> SingleResult(Type contentType)
        {
            var results = await _indexProvider.GetByFilters(_filters, _sorts, contentType);
            return results.Hits.FirstOrDefault();
        }

        public async Task<ListResult> ListResult(Type contentType)
        {
            return await _indexProvider.GetByFilters(_filters, _sorts, contentType);
        }

        public async Task<bool> Any(Type contentType)
        {
            var result = await _indexProvider.GetByFilters(_filters, _sorts, contentType);
            return result != null && result.Hits.Any();
        }

        public async Task<bool> Any<T>()
        {
            var result = await _indexProvider.GetByFilters(_filters, _sorts, typeof(T));
            return result != null && result.Hits.Any();
        }
    }
}