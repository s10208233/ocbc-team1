using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult TransferHistory()
        {
            return View();
        }

        public IActionResult UserLogout()
        {
            HttpContext.Session.Remove("login");
            HttpContext.Session.Remove("fullname");
            return RedirectToAction("Index", "Home");
        }
    }
}
