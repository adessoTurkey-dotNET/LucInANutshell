using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.Model
{
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
}
