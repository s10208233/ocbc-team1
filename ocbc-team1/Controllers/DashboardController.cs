using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocbc_team1.DAL;
using ocbc_team1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.Controllers
{
    public class DashboardController : Controller
    {
        private TransactionDAL transactionContext = new TransactionDAL();
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
            return View();
        }

        public IActionResult Transfer()  
        {
            ViewData["TransferType"] = new List<string> { "Using OCBC Acount Number", "Using Phone Number" };
            ViewData["UserOwnAccountList"] = transactionContext.getBankAccountList(HttpContext.Session.GetString("accesscode"));
            return View();
        }

        [HttpPost]
        public IActionResult CreateTransfer(TransferViewModel tfViewModel) 
        {
            //if (tfViewModel.PhoneNumber == null)
            //{
            //    TempData["ErrorMessage"] = "Please enter a phone number to transfer to";
            //}
            //if (tfViewModel.From_AccountNumber == null)
            //{
            //    TempData["ErrorMessage"] = "Please enter a bank account number to transfer to"; 
            //}
            transactionContext.transferFunds(tfViewModel, HttpContext.Session.GetString("accesscode"));
            return RedirectToAction("Index", "Dashboard");
        }
        
        public IActionResult UserLogout()
        {
            HttpContext.Session.Remove("login");
            HttpContext.Session.Remove("fullname");
            return RedirectToAction("Index", "Home");
        }
    }
}
