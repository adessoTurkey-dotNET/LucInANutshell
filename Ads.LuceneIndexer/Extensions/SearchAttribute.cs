namespace Ads.LuceneIndexer.Extensions
{
    //By default store : true token : false 
    public class SearchAttribute : Attribute
    {
        public bool IsTextField { get; set; }

        public bool Store { get; set; }

        public bool IsKey { get; set; }
    }
}
