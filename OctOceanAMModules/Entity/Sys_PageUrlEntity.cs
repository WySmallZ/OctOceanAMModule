using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OctOceanAMModules.Entity
{
    public class Sys_PageUrlEntity
    {
        /// <summary>
        /// 页面Id
        /// </summary>
        public int PageId { get; set; }
        /// <summary>
        /// 页面Url
        /// </summary>
        public string PageUrl { get; set; }

        /// <summary>
        /// 页面标题
        /// </summary>
        public string PageTitle { get; set; }

        /// <summary>
        /// 父级菜单的PageId，注意：该属性标注只针对主菜单而不是功能
        /// </summary>
        public int ParentMenuPageId { get; set; }
        /// <summary>
        /// 当前功能所在的页面Id，比如如果一个页面有一个按钮或链接（按钮会打开新的页面），PageId代表的就是新打开的页面的Id，ParentPageId代表的就是按钮所在的页面Id
        /// </summary>
        public int ParentPageId { get; set; }

        /// <summary>
        /// 菜单排序的序号
        /// </summary>
        public int MenuSortNum { get; set; }
        /// <summary>
        /// 当前是否是功能页面，即由页面的按钮或链接打开的页面而不是主菜单打开的
        /// </summary>
        public bool IsFunPage => ParentPageId > 0;
        /// <summary>
        /// 是否是主菜单页面
        /// </summary>
        public bool IsMenu => !(ParentPageId > 0 && ParentMenuPageId == 0);

       
    }


    
}
