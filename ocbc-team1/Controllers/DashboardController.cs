using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocbc_team1.DAL;
using ocbc_team1.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace ocbc_team1.Controllers
{
    public class DashboardController : Controller
    {
        static TelegramBotClient Bot = new TelegramBotClient("2106855009:AAEVAKqEbNj6W7GeZoOLkgmF8XgsL7ZvG2o");
        private TransactionDAL transactionContext = new TransactionDAL();
        private NewBankAccountDAL newaccountContext = new NewBankAccountDAL();
        private TelegramDAL teleContext = new TelegramDAL();
        private CurrencyDAL currContext = new CurrencyDAL();
        string accountSid = "AC33d8de9089a6d0c154358213b4772ebf";
        string apiKey = "SK754a190e66db43863ae52ebea4c88b82";
        string apiSecret = "GESQ4q7mWcypxwHAycBg8o2CaQdr0oaZ";
        public IActionResult Index()
        {
            string accesscode = HttpContext.Session.GetString("accesscode");
            List<BankAccount> bankAccountList = transactionContext.getBankAccountList(accesscode);
            transactionContext.CheckScheduledTransferList();
            return View(bankAccountList);
        }
        
        public IActionResult TransferHistory(int AccountNumber)
        {
            int accNo = AccountNumber;
            ViewData["AccountNo"] = accNo;
            string accesscode = HttpContext.Session.GetString("accesscode");
            List<Transaction> transactionList = transactionContext.getTransactionList(accesscode);
            transactionContext.CheckScheduledTransferList();
            return View(transactionList);
        }

        public IActionResult ScheduledTransferHistory(int AccountNumber)
        {
            int accNo = AccountNumber;
            ViewData["AccountNo"] = accNo;
            string accesscode = HttpContext.Session.GetString("accesscode");
            List<TransferViewModel> transactionList = transactionContext.GetScheduledTransferList();
            transactionContext.CheckScheduledTransferList();
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
            }
            else if (currContext.verifyCurrency(tfViewModel.TransferCurrency) == false)
            {
                TempData["ErrorMessage"] = "Invalid currency, please try again";
                return RedirectToAction("Transfer", "Dashboard");
            }
            else if (transactionContext.checkSenderFunds(accesscode, tfViewModel.From_AccountNumber, tfViewModel.TransferAmount, tfViewModel.TransferCurrency))
            {
                TempData["ErrorMessage"] = "This account has insufficient funds, please try again";
                return RedirectToAction("Transfer", "Dashboard");
            }
            //ViewData["TFVM"] = tfViewModel;
            return RedirectToAction("PostTransferOTP", "Dashboard", tfViewModel);
        }

        [HttpPost]
        public IActionResult CreateScheduledTransfer(TransferViewModel tfvm)
        {
            tfvm.accesscode = HttpContext.Session.GetString("accesscode");
            tfvm.isScheduled = true;
            string accesscode = HttpContext.Session.GetString("accesscode");
            if (transactionContext.checkScheduleRecipient(tfvm) == false)
            {
                TempData["ErrorMessage"] = "Recipient doesn't exist , please try again";
                return RedirectToAction("ScheduledTransfer", "Dashboard");
            }
            else if (accesscode == tfvm.From_AccountNumber)
            {
                TempData["ErrorMessage"] = "Unable to send to yourself , please try again";
                return RedirectToAction("ScheduledTransfer", "Dashboard");
            }
            else if (tfvm.TransferAmount <= 0)
            {
                TempData["ErrorMessage"] = "Invalid amount, please try again";
                return RedirectToAction("ScheduledTransfer", "Dashboard");
            }
            else if (currContext.verifyCurrency(tfvm.TransferCurrency) == false)
            {
                TempData["ErrorMessage"] = "Invalid currency, please try again";
                return RedirectToAction("ScheduledTransfer", "Dashboard");
            }
            else if (transactionContext.checkSenderFunds(accesscode, tfvm.From_AccountNumber, tfvm.TransferAmount, tfvm.TransferCurrency))
            {
                TempData["ErrorMessage"] = "This account has insufficient funds, please try again";
                return RedirectToAction("ScheduledTransfer", "Dashboard");
            }
            else if (tfvm.TransferDate < DateTime.Now)
            {
                TempData["ErrorMessage"] = "This date is not valid, please try again";
                return RedirectToAction("ScheduledTransfer", "Dashboard");
            }
            HttpContext.Session.SetString("transferdate", tfvm.TransferDate.ToString());
            return RedirectToAction("PostTransferOTP", "Dashboard", tfvm);
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
                string OTPtype = teleContext.getOTPType(accesscode);
                if (OTPtype == "SMS")
                {
                    int phoneno = Convert.ToInt32(teleContext.getPhoneNumber(accesscode));
                    TwilioClient.Init(apiKey, apiSecret, accountSid);
                    var message = MessageResource.Create(
                    body: "Your OTP is: " + rOTP,
                    from: new Twilio.Types.PhoneNumber("+19377779542"),
                    to: new Twilio.Types.PhoneNumber("+65" + phoneno));
                }
                else if (OTPtype == null || OTPtype == "Telegram")
                {
                    if (teleContext.getTelegramChatId(accesscode) != null)
                    {
                        string chatid = Convert.ToString(teleContext.getTelegramChatId(accesscode));
                        sendMessage(chatid, text);
                    }
                }
                        
                return View(new PostTransferOTP_ViewModel { tfvm = tfvm, OTP = null });
            }
            else
            {
                return RedirectToAction("TransferConnectionError", "Dashboard", tfvm);
            }
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
                    // SCHEDULED TRANSFER ALTERNATIVE
                    if (ptfVM.tfvm.isScheduled == true)
                    {
                        string accesscode = HttpContext.Session.GetString("accesscode");
                        ptfVM.tfvm.accesscode = accesscode;
                        ptfVM.tfvm.TransferDate = Convert.ToDateTime(HttpContext.Session.GetString("transferdate"));
                        transactionContext.scheduledTransferFunds(ptfVM.tfvm);
                        if (teleContext.getTelegramChatId(accesscode) != null)
                        {
                            string chatid = Convert.ToString(teleContext.getTelegramChatId(accesscode));
                            sendMessage(chatid, $"You have successfully scheduled a transfer of {ptfVM.tfvm.TransferCurrency} {ptfVM.tfvm.TransferAmount} to {ptfVM.tfvm.To_AccountNumber}, transfer will execute on the {ptfVM.tfvm.TransferDate}");
                        }
                        TempData["SuccessMessage"] = $"You have successfully scheduled a transfer of {ptfVM.tfvm.TransferCurrency} {ptfVM.tfvm.TransferAmount} to {ptfVM.tfvm.To_AccountNumber}, transfer will execute on the {ptfVM.tfvm.TransferDate}";
                        return RedirectToAction("Index", "Dashboard");
                    }
                    // NORMAL TRANSFER FLOW
                    string dCheck = transactionContext.transferFunds(ptfVM.tfvm, HttpContext.Session.GetString("accesscode"));
                    if (dCheck == "true")
                    {
                        TempData["SuccessMessage"] = "You have sucessfully transferred " + ptfVM.tfvm.TransferCurrency + " " + ptfVM.tfvm.TransferAmount + " to " + ptfVM.tfvm.PhoneNumber + ptfVM.tfvm.To_AccountNumber;
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else if (dCheck == "false")
                    {
                        return RedirectToAction("TransferConnectionError", "Dashboard", ptfVM.tfvm);
                    }
                    else if (dCheck == "tfail")
                    {
                        return RedirectToAction("TransferConnectionError", "Dashboard", ptfVM.tfvm);
                    }
                    else if (dCheck == "ufail")
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

            return RedirectToAction("PostTransferOTP", "Dashboard", tfvm);
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
            } else if (currContext.verifyCurrency(nbaViewModel.AccountCurrency) == false){
                TempData["ErrorMessage"] = "Choose a valid currency (like SGD)";
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

        public IActionResult Profile()
        {
            string accesscode = HttpContext.Session.GetString("accesscode");
            TempData["currentotptype"] = "";
            TempData["UserObject"] = null;
            LoginDAL loginContext = new LoginDAL();
            List<User> userlist = loginContext.retrieveUserList();
            foreach (User u in userlist)
            {
                if (u.AccessCode == accesscode)
                {
                    TempData["currentotptype"] = u.TypeOTP;
                    if (u.TypeOTP == null)
                    { TempData["currentotptype"] = "Telegram"; }
                    TempData["UserObject"] = u;
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitUpdateUserProfileForm(UpdateUserProfileForm form)
        {
            string accesscode = HttpContext.Session.GetString("accesscode");

            try
            {
                if (form.ProfilePictureFile != null)
                {
                    form.ProfilePic_StringIdentifier = form.ProfilePictureFile.FileName;
                    //  Image Uploading
                    //  Storing image file on server temporarily
                    string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\TempImage\\" + form.ProfilePictureFile.FileName);
                    using (var fileSteam = new FileStream(savePath, FileMode.Create)) { form.ProfilePictureFile.CopyTo(fileSteam); }
                    //  Upload to cloudinary from server storage
                    Cloudinary cloudinary = new Cloudinary(new Account { ApiKey = "493755983692144", ApiSecret = "9lJhZP0e5XiDFDmY-yCFwwpE9vA", Cloud = "ocbcteam1" });
                    ImageUploadParams imageuploadparams = new ImageUploadParams() { File = new FileDescription(savePath), UseFilename = true, Tags = form.ProfilePic_StringIdentifier };
                    ImageUploadResult imageuploadresult = cloudinary.Upload(imageuploadparams);
                    string imageurl = imageuploadresult.SecureUri.AbsoluteUri;
                    form.ProfilePic_Url = imageurl;

                    //  Delete temporary Image File file on server
                    FileInfo fileinfo = new FileInfo(savePath);
                    if (fileinfo != null) { System.IO.File.Delete(savePath); }
                }
                UpdateProfileDAL updal = new UpdateProfileDAL();
                updal.UpdateUserProfile(accesscode, form);
                TempData["Message"] = "Profile Updated";
                return RedirectToAction("Profile", "Dashboard");
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = $"Failed to update profile, {e.Message}";
                return RedirectToAction("Profile", "Dashboard");

            }
        }

        public IActionResult DeleteProfilePicture(string stringidentifier)
        {
            string accesscode = HttpContext.Session.GetString("accesscode");
            Cloudinary cloudinary = new Cloudinary(new Account { ApiKey = "493755983692144", ApiSecret = "9lJhZP0e5XiDFDmY-yCFwwpE9vA", Cloud = "ocbcteam1" });
            cloudinary.DeleteResourcesByTag(stringidentifier);
            UpdateProfileDAL updal = new UpdateProfileDAL();
            updal.DeleteProfilePicture(accesscode);
            TempData["Message"] = "Profile Picture Removed";
            return RedirectToAction("Profile", "Dashboard");
        }

        public IActionResult ChangeProfile(string typeotp)
        {
            string accesscode = HttpContext.Session.GetString("accesscode");
            teleContext.setOTPType(accesscode, typeotp);
            return RedirectToAction("Profile","Dashboard");
        }
    }
}
