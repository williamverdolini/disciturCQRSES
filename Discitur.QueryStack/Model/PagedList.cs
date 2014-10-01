using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Discitur.QueryStack.Model
{
    public class PagedList<T>
    {
        public int StartRow { get; set; }
        public int Count { get; set; }
        public int PageSize { get; set; }
        public List<T> Records { get; set; }
    }
}