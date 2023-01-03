using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ads.LuceneIndexer.Models
{
    public class Result<T>
    {
        public float Score { get; set; }
        public T Hit { get; set; }
    }
}
