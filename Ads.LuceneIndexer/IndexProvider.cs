using Ads.LuceneIndexer.Extensions;
using Ads.LuceneIndexer.FilterBuilder;
using Ads.LuceneIndexer.Interfaces;
using Ads.LuceneIndexer.Models;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Ads.LuceneIndexer
{
    public class IndexProvider : IIndexProvider
    {
        private readonly LuceneConfig _luceneConfig;
        private readonly IDocumentIndexer _indexer;
        private readonly ILogger<IndexProvider> _logger;
        private readonly string _path;

        public IndexProvider(
            LuceneConfig config,
            IDocumentIndexer indexer,
            ILoggerFactory loggerFactory
            //APathProvider path
            )
        {
            this._luceneConfig = config;
            this._indexer = indexer;
            this._logger = loggerFactory.CreateLogger<IndexProvider>();
            this._path = Path.GetFullPath(Path.Combine($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}", @"..\..\..\settings"));

            CreatePathIfNotExists(_path);
        }

        private void CreatePathIfNotExists(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            var directory = new DirectoryInfo(path);
            if (!directory.Exists)
            {
                directory.Create();
            }
        }

        private Lucene.Net.Store.Directory GetDirectory(string indexName)
        {
            var directoryInfo = new DirectoryInfo(Path.Combine(_path, indexName));
            return FSDirectory.Open(directoryInfo);
        }


        public Task Delete(Type contentType, string documentId)
        {
            return Task.Run(() =>
            {
                using (var analyzer = new StandardAnalyzer(_luceneConfig.Version))
                {
                    var config = new IndexWriterConfig(_luceneConfig.Version, analyzer);
                    using (var writer = new IndexWriter(GetDirectory(contentType.Name), config))
                    {
                        writer.DeleteDocuments(new Term(GetKeyName(contentType), documentId));
                        if (writer.HasDeletions())
                        {
                            writer.ForceMergeDeletes();
                        }
                        writer.Dispose();
                    }
                }
            });
        }


        public Task DeleteIndex(string indexName)
        {
            return Task.Run(() =>
            {
                var directory = new DirectoryInfo(Path.Combine(_path, indexName));
                if (!directory.Exists) return;

                directory.Delete(true);
            });
        }

        public async Task<ListResult<T>> GetByFilters<T>(IList<NexusFilter> filters, IList<NexusSort> sorts)
        {
            var contentType = typeof(T);
            var listResult = await GetByFilters(filters, sorts, contentType);

            IList<Result<T>> indexResults = new List<Result<T>>();
            foreach (var listResultItem in listResult.Hits)
            {
                indexResults.Add(new Result<T>()
                {
                    Hit = (T)listResultItem.Hit,
                    Score = listResultItem.Score
                });
            }

            return new ListResult<T>
            {
                Count = listResult.Count,
                Hits = indexResults,
                MaxScore = listResult.MaxScore
            };
        }

        public async Task<ListResult> GetByFilters(IList<NexusFilter> filters, IList<NexusSort> sorts, Type contentType)
        {
            var directory = GetDirectory(contentType.Name);
            List<Result<object>> indexResults = new List<Result<object>>();

            return await Task.Run(() =>
            {
                int count;
                float maxScore;
                using (var indexReader = DirectoryReader.Open(directory))
                {
                    var indexSearcher = new IndexSearcher(indexReader);
                    Query query = new MatchAllDocsQuery();

                    if (filters.Any())
                    {
                        query = new BooleanQuery();
                        foreach (var filter in filters)
                        {
                            ((BooleanQuery)query).Add(filter.Query, filter.OccurType);
                        }
                    }

                    var sort = new Lucene.Net.Search.Sort();
                    if (sorts.Any())
                    {
                        sort.SetSort(sorts.Select(x => x.SortField).ToArray());
                    }
                    else
                    {
                        sort.SetSort(SortField.FIELD_SCORE);
                    }

                    var hits = indexSearcher.Search(query, _luceneConfig.BatchSize, sort);
                    count = hits.TotalHits;
                    maxScore = hits.MaxScore;


                    for (int i = 0; i < hits.TotalHits; i++)
                    {
                        Document doc = indexSearcher.Doc(hits.ScoreDocs[i].Doc);
                        var contentItem = _indexer.MapFrom(doc, contentType);
                        indexResults.Add(new Result<object>
                        {
                            Hit = contentItem,
                            Score = hits.ScoreDocs[i].Score
                        });
                    }

                    indexReader.Dispose();
                }

                return new ListResult
                {
                    Hits = indexResults,
                    Count = count,
                    MaxScore = maxScore
                };
            });
        }

        public Result<object> GetDocumentById(Type contentType, string id)
        {
            var directory = GetDirectory(contentType.Name);

            using (var indexReader = DirectoryReader.Open(directory))
            {
                var indexSearcher = new IndexSearcher(indexReader);

                var query = new TermQuery(new Term(GetKeyName(contentType), id));
                var hits = indexSearcher.Search(query, _luceneConfig.BatchSize);

                var doc = indexSearcher.Doc(hits.ScoreDocs[0].Doc);
                var document = _indexer.MapFrom(doc, contentType);
                var result = new Result<object>
                {
                    Hit = document,
                    Score = hits.ScoreDocs[0].Score
                };

                indexReader.Dispose();
                return result;
            }
        }

        public Result<T> GetDocumentById<T>(string id)
        {
            var indexResult = GetDocumentById(typeof(T), id);
            return new Result<T>
            {
                Hit = (T)indexResult.Hit,
                Score = indexResult.Score
            };
        }

        public FilterBuilder.FilterBuilder Search()
        {
            return new FilterBuilder.FilterBuilder(this);
        }


        public Task Store(IList<object> contentItems, string indexName)
        {
            if (!contentItems.Any())
            {
                return Task.FromResult(0);
            }

            return Task.Run(() =>
            {
                using (var analyzer = new StandardAnalyzer(_luceneConfig.Version))
                {
                    var config = new IndexWriterConfig(_luceneConfig.Version, analyzer);
                    using (var writer = new IndexWriter(GetDirectory(indexName), config))
                    {
                        foreach (var contentItem in contentItems)
                        {
                            try
                            {
                                var doc = _indexer.MapToDocument(contentItem);
                                writer.AddDocument(doc);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, $"Could not add document to index");
                            }
                        }

                        writer.Dispose();
                    }
                }
            });
        }


        public Task<bool> Update(object contentItem, string id)
        {
            string indexName = contentItem.GetType().Name;
            return Task.Run(() =>
            {
                try
                {
                    using (var analyzer = new StandardAnalyzer(_luceneConfig.Version))
                    {
                        var config = new IndexWriterConfig(_luceneConfig.Version, analyzer);
                        using (var writer = new IndexWriter(GetDirectory(indexName), config))
                        {
                            var doc = _indexer.MapToDocument(contentItem);
                            writer.UpdateDocument(new Term(GetKeyName(contentItem.GetType()), id), doc);

                            if (writer.HasDeletions())
                            {
                                writer.ForceMergeDeletes();
                            }

                            writer.Dispose();
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not update content item {0}", id);
                    return false;
                }
            });
        }


        private string GetKeyName(Type contentType)
        {
            var keyProperty =
                contentType.GetProperties()
                    .FirstOrDefault(x =>
                        x.GetCustomAttribute<SearchAttribute>() != null &&
                        x.GetCustomAttribute<SearchAttribute>().IsKey
                    );

            return keyProperty == null ? "Id" : keyProperty.Name;
        }

        public Task Delete<T>(string documentId)
        {
            return Delete(typeof(T), documentId);
        }
    }
}
