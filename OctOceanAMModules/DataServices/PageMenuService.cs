using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using OctOceanAMModules.Entity;

namespace OctOceanAMModules.DataServices
{
    public class PageMenuService
    {
        private readonly string SqlConnectionString = string.Empty;
        public PageMenuService(ConfigService configService)
        {
            this.SqlConnectionString = configService.SqlConntcionString;
        }







        //查询出两个表的数据
        public Dictionary<int, Sys_PageMenuEntity> getSys_PageMenuEntityDic()
        {
            List<Sys_PageUrlEntity> pageUrlList = new List<Sys_PageUrlEntity>();
            Dictionary<int, List<Sys_PageFunEntity>> dic_pageId_FunEntity = new Dictionary<int, List<Sys_PageFunEntity>>();

            string sql = @"
SELECT FunId,PageId,FunCode,FunName FROM Sys_PageFun;
SELECT PageId,PageUrl,PageTitle,ParentMenuPageId,ParentPageId,MenuSortNum FROM Sys_PageUrl  ORDER BY ParentMenuPageId ASC,MenuSortNum ASC, ParentPageId ASC;"; //排序必不可少
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                using (var multi = connection.QueryMultiple(sql))
                {
                    dic_pageId_FunEntity = multi.Read<Sys_PageFunEntity>().GroupBy(a => a.PageId).ToDictionary(a => a.Key, b => b.ToList());
                    pageUrlList = multi.Read<Sys_PageUrlEntity>().ToList();
                }
            }


            ////获取页面的子链接
            //Dictionary<int, List<Sys_PageUrlEntity>> dic_pageId_ChildMenuPageId =
            //     pageUrlList.GroupBy(a => a.ParentMenuPageId).OrderBy(b => b.Key)
            //    .ToDictionary(b => b.Key, c => c.OrderBy(d => d.ParentPageId).ToList());


            Dictionary<int, Sys_PageMenuEntity> dic_pageId_menuentity = new Dictionary<int, Sys_PageMenuEntity>();
            foreach (var urlentity in pageUrlList)
            {
                //判断是否是普通链接
                if (urlentity.IsMenu)
                {

                    if (urlentity.ParentMenuPageId > 0)
                    {
                        //如果不是根链接，是子链接，因为上面排序规则，实际执行时，并不会走该if中的语句
                        if (!dic_pageId_menuentity.ContainsKey(urlentity.ParentMenuPageId))
                        {
                            //查找根链接
                            var pe = pageUrlList.FirstOrDefault(u => u.PageId == urlentity.ParentMenuPageId);
                            if (pe != null)
                            {
                                dic_pageId_menuentity.Add(urlentity.ParentMenuPageId
                                , new Sys_PageMenuEntity()
                                {
                                    PageId = urlentity.ParentMenuPageId,
                                    Sys_PageUrl = pe,
                                    ChirldFunPageUrls = new List<Sys_PageUrlEntity>(),
                                    ChirldMenuPageUrls = new List<Sys_PageUrlEntity>(),
                                    PageFuns = dic_pageId_FunEntity.ContainsKey(urlentity.ParentMenuPageId) ? dic_pageId_FunEntity[urlentity.ParentMenuPageId] : null
                                });

                                dic_pageId_menuentity[urlentity.ParentMenuPageId].ChirldMenuPageUrls.Add(urlentity);
                            }
                        }
                        else
                        {
                            dic_pageId_menuentity[urlentity.ParentMenuPageId].ChirldMenuPageUrls.Add(urlentity);
                        }

                    }
                    else
                    {
                        //如果是根链接
                        if (!dic_pageId_menuentity.ContainsKey(urlentity.PageId))
                        {
                            dic_pageId_menuentity.Add(urlentity.PageId
                                , new Sys_PageMenuEntity()
                                {
                                    PageId = urlentity.PageId,
                                    Sys_PageUrl = urlentity,
                                    ChirldFunPageUrls = new List<Sys_PageUrlEntity>(),
                                    ChirldMenuPageUrls = new List<Sys_PageUrlEntity>(),
                                    PageFuns = dic_pageId_FunEntity.ContainsKey(urlentity.PageId) ? dic_pageId_FunEntity[urlentity.PageId] : null
                                });
                        }
                    }

                }
                //判断是否是页面功能链接
                if (urlentity.IsFunPage)
                {
                    if (!dic_pageId_menuentity.ContainsKey(urlentity.ParentPageId))
                    {
                        //查找根链接
                        var pe = pageUrlList.FirstOrDefault(u => u.PageId == urlentity.ParentPageId);
                        if (pe != null)
                        {
                            dic_pageId_menuentity.Add(urlentity.ParentPageId
                                   , new Sys_PageMenuEntity()
                                   {
                                       PageId = urlentity.ParentPageId,
                                       Sys_PageUrl = pe,
                                       ChirldFunPageUrls = new List<Sys_PageUrlEntity>(),
                                       ChirldMenuPageUrls = new List<Sys_PageUrlEntity>(),
                                       PageFuns = dic_pageId_FunEntity.ContainsKey(urlentity.ParentPageId) ? dic_pageId_FunEntity[urlentity.ParentPageId] : null
                                   });

                            dic_pageId_menuentity[urlentity.ParentPageId].ChirldFunPageUrls.Add(urlentity);
                        }
                    }
                    else
                    {
                        dic_pageId_menuentity[urlentity.ParentPageId].ChirldFunPageUrls.Add(urlentity);
                    }
                }

            }
            return dic_pageId_menuentity;




        }





