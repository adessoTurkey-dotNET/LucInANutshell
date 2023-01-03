using System.Reflection;
using Ads.LuceneIndexer.Extensions;
using Ads.LuceneIndexer.Interfaces;
using Lucene.Net.Documents;

namespace Ads.LuceneIndexer.FieldConverters
{
    // A field that is indexed and tokenized, without term vectors.
    // see https://lucenenet.apache.org/docs/3.0.3/d2/d6b/_field_8cs_source.html
    public class TextFieldConverter : AField, IField
    {
        public bool IsMatched(PropertyInfo property)
        {
            var type = GetPropertyType(property);
            var searchAttribute = property.GetCustomAttribute<SearchAttribute>();
            return searchAttribute != null && searchAttribute.IsTextField && type == typeof(string);
        }

        public Field MapTo(PropertyInfo property, object val, string name)
        {
            return new TextField(name, val.ToString(), GetStore(property));
        }

        public object? MapFrom(Field field)
        {
            return field.GetStringValue();
        }
    }
}