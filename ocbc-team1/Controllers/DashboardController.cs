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

        [HttpPost]
        public IActionResult CreateTransfer(TransferViewModel tfViewModel) 
        {
            //if (tfViewModel.From_AccountNumber == null)
            //{
            //    TempData["ErrorMessage"] = "Please enter a bank account number to transfer to"; 
            //}
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
        
        public IActionResult UserLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
