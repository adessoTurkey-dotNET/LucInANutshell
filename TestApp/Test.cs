using Ads.LuceneIndexer.Extensions;
using Ads.LuceneIndexer.Interfaces;
using Bogus;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TestApp.Model;
using Xunit;

namespace TestApp
{
    public class Test
    {
        private readonly IList<Order> _orderList;
        private ServiceProvider _serviceProvider;

        public Test()
        {
            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddDocumentIndexer()
                .AddIndexProvider()

                .BuildServiceProvider();

            _orderList = Init();
        }

        private IList<Order> Init()
        {
            var billingDetailsFaker = new Faker<BillingDetails>()
                .RuleFor(x => x.Email, setter => setter.Person.Email)
                .RuleFor(x => x.CustomerName, setter => setter.Person.FullName)
                .RuleFor(x => x.Address, setter => setter.Address.FullAddress())
                .RuleFor(x => x.CreatedOn, setter => setter.Date.Soon().TrimMilliseconds())
                .RuleFor(x => x.IsShipped, setter => setter.Random.Bool());

            var itemFaker = new Faker<Item>()
                .RuleFor(x => x.Id, f => f.Random.Int(1, 100))
                .RuleFor(x => x.Name, setter => setter.Lorem.Word());


            var orderFaker = new Faker<Order>()
                .RuleFor(x => x.BillingDetails, billingDetailsFaker)
                .RuleFor(p => p.Items, f => f.Make(10, () => itemFaker.Generate()))
                .RuleFor(p => p.Id, f => f.IndexFaker.ToString())
                .RuleFor(x => x.Currency, setter => setter.Finance.Currency().Code)
                .RuleFor(x => x.Price, setter => setter.Random.Double(0, 100))
                .RuleFor(x => x.Priority, setter => setter.Random.Enum<Priority>())
                .RuleFor(x => x.OrderDate, setter => setter.Date.Soon().TrimMilliseconds())
                .RuleFor(x => x.OrderDetail, setter => $"detail{setter.IndexFaker}");

            var orderList = orderFaker.Generate(10);
            Task.Run(() => DisposeAsync()).Wait();
            var indexProvider = _serviceProvider.GetService<IIndexProvider>();
            Task.Run(() => indexProvider.Store(orderList.Cast<object>().ToList(), nameof(Order))).Wait();

            return orderList;
        }


        [Fact]
        public void Test_JsonMapping()
        {
            var documentIndexer = _serviceProvider.GetService<IDocumentIndexer>();
            var target = _orderList.First();

            var document = documentIndexer.MapToDocument(target);

            var objectFromDocument = documentIndexer.MapFrom<Order>(document);

            Assert.Equivalent(target, objectFromDocument);
        }


        [Fact]
        public async Task Test_Search_ReturnAll()
        {
            var indexProvider = _serviceProvider.GetService<IIndexProvider>();

            var listResult =
                await indexProvider.Search()
                    .ListResult(typeof(Order));

            Assert.Equal(10, listResult.Count);
        }


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

        [Fact]
        public void Test_Get_Document_By_Id()
        {
            var indexProvider = _serviceProvider.GetService<IIndexProvider>();

            //var x = _orderList.FirstOrDefault(x => x.Id == "3");
            var order = indexProvider.GetDocumentById<Order>("3");

            Assert.Equal("3", order.Hit.Id);
        }



        [Fact]
        public async Task Test_Sort_By_Date_Asc()
        {
            var indexProvider = _serviceProvider.GetService<IIndexProvider>();

            var orderedList = _orderList.OrderByDescending(x => x.OrderDate);

            var orderedSearchResult =
                await indexProvider.Search()
                    .Sort(() => new Lucene.Net.Search.SortField("OrderDate", SortFieldType.STRING, true))
                    .ListResult<Order>();

            Assert.Equal(orderedList.First().Id, orderedSearchResult.Hits.First().Hit.Id);
            Assert.Equal(orderedList.Last().Id, orderedSearchResult.Hits.Last().Hit.Id);
        }



        public async Task DisposeAsync()
        {
            var indexProvider = _serviceProvider.GetService<IIndexProvider>();

            await indexProvider.DeleteIndex(nameof(Order));
        }

    }
}
