using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocbc_team1.DAL;
using ocbc_team1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;

namespace ocbc_team1.Controllers
{
    public class DashboardController : Controller
    {
        static TelegramBotClient Bot = new TelegramBotClient("2106855009:AAEVAKqEbNj6W7GeZoOLkgmF8XgsL7ZvG2o");
        private TransactionDAL transactionContext = new TransactionDAL();
        private NewBankAccountDAL newaccountContext = new NewBankAccountDAL();
        private TelegramDAL teleContext = new TelegramDAL();
        public IActionResult Index()
        {
            string accesscode = HttpContext.Session.GetString("accesscode");
            List<BankAccount> bankAccountList = transactionContext.getBankAccountList(accesscode);
            return View(bankAccountList);
        }
        
        public IActionResult TransferHistory(int AccountNumber)
        {
            int accNo = AccountNumber;
            ViewData["AccountNo"] = accNo;
            string accesscode = HttpContext.Session.GetString("accesscode");
            List<Transaction> transactionList = transactionContext.getTransactionList(accesscode);
            return View(transactionList);
        }

        public IActionResult Transfer()  
        {
            
            ViewData["TransferType"] = new List<string> { "Using OCBC Acount Number", "Using Phone Number" };
            ViewData["UserOwnAccountList"] = transactionContext.getBankAccountList(HttpContext.Session.GetString("accesscode"));
            return View();
        }

        public IActionResult transferOTP()
        {
            string accesscode = HttpContext.Session.GetString("accesscode");
            Random rnd = new Random();
            string rOTP = Convert.ToString(rnd.Next(000000, 999999));
            HttpContext.Session.SetString("otp", rOTP);
            string text = "Your OTP is: " + rOTP;
            if (teleContext.getTelegramChatId(accesscode) != null)
            {
                string chatid = Convert.ToString(teleContext.getTelegramChatId(accesscode));
                sendMessage(chatid, text);

            }
            return View();
        }
        public async Task sendMessage(string destID, string text)
        {
            await Bot.SendTextMessageAsync(destID, text);

        }

        [HttpPost]
        public IActionResult CreateTransfer(TransferViewModel tfViewModel) 
        {
            if(tfViewModel.OTP != HttpContext.Session.GetString("otp"))
            {
                TempData["ErrorMessage"] = "Invalid OTP";
                return RedirectToAction("Transfer", "Dashboard", tfViewModel);
            }
            if (transactionContext.checkRecipient(tfViewModel) == false)
            {
                TempData["ErrorMessage"] = "Recipient Doesn't exsist please try again";
                return RedirectToAction("Transfer", "Dashboard");
            }
            else
            {
                transactionContext.transferFunds(tfViewModel, HttpContext.Session.GetString("accesscode"));
                TempData["SuccessMessage"] = "Transfer made!";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        public IActionResult NewBankAccount()
        {
            ViewData["UserOwnAccountList"] = transactionContext.getBankAccountList(HttpContext.Session.GetString("accesscode"));
            return View();
        }

        [HttpPost]
        public IActionResult AddAccount(NewBankAccountViewModel nbaViewModel)
        {
            newaccountContext.createNewBankAccount(nbaViewModel, HttpContext.Session.GetString("accesscode"));
            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult UserLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
