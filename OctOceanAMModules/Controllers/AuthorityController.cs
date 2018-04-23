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

namespace OctOceanAMModules.Controllers
{
    public class AuthorityController : Controller
    {
        private readonly PageMenuService _pageMenuService;
        public AuthorityController(PageMenuService pageMenuService)
        {
            _pageMenuService = pageMenuService;
        }



        private void AddMenu(Sys_PageMenuEntity entity, Dictionary<int, Sys_PageMenuEntity> dic, List<AuthorityJsonData> authorityvms)
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

    
             

            //先添加菜单
            authorityvms.Add(new AuthorityJsonData { Id = vm.PageId.ToString(), PId = vm.ParentId.ToString(), Name = vm.PageTitle, Checked = false });
            if (vm.Funs != null && vm.Funs.Any())
            {
                foreach (var f in vm.Funs)
                {
                    if (!f.IsFunMenuStatus)
                    {
                        authorityvms.Add(new AuthorityJsonData { Id = "f_" + f.FunId, PId = f.PageId.ToString(), Name = f.FunName, Checked = false });
                    }

                }
            }



            foreach (var _entity in entity.ChirldMenuPageUrls.OrderBy(a => a.MenuSortNum).ToList())
            {
                //如果该页面是父级页面，就继续递归添加子级
                if (dic.ContainsKey(_entity.PageId))
                {
                    AddMenu(dic[_entity.PageId], dic, authorityvms);
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
            
                    

                    authorityvms.Add(new AuthorityJsonData { Id = vm2.PageId.ToString(), PId = vm2.ParentId.ToString(), Name = vm2.PageTitle, Checked = false  });
                    if (vm2.Funs != null && vm2.Funs.Any())
                    {
                        foreach (var f in vm2.Funs)
                        {
                            if (!f.IsFunMenuStatus)
                            {
                                authorityvms.Add(new AuthorityJsonData { Id = "f_" + f.FunId, PId = f.PageId.ToString(), Name = f.FunName, Checked = false});
                            }
                        }
                    }
                }
            }


        }


        public IActionResult Index()
        {

            List<AuthorityJsonData> authoritydataList = new List<AuthorityJsonData>();
            Dictionary<int, Sys_PageMenuEntity> dic = _pageMenuService.getSys_PageMenuEntityDic();

            foreach (int pageId in dic.Keys)
            {
                //如果一个父级A包含子级B，子级B又包含子级C，那么子级B既存在于key为A的子集合中，同时本身也作为父级存在于Key中，所以此处为了避免重复添加，必须进行是否已经添加判断
                if (authoritydataList.FirstOrDefault(a => a.PId == pageId.ToString()) == null)
                {
                    AddMenu(dic[pageId], dic, authoritydataList);
                }
            }

            AuthorityViewModel vm = new AuthorityViewModel();
            //使用骆驼峰命名属性，注意此行很重要，因为checked是C#关键字，只能使用Checked，然后调用下述代码自动转换成小写，而不能直接使用checked
            vm.AllAuthorityData = JsonConvert.SerializeObject(authoritydataList, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            return View(vm);








        }
    }
}