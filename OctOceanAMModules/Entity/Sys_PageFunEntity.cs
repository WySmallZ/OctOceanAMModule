using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OctOceanAMModules.Entity
{
    public class Sys_PageFunEntity
    {
        /// <summary>
        /// 功能Id
        /// </summary>
        public int FunId { get; set; }
        /// <summary>
        /// 功能所在的页面Id
        /// </summary>
        public int PageId { get; set; }
        /// <summary>
        /// 功能Code
        /// </summary>
        public string FunCode { get; set; }
        /// <summary>
        /// 功能名称
        /// </summary>
        public string FunName { get; set; }

        /// <summary>
        /// 是否会关联新的页面
        /// </summary>
        public int HasNewPage { get; set; }
        /// <summary>
        /// 关联的的新的页面的Id
        /// </summary>
        public int NewPageId { get; set; }

        /// <summary>
        /// 是否是功能菜单项，即可以打开新的页面
        /// </summary>
        public bool IsFunMenuStatus => HasNewPage > 0;


    }
}
