using System.Reflection;
using Ads.LuceneIndexer.Extensions;
using Ads.LuceneIndexer.Interfaces;
using Lucene.Net.Documents;
using Newtonsoft.Json;

namespace Ads.LuceneIndexer.FieldConverters
{
    public class ObjectFieldConverter : AField, IField
    {
        public bool IsMatched(PropertyInfo property)
        {
            var type = GetPropertyType(property);
            return !type.IsPrimitiveType() && !type.IsTypeCollection() && type == typeof(object);
        }

        //Serialize the json and map
        public Field MapTo(PropertyInfo property, object val, string name)
        {
            return new StringField(name, JsonConvert.SerializeObject(val), GetStore(property));
        }

        public object? MapFrom(Field field)
        {
            return JsonConvert.DeserializeObject(field.GetStringValue());
        }
    }
}