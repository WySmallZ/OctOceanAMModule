using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OctOceanAMModules.Entity;

namespace OctOceanAMModules.Models
{
    public class MenuViewModel
    {
        public int PageId { get; set; }
        public string PageUrl { get; set; }
        public int ParentId { get; set; }

        public string PageTitle { get; set; }

        public int PageSortNum { get; set; }

        public IList<Sys_PageFunEntity> Funs { get; set; }
        public bool HasChirldPageUrl { get; set; }
        public bool IsFunPage { get; set; }

        public bool HasFuns => (Funs != null && Funs.Any());


        
    }
}
