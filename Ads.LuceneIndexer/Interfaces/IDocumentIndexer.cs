using Lucene.Net.Documents;

namespace Ads.LuceneIndexer.Interfaces
{
    public interface IDocumentIndexer
    {
        T MapFrom<T>(Document source);
        object MapFrom(Document source, Type contentType);
        Document MapToDocument(object source);
    }
}
