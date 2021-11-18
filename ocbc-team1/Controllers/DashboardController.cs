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
            string accesscode = HttpContext.Session.GetString("accesscode");
            if (transactionContext.checkRecipient(tfViewModel) == false)
            {
                TempData["ErrorMessage"] = "Recipient Doesn't exist , please try again";
                return RedirectToAction("Transfer", "Dashboard");
            }
            else if (tfViewModel.TransferAmount <= 0)
            {
                TempData["ErrorMessage"] = "Invalid Amount, please try again";
                return RedirectToAction("Transfer", "Dashboard");
            } else if (transactionContext.checkSenderFunds(accesscode, tfViewModel.From_AccountNumber, tfViewModel.TransferAmount))
            {
                TempData["ErrorMessage"] = "This account has insufficient Funds, please try again";
                return RedirectToAction("Transfer", "Dashboard");
            }
            //ViewData["TFVM"] = tfViewModel;
            return RedirectToAction("postTransferOTP", "Dashboard", tfViewModel);
        }

        public ActionResult PostTransferOTP(TransferViewModel tfvm)
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
            return View(new PostTransferOTP_ViewModel { tfvm = tfvm, OTP = null });

        }
        [HttpPost]
        public IActionResult SubmitPostTransferOTP(PostTransferOTP_ViewModel ptfVM)
        {
            if (ptfVM.OTP != HttpContext.Session.GetString("otp"))
            {
                TempData["ErrorMessage"] = "Invalid OTP";
                return RedirectToAction("Transfer", "Dashboard", ptfVM.tfvm);
            }
            else
            {
                transactionContext.transferFunds(ptfVM.tfvm, HttpContext.Session.GetString("accesscode"));
                TempData["SuccessMessage"] = "You have sucessfully transferred $" + ptfVM.tfvm.TransferAmount + " to " + ptfVM.tfvm.PhoneNumber + ptfVM.tfvm.To_AccountNumber;
                return RedirectToAction("Index", "Dashboard");
            }
            return null;
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
