using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OctOceanAMModules.DataServices;
using OctOceanAMModules.Entity;
using OctOceanAMModules.Models;

namespace OctOceanAMModules.Controllers
{
    public class HomeController : Controller
    {
        private readonly RoleService roleService;
        public HomeController(RoleService _roleService)
        {
            this.roleService =  _roleService;
        }
        public IActionResult Index()
        {
            return View();
        }

      

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
