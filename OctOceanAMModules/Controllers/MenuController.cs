using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OctOceanAMModules.DataServices;
using OctOceanAMModules.Entity;
using OctOceanAMModules.Models;

namespace OctOceanAMModules.Controllers
{
    public class MenuController : Controller
    {
        private readonly PageMenuService _pageMenuService;

        public MenuController(PageMenuService pageMenuService)
        {
            _pageMenuService = pageMenuService;
        }


        private void AddMenu(Sys_PageUrlEntity_Ex entity, Dictionary<int, Sys_PageUrlEntity_Ex> dic, List<MenuViewModel> menuList)
        {

            menuList.Add(new MenuViewModel()
            {
                PageId = entity.PageId,
                PageUrl = entity.Sys_PageUrl.PageUrl,
                ParentId = entity.Sys_PageUrl.ParentMenuPageId,
                PageSortNum = entity.Sys_PageUrl.MenuSortNum,
                PageTitle = entity.Sys_PageUrl.PageTitle,
                Funs = entity.Sys_PageUrl.PageFuns,
                HasChirldPageUrl = (entity.ChirldMenuPageUrls != null && entity.ChirldMenuPageUrls.Count > 0),
                IsFunPage = entity.Sys_PageUrl.IsFunPageStatus

            });

            foreach (Sys_PageUrlEntity _entity in entity.ChirldMenuPageUrls.OrderBy(a => a.MenuSortNum).ToList())
            {
                //如果该页面是父级页面，就继续递归添加子级
                if (dic.ContainsKey(_entity.PageId))
                {
                    AddMenu(dic[_entity.PageId], dic, menuList);
                }
                else
                {
                    //如果不是父级页面，添加本身
                    menuList.Add(new MenuViewModel()
                    {
                        PageId = _entity.PageId,
                        PageUrl = _entity.PageUrl,
                        ParentId = _entity.ParentMenuPageId,
                        PageSortNum = _entity.MenuSortNum,
                        PageTitle = _entity.PageTitle,
                        Funs = _entity.PageFuns,
                        HasChirldPageUrl = false,
                        IsFunPage = _entity.IsFunPageStatus
                    });
                }
            }

        }




        public IActionResult Index()
        {
            List<MenuViewModel> MenuList = new List<MenuViewModel>();
            Dictionary<int, Sys_PageUrlEntity_Ex> dic = DataServices.PageMenuFun.PageMenuAndFunPool.Dic_ParentPageId_ChirldPageMenus;

            foreach (int pageId in dic.Keys)
            {
                //如果一个父级A包含子级B，子级B又包含子级C，那么子级B既存在于key为A的子集合中，同时本身也作为父级存在于Key中，所以此处为了避免重复添加，必须进行是否已经添加判断
                if (MenuList.FirstOrDefault(a => a.PageId == pageId) == null)
                {
                    AddMenu(dic[pageId], dic, MenuList);
                }
            }

            return View(MenuList);
        }
        [HttpGet]
        public IActionResult Edit(int PageId, int ParentId = 0)
        {

            var entity = _pageMenuService.GetSys_PageUrlEntity(PageId);
            if (entity == null)
            {
                entity = new Sys_PageUrlEntity() { ParentMenuPageId = ParentId };
            }
            return View(entity);
        }







        [HttpPost]
        public IActionResult Edit([FromForm]Sys_PageUrlEntity entity, string menufuns)
        {

            //todo:验证输入的父级菜单ID是否有效

            var fs = JsonConvert.DeserializeObject<List<Sys_PageFunEntity>>(menufuns);
            if (fs != null)
            {
                fs.ForEach(f => f.PageId = entity.PageId);
                entity.PageFuns = fs;
            }


            int PageId = entity.PageId;

            //判断是否存在维护的code
            var _tempentity = _pageMenuService.GetSys_PageUrlEntityNotFuns(entity.PageTitle, entity.ParentMenuPageId);
            //修改操作
            if (entity.PageId > 0)
            {
                if (_tempentity == null || _tempentity.PageId == entity.PageId)
                {
                    if (_pageMenuService.UpdateSys_PageUrl(entity))
                    {
                        ViewData["Status"] = 1;
                    }
                    else
                    {
                        ViewData["Status"] = -2;
                    }
                }
                else
                {
                    //已存在rolecode
                    ViewData["Status"] = -1;
                }
            }
            else
            {
                if (_tempentity == null)
                {
                    //新增
                    _pageMenuService.InsertSys_PageUrl(entity, out PageId);
                    ViewData["Status"] = 1;
                }
                else
                {
                    //已存在rolecode
                    ViewData["Status"] = -1;
                }
            }


            return View(_pageMenuService.GetSys_PageUrlEntity(PageId));
        }
 
        public   IActionResult Delete(int PageId)
        {
            //删除角色
            _pageMenuService .DeleteSys_PageUrlAndFuns(PageId);

            return Json(new { status = 1 });
       
        }
    }
}