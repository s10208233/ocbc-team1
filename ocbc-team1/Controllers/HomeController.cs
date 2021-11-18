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
using Telegram.Bot;

namespace ocbc_team1.Controllers
{
    public class HomeController : Controller
    {
        static TelegramBotClient Bot = new TelegramBotClient("2106855009:AAEVAKqEbNj6W7GeZoOLkgmF8XgsL7ZvG2o");
        private SignupDAL signupContext = new SignupDAL();
        private LoginDAL loginContext = new LoginDAL();
        private TelegramDAL teleContext = new TelegramDAL();
        private string text = "";
        private string accesscode = "";

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

        public IActionResult OTP()
        {
            text = "";
            accesscode = "";
            accesscode = HttpContext.Session.GetString("accesscode");
            Random rnd = new Random();
            string rOTP = Convert.ToString(rnd.Next(000000, 999999));
            HttpContext.Session.SetString("otp", rOTP);
            text = "Your OTP is: " + rOTP;
            if (teleContext.getTelegramChatId(accesscode) != null)
            {
                string chatid = Convert.ToString(teleContext.getTelegramChatId(accesscode));
                sendMessage(chatid, text);

            }
            else
            {                
            Bot.StartReceiving();
            Bot.OnMessage += Bot_OnMessage;
            }

            return View();
        }

        [HttpPost]
        public IActionResult OTP(OTPViewModel otpVM)
        {
            if (ModelState.IsValid)
            {
                if(HttpContext.Session.GetString("otp") == otpVM.OTP)
                {
                    HttpContext.Session.SetString("login", "true");
                    return RedirectToAction("Index", "Dashboard");
                }
                TempData["Message"] = "Invalid OTP. Try Again!";
                return View();
            }
            else
            {
                return View(otpVM);
            }
        }
        public async Task sendMessage(string destID, string text)
        {            
            await Bot.SendTextMessageAsync(destID, text);

        }
                
        private void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            string chatid = Convert.ToString(e.Message.Chat.Id);
            sendMessage(chatid, text);
            teleContext.setTelegramChatId(accesscode, Convert.ToInt32(chatid));
            text = "";
            accesscode = "";
        }       

        [HttpPost] 
        public IActionResult Login(LoginViewModel loginVM) 
        {
            if(ModelState.IsValid)
            {
                LoginDAL loginContext = new LoginDAL();
                List<User> userlist = loginContext.retrieveUserList();
                if (userlist == null)
                {
                    TempData["Message"] = "No users in the database.";
                    return View();
                }
                foreach (User user in userlist)
                {
                    if (loginVM.AccessCode == user.AccessCode && loginVM.Pin == user.BankPIN)
                    {
                        HttpContext.Session.SetString("fullname", string.Format("{0} {1}", user.FirstName, user.LastName));
                        HttpContext.Session.SetString("accesscode", user.AccessCode);
                        return RedirectToAction("OTP", "Home");
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
            return View();
        }

        [HttpPost]
        public IActionResult Signup(SignUpViewModel userinput)
        {
            if (ModelState.IsValid)
            {
                List<Card> validcardlist = signupContext.retrieveShortCardList(signupContext.retrieveCardList());
                List<User> existinguserlist = loginContext.retrieveUserList();
                foreach (Card c in validcardlist)
                {
                    if (c.CardNumber == userinput.CardNumber && c.CardPIN == userinput.BankPIN)
                    {
                        if (existinguserlist != null) 
                        {
                            foreach (User u in existinguserlist)
                            {
                                if (u.CardNumber == userinput.CardNumber ||
                                    u.BankPIN == userinput.BankPIN ||
                                    u.Email == userinput.Email ||
                                    u.IC == userinput.IC
                                    ) { TempData["Message"] = "The information you have entered is not valid or unique."; return View(userinput); }
                            }
                        }
                        TempData["SignupSuccessMessage"] = "We've sent your access code to your email address, check your inbox.";
                        signupContext.completeUserSignUp(userinput);
                        return RedirectToAction("Login", "Home");
                        
                        
                    }
                }
                TempData["Message"] = "The card number you have entered is invalid.";
                return View(userinput);
            }
            else
            {
                //Input validation fails, return to the Create view
                //to display error message
                return View(userinput);
            }

        }
               
        public IActionResult forgetPassword()
        {
            return View();
        }
        [HttpPost]
        public IActionResult forgetPassword(ResendAcessCode Rmail)
        {
            loginContext.ResendAccessCode(Rmail.Email);
            return RedirectToAction("Login","Home");
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
