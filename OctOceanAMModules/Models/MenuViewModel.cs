using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OctOceanAMModules.Models
{
    public class MenuViewModel
    {
        public int PageId { get; set; }
        public string PageUrl { get; set; }
        public int ParentId { get; set; }

        public string PageTitle { get; set; }

        public int PageSortNum { get; set; }
        
    }
}
