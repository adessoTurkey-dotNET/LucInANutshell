using Ads.LuceneIndexer.Extensions;

namespace TestApp.Model
{
    public class BillingDetails
    {
        public string CustomerName { get; set; }
        [SearchAttribute(Store = false)]
        public string? Address { get; set; }

        public string Email { get; set; }
        [SearchAttribute(IsTextField = true)]
        public string Phone { get; set; }

        public int PostalCode { get; set; }
        public bool IsShipped { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}