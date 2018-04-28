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

        public bool InsertSys_PageUrl(Sys_PageUrlEntity _sys_PageUrlEntity, out int PageId)
        {
            PageId = 0;
            string sql = "INSERT INTO Sys_PageUrl ( PageUrl,PageTitle, ParentMenuPageId,MenuSortNum,IsFunPage ) VALUES  (@PageUrl, @PageTitle,@ParentMenuPageId,  @MenuSortNum,@IsFunPage);SELECT @@IDENTITY;";


            string insertsql = "INSERT INTO Sys_PageFun(PageId, FunCode, FunName, HasNewPage,NewPageId) VALUES(@PageId, @FunCode, @FunName, @HasNewPage,@NewPageId);";

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
                            MenuSortNum = _sys_PageUrlEntity.MenuSortNum,
                            IsFunPage = _sys_PageUrlEntity.IsFunPage
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
                                        MenuSortNum = 100,
                                        IsFunPage = 1
                                    }, tran));
                                }
                                conn.Execute(insertsql, new
                                {
                                    PageId = _pageId,
                                    FunCode = pue.FunCode,
                                    FunName = pue.FunName,
                                    HasNewPage = pue.IsFunMenuStatus ? 1 : 0,
                                    NewPageId = menupageid
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
            var oldpageurlentity = GetSys_PageUrlEntity(_sys_PageUrlEntity.PageId);
            var oldfunId = oldpageurlentity.PageFuns.Select(a => a.FunId).ToArray();
            var newfunId = _sys_PageUrlEntity.PageFuns?.Select(b => b.FunId).ToArray();


            string insert_menu_sql = "INSERT INTO Sys_PageUrl ( PageUrl,PageTitle, ParentMenuPageId, MenuSortNum,IsFunPage ) VALUES  (@PageUrl, @PageTitle,@ParentMenuPageId,@MenuSortNum,@IsFunPage);SELECT @@IDENTITY;";
            string update_menu_sql = "UPDATE Sys_PageUrl SET PageUrl=@PageUrl,PageTitle=@PageTitle,ParentMenuPageId=@ParentMenuPageId,MenuSortNum=@MenuSortNum,IsFunPage=@IsFunPage  WHERE PageId=@PageId";
            string delete_menu_sql = "DELETE FROM Sys_PageUrl WHERE PageId=@PageId Or ParentMenuPageId=@PageId; DELETE FROM Sys_PageFun WHERE PageId=@PageId;";

            string del_fun_sql = "DELETE FROM Sys_PageFun WHERE FunId = @FunId;";
            string insert_fun_sql = "INSERT INTO Sys_PageFun(PageId, FunCode, FunName, HasNewPage,NewPageId) VALUES(@PageId, @FunCode, @FunName, @HasNewPage,@NewPageId);";
            string update_fun_sql = "UPDATE Sys_PageFun SET PageId = @PageId, FunCode = @FunCode, FunName = @FunName, HasNewPage = @HasNewPage,NewPageId=@NewPageId WHERE FunId=@FunId;";

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
                            MenuSortNum = _sys_PageUrlEntity.MenuSortNum,
                            IsFunPage = oldpageurlentity.IsFunPageStatus ? 1 : 0
                        }, tran);

                        if (oldpageurlentity.PageFuns != null)
                        {
                            //执行功能菜单的删除操作//查询出旧的在新的不存在的funid，即要删除的FunId
                            foreach (var oldpf in oldpageurlentity.PageFuns)
                            {
                                //如果新的不存在旧的数据，就先删除旧的
                                if (!newfunId.Contains(oldpf.FunId))
                                {
                                    //判断是否是功能链接
                                    if (oldpf.IsFunMenuStatus)
                                    {
                                        //先删除链接数据
                                        connection.Execute(delete_menu_sql, new { PageId = oldpf.NewPageId }, tran);
                                    }

                                    //再删除本身
                                    connection.Execute(del_fun_sql, new { FunId = oldpf.FunId }, tran);
                                }
                            }
                        }



                        //遍历新的数据，判断是修改操作还是新增操作
                        if (_sys_PageUrlEntity.PageFuns != null)
                        {
                            foreach (var pue in _sys_PageUrlEntity.PageFuns)
                            {
                                //如果是修改操作
                                if (oldfunId.Contains(pue.FunId))
                                {
                                    var oldfe = oldpageurlentity.PageFuns.First(o => o.FunId == pue.FunId);
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
                                                HasNewPage = 1,
                                                FunId = pue.FunId,
                                                NewPageId = oldfe.NewPageId
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
                                                MenuSortNum = 100,
                                                IsFunPage = 1
                                            }, tran));
                                            //修改新的菜单
                                            connection.Execute(update_fun_sql, new
                                            {
                                                PageId = _sys_PageUrlEntity.PageId,
                                                FunCode = pue.FunCode,
                                                FunName = pue.FunName,
                                                HasNewPage = 1,
                                                FunId = pue.FunId,
                                                NewPageId = _menupageId
                                            }, tran);


                                        }
                                    }
                                    else
                                    {
                                        //如果新的没有关联，旧的有关联
                                        if (oldfe.IsFunMenuStatus)
                                        {
                                            //删除旧的关联菜单
                                            connection.Execute(delete_menu_sql, new { PageId = oldfe.NewPageId }, tran);
                                        }
                                        //修改fun
                                        connection.Execute(update_fun_sql, new
                                        {
                                            PageId = _sys_PageUrlEntity.PageId,
                                            FunCode = pue.FunCode,
                                            FunName = pue.FunName,
                                            HasNewPage = 0,
                                            FunId = pue.FunId,
                                            NewPageId = 0
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
                                            MenuSortNum = 100,
                                            IsFunPage = 1
                                        }, tran));
                                    }

                                    //添加新的fun
                                    connection.Execute(insert_fun_sql, new
                                    {
                                        PageId = _sys_PageUrlEntity.PageId,
                                        FunCode = pue.FunCode,
                                        FunName = pue.FunName,
                                        HasNewPage = pue.IsFunMenuStatus ? 1 : 0,
                                        NewPageId = _pageid
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


        public bool DeleteSys_PageUrlAndFuns(int PageId)
        {
            //获取当前页面和当前页面下的所有子页面以及页面中包含的功能集合
            Tuple<int[], int[]> pagesandfuns = PageMenuFun.PageMenuAndFunPool.GetChildPageAndAllFunsByPageId(PageId);
            int[] pageids = pagesandfuns.Item1;
            int[] funs = pagesandfuns.Item2;
            string del_page_sql = "DELETE FROM Sys_PageUrl  WHERE PageId=@PageId ";
            string del_fun_sql = "DELETE FROM Sys_PageFun WHERE FunId=@FunId ";
            using (var conn = new SqlConnection(SqlConnectionString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        //删除相关的功能
                        conn.Execute(del_fun_sql, funs.Select(b => new { FunId = b }), tran);
                        //删除旧的关联菜单
                        conn.Execute(del_page_sql, pageids.Select(a => new { PageId = a }), tran);
                       


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


        public Sys_PageUrlEntity GetSys_PageUrlEntity(int PageId)
        {
            Sys_PageUrlEntity entity = null;
            IList<Sys_PageFunEntity> funs = null;

            string sql = @" 
SELECT PageId, PageUrl, PageTitle,ParentMenuPageId,MenuSortNum,IsFunPage FROM Sys_PageUrl WHERE PageId=@PageId;
SELECT FunId,PageId,FunCode,FunName,HasNewPage,NewPageId FROM Sys_PageFun WHERE PageId=@PageId;";
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



        public Sys_PageUrlEntity GetSys_PageUrlEntityNotFuns(string PageTitle, int ParentMenuPageId)
        {
            string sql = " SELECT PageId, PageUrl, PageTitle,ParentMenuPageId,MenuSortNum,IsFunPage FROM Sys_PageUrl WHERE PageTitle=@PageTitle AND ParentMenuPageId=@ParentMenuPageId";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<Sys_PageUrlEntity>(sql, new { PageTitle = PageTitle, ParentMenuPageId = ParentMenuPageId });
            }
        }



        public List<Sys_PageFunEntity> GetSys_PageFunEntityList(int PageId)
        {
            string sql = "SELECT FunId,PageId,FunCode,FunName,HasNewPage,NewPageId FROM Sys_PageFun WHERE PageId=@PageId;";
            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                return connection.Query<Sys_PageFunEntity>(sql, new { PageId = PageId }).ToList();

            }
        }
    }
}
