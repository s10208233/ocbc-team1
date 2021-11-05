using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ocbc_team1.DAL;
using ocbc_team1.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
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
        public IActionResult Login(LoginViewModel loginVM) 
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
                        string userName = user.FirstName + " " + user.LastName;
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
        public IActionResult Signup(SignUpModel user)
        {
            if (ModelState.IsValid)
            {
                //check if it stores
                var CardNumber = user.CardNumber;

                //Redirect user to Competitor/Index view
                HttpContext.Session.SetString("email", user.Email);
                string userName = user.FirstName + " " + user.LastName;
                HttpContext.Session.SetString("fullname", userName);
                int accessCode = CreateAccessCode();
                sendAccessCode(accessCode);
                return RedirectToAction("Login");
            }
            else
            {
                //Input validation fails, return to the Create view
                //to display error message
                return View(user);
            }

        }
        public int CreateAccessCode()
        {
            Random rnd = new Random();
            int value = rnd.Next(100000, 999999);
            return CheckAccessCode(value);
            
        }
        public int CheckAccessCode(int value)
        {
            List<TestUser> userList = loginContext.LoginList();
            int nAccessCode = 0;
            foreach (TestUser user in userList)
            {
                if (value == Convert.ToInt32(user.AccessCode))
                {
                    Random rnd = new Random();
                    nAccessCode = rnd.Next(100000, 999999);
                    CheckAccessCode(nAccessCode);                    
                }
                else
                {
                    nAccessCode = value;
                }
            }
            return nAccessCode;
        }
        public void sendAccessCode(int value)
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("OCBC Team 1", "ocbcteam1@gmail.com"));
            string rEmail = HttpContext.Session.GetString("email");
            string rName = HttpContext.Session.GetString("fullname");
            message.To.Add(MailboxAddress.Parse(rEmail));
            message.Subject = "OCBC Account Access Code";
            message.Body = new TextPart("html")
            {
                Text = "Welcome " + rName + "<br>Thank You For Signing Up at OCBC Bank." +
                "<br>Here is Your Access Code" +
                "<br>You will need this code to Log In" +
                "<br>Access Code: " + Convert.ToString(value)

            };
            string emailAddress = "ocbcteam1@gmail.com";
            string password = "ocbcteam1";
            SmtpClient client = new SmtpClient();
            try
            {
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate(emailAddress, password);
                client.Send(message);

                Console.WriteLine("Email Sent!.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
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
