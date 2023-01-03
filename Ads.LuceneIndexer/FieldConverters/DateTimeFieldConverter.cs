using System;
using System.Reflection;
using Ads.LuceneIndexer.Interfaces;
using Lucene.Net.Documents;

namespace Ads.LuceneIndexer.FieldConverters
{
    public class DateTimeFieldMapper : AField, IField
    {
        public bool IsMatched(PropertyInfo property)
        {
            return GetPropertyType(property) == typeof(DateTime);
        }

        public Field MapTo(PropertyInfo property, object val, string name)
        {
            var convertedValue = (DateTime)val;
            if (convertedValue.Year == 1)
            {
                convertedValue = DateTime.SpecifyKind(convertedValue, DateTimeKind.Utc);
            }

            return new StringField(name,
                convertedValue.ToString("o"),
                GetStore(property));
        }

        public object? MapFrom(Field field)
        {
            var v = field.GetStringValue();
            return v == null ? null : DateTime.Parse(v);
        }
    }
}