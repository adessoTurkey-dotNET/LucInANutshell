using System;
using System.Reflection;
using Ads.LuceneIndexer.Interfaces;
using Lucene.Net.Documents;

namespace Ads.LuceneIndexer.FieldConverters
{
    public class StringFieldConverter : AField, IField
    {
        public bool IsMatched(PropertyInfo property)
        {
            var type = GetPropertyType(property);
            return type == typeof(string) || type == typeof(object) || type == typeof(String) || type == typeof(Object);
        }

        public Field MapTo(PropertyInfo property, object val, string name)
        {
            return new StringField(name, val.ToString(), GetStore(property));
        }

        public object? MapFrom(Field field)
        {
            return field.GetStringValue();
        }
    }
}