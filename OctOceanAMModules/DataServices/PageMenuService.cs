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
SELECT FunId,PageId,FunCode,FunName,IsFunMenu,MenuPageId FROM Sys_PageFun;
SELECT PageId,PageUrl,PageTitle,ParentMenuPageId,MenuSortNum FROM Sys_PageUrl  ORDER BY ParentMenuPageId ASC,MenuSortNum ASC;"; //排序必不可少
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                using (var multi = connection.QueryMultiple(sql))
                {
                    dic_pageId_FunEntity = multi.Read<Sys_PageFunEntity>().GroupBy(a => a.PageId).ToDictionary(a => a.Key, b => b.ToList());
                    pageUrlList = multi.Read<Sys_PageUrlEntity>().ToList();
                }
            }

            Dictionary<int, Sys_PageMenuEntity> dic_pageId_menuentity = new Dictionary<int, Sys_PageMenuEntity>();
            foreach (var urlentity in pageUrlList)
            {

                urlentity.PageFuns = dic_pageId_FunEntity.ContainsKey(urlentity.ParentMenuPageId) ? dic_pageId_FunEntity[urlentity.ParentMenuPageId] : null;




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

                                ChirldMenuPageUrls = new List<Sys_PageUrlEntity>()
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

                                ChirldMenuPageUrls = new List<Sys_PageUrlEntity>()
                            });
                    }
                }




            }
            return dic_pageId_menuentity;




        }





        public bool InsertSys_PageUrl(Sys_PageUrlEntity _sys_PageUrlEntity, out int PageId)
        {
            PageId = 0;
            string sql = "INSERT INTO Sys_PageUrl ( PageUrl,PageTitle, ParentMenuPageId,MenuSortNum ) VALUES  (@PageUrl, @PageTitle,@ParentMenuPageId,  @MenuSortNum);SELECT @@IDENTITY;";


            string insertsql = "INSERT INTO Sys_PageFun(PageId, FunCode, FunName, IsFunMenu,MenuPageId) VALUES(@PageId, @FunCode, @FunName, @IsFunMenu,@MenuPageId);";

            using (SqlConnection conn = new SqlConnection(this.SqlConnectionString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        //添加一条主菜单
                        object _objpageId = conn.ExecuteScalar(sql, new
                        {
                            PageUrl = _sys_PageUrlEntity.PageUrl,
                            PageTitle = _sys_PageUrlEntity.PageTitle,
                            ParentMenuPageId = _sys_PageUrlEntity.ParentMenuPageId,
                            MenuSortNum = _sys_PageUrlEntity.MenuSortNum
                        }, tran);

                        int _pageId = Convert.ToInt32(_objpageId);
                        PageId = _pageId;

                        //逐条判断是否需要创建功能菜单，如果需要创建功能菜单，就先创建菜单数据，再创建功能数据
                        if (_sys_PageUrlEntity.PageFuns != null)
                        {
                            foreach (var pue in _sys_PageUrlEntity.PageFuns)
                            {
                                int menupageid = 0;
                                //如果需要关联菜单，就先创建一条功能菜单
                                if (pue.IsFunMenuStatus)
                                {
                                    menupageid = Convert.ToInt32(conn.ExecuteScalar(sql, new
                                    {
                                        PageUrl = "",
                                        PageTitle = $"{pue.FunName}-未配置",
                                        ParentMenuPageId = _pageId,
                                        MenuSortNum = 100
                                    }, tran));
                                }
                                conn.Execute(insertsql, new
                                {
                                    PageId = _pageId,
                                    FunCode = pue.FunCode,
                                    FunName = pue.FunName,
                                    IsFunMenu = pue.IsFunMenuStatus ? 1 : 0,
                                    MenuPageId = menupageid
                                }, tran);

                            }
                        }


                        tran.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                        return false;
                    }



                }

            }
        }

        public bool UpdateSys_PageUrl(Sys_PageUrlEntity _sys_PageUrlEntity)
        {
            //查询出旧的数据
            var oldFunEntityList = GetSys_PageFunEntityList(_sys_PageUrlEntity.PageId);
            var oldfunId = oldFunEntityList.Select(a => a.FunId).ToArray();
            var newfunId = _sys_PageUrlEntity.PageFuns?.Select(b => b.FunId).ToArray();


            string insert_menu_sql = "INSERT INTO Sys_PageUrl ( PageUrl,PageTitle, ParentMenuPageId, MenuSortNum ) VALUES  (@PageUrl, @PageTitle,@ParentMenuPageId,@MenuSortNum);SELECT @@IDENTITY;";
            string update_menu_sql = "UPDATE Sys_PageUrl SET PageUrl=@PageUrl,PageTitle=@PageTitle,ParentMenuPageId=@ParentMenuPageId,MenuSortNum=@MenuSortNum  WHERE PageId=@PageId";
            string delete_menu_sql = "DELETE FROM Sys_PageUrl WHERE PageId=@PageId;";

            string del_fun_sql = "DELETE FROM Sys_PageFun WHERE FunId = @FunId;";
            string insert_fun_sql = "INSERT INTO Sys_PageFun(PageId, FunCode, FunName, IsFunMenu,MenuPageId) VALUES(@PageId, @FunCode, @FunName, @IsFunMenu,@MenuPageId);";
            string update_fun_sql = "UPDATE Sys_PageFun SET PageId = @PageId, FunCode = @FunCode, FunName = @FunName, IsFunMenu = @IsFunMenu,MenuPageId=@MenuPageId WHERE FunId=@FunId;";

            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                using (var tran = connection.BeginTransaction())
                {
                    try
                    {
                        //先执行主菜单的修改操作
                        connection.Execute(update_menu_sql, new
                        {
                            PageId = _sys_PageUrlEntity.PageId,
                            PageTitle = _sys_PageUrlEntity.PageTitle,
                            PageUrl = _sys_PageUrlEntity.PageUrl,
                            ParentMenuPageId = _sys_PageUrlEntity.ParentMenuPageId,
                            MenuSortNum = _sys_PageUrlEntity.MenuSortNum
                        }, tran);


                        //执行功能菜单的删除操作//查询出旧的在新的不存在的funid，即要删除的FunId
                        connection.Execute(del_fun_sql, oldfunId.Where(c => newfunId.Contains(c) == false).Select(id => new { FunId = id }), tran);

                        //遍历新的数据，判断是修改操作还是新增操作
                        if (_sys_PageUrlEntity.PageFuns != null)
                        {
                            foreach (var pue in _sys_PageUrlEntity.PageFuns)
                            {
                                //如果是修改操作
                                if (oldfunId.Contains(pue.FunId))
                                {
                                    var oldfe = oldFunEntityList.First(o => o.FunId == pue.FunId);
                                    if (pue.IsFunMenuStatus)
                                    {
                                        //如果新的有关联菜单，旧的也有
                                        if (oldfe.IsFunMenuStatus)
                                        {
                                            //直接执行update
                                            connection.Execute(update_fun_sql, new
                                            {
                                                PageId = _sys_PageUrlEntity.PageId,
                                                FunCode = pue.FunCode,
                                                FunName = pue.FunName,
                                                IsFunMenu = 1,
                                                FunId = pue.FunId,
                                                MenuPageId = oldfe.MenuPageId
                                            }, tran);
                                        }
                                        else
                                        {
                                            //如果新的有关联，旧的没有关联，先创建关联菜单
                                            //添加一条主菜单
                                            int _menupageId = Convert.ToInt32(connection.ExecuteScalar(insert_menu_sql, new
                                            {
                                                PageUrl = "",
                                                PageTitle = $"{pue.FunName}-未配置",
                                                ParentMenuPageId = _sys_PageUrlEntity.PageId,
                                                MenuSortNum = 100
                                            }, tran));
                                            //修改新的菜单
                                            connection.Execute(update_fun_sql, new
                                            {
                                                PageId = _sys_PageUrlEntity.PageId,
                                                FunCode = pue.FunCode,
                                                FunName = pue.FunName,
                                                IsFunMenu = 1,
                                                FunId = pue.FunId,
                                                MenuPageId = _menupageId
                                            }, tran);


                                        }
                                    }
                                    else
                                    {
                                        //如果新的没有关联，旧的有关联
                                        if (oldfe.IsFunMenuStatus)
                                        {
                                            //删除旧的关联菜单
                                            connection.Execute(delete_menu_sql, new { PageId = oldfe.MenuPageId }, tran);
                                        }
                                        //修改fun
                                        connection.Execute(update_fun_sql, new
                                        {
                                            PageId = _sys_PageUrlEntity.PageId,
                                            FunCode = pue.FunCode,
                                            FunName = pue.FunName,
                                            IsFunMenu = 0,
                                            FunId = pue.FunId,
                                            MenuPageId = 0
                                        }, tran);
                                    }
                                }
                                else
                                {
                                    //如果是新增操作
                                    int _pageid = 0;

                                    if (pue.IsFunMenuStatus)
                                    {
                                        //如果有关联菜单，先添加一条关联菜单
                                        _pageid = Convert.ToInt32(connection.ExecuteScalar(insert_menu_sql, new
                                        {
                                            PageUrl = "",
                                            PageTitle = $"{pue.FunName}-未配置",
                                            ParentMenuPageId = _sys_PageUrlEntity.PageId,
                                            MenuSortNum = 100
                                        }, tran));
                                    }

                                    //添加新的fun
                                    connection.Execute(insert_fun_sql, new
                                    {
                                        PageId = _sys_PageUrlEntity.PageId,
                                        FunCode = pue.FunCode,
                                        FunName = pue.FunName,
                                        IsFunMenu = pue.IsFunMenuStatus ? 1 : 0,
                                        MenuPageId = _pageid
                                    }, tran);
                                }
                            }
                        }




                        tran.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        return false;
                    }




                }



            }
        }


        public async Task<int> DeleteSys_PageUrlAsync(int PageId)
        {
            string sql = "DELETE FROM Sys_PageUrl  WHERE PageId=@PageId Or ParentMenuPageId=@PageId ";
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
            Sys_PageUrlEntity entity = null;
            IList<Sys_PageFunEntity> funs = null;

            string sql = @" 
SELECT PageId, PageUrl, PageTitle,ParentMenuPageId,MenuSortNum FROM Sys_PageUrl WHERE PageId=@PageId;
SELECT FunId,PageId,FunCode,FunName,IsFunMenu,MenuPageId FROM Sys_PageFun WHERE PageId=@PageId;";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();

                using (var multi = connection.QueryMultiple(sql, new { PageId = PageId }))
                {
                    entity = multi.ReadFirstOrDefault<Sys_PageUrlEntity>();
                    if (entity == null)
                        return null;
                    funs = multi.Read<Sys_PageFunEntity>().ToList();
                    entity.PageFuns = funs;
                    return entity;
                }

            }
        }



        public Sys_PageUrlEntity GetSys_PageUrlEntity(string PageTitle, int ParentMenuPageId)
        {
            string sql = " SELECT PageId, PageUrl, PageTitle,ParentMenuPageId,MenuSortNum FROM Sys_PageUrl WHERE PageTitle=@PageTitle AND ParentMenuPageId=@ParentMenuPageId";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<Sys_PageUrlEntity>(sql, new { PageTitle = PageTitle, ParentMenuPageId = ParentMenuPageId });
            }
        }



        public List<Sys_PageFunEntity> GetSys_PageFunEntityList(int PageId)
        {
            string sql = "SELECT FunId,PageId,FunCode,FunName,IsFunMenu,MenuPageId FROM Sys_PageFun WHERE PageId=@PageId;";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.Query<Sys_PageFunEntity>(sql, new { PageId = PageId }).ToList();

            }
        }
    }
}
