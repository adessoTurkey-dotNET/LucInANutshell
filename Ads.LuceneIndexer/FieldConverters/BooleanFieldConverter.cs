using Ads.LuceneIndexer.Interfaces;
using Lucene.Net.Documents;
using System.Reflection;

namespace Ads.LuceneIndexer.FieldConverters
{
    public class BooleanFieldConverter : AField, IField
    {
        public bool IsMatched(PropertyInfo property)
        {
            var type = GetPropertyType(property);
            return type == typeof(bool);
        }

        public object? MapFrom(Field field)
        {
            var v = field.GetStringValue();
            return v == null ? null : bool.Parse(v);
        }

        public Field MapTo(PropertyInfo property, object val, string name)
        {
            bool convertedValue = (bool)val;
            return new StringField(name,
                convertedValue
                    ? Boolean.TrueString
                    : Boolean.FalseString, GetStore(property));
        }
    }
}
