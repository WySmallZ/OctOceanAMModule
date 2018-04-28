using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OctOceanAMModules.Models
{
    public class AuthorityJsonData
    {
        /// <summary>
        /// PageId 或者“f_”+FunId
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 当IsFun为true时，该值才有效，表示对应的FunId
        /// </summary>
        public string FunId { get; set; }
        /// <summary>
        /// 所在的父级页面的Id
        /// </summary>
        public string PId { get; set; }
        /// <summary>
        /// 当前是否为功能而非菜单页面
        /// </summary>
        public bool IsFun { get; set; }
        /// <summary>
        /// 当前是否为功能关联页面，注意，该实体同时应用于菜单、页面和具体功能
        /// </summary>
        public bool IsFunPage { get; set; }

        public string Name { get; set; }
    }



    public class TreeJsonData : AuthorityJsonData
    {


        public bool Checked { get; set; }

       

        public string IconSkin { get; set; }

    }


    public class RoleAuthrotyJsonData
    {
        public int P { get; set; }
        public string Name { get; set; }
        public int [] Fs { get; set; }
    }








}
