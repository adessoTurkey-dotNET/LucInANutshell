using Ads.LuceneIndexer.Extensions;
using System.Reflection;
using LuceneDocument = Lucene.Net.Documents;
namespace Ads.LuceneIndexer.Interfaces
{
    public abstract class AField
    {
        protected Type GetPropertyType(PropertyInfo propertyInfo)
        {
            if (propertyInfo.IsPropertyCollection())
            {
                return propertyInfo.GetPropertyType()
                    .GetGenericArguments()
                    .Single();
            }

            return propertyInfo.GetPropertyType();
        }

        protected LuceneDocument.Field.Store GetStore(PropertyInfo propertyInfo)
        {
            var searchAttribute = propertyInfo.GetCustomAttribute<SearchAttribute>();
            if (searchAttribute == null)
            {
                return LuceneDocument.Field.Store.YES;
            }

            return searchAttribute.Store ? LuceneDocument.Field.Store.YES : LuceneDocument.Field.Store.NO;
        }
    }
}