        public int InsertSys_PageUrl(Sys_PageUrlEntity sys_PageUrlEntity)
        {
            string sql = "INSERT INTO Sys_PageUrl ( PageUrl,PageTitle, ParentMenuPageId, ParentPageId,MenuSortNum ) VALUES  (@PageUrl, @PageTitle,@ParentMenuPageId, @ParentPageId,@MenuSortNum)";
            using (SqlConnection conn = new SqlConnection(this.SqlConnectionString))
            {
                conn.Open();
                return conn.Execute(sql, new
                {
                    PageUrl = sys_PageUrlEntity.PageUrl,
                    PageTitle = sys_PageUrlEntity.PageTitle,
                    ParentMenuPageId = sys_PageUrlEntity.ParentMenuPageId,
                    ParentPageId = sys_PageUrlEntity.ParentPageId,
                    MenuSortNum = sys_PageUrlEntity.MenuSortNum
                });
            }
        }

        public int UpdateSys_PageUrl(Sys_PageUrlEntity sys_PageUrlEntity)
        {
            string sql = "UPDATE Sys_PageUrl SET PageUrl=@PageUrl,PageTitle=@PageTitle,ParentMenuPageId=@ParentMenuPageId,ParentPageId=@ParentPageId,MenuSortNum=@MenuSortNum  WHERE PageId=@PageId";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.Execute(sql, new
                {
                    PageId = sys_PageUrlEntity.PageId,
                    PageTitle = sys_PageUrlEntity.PageTitle,
                    PageUrl = sys_PageUrlEntity.PageUrl,
                    ParentMenuPageId = sys_PageUrlEntity.ParentMenuPageId,
                    ParentPageId = sys_PageUrlEntity.ParentPageId,
                    MenuSortNum = sys_PageUrlEntity.MenuSortNum
                });
            }
        }


        public async Task<int> DeleteSys_PageUrlAsync(int PageId)
        {
            string sql = "DELETE FROM Sys_PageUrl  WHERE PageId=@PageId Or ParentMenuPageId=@PageId Or ParentPageId=@PageId";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return await connection.ExecuteAsync(sql, new
                {
                    PageId = PageId
                });
            }
        }


        public Sys_PageUrlEntity GetSys_PageUrlEntity(int PageId)
        {
            string sql = " SELECT PageId, PageUrl, PageTitle,ParentMenuPageId, ParentPageId,MenuSortNum FROM Sys_PageUrl WHERE PageId=@PageId";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<Sys_PageUrlEntity>(sql, new { PageId = PageId });
            }
        }

        public Sys_PageUrlEntity GetSys_PageUrlEntity(string PageTitle,int ParentMenuPageId)
        {
            string sql = " SELECT PageId, PageUrl, PageTitle,ParentMenuPageId, ParentPageId,MenuSortNum FROM Sys_PageUrl WHERE PageTitle=@PageTitle AND ParentMenuPageId=@ParentMenuPageId";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<Sys_PageUrlEntity>(sql, new { PageTitle = PageTitle, ParentMenuPageId = ParentMenuPageId });
            }
        }

    }
}
