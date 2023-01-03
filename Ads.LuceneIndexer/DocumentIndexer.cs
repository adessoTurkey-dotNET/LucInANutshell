using Ads.LuceneIndexer.Extensions;
using Ads.LuceneIndexer.Interfaces;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Newtonsoft.Json;
using System.Collections;
using System.Reflection;

namespace Ads.LuceneIndexer
{
    public class DocumentIndexer : IDocumentIndexer
    {
        private readonly IEnumerable<IField> _fields;

        public DocumentIndexer(IEnumerable<IField> fields)
        {
            _fields = fields;
        }

        /// <summary>
        /// Returns the object from given document.
        /// </summary>
        public T MapFrom<T>(Document document)
        {
            return (T)MapFrom(document, typeof(T));
        }


        /// <summary>
        /// Returns the object from given document.
        /// </summary>
        public object MapFrom(Document document, Type type)
        {
            var contentItem = Activator.CreateInstance(type);
            return GetValue(document.Fields, contentItem, 0);
        }



        /// <summary>
        /// Maps a given object to a Lucene document
        /// </summary>
        public Document MapToDocument(object obj)
        {
            var document = new Document();
            var fields = GetFields(obj, new List<Field>(), string.Empty);
            foreach (var field in fields)
            {
                if (field == null) continue;

                document.Add(field);
            }

            return document;
        }

        private Field? GetFieldFromValue(PropertyInfo propertyInfo, object value, string name)
        {
            var propertyMapper = GetFieldMapper(propertyInfo);
            return propertyMapper?.MapTo(propertyInfo, value, name);
        }

        public static Document JsonToDocument(string json)
        {
            // Deserialize the JSON string into an object
            object obj = JsonConvert.DeserializeObject(json);

            // Create a new document
            Document doc = new Document();

            // Get all of the object's properties
            PropertyInfo[] properties = obj.GetType().GetProperties();

            // Iterate through each property and add it to the document
            foreach (PropertyInfo property in properties)
            {
                // Get the value of the property
                object propertyValue = property.GetValue(obj, null);

                // Convert the value to a string (you may need to use a different method for converting, depending on the type of the property)
                string propertyValueString = propertyValue.ToString();

                // Add the property to the document as a field
                doc.Add(new Field(property.Name, propertyValueString, Field.Store.YES, Field.Index.ANALYZED));
            }

            return doc;
        }

        private IList<Field> GetFields(object source, IList<Field> fields, string prefix)
        {

            foreach (var propertyInfo in source.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0))
            {
                if (propertyInfo.GetIndexParameters() == null)
                {
                    continue;
                }
                object propertyValue = propertyInfo.GetValue(source, null);
                if (propertyValue == null) continue;

                string name = !string.IsNullOrEmpty(prefix)
                    ? $"{prefix}.{propertyInfo.Name}"
                    : propertyInfo.Name;

                if (propertyInfo.IsPropertyCollection())
                {
                    IList propertyValueList = (IList)propertyValue;
                    if (propertyValueList.Count == 0)
                    {
                        continue;
                    }

                    var x = propertyInfo.GetPropertyType().GetGenericArguments().ToList();

                    var genericType = propertyInfo.GetPropertyType().GetGenericArguments().Single();
                    if (genericType.IsPrimitiveType())
                    {
                        foreach (var itemValue in propertyValueList)
                        {
                            var f = GetFieldFromValue(propertyInfo, itemValue, name);
                            if (f == null) continue;
                            fields.Add(f);
                        }
                    }
                    else
                    {
                        foreach (var itemValue in propertyValueList)
                        {
                            GetFields(itemValue, fields, name);
                        }
                    }

                    continue;
                }

                if (!propertyInfo.IsPrimitiveType() && propertyInfo.GetPropertyType() != typeof(object))
                {
                    GetFields(propertyValue, fields, name);
                    continue;
                }

                var fieldFromValue = GetFieldFromValue(propertyInfo, propertyValue, name);
                if (fieldFromValue != null)
                    fields.Add(fieldFromValue);
            }

