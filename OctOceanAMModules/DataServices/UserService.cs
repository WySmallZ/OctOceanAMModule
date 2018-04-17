using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using OctOceanAMModules.Entity;
using Dapper;
using System.Threading.Tasks;
using System.Linq;

namespace OctOceanAMModules.DataServices
{
    public class UserService
    {
        private readonly string SqlConnectionString = string.Empty;
        public UserService(ConfigService configService)
        {
            this.SqlConnectionString = configService.SqlConntcionString;
        }


        public int InsertUser(Sys_UserEntity sys_UserEntity)
        {
            string sql = "INSERT INTO Sys_User ( UserCode, UserPassWord, UserName, UserMail ) VALUES  (@UserCode, @UserPassWord, @UserName, @UserMail)";
            using (SqlConnection conn = new SqlConnection(this.SqlConnectionString))
            {
                conn.Open();
                return conn.Execute(sql, new {
                    UserCode = sys_UserEntity.UserCode,
                    UserPassWord = sys_UserEntity.UserPassWord,
                    UserName = sys_UserEntity.UserName,
                    UserMail = sys_UserEntity.UserMail 
                });
            }
        }

        public int UpdateUser(Sys_UserEntity sys_UserEntity)
        {
            string sql = "UPDATE Sys_User SET UserCode=@UserCode,UserPassWord=@UserPassWord,UserName=@UserName,UserMail=@UserMail WHERE UserId=@UserId";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.Execute(sql, new
                {
                    UserId=sys_UserEntity.UserId,
                    UserCode = sys_UserEntity.UserCode,
                    UserPassWord = sys_UserEntity.UserPassWord,
                    UserName = sys_UserEntity.UserName,
                    UserMail = sys_UserEntity.UserMail
                });
            }
        }


        public async Task<int> DeleteUserAsync(int UserId)
        {
            string sql = "DELETE FROM Sys_User  WHERE UserId=@UserId";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return await connection.ExecuteAsync(sql, new
                {
                    UserId = UserId
                });
            }
        }


        public Sys_UserEntity GetSys_UserEntity(int UserId)
        {
            string sql = "SELECT UserId,UserCode,UserPassWord,UserName,UserMail FROM Sys_User WHERE UserId=@UserId";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<Sys_UserEntity>(sql, new { UserId = UserId });
            }
        }


        public Sys_UserEntity  GetSys_UserEntity(string UserCode)
        {
            string sql = "SELECT UserId,UserCode,UserPassWord,UserName,UserMail FROM Sys_User WHERE UserCode=@UserCode";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<Sys_UserEntity>(sql, new { UserCode = UserCode });
            }
        }


        public IList<Sys_UserEntity> GetUsersWithPager(int PageIndex, int PageSize, out int SumCount)
        {
            int start = (PageIndex - 1) * PageSize + 1;
            int end = PageIndex * PageSize;
            string sql = string.Format(@"
SELECT COUNT(1) FROM Sys_User;
with wt as 
(
    select ROW_NUMBER() OVER(ORDER BY UserId) AS SNumber,UserId,UserCode,UserPassWord,UserName,UserMail FROM Sys_User
)
select UserId,UserCode,UserPassWord,UserName,UserMail  from wt where wt.SNumber BETWEEN {0} AND {1} ;", start, end);

            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                using (var multi = connection.QueryMultiple(sql))
                {


                    SumCount = multi.Read<int>().First();
                    return multi.Read<Sys_UserEntity>().ToList();
                }
            }


        }


    }
}
