﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OctOceanAMModules.Entity
{
    public class Sys_PageMenuEntity 
    {
        public int PageId { get; set; }
        public Sys_PageUrlEntity Sys_PageUrl { get; set; }

        public IList<Sys_PageFunEntity> PageFuns { get; set; }

        public IList<Sys_PageUrlEntity> ChirldMenuPageUrls { get; set; }

        public IList<Sys_PageUrlEntity> ChirldFunPageUrls { get; set; }
    }
}