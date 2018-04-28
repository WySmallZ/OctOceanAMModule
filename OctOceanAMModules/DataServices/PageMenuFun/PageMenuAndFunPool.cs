using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using OctOceanAMModules.Entity;
using Dapper;

namespace OctOceanAMModules.DataServices.PageMenuFun
{
    public class PageMenuAndFunPool
    {
        public static string SqlConnectionString = string.Empty;

        //存储每一个页面对应的所有Funs
        private static Dictionary<int, List<Sys_PageFunEntity>> Dic_PageId_Funs = null;

        //存储拥有子页面的父级页面信息，该键为拥有子页面的父级PageId 。如果没有子页面的话，就不会存储
        public static Dictionary<int, Sys_PageUrlEntity_Ex> Dic_ParentPageId_ChirldPageMenus = null;

        //存储每一个PageUrlEntity，PageId作为键，方便快速根据PageId，获取对应的信息
        public static Dictionary<int, Sys_PageUrlEntity> Dic_PageId_PageUrlEntity = null;




        public static void RefreshAllMenuAndFunData()
        {
            //每一个page对应的所有funs
            Dic_PageId_Funs = new Dictionary<int, List<Sys_PageFunEntity>>();
            Dic_PageId_PageUrlEntity = new Dictionary<int, Sys_PageUrlEntity>();
            //定义一个键值对，该键为拥有子页面的父级PageId 
            Dic_ParentPageId_ChirldPageMenus = new Dictionary<int, Sys_PageUrlEntity_Ex>();



            List<Sys_PageUrlEntity> _pageUrlList = new List<Sys_PageUrlEntity>();
            //查询出两个表的数据
            string sql = @"
SELECT FunId,PageId,FunCode,FunName,HasNewPage,NewPageId FROM Sys_PageFun;
SELECT PageId,PageUrl,PageTitle,ParentMenuPageId,MenuSortNum,IsFunPage FROM Sys_PageUrl  ORDER BY ParentMenuPageId ASC,MenuSortNum ASC;"; //排序必不可少


            using (var connection = new SqlConnection(SqlConnectionString))
            {
                connection.Open();
                using (var multi = connection.QueryMultiple(sql))
                {
                    Dic_PageId_Funs = multi.Read<Sys_PageFunEntity>().GroupBy(a => a.PageId).ToDictionary(a => a.Key, b => b.ToList());
                    _pageUrlList = multi.Read<Sys_PageUrlEntity>().ToList();
                }
            }




            //遍历每一个url
            foreach (Sys_PageUrlEntity urlentity in _pageUrlList)
            {
                urlentity.PageFuns = Dic_PageId_Funs.ContainsKey(urlentity.PageId) ? Dic_PageId_Funs[urlentity.PageId] : null;
                Dic_PageId_PageUrlEntity[urlentity.PageId] = urlentity;



                if (urlentity.ParentMenuPageId > 0)
                {
                    //如果不是根链接，是子链接，因为上面排序规则，实际执行时，并不会走该if中的语句
                    if (!Dic_ParentPageId_ChirldPageMenus.ContainsKey(urlentity.ParentMenuPageId))
                    {
                        //查找根链接
                        var pe = _pageUrlList.FirstOrDefault(u => u.PageId == urlentity.ParentMenuPageId);
                        if (pe != null)
                        {
                            //集合中的所有pagefuns可能没有重新赋值完，因此在此处为了数据的准确性，重新取最新的
                            pe.PageFuns = Dic_PageId_Funs.ContainsKey(pe.PageId) ? Dic_PageId_Funs[pe.PageId] : null;

                            Dic_ParentPageId_ChirldPageMenus.Add(urlentity.ParentMenuPageId
                            , new Sys_PageUrlEntity_Ex()
                            {
                                PageId = urlentity.ParentMenuPageId,
                                Sys_PageUrl = pe,
                                ////如果该页是功能页,找出该功能页所在的父级页面中的所有功能
                                ChirldMenuPageUrls = new List<Sys_PageUrlEntity>()
                            });

                            Dic_ParentPageId_ChirldPageMenus[urlentity.ParentMenuPageId].ChirldMenuPageUrls.Add(urlentity);
                        }
                    }
                    else
                    {
                        Dic_ParentPageId_ChirldPageMenus[urlentity.ParentMenuPageId].ChirldMenuPageUrls.Add(urlentity);
                    }

                }
                else
                {
                    //如果是根链接
                    if (!Dic_ParentPageId_ChirldPageMenus.ContainsKey(urlentity.PageId))
                    {
                        Dic_ParentPageId_ChirldPageMenus.Add(urlentity.PageId
                            , new Sys_PageUrlEntity_Ex()
                            {
                                PageId = urlentity.PageId,
                                Sys_PageUrl = urlentity,
                                ChirldMenuPageUrls = new List<Sys_PageUrlEntity>()
                            });
                    }
                }

            }




        }



        /// <summary>
        /// 根据功能页面的Id获取该功能Id
        /// </summary>
        /// <param name="FunPageId"></param>
        /// <returns></returns>
        public static int GetFunIdByFunPage(int FunPageId)
        {
            if (Dic_PageId_PageUrlEntity.ContainsKey(FunPageId))
            {

                //获取页面的上级PageId
                int parentpageid = Dic_PageId_PageUrlEntity[FunPageId].ParentMenuPageId;
                //获取上级页面中的所有功能
                if (Dic_PageId_Funs.ContainsKey(parentpageid))
                {
                    var fun = Dic_PageId_Funs[parentpageid].FirstOrDefault(a => a.IsFunMenuStatus && a.NewPageId == FunPageId);
                    if (fun != null)
                    {
                        return fun.FunId;
                    }
                }

            }
            return 0;



        }




        private static void GetChildPageAndAllFunsBySys_PageUrlEntity_Ex(Sys_PageUrlEntity_Ex spe, List<int> allChildPageIds, List<int> allFunIds)
        {
            //添加当前页面
            allChildPageIds.Add(spe.PageId);
            //添加页面中的功能
            if (spe.Sys_PageUrl.PageFuns != null && spe.Sys_PageUrl.PageFuns.Any())
            {
                allFunIds.AddRange(spe.Sys_PageUrl.PageFuns.Select(a => a.FunId));
            }
            if (spe.ChirldMenuPageUrls != null && spe.ChirldMenuPageUrls.Any())
            {
                foreach (Sys_PageUrlEntity _s in spe.ChirldMenuPageUrls)
                {
                    GetChildPageAndAllFunsBySys_PageUrlEntity_Ex(
                        Dic_ParentPageId_ChirldPageMenus[_s.PageId],
                        allChildPageIds,
                        allFunIds);
                }

            }
        }

        public static Tuple<int [],int []> GetChildPageAndAllFunsByPageId(int PageId)
        {
            if (Dic_ParentPageId_ChirldPageMenus == null)
            {
                RefreshAllMenuAndFunData();
            }
            List<int> AllPageIds = new List<int>();
            List<int> AllFunIds = new List<int>();
            GetChildPageAndAllFunsBySys_PageUrlEntity_Ex(Dic_ParentPageId_ChirldPageMenus[PageId], AllPageIds, AllFunIds);

            return new Tuple<int[], int[]>(AllPageIds.Distinct().ToArray(), AllFunIds.Distinct().ToArray());



        }


    }
}
