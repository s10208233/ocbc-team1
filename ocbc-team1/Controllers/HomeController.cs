using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ocbc_team1.DAL;
using ocbc_team1.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.Controllers
{
    public class HomeController : Controller
    {
        private LoginDAL loginContext = new LoginDAL();
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            
            return View();
        }
        public IActionResult Login()
        {
            HttpContext.Session.Clear();
            return View();
        }
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UserLogin(LoginViewModel loginVM) 
        {
            if(ModelState.IsValid)
            {
                LoginDAL loginContext = new LoginDAL();
                List<TestUser> userList = loginContext.LoginList();
                foreach (TestUser user in userList)
                {
                    if (loginVM.AccessCode == user.AccessCode && loginVM.Pin == user.BankPIN)
                    {
                        HttpContext.Session.SetString("login", "true");
                        string userName = user.FirstName + user.LastName;
                        HttpContext.Session.SetString("fullname", userName);
                        return RedirectToAction("Index", "Dashboard");
                    }
                }
                TempData["Message"] = "Invalid Login Credentials!";
                return View();
            }
            else
            {
                //Input validation fails, return to the Login view
                //to display error message
                return View(loginVM);
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Signup(User user)
        {
            if (ModelState.IsValid)
            {
                //check if it stores
                var CardNumber = user.CardNumber;

                //Redirect user to Competitor/Index view
                return RedirectToAction("Login");
            }
            else
            {
                //Input validation fails, return to the Create view
                //to display error message
                return View(user);
            }

        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
