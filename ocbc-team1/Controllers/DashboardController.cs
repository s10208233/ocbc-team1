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
        private List<string> TypeOfTransfer = new List<string> { "Using OCBC Acount Number", "Using Nric/Fin","Using Phone Number" };
        public IActionResult Index()
        {
            string accesscode = HttpContext.Session.GetString("accesscode");
            List<BankAccount> bankAccountList = transactionContext.getBankAccountList(accesscode);
            return View(bankAccountList);
        }
        
        public IActionResult TransferHistory()
        {
            return View();
        }

        public ActionResult Transfer()  
        {
            ViewData["TypeTransfer"] = TypeOfTransfer;
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
