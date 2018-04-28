using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OctOceanAMModules.Entity
{
    public class Sys_PageUrlEntity_Ex
    {
        public int PageId { get; set; }
        public Sys_PageUrlEntity Sys_PageUrl { get; set; }
        public IList<Sys_PageUrlEntity> ChirldMenuPageUrls { get; set; }

    }
}
