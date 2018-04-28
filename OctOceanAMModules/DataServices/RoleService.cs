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



        public int InsertRole(Sys_RoleEntity _sys_RoleEntity)
        {
            string sql = "INSERT INTO Sys_Role ( RoleCode, RoleName,Authorities ) VALUES  ( @RoleCode,@RoleName,@Authorities)";
            using (SqlConnection conn = new SqlConnection(this.SqlConnectionString))
            {
                conn.Open();
                return conn.Execute(sql, new { RoleCode = _sys_RoleEntity.RoleCode, RoleName = _sys_RoleEntity.RoleName, Authorities=_sys_RoleEntity.Authorities });
            }
        }

        public int InsetRole(IList<Sys_RoleEntity> _sys_Roles_Entity)
        {
            string sql = "INSERT INTO Sys_Role ( RoleCode, RoleName,Authorities ) VALUES  ( @RoleCode,@RoleName,@Authorities)";
            using (SqlConnection conn = new SqlConnection(this.SqlConnectionString))
            {
                conn.Open();
                return conn.Execute(sql,
                    _sys_Roles_Entity.Select(a => new { RoleCode = a.RoleCode, RoleName = a.RoleName, Authorities=a.Authorities }).ToArray()
                    );
            }
        }

        public int UpdateRole(Sys_RoleEntity _sys_RoleEntity)
        {
            string sql = "UPDATE Sys_Role SET RoleCode=@RoleCode,RoleName=@RoleName,Authorities=@Authorities WHERE RoleId=@RoleId";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.Execute(sql, new
                {
                    RoleCode = _sys_RoleEntity.RoleCode,
                    RoleName = _sys_RoleEntity.RoleName,
                    RoleId = _sys_RoleEntity.RoleId,
                    Authorities=_sys_RoleEntity.Authorities
                });
            }
        }

        public int UpdateRole(IList<Sys_RoleEntity> sys_Roles)
        {
            string sql = "UPDATE Sys_Role SET RoleCode=@RoleCode,RoleName=@RoleName,Authorities=@Authorities WHERE RoleId=@RoleId";
            using (SqlConnection conn = new SqlConnection(this.SqlConnectionString))
            {
                conn.Open();
                return conn.Execute(sql,
                    sys_Roles.Select(a => new
                    {
                        RoleCode = a.RoleCode,
                        RoleName = a.RoleName,
                        RoleId = a.RoleId,
                        Authorities=a.Authorities
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
            string sql = "SELECT RoleId,RoleCode,RoleName,Authorities FROM Sys_Role";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.Query<Sys_RoleEntity>(sql).ToList();
            }
        }

        public Sys_RoleEntity GetSys_RoleEntity(int RoleId)
        {
            string sql = "SELECT RoleId,RoleCode,RoleName,Authorities FROM Sys_Role WHERE RoleId=@RoleId";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<Sys_RoleEntity>(sql, new { RoleId = RoleId });
            }
        }

        public Sys_RoleEntity GetSys_RoleEntity(string RoleCode)
        {
            string sql = "SELECT RoleId,RoleCode,RoleName,Authorities FROM Sys_Role WHERE RoleCode=@RoleCode";
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
    select ROW_NUMBER() OVER(ORDER BY RoleId) AS SNumber,RoleId,RoleCode,RoleName,Authorities FROM Sys_Role
)
select RoleId,RoleCode,RoleName,Authorities from wt where wt.SNumber BETWEEN {0} AND {1} ;", start,end);

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
