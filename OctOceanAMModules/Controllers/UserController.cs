using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OctOceanAMModules.DataServices;
using OctOceanAMModules.Entity;

namespace OctOceanAMModules.Controllers
{
    public class UserController : Controller
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int UserId)
        {
            var entity = _userService.GetSys_UserEntity(UserId);
            return View(entity);
        }


        [HttpPost]
        public IActionResult Edit([FromForm]Sys_UserEntity entity)
        {
            //判断是否存在维护的code
            var _tempentity = _userService.GetSys_UserEntity(entity.UserCode);
            //修改操作
            if (entity.UserId > 0)
            {
                if (_tempentity == null || _tempentity.UserId == entity.UserId)
                {
                    if (_userService.UpdateUser(entity) > 0)
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
                    _userService.InsertUser(entity);
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


        public object UsersData(int PageIndex = 1, int PageSize = 10)
        {
            //注意：该语句按照C#7.0进行的out调用
            var data = _userService.GetUsersWithPager(PageIndex, PageSize, out int SumCount);
            return new { code = 0, msg = "", count = SumCount, data = data };

        }

        public async Task<object> Delete(int RoleId)
        {
            //删除角色
            await _userService.DeleteUserAsync(RoleId);
            return new { status = 1 };
            //删除该角色下的权限
        }
    }
}