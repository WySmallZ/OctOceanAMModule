using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OctOceanAMModules.DataServices;
using OctOceanAMModules.Entity;
using OctOceanAMModules.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OctOceanAMModules.DataServices.PageMenuFun;
using OctOceanAMModules.Utils;

namespace OctOceanAMModules.Controllers
{
    public class AuthorityController : Controller
    {
        private readonly RoleService _roleService;
        public AuthorityController(RoleService roleService)
        {
            _roleService = roleService;
        }







        private void ShowMenuAndFuns(Sys_PageUrlEntity_Ex entity, List<TreeJsonData> treeDataList, Dictionary<int, int[]> dic_RolePageFuns)
        {
            MenuViewModel vm = new MenuViewModel()
            {
                PageId = entity.PageId,
                PageUrl = entity.Sys_PageUrl.PageUrl,
                ParentId = entity.Sys_PageUrl.ParentMenuPageId,
                PageSortNum = entity.Sys_PageUrl.MenuSortNum,
                PageTitle = entity.Sys_PageUrl.PageTitle,
                Funs = entity.Sys_PageUrl.PageFuns,
                HasChirldPageUrl = (entity.ChirldMenuPageUrls != null && entity.ChirldMenuPageUrls.Count > 0),
                IsFunPage = entity.Sys_PageUrl.IsFunPageStatus

            };


            //先添加菜单，注意：菜单的FunId是当前页面的FunId，也就是如果当前页面是功能页面，该值才有效，值为FunId
            treeDataList.Add(new TreeJsonData
            {
                Id = vm.PageId.ToString(),
                FunId = PageMenuAndFunPool.GetFunIdByFunPage(vm.PageId).ToString(),
                PId = vm.ParentId.ToString(),
                Name = vm.PageTitle,
                Checked = dic_RolePageFuns != null && dic_RolePageFuns.ContainsKey(vm.PageId),
                IconSkin = vm.IsFunPage ? "fun" : ((vm.HasFuns || (!vm.HasChirldPageUrl)) ? "page" : "folder"),
                IsFun = vm.IsFunPage,
                IsFunPage = vm.IsFunPage
            });

            if (vm.Funs != null && vm.Funs.Any())
            {
                foreach (var f in vm.Funs)
                {

                    if (!f.IsFunMenuStatus)
                    {
                        treeDataList.Add(new TreeJsonData
                        {
                            Id = "f_" + f.FunId,
                            FunId = f.FunId.ToString(),
                            PId = f.PageId.ToString(),
                            Name = f.FunName,
                            Checked = dic_RolePageFuns != null && dic_RolePageFuns.ContainsKey(f.PageId) && dic_RolePageFuns[f.PageId].Contains(f.FunId),
                            IconSkin = "fun",
                            IsFun = true,
                            IsFunPage = false
                        });
                    }

                }
            }

            foreach (Sys_PageUrlEntity _entity in entity.ChirldMenuPageUrls.OrderBy(a => a.MenuSortNum).ToList())
            {
                //如果该页面是父级页面，就继续递归添加子级
                if (DataServices.PageMenuFun.PageMenuAndFunPool.Dic_ParentPageId_ChirldPageMenus.ContainsKey(_entity.PageId))
                {
                    ShowMenuAndFuns(DataServices.PageMenuFun.PageMenuAndFunPool.Dic_ParentPageId_ChirldPageMenus[_entity.PageId], treeDataList, dic_RolePageFuns);
                }
                else
                {
                    //如果不是父级页面，添加本身
                    MenuViewModel vm2 = new MenuViewModel()
                    {
                        PageId = _entity.PageId,
                        PageUrl = _entity.PageUrl,
                        ParentId = _entity.ParentMenuPageId,
                        PageSortNum = _entity.MenuSortNum,
                        PageTitle = _entity.PageTitle,
                        Funs = _entity.PageFuns,
                        HasChirldPageUrl = false,
                        IsFunPage = _entity.IsFunPageStatus


                    };

                    treeDataList.Add(new TreeJsonData
                    {
                        Id = vm2.PageId.ToString(),
                        FunId = PageMenuAndFunPool.GetFunIdByFunPage(vm2.PageId).ToString(),
                        PId = vm2.ParentId.ToString(),
                        Name = vm2.PageTitle,
                        Checked = dic_RolePageFuns != null && dic_RolePageFuns.ContainsKey(vm2.PageId),
                        IconSkin = vm2.IsFunPage ? "fun" : ((vm2.HasFuns || (!vm2.HasChirldPageUrl)) ? "page" : "folder"),
                        IsFun = vm2.IsFunPage,
                        IsFunPage = vm2.IsFunPage
                    });
                    if (vm2.Funs != null && vm2.Funs.Any())
                    {
                        foreach (var f in vm2.Funs)
                        {

                            if (!f.IsFunMenuStatus)
                            {
                                treeDataList.Add(new TreeJsonData
                                {
                                    Id = "f_" + f.FunId,
                                    FunId = f.FunId.ToString(),
                                    PId = f.PageId.ToString(),
                                    Name = f.FunName,
                                    Checked = dic_RolePageFuns != null && dic_RolePageFuns.ContainsKey(f.PageId) && dic_RolePageFuns[f.PageId].Contains(f.FunId),
                                    IconSkin = "fun",
                                    IsFun = true,
                                    IsFunPage = false
                                });
                            }
                        }
                    }
                }
            }


        }


