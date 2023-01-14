# LucInANutshell


Lucene is a full-text search engine within the scope of the Apache Lucene project.

It consists of the infrastructure of ElasticSearch and Solr, which are among the search engines that are very popular in today's technology. Lucene is implemented in  Java and the .NET version is made available to .NET users.


How to add the libraries to a project:
```
   _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddDocumentIndexer()  //To add document to given type converter
                .AddIndexProvider()   //To add Lucene indexer, to store and search on lucene indices.
```

Object Mapping
Any property that is of type object gets mapped to a JSON string in the field value, but if it has an actual type declared, that type's properties will be mapped individually with dot notation.
Example : “Order.BillingDetails.Address”
converted from its type to Lucene.net field type respectively(FieldConverters). The implementation detail is as follows. If the type is declared. Every property is 

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



Then converted types are added to documents. With GetFields method called recursively, Nested objects are mapped using reflection.




    public interface IDocumentIndexer
    {
        T MapFrom<T>(Document source);
        object MapFrom(Document source, Type contentType);
        Document MapToDocument(object source);
    }


The IDocumentIndexer interface has 3 methods. When you want to map from a document with a given type you can use MapFrom method which returns a T depending on the given object.

When you want to create a document with a given JSON object you can use the MapToDocument method.



Search Attribute
The search attribute comes with three properties, by default store is true and isTextField is false, this custom attribute determines how to store given JSON, if you set IsKey= true that property will be determined as key. If you set IsTextField = true, the property will be set as TextField in the document.

    //By default store : true token : false 
    public class SearchAttribute : Attribute
    {
        public bool IsTextField { get; set; }

        public bool Store { get; set; }

        public bool IsKey { get; set; }
    }



Usage

Example Model can be seen as below:

    public class Order
    {
        public string Id { get; set; }

        public double Price { get; set; }

        public string Currency { get; set; }

        public BillingDetails BillingDetails { get; set; }

        public IList<Item> Items { get; set; }

        public Priority Priority { get; set; }

        public DateTime OrderDate { get; set; }

        public string OrderDetail { get; set; }
    }

    public enum Priority
    { 
        Minor,
        Medium,
        Major,
        Critical
    }




And the usage of the JSON indexer can be seen as followed:


        [Fact]
        public void Test_JsonMapping()
        {
            var documentIndexer = _serviceProvider.GetService<IDocumentIndexer>();
            var target = _orderList.First();

            var document = documentIndexer.MapToDocument(target);

            var objectFromDocument = documentIndexer.MapFrom<Order>(document);

            Assert.Equivalent(target, objectFromDocument);
        }



When you want to perform search on stored documents: 



        [Fact]
        public async Task Test_DeepLevelSearchWithShould()
        {
            var indexProvider = _serviceProvider.GetService<IIndexProvider>();

            var result = await indexProvider.Search()
                .Should(() => new TermQuery(new Term("OrderDetail", "detail7")))
                .Should(() => new TermQuery(new Term("OrderDetail", "detail5")))
                .ListResult(typeof(Order));

            Assert.Equal(2, result.Count);
        }
 
 
 
