using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discitur.QueryStack.ViewModel
{
    public class LessonViewModel
    {
        public string Keyword { get; set; }
        public bool InContent { get; set; }
        public string Discipline { get; set; }
        public string School { get; set; }
        public string Classroom { get; set; }
        public int? Rate { get; set; }
        public string Tags { get; set; }
        public string PublishedOn { get; set; }
        public string PublishedBy { get; set; }
        public int StartRow { get; set; }
        public int PageSize { get; set; }
        public string OrderBy { get; set; }
        public string OrderDir { get; set; }
    }
}