        public IActionResult Index(int RoleId)
        {
            //获取该角色现有的权限
            var roleentity = _roleService.GetSys_RoleEntity(RoleId);
            if (roleentity == null)
            {
                return NotFound();
            }




            Dictionary<int, int[]> dic_RolePageFuns = null;


            if (!string.IsNullOrWhiteSpace(roleentity.Authorities))
            {
                var rajds = JsonConvert.DeserializeObject<RoleAuthrotyJsonData[]>(roleentity.Authorities);
                dic_RolePageFuns = rajds.ToDictionary(a => a.P, v => v.Fs);

            }


            List<TreeJsonData> authoritydataList = new List<TreeJsonData>();


            foreach (int pageId in DataServices.PageMenuFun.PageMenuAndFunPool.Dic_ParentPageId_ChirldPageMenus.Keys)
            {
                //如果一个父级A包含子级B，子级B又包含子级C，那么子级B既存在于key为A的子集合中，同时本身也作为父级存在于Key中，所以此处为了避免重复添加，必须进行是否已经添加判断
                if (authoritydataList.FirstOrDefault(a => a.PId == pageId.ToString()) == null)
                {
                    ShowMenuAndFuns(DataServices.PageMenuFun.PageMenuAndFunPool.Dic_ParentPageId_ChirldPageMenus[pageId], authoritydataList,dic_RolePageFuns);
                }
            }

            AuthorityViewModel vm = new AuthorityViewModel();
            //使用骆驼峰命名属性，注意此行很重要，因为checked是C#关键字，只能使用Checked，然后调用下述代码自动转换成小写，而不能直接使用checked
            vm.AllAuthorityData = JsonConvert.SerializeObject(authoritydataList, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            vm.RoleId = RoleId;
            return View(vm);


        }

        [HttpPost]
        public IActionResult Index([FromForm]string checkedData, int roleId)
        {
            var roleentity = _roleService.GetSys_RoleEntity(roleId);
            if (roleentity == null)
            {
                return NotFound();
            }


            var data = JsonConvert.DeserializeObject<AuthorityJsonData[]>(checkedData);

            Dictionary<int, List<int>> DicPageFuns = new Dictionary<int, List<int>>();

            foreach (AuthorityJsonData ajd in data)
            {
                //如果是功能页面
                if (ajd.IsFunPage)
                {
                    //添加该页面的父级页面和该页面
                    int parentid = ConvertHelper.ToInt32(ajd.PId);
                    int pageid = ConvertHelper.ToInt32(ajd.Id);
                    if (parentid > 0)
                    {
                        if (!DicPageFuns.ContainsKey(parentid))
                        {
                            DicPageFuns.Add(parentid, new List<int>());
                        }
                        if (ConvertHelper.ToInt32(ajd.FunId) > 0)
                        {
                            DicPageFuns[parentid].Add(ConvertHelper.ToInt32(ajd.FunId));
                        }
                    }

                    if (!DicPageFuns.ContainsKey(pageid))
                    {
                        DicPageFuns.Add(pageid, new List<int>());
                    }

                }
                else
                {
                    //如果是按钮
                    if (ajd.IsFun)
                    {
                        int pageid = ConvertHelper.ToInt32(ajd.PId);
                        if (pageid > 0)
                        {
                            if (!DicPageFuns.ContainsKey(pageid))
                            {
                                DicPageFuns.Add(pageid, new List<int>());
                            }
                            if (ConvertHelper.ToInt32(ajd.FunId) > 0)
                            {
                                DicPageFuns[pageid].Add(ConvertHelper.ToInt32(ajd.FunId));
                            }
                        }
                    }
                    else
                    {
                        //不是按钮，就是普通页面
                        if (!DicPageFuns.ContainsKey(ConvertHelper.ToInt32(ajd.Id)))
                        {
                            DicPageFuns.Add(ConvertHelper.ToInt32(ajd.Id), new List<int>());
                        }
                    }
                }

            }

            var obj = DicPageFuns.Select(a => new RoleAuthrotyJsonData { P = a.Key, Fs = a.Value.ToArray() });

            string str = JsonConvert.SerializeObject(obj);

            roleentity.Authorities = str;
            _roleService.UpdateRole(roleentity);


            return Json(new { status = true });
        }
    }
}