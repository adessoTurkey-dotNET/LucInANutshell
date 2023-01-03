using System.Reflection;
using Ads.LuceneIndexer.Interfaces;
using Lucene.Net.Documents;

namespace Ads.LuceneIndexer.FieldConverters
{
    public class DoubleFieldConverter : AField, IField
    {
        public bool IsMatched(PropertyInfo property)
        {
            return GetPropertyType(property) == typeof(double);
        }

        public Field MapTo(PropertyInfo property, object val, string name)
        {
            return new DoubleField(name, (double)val, GetStore(property));
        }

        public object? MapFrom(Field field)
        {
            return field.GetDoubleValue();
        }
    }
}