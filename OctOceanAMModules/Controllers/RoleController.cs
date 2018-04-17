using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OctOceanAMModules.DataServices;
using OctOceanAMModules.Entity;

namespace OctOceanAMModules.Controllers
{
    public class RoleController : Controller
    {
        private readonly RoleService _roleService;
        public RoleController(RoleService roleService)
        {
            _roleService = roleService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int RoleId)
        {
            var entity= _roleService.GetSys_RoleEntity(RoleId);
            return View(entity);
        }

        [HttpPost]
        public IActionResult Edit([FromForm]Sys_RoleEntity entity)
        {
            //判断是否存在维护的code
            var _tempentity = _roleService.GetSys_RoleEntity(entity.RoleCode);
            //修改操作
            if (entity.RoleId > 0)
            {
                if (_tempentity == null || _tempentity.RoleId == entity.RoleId)
                {
                    if (_roleService.UpdateRole(entity) > 0)
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
                    _roleService.InsertRole(entity);
                    ViewData["Status"] = 1;
                }
                else
                {
                    //已存在rolecode
                    ViewData["Status"] = -1;
                }
            }

             
            return View();
        }

        public object RolesData(int PageIndex = 1, int PageSize = 10)
        {
            //注意：该语句按照C#7.0进行的out调用
          var data=  _roleService.GetRolesWithPager(PageIndex, PageSize, out int SumCount);
            return new { code = 0, msg = "", count = SumCount, data = data };

        }

        public async Task<object> Delete(int RoleId)
        {
            //删除角色
            await _roleService.DeleteRoleAsync(RoleId);
            return new { status = 1 };
            //删除该角色下的权限
        }
    }
}