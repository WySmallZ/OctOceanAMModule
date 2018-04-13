using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OctOceanAMModules.Entity
{
    public class Sys_UserEntity
    {
        public int UserId { get; set; }
        public string UserCode { get; set; }
        public string UserPassWord { get; set; }
        public string UserName { get; set; }
        public string UserMail { get; set; }
    }
}
