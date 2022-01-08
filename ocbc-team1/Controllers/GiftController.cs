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
    public class GiftController : Controller
    {
        private TransactionDAL transactionContext = new TransactionDAL();
        private LoginDAL loginContext = new LoginDAL();
        private GiftDAL giftContext = new GiftDAL();

        // Assignment 2: GIFT
        public ActionResult Inbox()
        {
            string accesscode = HttpContext.Session.GetString("accesscode");
            Dictionary<string, Gift> giftdictionary = giftContext.RetrieveGiftDictionary();
            Dictionary<string, Gift> unopened_giftdictionary = new Dictionary<string, Gift>();
            Dictionary<string, Gift> opened_giftdictionary = new Dictionary<string, Gift>();
            Dictionary<string, Gift> sent_giftdictionary = new Dictionary<string, Gift>();

            foreach (var row in giftdictionary)
            {
                if (row.Value.Sender.AccessCode == accesscode || row.Value.Receipient.AccessCode == accesscode)
                {
                    if (row.Value.Receipient.AccessCode == accesscode && row.Value.Received == false) { unopened_giftdictionary.Add(row.Key, row.Value); }
                    if (row.Value.Receipient.AccessCode == accesscode && row.Value.Received == true) { opened_giftdictionary.Add(row.Key, row.Value); }
                    if (row.Value.Sender.AccessCode == accesscode) { sent_giftdictionary.Add(row.Key, row.Value); }
                }
            }
            ViewData["UnopenedGiftsDictionary"] = unopened_giftdictionary;
            ViewData["OpenedGiftsDictionary"] = opened_giftdictionary;
            ViewData["SentGiftsDictionary"] = sent_giftdictionary;
            return View();
        }

        public IActionResult ChangeTab(string tabstring)
        {
            if (tabstring == "UnopenedGifts")
            {
                TempData["TabString"] = "UnopenedGifts";
            }
            if (tabstring == "OpenedGifts")
            {
                TempData["TabString"] = "OpenedGifts";
            }
            if (tabstring == "SentGifts")
            {
                TempData["TabString"] = "SentGifts";
            }
            return RedirectToAction("Inbox","Gift");
        }


        public IActionResult OpenGift(string giftkey)
        {
            giftContext.OpenGift(giftkey);
            return RedirectToAction("Inbox", "Gift");
        }

        public ActionResult CreateGift()
        {
            Dictionary<string, List<string>> stickerdictionary = new Dictionary<string, List<string>>();
            List<string> generalstickers = new List<string> { 
                "https://assets9.lottiefiles.com/temp/lf20_6B7ySS.json",
                "https://assets6.lottiefiles.com/packages/lf20_pGUlZz.json",
                "https://assets2.lottiefiles.com/packages/lf20_qsrtwdyv.json",
                "https://assets8.lottiefiles.com/packages/lf20_8o8qzD.json",
                "https://assets8.lottiefiles.com/packages/lf20_0iwDV6.json"
            };
            List<string> festivestickers = new List<string> {
                "https://assets3.lottiefiles.com/packages/lf20_2xzPFP.json",
                "https://assets4.lottiefiles.com/temp/lf20_OyYIXu.json",
                "https://assets3.lottiefiles.com/packages/lf20_kfVccK/data.json",
                "https://assets3.lottiefiles.com/private_files/lf30_7h6zulqv.json",
                "https://assets5.lottiefiles.com/packages/lf20_W2mq7z.json",
                "https://assets5.lottiefiles.com/packages/lf20_tGWH9X.json",
                "https://assets10.lottiefiles.com/private_files/lf30_ih0hvzzx.json",
                "https://assets10.lottiefiles.com/private_files/lf30_d7snhxoj.json",
                "https://assets1.lottiefiles.com/packages/lf20_atl2nbkz.json",
                "https://assets3.lottiefiles.com/packages/lf20_2t33h0yc.json",
                "https://assets3.lottiefiles.com/packages/lf20_6ylhuyam.json",
                "https://assets1.lottiefiles.com/packages/lf20_oi3adcq2.json",
                //Xmas
                "https://assets9.lottiefiles.com/packages/lf20_9gmlwgi8.json",
                "https://assets9.lottiefiles.com/packages/lf20_ydn9vde0.json",
                "https://assets9.lottiefiles.com/packages/lf20_m3keitix.json",
                "https://assets9.lottiefiles.com/packages/lf20_i7y3y8fi.json"
            };

            stickerdictionary.Add("General",generalstickers);
            stickerdictionary.Add("Festive", festivestickers);
            string accesscode = HttpContext.Session.GetString("accesscode");
            if (accesscode == null) { return RedirectToAction("Login", "Home"); }
            ViewData["UserOwnAccountList"] = transactionContext.getBankAccountList(accesscode);
            ViewData["StickerDictionary"] = stickerdictionary;
            return View();
        }

        public IActionResult SubmitCreateGift(CreateGift_ViewModel form)
        {
            //  Prepare Gift Modal
            string accesscode = HttpContext.Session.GetString("accesscode");
            User sender = null;
            User receipient = null;

            foreach (User u in loginContext.retrieveUserList())
            {
                if (u.AccessCode == accesscode) { sender = u; }
                if (u.PhoneNumber == form.To_PhoneNumber) { receipient = u; }
                if (sender != null && receipient != null) { break; }
                foreach (BankAccount ba in u.AccountsList)
                {
                    if (ba.AccountNumber == Convert.ToInt32(form.To_AccountNumber)) { receipient = u; }
                }
            }
            Transaction gifttransaction = null;
            if (form.To_PhoneNumber != null && form.To_AccountNumber == null)
            {
                gifttransaction = new Transaction()
                {
                    Amount = Math.Round(form.Amount, 2),
                    From_AccountNumber = Convert.ToInt32(form.From_AccountNumber),
                    TimeSent = DateTime.Now,
                    To_AccountNumber = receipient.AccountsList[0].AccountNumber
                };
            }
            if (form.To_PhoneNumber == null && form.To_AccountNumber != null)
            {
                gifttransaction = new Transaction()
                {
                    Amount = Math.Round(form.Amount, 2),
                    From_AccountNumber = Convert.ToInt32(form.From_AccountNumber),
                    TimeSent = DateTime.Now,
                    To_AccountNumber = Convert.ToInt32(form.To_AccountNumber)
                };
            }

            //  End of preparing gift modal, create gift modal
            Gift newgift = new Gift()
            {
                Sender = sender,
                Receipient = receipient,
                transaction = gifttransaction,
                Received = false,
                sticker_src = form.sticker_src,
                Message = form.Message
            };
            // Validate
            if (sender == receipient) {
                TempData["Message"] = "Unable to send gift, your are trying to send a gift to yourself.";
                return RedirectToAction("CreateGift", "Gift");
            }
            foreach (BankAccount ba in sender.AccountsList)
            {
                if (ba.AccountNumber == Convert.ToInt32(form.From_AccountNumber))
                {
                    if (form.Amount > ba.AmountAvaliable)
                    {
                        TempData["Message"] = $"Unable to send gift, the bank account {form.From_AccountNumber} has insufficient fund";
                        return RedirectToAction("CreateGift", "Gift");
                    }
                }
            }
            giftContext.SendGift(newgift);
            TempData["Message"] = $"Gift of ${form.Amount} successfully sent to {receipient.FirstName + receipient.LastName}!";
            return RedirectToAction("CreateGift", "Gift");

        }

    }
}