            return fields;
        }

        private object GetValue(IList<IIndexableField> fields, object parent, int level)
        {
            var properties = parent.GetType().GetProperties();
            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.IsPropertyCollection())
                {
                    var genericType = propertyInfo.GetPropertyType().GetGenericArguments().Single();
                    var listType = typeof(List<>).MakeGenericType(genericType);

                    object nestedCollection = Activator.CreateInstance(listType);
                    var listFields =
                        fields.Where(x =>
                        {
                            var nameSplit = x.Name.Split('.');
                            if (nameSplit.Length > 1)
                            {
                                return nameSplit[level].Equals(propertyInfo.Name);
                            }

                            return x.Name.Equals(propertyInfo.Name);
                        })
                            .ToList();

                    if (genericType.IsPrimitiveType())
                    {
                        foreach (var indexableField in listFields)
                        {
                            var v = GetValueFromField(propertyInfo, (Field)indexableField);
                            if (v == null) continue;
                            _ = (nestedCollection as IList).Add(v);
                        }
                    }
                    else if (listFields.Any())
                    {
                        var firstField = listFields.First();
                        var firstFieldList =
                            listFields.Where(x => x.Name.Equals(firstField.Name))
                                .ToList();

                        foreach (var indexableField in firstFieldList)
                        {
                            IList<IIndexableField> groupedFields;
                            if (indexableField.Equals(firstFieldList.Last()))
                            {
                                var skip = listFields.IndexOf(indexableField);
                                var take = listFields.Count - skip;


                                groupedFields = listFields.Skip(skip).Take(take).ToList();
                            }
                            else
                            {
                                var firstFieldIdx = firstFieldList.IndexOf(indexableField);
                                var nextFirstField = firstFieldList[firstFieldIdx + 1];

                                var skip = listFields.IndexOf(indexableField);
                                var take = listFields.IndexOf(nextFirstField) - skip;


                                groupedFields = listFields.Skip(skip).Take(take).ToList();
                            }

                            object nestedComplexType = Activator.CreateInstance(genericType);
                            ((IList)nestedCollection).Add(GetValue(groupedFields, nestedComplexType, level + 1));
                        }
                    }

                    propertyInfo.SetValue(parent, nestedCollection);
                    continue;
                }

                if (!propertyInfo.IsPrimitiveType() && propertyInfo.GetPropertyType() != typeof(object))
                {
                    object nestedComplexType = Activator.CreateInstance(propertyInfo.GetPropertyType());
                    var listFields =
                        fields.Where(x => x.Name.Split('.')[level].Equals(propertyInfo.Name))
                            .ToList();

                    propertyInfo.SetValue(parent, GetValue(listFields, nestedComplexType, level + 1));
                    continue;
                }

                var field =
                    fields.FirstOrDefault(x => x.Name.Split('.')[level].Equals(propertyInfo.Name));

                if (field == null)
                {
                    continue;
                }

                propertyInfo.SetValue(parent, GetValueFromField(propertyInfo, (Field)field));
            }

            return parent;
        }

        public IField? GetFieldMapper(PropertyInfo? propertyInfo)
        {
            if (propertyInfo == null) return null;

            return _fields
                .FirstOrDefault(x => x.IsMatched(propertyInfo));
        }

        private object? GetValueFromField(PropertyInfo propertyInfo, Field field)
        {
            var propertyMapper = GetFieldMapper(propertyInfo);
            return propertyMapper != null
                    ? propertyMapper.MapFrom(field)
                    : Parse(field.GetStringValue(), propertyInfo.GetPropertyType());
        }

        protected virtual object? Parse(string value, Type type)
        {
            if (value == null) return null;

            // Optimization - don't bother calling 
            if (type == typeof(string))
            {
                return value;
            }

            // string -> DateTime
            if (type == typeof(DateTime))
            {
                return DateTools.StringToDate(value);
            }

            return Convert.ChangeType(value, type);
        }

    }
}
