using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OctOceanAMModules.DataServices
{
    public class ConfigService
    {
        public  readonly string SqlConntcionString = string.Empty;
        public ConfigService(string _SqlConntcionString)
        {
            SqlConntcionString = _SqlConntcionString;
        }

       
    }
}
