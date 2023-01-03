using System;
using System.Reflection;
using Ads.LuceneIndexer.Interfaces;
using Lucene.Net.Documents;

namespace Ads.LuceneIndexer.FieldConverters
{
    public class EnumFieldConverter : AField, IField
    {
        public bool IsMatched(PropertyInfo property)
        {
            return GetPropertyType(property).IsEnum;
        }

        public Field MapTo(PropertyInfo property, object val, string name)
        {
            return new Int32Field(name, (Int32)val, GetStore(property));
        }

        public object? MapFrom(Field value)
        {
            return value.GetInt32Value();
        }
    }
}