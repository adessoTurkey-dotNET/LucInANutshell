using Ads.LuceneIndexer.FilterBuilder;
using Ads.LuceneIndexer.Models;

namespace Ads.LuceneIndexer.Interfaces
{
    /// <summary>
    /// Methods that can be Performed 
    /// </summary>
    public interface IIndexProvider

    {  /// <summary>
       /// Deletes index with given path name
       /// </summary>
        Task DeleteIndex(string indexName);

        /// <summary>
        /// Stores given objects with provided path.
        /// </summary>
        Task Store(IList<object> contentItems, string indexName);

        /// <summary>
        /// Deletes the document with given id and content type
        /// </summary>
        Task Delete(Type contentType, string documentId);


        /// <summary>
        /// Deletes the document with given id and content type
        /// </summary>
        Task Delete<T>(string documentId);


        /// <summary>
        /// Updates (Removes and reinserts the document with given id.)
        /// </summary>
        Task<bool> Update(object contentItem, string id);


        /// <summary>
        /// Gets document by its id, the id is either the property defined as IsKey, if that is not provided, The property named Id will be the id.
        /// </summary>
        Result<object> GetDocumentById(Type contentType, string id);

        /// <summary>
        /// Gets document by its id, the id is either the property defined as IsKey, if that is not provided, The property named Id will be the id.
        /// </summary>
        Result<T> GetDocumentById<T>(string id);

        /// <summary>
        /// Searches for the results with given additional filtering parameters.
        /// </summary>
        FilterBuilder.FilterBuilder Search();


        Task<ListResult<T>> GetByFilters<T>(IList<NexusFilter> filters, IList<NexusSort> sorts);
        Task<ListResult> GetByFilters(IList<NexusFilter> filters, IList<NexusSort> sorts, Type contentType);
    }
}
