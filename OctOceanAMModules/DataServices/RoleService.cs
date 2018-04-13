using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using OctOceanAMModules.Entity;
using Dapper;

namespace OctOceanAMModules.DataServices
{
    public class RoleService
    {
        private readonly string SqlConnectionString = string.Empty;
        public RoleService(ConfigService configService)
        {
            this.SqlConnectionString = configService.SqlConntcionString;
        }



        public int InsertRole(Sys_RoleEntity sys_RoleEntity)
        {
            string sql = "INSERT INTO Sys_Role ( RoleCode, RoleName ) VALUES  ( @RoleCode,@RoleName)";
            using (SqlConnection conn = new SqlConnection(this.SqlConnectionString))
            {
                conn.Open();
                return conn.Execute(sql, new { RoleCode = sys_RoleEntity.RoleCode, RoleName = sys_RoleEntity.RoleName });
            }
        }

        public int InsetRole(IList<Sys_RoleEntity> sys_Roles)
        {
            string sql = "INSERT INTO Sys_Role ( RoleCode, RoleName ) VALUES  ( @RoleCode,@RoleName)";
            using (SqlConnection conn = new SqlConnection(this.SqlConnectionString))
            {
                conn.Open();
                return conn.Execute(sql,
                    sys_Roles.Select(a => new { RoleCode = a.RoleCode, RoleName = a.RoleName }).ToArray()
                    );
            }
        }

        public int UpdateRole(Sys_RoleEntity sys_RoleEntity)
        {
            string sql = "UPDATE Sys_Role SET RoleCode=@RoleCode,RoleName=@RoleName WHERE RoleId=@RoleId";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.Execute(sql, new
                {
                    RoleCode = sys_RoleEntity.RoleCode,
                    RoleName = sys_RoleEntity.RoleName,
                    RoleId = sys_RoleEntity.RoleId
                });
            }
        }

        public int UpdateRole(IList<Sys_RoleEntity> sys_Roles)
        {
            string sql = "UPDATE Sys_Role SET RoleCode=@RoleCode,RoleName=@RoleName WHERE RoleId=@RoleId";
            using (SqlConnection conn = new SqlConnection(this.SqlConnectionString))
            {
                conn.Open();
                return conn.Execute(sql,
                    sys_Roles.Select(a => new
                    {
                        RoleCode = a.RoleCode,
                        RoleName = a.RoleName,
                        RoleId = a.RoleId
                    }
                    ).ToArray()
                );
            }
        }

        public async Task<int> DeleteRoleAsync(int RoleId)
        {
            string sql = "DELETE FROM Sys_Role  WHERE RoleId=@RoleId";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return await connection.ExecuteAsync(sql, new
                {
                    RoleId = RoleId
                });
            }
        }

        public int DeleteRole(int[] RoleIds)
        {
            string sql = "DELETE FROM Sys_Role  WHERE RoleId=@RoleId";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.Execute(sql,
                    RoleIds.Select(a => new { RoleId = a }).ToArray());
            }
        }

        public int DeleteRoleWithIn(int[] RoleIds)
        {
            string sql = "DELETE FROM Sys_Role  WHERE RoleId in @RoleIds";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.Execute(sql, new { RoleIds = RoleIds });
            }
        }

        public List<Sys_RoleEntity> GetRoleList()
        {
            string sql = "SELECT RoleId,RoleCode,RoleName FROM Sys_Role";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.Query<Sys_RoleEntity>(sql).ToList();
            }
        }

        public Sys_RoleEntity GetSys_RoleEntity(int RoleId)
        {
            string sql = "SELECT RoleId,RoleCode,RoleName FROM Sys_Role WHERE RoleId=@RoleId";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<Sys_RoleEntity>(sql, new { RoleId = RoleId });
            }
        }

        public Sys_RoleEntity GetSys_RoleEntity(string RoleCode)
        {
            string sql = "SELECT RoleId,RoleCode,RoleName FROM Sys_Role WHERE RoleCode=@RoleCode";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<Sys_RoleEntity>(sql, new { RoleCode = RoleCode });
            }
        }


        public IList<Sys_RoleEntity> GetRolesWithPager(int PageIndex  , int PageSize, out int SumCount)
        {
            int start = (PageIndex - 1) * PageSize + 1;
            int end = PageIndex * PageSize;
            string sql =string.Format(@"
SELECT COUNT(1) FROM Sys_Role;
with wt as 
(
    select ROW_NUMBER() OVER(ORDER BY RoleId) AS SNumber,RoleId,RoleCode,RoleName FROM Sys_Role
)
select RoleId,RoleCode,RoleName from wt where wt.SNumber BETWEEN {0} AND {1} ;", start,end);

            using(var connection=new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                using (var multi = connection.QueryMultiple(sql))
                {
                    

                    SumCount = multi.Read<int>().First();
                    return multi.Read<Sys_RoleEntity>().ToList();
                }
            }


        }

    }
}
