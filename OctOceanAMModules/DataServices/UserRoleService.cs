using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OctOceanAMModules.DataServices
{
    public class UserRoleService
    {
        private readonly string SqlConnectionString = string.Empty;
        public UserRoleService(ConfigService configService)
        {
            this.SqlConnectionString = configService.SqlConntcionString;
        }
    }
}
