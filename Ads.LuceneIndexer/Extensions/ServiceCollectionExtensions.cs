using Ads.LuceneIndexer.FieldConverters;
using Ads.LuceneIndexer.Interfaces;
using Ads.LuceneIndexer.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Ads.LuceneIndexer.Extensions
{

    /// <summary>
    /// Static methods to add IndexProvider and DocumentIndexer
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIndexProvider(this IServiceCollection services)
        {

            var luceneConfig = new LuceneConfig()
            {
                Version = Lucene.Net.Util.LuceneVersion.LUCENE_48,
                //size to fetch default is 1024 https://www.elastic.co/guide/en/elasticsearch/reference/7.17/search-settings.html
                BatchSize = 1024
            };

            services.AddSingleton(luceneConfig);
            services.AddScoped<IIndexProvider, IndexProvider>();

            return services;
        }

        public static IServiceCollection AddDocumentIndexer(this IServiceCollection services)
        {

            services.AddScoped<IDocumentIndexer, DocumentIndexer>();

            foreach (var type in typeof(StringFieldConverter).Assembly.DefinedTypes)
            {
                if (type.ImplementedInterfaces.Contains(typeof(IField)))
                {
                    services.AddScoped(typeof(IField), type);
                }
            }

            return services;
        }
    }
}
