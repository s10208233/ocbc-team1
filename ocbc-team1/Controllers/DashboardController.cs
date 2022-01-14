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

        public IActionResult ScheduledTransfer()
        {
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
                TempData["ErrorMessage"] = "Recipient doesn't exist , please try again";
                return RedirectToAction("Transfer", "Dashboard");
            }
            else if (tfViewModel.TransferAmount <= 0)
            {
                TempData["ErrorMessage"] = "Invalid amount, please try again";
                return RedirectToAction("Transfer", "Dashboard");
            } else if (transactionContext.checkSenderFunds(accesscode, tfViewModel.From_AccountNumber, tfViewModel.TransferAmount))
            {
                TempData["ErrorMessage"] = "This account has insufficient funds, please try again";
                return RedirectToAction("Transfer", "Dashboard");
            }
            //ViewData["TFVM"] = tfViewModel;
            return RedirectToAction("postTransferOTP", "Dashboard", tfViewModel);
        }

        [HttpPost]
        public IActionResult CreateScheduledTransfer(ScheduledTransfer ScheduledTransfer)
        {
            string accesscode = HttpContext.Session.GetString("accesscode");
            if (transactionContext.checkScheduleRecipient(ScheduledTransfer) == false)
            {
                TempData["ErrorMessage"] = "Recipient doesn't exist , please try again";
                return RedirectToAction("ScheduledTransfer", "Dashboard");
            }
            else if (ScheduledTransfer.TransferAmount <= 0)
            {
                TempData["ErrorMessage"] = "Invalid amount, please try again";
                return RedirectToAction("ScheduledTransfer", "Dashboard");
            }
            else if (transactionContext.checkSenderFunds(accesscode, ScheduledTransfer.From_AccountNumber, ScheduledTransfer.TransferAmount))
            {
                TempData["ErrorMessage"] = "This account has insufficient funds, please try again";
                return RedirectToAction("ScheduledTransfer", "Dashboard");
            }
            else if (ScheduledTransfer.TransferDate < DateTime.Now)
            {
                TempData["ErrorMessage"] = "This date is not valid, please try again";
                return RedirectToAction("ScheduledTransfer", "Dashboard");
            }
            //ViewData["TFVM"] = tfViewModel;
            return RedirectToAction("postTransferOTP", "Dashboard", ScheduledTransfer);
        }

        public ActionResult PostTransferOTP(TransferViewModel tfvm)
        {
            bool con = transactionContext.checkConnectivity();
            if (con == true)
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
            else
            {
                return RedirectToAction("TransferConnectionError", "Dashboard", tfvm);
            }
            

        }
        public IActionResult Profile()
        {

            return View();

        }

        public IActionResult ChangeProfile(string typeotp)
        {
            string accesscode = HttpContext.Session.GetString("accesscode");
            teleContext.setOTPType(accesscode, typeotp);
            return View();
        }

        [HttpPost]
        public IActionResult SubmitPostTransferOTP(PostTransferOTP_ViewModel ptfVM)
        {
            //if no internet
            //return RedirectToAction("TransferConnectionError", "Dashboard", ptfVM);

            if (ptfVM.OTP != HttpContext.Session.GetString("otp"))
            {
                TempData["ErrorMessage"] = "Invalid OTP";
                return RedirectToAction("Transfer", "Dashboard", ptfVM.tfvm);
            }
            else
            {
                bool con = transactionContext.checkConnectivity();
                if (con == true)
                {
                    if (ptfVM.tfvm.fail == "fail")
                    {
                        return RedirectToAction("TransferConnectionError", "Dashboard", ptfVM.tfvm);
                    }
                    bool dCheck = transactionContext.transferFunds(ptfVM.tfvm, HttpContext.Session.GetString("accesscode"));
                    if (dCheck == true)
                    {
                        TempData["SuccessMessage"] = "You have sucessfully transferred $" + ptfVM.tfvm.TransferAmount + " to " + ptfVM.tfvm.PhoneNumber + ptfVM.tfvm.To_AccountNumber;
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        return RedirectToAction("TransferConnectionError", "Dashboard", ptfVM.tfvm);
                    }
                    
                }
                else
                {
                    return RedirectToAction("TransferConnectionError", "Dashboard", ptfVM.tfvm);
                }
                
            }
            return null;
        }

        public ActionResult TransferConnectionError(TransferViewModel tfvm)
        {
            return View(tfvm);
        }

        public ActionResult RetryTransferConnectionError(TransferViewModel tfvm)
        {

            return RedirectToAction("postTransferOTP", "Dashboard", tfvm);
        }

        public IActionResult NewBankAccount()
        {
            ViewData["UserOwnAccountList"] = transactionContext.getBankAccountList(HttpContext.Session.GetString("accesscode"));
            return View();
        }

        [HttpPost]
        public IActionResult AddAccount(NewBankAccountViewModel nbaViewModel)
        {
            if (nbaViewModel.AmountRemaining < 0)
            {
                TempData["ErrorMessage"] = "Choose between 0 and 1000000 in Balance";
                return RedirectToAction("NewBankAccount", "Dashboard");
            } else if (nbaViewModel.AmountRemaining > 1000000)
            {
                TempData["ErrorMessage"] = "Choose between 0 and 1000000 in Balance";
                return RedirectToAction("NewBankAccount", "Dashboard");
            }
            nbaViewModel.AmountRemaining = Math.Round(nbaViewModel.AmountRemaining, 2);
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
