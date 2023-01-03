using Lucene.Net.Documents;
using System.Reflection;

namespace Ads.LuceneIndexer.Interfaces
{
    //https://lucenenet.apache.org/docs/3.0.3/d2/d6b/_field_8cs_source.html
    public interface IField
    {
        bool IsMatched(PropertyInfo property);

        Field MapTo(PropertyInfo property, object val, string name);

        object? MapFrom(Field field);
    }
}
