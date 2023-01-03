using Ads.LuceneIndexer.Extensions;
using Ads.LuceneIndexer.Interfaces;
using Lucene.Net.Documents;
using Lucene.Net.Util;
using System.Reflection;

namespace Ads.LuceneIndexer.FieldConverters
{
    internal class ByteFieldConverter : AField, IField
    {
        public bool IsMatched(PropertyInfo property)
        {
            return property.GetPropertyType() == typeof(byte[]);
        }

        public object? MapFrom(Field field)
        {
            var binaryValue = field.GetBinaryValue();
            return binaryValue?.Bytes;
        }

        public Field MapTo(PropertyInfo property, object val, string name)
        {
            return new StoredField(name, new BytesRef((byte[])val));
        }
    }
}
