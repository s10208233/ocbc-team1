using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using ocbc_team1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Twilio;
using Twilio.Rest.Api.V2010.Account;


namespace ocbc_team1.DAL
{
    public class TransactionDAL
    {
        static TelegramBotClient Bot = new TelegramBotClient("2106855009:AAEVAKqEbNj6W7GeZoOLkgmF8XgsL7ZvG2o");
        private LoginDAL loginContext = new LoginDAL();
        private string recName = "";
        private TelegramDAL teleContext = new TelegramDAL();
        private CurrencyDAL currContext = new CurrencyDAL();
        string accountSid = "AC33d8de9089a6d0c154358213b4772ebf";
        string apiKey = "SK754a190e66db43863ae52ebea4c88b82";
        string apiSecret = "GESQ4q7mWcypxwHAycBg8o2CaQdr0oaZ";

        IFirebaseConfig ifc = new FirebaseConfig()
        {
            AuthSecret = "Sa3cJdieiAEHpAPK7Z243SRtpxia29x6gzwaoz1g",
            BasePath = "https://failsafefundtransfer-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };
        IFirebaseClient ifclient;
        private object client;

        public List<BankAccount> getBankAccountList(string accesscode) 
        {
            List<User> userlist = loginContext.retrieveUserList() ;
            foreach (User u in userlist) 
            {
                if (u.AccessCode == accesscode) 
                {
                    return u.AccountsList;
                }
            }
            return null;
        }

        public List<int> getBankAccountIntList(string accesscode)
        {
            List<int> returndata = new List<int>();
            foreach (BankAccount ba in getBankAccountList(accesscode))
            {
                returndata.Add(ba.AccountNumber);
            }
            return returndata;
        }

        public List<Transaction> getTransactionList(string accesscode)
        {
            List<User> userlist = loginContext.retrieveUserList();
            foreach (User u in userlist)
            {
                if (u.AccessCode == accesscode)
                {
                    return u.TransactionList;
                }
            }
            return null;
        }

        public List<TransferViewModel> GetScheduledTransferList()
        {
                List<TransferViewModel> ScheduledTransferList = new List<TransferViewModel>();
                ifclient = new FireSharp.FirebaseClient(ifc);
                if (ifclient != null)
                {
                    FirebaseResponse firebaseresponse = ifclient.Get("ScheduledTransfer");
                    ScheduledTransferList = firebaseresponse.ResultAs<List<TransferViewModel>>();
                }

                return ScheduledTransferList;
        }

        public void CheckScheduledTransferList()
        {
            List<TransferViewModel> NewScheduledTransferList = new List<TransferViewModel>();
            NewScheduledTransferList = GetScheduledTransferList();
            int counter = 0;
            if (NewScheduledTransferList != null)
            {
                foreach (var items in NewScheduledTransferList)
                {
                    counter++;
                    if (items != null)
                    {
                        if (items.TransferDate <= DateTime.Now)
                        {
                            string ScheduledAccess = items.accesscode;
                            List<User> userlist = loginContext.retrieveUserList();
                            foreach (User u in userlist)
                            {
                                if (u.AccessCode == ScheduledAccess)
                                {
                                    foreach (var accounts in u.AccountsList)
                                    {
                                        if (accounts.AccountNumber == Convert.ToInt32(items.From_AccountNumber))
                                        {
                                            if (accounts.AmountAvaliable > items.TransferAmount)
                                            {
                                                transferFunds(items, items.accesscode);
                                                FirebaseResponse deletionReponse = ifclient.Delete("ScheduledTransfer/" + (counter - 1));
                                            }
                                            else
                                            {
                                                if (ifclient != null)
                                                {
                                                    ifclient.Set("ScheduledTransfer/" + (counter - 1) + "/fail", "Failed");
                                                }
                                                items.fail = "Failed";
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }

        public bool scheduledTransferFunds(TransferViewModel tfVM)
        {
            List<TransferViewModel> ScheduledtransferList = GetScheduledTransferList();

                if (ScheduledtransferList == null)
                {
                    List<TransferViewModel> NewScheduledtransactionlist = new List<TransferViewModel>();
                    NewScheduledtransactionlist.Add(new TransferViewModel
                    {
                        To_AccountNumber =(tfVM.To_AccountNumber),
                        From_AccountNumber = (tfVM.From_AccountNumber),
                        TransferCurrency = tfVM.TransferCurrency,
                        TransferAmount = tfVM.TransferAmount,
                        TransferDate = tfVM.TransferDate,
                        accesscode= tfVM.accesscode,
                        fail= tfVM.fail,
                        isScheduled = tfVM.isScheduled,
                    }) ;
                    ScheduledtransferList = NewScheduledtransactionlist;

                }
                else
                {
                    ScheduledtransferList.Add(new TransferViewModel
                    {
                        To_AccountNumber =tfVM.To_AccountNumber,
                        From_AccountNumber = tfVM.From_AccountNumber,
                        TransferCurrency = tfVM.TransferCurrency,
                        TransferAmount = tfVM.TransferAmount,
                        TransferDate = tfVM.TransferDate,
                        accesscode = tfVM.accesscode,
                        fail = tfVM.fail,
                        isScheduled = tfVM.isScheduled,
                    });
                }
            ifclient = new FireSharp.FirebaseClient(ifc);
            if (ifclient != null)
            {
                ifclient.Set("ScheduledTransfer/", ScheduledtransferList);
            }
            return true;
        }

        public bool checkRecipient(TransferViewModel tfVM)
        {
            List<User> userslist = loginContext.retrieveUserList();

            for (int i = 0; i < userslist.Count; i++)
            {
                for (int j = 0; j < userslist[i].AccountsList.Count; j++)
                {
                    if (Convert.ToString(userslist[i].AccountsList[j].AccountNumber) == tfVM.To_AccountNumber && tfVM.From_AccountNumber != tfVM.To_AccountNumber)
                    { 
                        return true;
                    }
                    else if (userslist[i].PhoneNumber == tfVM.PhoneNumber)
                    {
                        return true;
                    }

                }
            }
            return false;
        }
        public bool checkScheduleRecipient(TransferViewModel tfVM)
        {
            List<User> userslist = loginContext.retrieveUserList();

            for (int i = 0; i < userslist.Count; i++)
            {
                for (int j = 0; j < userslist[i].AccountsList.Count; j++)
                {
                    if (Convert.ToString(userslist[i].AccountsList[j].AccountNumber) == tfVM.To_AccountNumber && tfVM.From_AccountNumber != tfVM.To_AccountNumber)
                    {
                        return true;
                    }
                    else if (userslist[i].PhoneNumber == tfVM.PhoneNumber)
                    {
                        return true;
                    }

                }
            }
            return false;
        }

        public async Task sendMessage(string destID, string text)
        {
            await Bot.SendTextMessageAsync(destID, text);

        }
        public string getName(string accesscode)
        {
            string sName = "";
            List<User> userl = loginContext.retrieveUserList();
            if (userl != null)
            {                
                foreach (User u in userl)
                {
                    if (u.AccessCode == accesscode)
                    {
                        sName = u.FirstName + " " + u.LastName;
                        return sName;
                    }
                    else
                    {
                        Console.WriteLine("error");
                    }

                }

            }
            return sName;
            
        }
        public bool checkConnectivity()
        {
            try
            {
                List<User> userslist = loginContext.retrieveUserList();
                if (userslist != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch( Exception e)
            {
                return false;
            }
            
        }

        public bool checkSenderFunds(string accesscode, string AccountNumber, double TransferAmount, string TransferCurrency)
        {
            // returns true if insufficient
            List<User> userslist = loginContext.retrieveUserList();
            if (userslist == null) { Console.WriteLine("uselist null, transfer failed"); return true; }
            for (int i = 0; i < userslist.Count; i++)
            {
                if (userslist[i].AccessCode == accesscode)
                {
                    for (int j = 0; j < userslist[i].AccountsList.Count; j++)
                    {
                        if (Convert.ToString(userslist[i].AccountsList[j].AccountNumber) == AccountNumber)
                        {
                            if (userslist[i].AccountsList[j].AccountCurrency == TransferCurrency)
                            {
                                if (userslist[i].AccountsList[j].AmountAvaliable > TransferAmount && userslist[i].AccountsList[j].AmountRemaining > TransferAmount)
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                double convertedAmount = currContext.convertCurrency(TransferAmount, TransferCurrency, userslist[i].AccountsList[j].AccountCurrency);
                                if (userslist[i].AccountsList[j].AmountAvaliable > convertedAmount && userslist[i].AccountsList[j].AmountRemaining > convertedAmount)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
        //public void scheduledTransferFunds(TransferViewModel tfVM)
        //{
        //    ifclient = new FireSharp.FirebaseClient(ifc);
        //    if (ifclient != null)
        //    {
        //        ifclient.Set("ScheduledTransaction/", tfVM);
        //    }
        //}
        public string transferFunds(TransferViewModel tfVM, string accesscode)
        {
            tfVM.TransferAmount = Math.Round(tfVM.TransferAmount, 2);
            //if (tfVM.To_AccountNumber != null && tfVM.PhoneNumber != null) { Console.WriteLine("Two transfer type has been input, transfer canceled"); return; }
            Thread.Sleep(5000);
            List<User> userslist = loginContext.retrieveUserList();
            if (userslist == null) { Console.WriteLine("uselist null, transfer failed"); return "ufail"; }
            //  By Bank Number
            if (tfVM.To_AccountNumber != null && tfVM.PhoneNumber == null)
            {
                //  Recipient
                for (int i = 0; i < userslist.Count; i++)
                {
                    for (int j = 0; j < userslist[i].AccountsList.Count; j++)
                    {
                        if (Convert.ToString(userslist[i].AccountsList[j].AccountNumber) == tfVM.To_AccountNumber)
                        {
                            double convertedAmount = currContext.convertCurrency(tfVM.TransferAmount, tfVM.TransferCurrency, userslist[i].AccountsList[j].AccountCurrency);
                            double Btransfer = userslist[i].AccountsList[j].AmountAvaliable;
                            double BtransferR = userslist[i].AccountsList[j].AmountRemaining;
                            userslist[i].AccountsList[j].AmountAvaliable += convertedAmount;
                            userslist[i].AccountsList[j].AmountRemaining += convertedAmount;
                            updateDB(userslist);
                            List<User> checklist = loginContext.retrieveUserList();
                            if (checklist == null) { Console.WriteLine("checklist null, transfer failed"); return "ufail"; }
                            for (int k = 0; k < checklist.Count; k++)
                            {
                                for (int l = 0; l < checklist[l].AccountsList.Count; l++)
                                {
                                    if (Convert.ToString(checklist[k].AccountsList[l].AccountNumber) == tfVM.To_AccountNumber)
                                    {
                                        if (checklist[k].AccountsList[l].AmountAvaliable == Btransfer + convertedAmount)
                                        {
                                            if (checklist[k].AccountsList[l].AmountRemaining == BtransferR + convertedAmount)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                userslist[i].AccountsList[j].AmountAvaliable = Btransfer;
                                                userslist[i].AccountsList[j].AmountRemaining = BtransferR;
                                                updateDB(userslist);
                                                return "tfail";
                                            }  
                                        }
                                        else
                                        {
                                            userslist[i].AccountsList[j].AmountAvaliable = Btransfer;
                                            userslist[i].AccountsList[j].AmountRemaining = BtransferR;
                                            updateDB(userslist);
                                            return "tfail";
                                        }
                                    }
                                }
                            }                                        
                            if (userslist[i].TransactionList == null)
                            {
                                List<Transaction> transactionlist = new List<Transaction>();
                                transactionlist.Add(new Transaction
                                {
                                    To_AccountNumber = Convert.ToInt32(tfVM.To_AccountNumber),
                                    From_AccountNumber = Convert.ToInt32(tfVM.From_AccountNumber),
                                    Amount = tfVM.TransferAmount,
                                    Currency = "SGD",
                                    TimeSent = DateTime.Now,
                                });
                                userslist[i].TransactionList = transactionlist;
                            }
                            else
                            {
                                userslist[i].TransactionList.Add(new Transaction
                                {
                                    To_AccountNumber = Convert.ToInt32(tfVM.To_AccountNumber),
                                    From_AccountNumber = Convert.ToInt32(tfVM.From_AccountNumber),
                                    Amount = tfVM.TransferAmount,
                                    Currency = "SGD",
                                    TimeSent = DateTime.Now,
                                });
                            }
                            recName = "";
                            recName = userslist[i].FirstName + " " + userslist[i].LastName;
                            string sName = getName(accesscode);
                            string text = "You have recieved " + tfVM.TransferCurrency + " " + tfVM.TransferAmount + " from " + sName + " on " + DateTime.Now.ToString("f");
                            string OTPtype = teleContext.getOTPType(accesscode);
                            if (OTPtype == "SMS")
                            {
                                TwilioClient.Init(apiKey, apiSecret, accountSid);
                                var message = MessageResource.Create(
                                body: text,
                                from: new Twilio.Types.PhoneNumber("+19377779542"),
                                to: new Twilio.Types.PhoneNumber("+65" + userslist[i].PhoneNumber));
                            }
                            else if (OTPtype == null || OTPtype == "Telegram")
                            {
                                sendMessage(Convert.ToString(userslist[i].TelegramChatID), text);
                            }
                            



                        }
                    }
                }
                // Sender
                for (int i = 0; i < userslist.Count; i++)
                {
                    int pC = 0;
                    int oC = 0;
                    if (userslist[i].AccessCode == accesscode)
                    {
                        for (int j = 0; j < userslist[i].AccountsList.Count; j++)
                        {
                            if (Convert.ToString(userslist[i].AccountsList[j].AccountNumber) == tfVM.From_AccountNumber)
                            {
                                if (userslist[i].AccountsList[j].AmountAvaliable > tfVM.TransferAmount && userslist[i].AccountsList[j].AmountRemaining > tfVM.TransferAmount)
                                {
                                    for (int p = 0; p < userslist.Count; p++)
                                    {
                                        for (int o = 0; o < userslist[p].AccountsList.Count; o++)
                                        {
                                            if (Convert.ToString(userslist[p].AccountsList[o].AccountNumber) == tfVM.To_AccountNumber)
                                            {
                                                pC = p;
                                                oC = o;
                                            }
                                        }
                                    }
                                    double convertedAmount = currContext.convertCurrency(tfVM.TransferAmount, tfVM.TransferCurrency, userslist[i].AccountsList[j].AccountCurrency);
                                    double BtransferS = userslist[i].AccountsList[j].AmountAvaliable;
                                    double BtransferRS = userslist[i].AccountsList[j].AmountRemaining;
                                    userslist[i].AccountsList[j].AmountAvaliable -= convertedAmount;
                                    userslist[i].AccountsList[j].AmountRemaining -= convertedAmount;
                                    updateDB(userslist);
                                    List<User> checklist = loginContext.retrieveUserList();
                                    for (int k = 0; k < checklist.Count; k++)
                                    {
                                        for (int l = 0; l < checklist[l].AccountsList.Count; l++)
                                        {
                                            if (Convert.ToString(checklist[k].AccountsList[l].AccountNumber) == tfVM.From_AccountNumber)
                                            {
                                                if (checklist[k].AccountsList[l].AmountAvaliable == BtransferS - convertedAmount)
                                                {
                                                    if (checklist[k].AccountsList[l].AmountRemaining == BtransferRS - convertedAmount)
                                                    {
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        userslist[pC].AccountsList[oC].AmountAvaliable = userslist[pC].AccountsList[oC].AmountAvaliable - convertedAmount;
                                                        userslist[pC].AccountsList[oC].AmountRemaining = userslist[pC].AccountsList[oC].AmountRemaining - convertedAmount;
                                                        userslist[i].AccountsList[j].AmountAvaliable = BtransferS;
                                                        userslist[i].AccountsList[j].AmountRemaining = BtransferRS;
                                                        updateDB(userslist);
                                                        return "tfail";
                                                    }
                                                }
                                                else
                                                {
                                                    userslist[pC].AccountsList[oC].AmountAvaliable = userslist[pC].AccountsList[oC].AmountAvaliable - convertedAmount;
                                                    userslist[pC].AccountsList[oC].AmountRemaining = userslist[pC].AccountsList[oC].AmountRemaining - convertedAmount;
                                                    userslist[i].AccountsList[j].AmountAvaliable = BtransferS;
                                                    userslist[i].AccountsList[j].AmountRemaining = BtransferRS;
                                                    updateDB(userslist);
                                                    return "tfail";
                                                }
                                            }
                                        }
                                    }
                                    if (userslist[i].TransactionList == null)
                                    {
                                        List<Transaction> transactionlist = new List<Transaction>();
                                        transactionlist.Add(new Transaction
                                        {
                                            To_AccountNumber = Convert.ToInt32(tfVM.To_AccountNumber),
                                            From_AccountNumber = Convert.ToInt32(tfVM.From_AccountNumber),
                                            Amount = tfVM.TransferAmount,
                                            Currency = tfVM.TransferCurrency,
                                            TimeSent = DateTime.Now,
                                        });
                                        userslist[i].TransactionList = transactionlist;
                                    }
                                    else
                                    {
                                        userslist[i].TransactionList.Add(new Transaction
                                        {
                                            To_AccountNumber = Convert.ToInt32(tfVM.To_AccountNumber),
                                            From_AccountNumber = Convert.ToInt32(tfVM.From_AccountNumber),
                                            Amount = tfVM.TransferAmount,
                                            Currency = tfVM.TransferCurrency,
                                            TimeSent = DateTime.Now,
                                        });
                                    }
                                    string text = "You have sent " + tfVM.TransferCurrency + " " + tfVM.TransferAmount + " to " + recName + " on " + DateTime.Now.ToString("f");
                                    string OTPtype = teleContext.getOTPType(accesscode);
                                    if (OTPtype == "SMS")
                                    {
                                        int phoneno = Convert.ToInt32(teleContext.getPhoneNumber(accesscode));
                                        TwilioClient.Init(apiKey, apiSecret, accountSid);
                                        var message = MessageResource.Create(
                                        body: text,
                                        from: new Twilio.Types.PhoneNumber("+19377779542"),
                                        to: new Twilio.Types.PhoneNumber("+65" + phoneno));
                                    }
                                    else if (OTPtype == null || OTPtype == "Telegram")
                                    {
                                        sendMessage(Convert.ToString(userslist[i].TelegramChatID), text);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //Phone number
            if (tfVM.To_AccountNumber == null && tfVM.PhoneNumber != null)
            {

                for (int i = 0; i < userslist.Count; i++)
                {
                    if (userslist[i].PhoneNumber == tfVM.PhoneNumber)
                    {
                        double convertedAmount = currContext.convertCurrency(tfVM.TransferAmount, tfVM.TransferCurrency, userslist[i].AccountsList[0].AccountCurrency);
                        double BPtransfer = userslist[i].AccountsList[0].AmountAvaliable;
                        double BPtransferR = userslist[i].AccountsList[0].AmountRemaining;
                        userslist[i].AccountsList[0].AmountAvaliable += convertedAmount;
                        userslist[i].AccountsList[0].AmountRemaining += convertedAmount;
                        updateDB(userslist);
                        List<User> checklist = loginContext.retrieveUserList();
                        for (int k = 0; k < checklist.Count; k++)
                        {                        
                           
                            if (checklist[k].PhoneNumber == tfVM.PhoneNumber)
                            {
                                if (checklist[k].AccountsList[0].AmountAvaliable == BPtransfer + convertedAmount)
                                {
                                    if (checklist[k].AccountsList[0].AmountAvaliable == BPtransferR + convertedAmount)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        userslist[i].AccountsList[0].AmountAvaliable = BPtransfer;
                                        userslist[i].AccountsList[0].AmountRemaining = BPtransferR;
                                        updateDB(userslist);
                                        return "tfail";
                                    }
                                }
                                else
                                {
                                    userslist[i].AccountsList[0].AmountAvaliable = BPtransfer;
                                    userslist[i].AccountsList[0].AmountRemaining = BPtransferR;
                                    updateDB(userslist);
                                    return "tfail";
                                }
                            }
                            
                        }
                        if (userslist[i].TransactionList == null)
                        {
                            List<Transaction> transactionlist = new List<Transaction>();
                            transactionlist.Add(new Transaction
                            {
                                To_AccountNumber = Convert.ToInt32(userslist[i].AccountsList[0].AccountNumber),
                                From_AccountNumber = Convert.ToInt32(tfVM.From_AccountNumber),
                                Amount = tfVM.TransferAmount,
                                Currency = tfVM.TransferCurrency,
                                TimeSent = DateTime.Now,
                            });
                            userslist[i].TransactionList = transactionlist;
                        }
                        else
                        {
                            userslist[i].TransactionList.Add(new Transaction
                            {
                                To_AccountNumber = Convert.ToInt32(userslist[i].AccountsList[0].AccountNumber),
                                From_AccountNumber = Convert.ToInt32(tfVM.From_AccountNumber),
                                Amount = tfVM.TransferAmount,
                                Currency = tfVM.TransferCurrency,
                                TimeSent = DateTime.Now,
                            });
                        }
                        recName = "";
                        recName = userslist[i].FirstName + " " + userslist[i].LastName;
                        string sName = getName(accesscode);
                        string text = "You have recieved " + tfVM.TransferCurrency + " " + tfVM.TransferAmount + " from " + sName + " on " + DateTime.Now.ToString("f");
                        string OTPtype = teleContext.getOTPType(accesscode);
                        if (OTPtype == "SMS")
                        {
                            TwilioClient.Init(apiKey, apiSecret, accountSid);
                            var message = MessageResource.Create(
                            body: text,
                            from: new Twilio.Types.PhoneNumber("+19377779542"),
                            to: new Twilio.Types.PhoneNumber("+65" + tfVM.PhoneNumber));
                        }
                        else if (OTPtype == null || OTPtype == "Telegram")
                        {
                            sendMessage(Convert.ToString(userslist[i].TelegramChatID), text);
                        }

                    }
                }
                for (int i = 0; i < userslist.Count; i++)
                {
                    int ppC = 0;
                    if (userslist[i].AccessCode == accesscode)
                    {
                        for (int j = 0; j < userslist[i].AccountsList.Count; j++)
                        {
                            if (Convert.ToString(userslist[i].AccountsList[j].AccountNumber) == tfVM.From_AccountNumber)
                            {
                                if (userslist[i].AccountsList[j].AmountAvaliable > tfVM.TransferAmount && userslist[i].AccountsList[j].AmountRemaining > tfVM.TransferAmount)
                                {
                                    for (int p = 0; p < userslist.Count; p++)
                                    {
                                        if (userslist[p].PhoneNumber == tfVM.PhoneNumber)
                                        {
                                            ppC = p;                                                                                        
                                        }
                                    }
                                    double convertedAmount = currContext.convertCurrency(tfVM.TransferAmount, tfVM.TransferCurrency, userslist[i].AccountsList[j].AccountCurrency);
                                    double BPtransferS = userslist[i].AccountsList[j].AmountAvaliable;
                                    double BPtransferRS = userslist[i].AccountsList[j].AmountRemaining;
                                    userslist[i].AccountsList[j].AmountAvaliable -= convertedAmount;
                                    userslist[i].AccountsList[j].AmountRemaining -= convertedAmount;
                                    updateDB(userslist);
                                    List<User> checklist = loginContext.retrieveUserList();
                                    for (int k = 0; k < checklist.Count; k++)
                                    {
                                        for (int l = 0; l < checklist[l].AccountsList.Count; l++)
                                        {
                                            if (Convert.ToString(checklist[k].AccountsList[l].AccountNumber) == tfVM.From_AccountNumber)
                                            {
                                                if (checklist[k].AccountsList[l].AmountAvaliable == BPtransferS - convertedAmount)
                                                {
                                                    if (checklist[k].AccountsList[l].AmountRemaining == BPtransferRS - convertedAmount)
                                                    {
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        userslist[ppC].AccountsList[0].AmountAvaliable = userslist[ppC].AccountsList[0].AmountAvaliable - convertedAmount;
                                                        userslist[ppC].AccountsList[0].AmountRemaining = userslist[ppC].AccountsList[0].AmountRemaining - convertedAmount;
                                                        userslist[i].AccountsList[j].AmountAvaliable = BPtransferS;
                                                        userslist[i].AccountsList[j].AmountRemaining = BPtransferRS;
                                                        updateDB(userslist);
                                                        return "tfail";
                                                    }
                                                }
                                                else
                                                {
                                                    userslist[ppC].AccountsList[0].AmountAvaliable = userslist[ppC].AccountsList[0].AmountAvaliable - convertedAmount;
                                                    userslist[ppC].AccountsList[0].AmountRemaining = userslist[ppC].AccountsList[0].AmountRemaining - convertedAmount;
                                                    userslist[i].AccountsList[j].AmountAvaliable = BPtransferS;
                                                    userslist[i].AccountsList[j].AmountRemaining = BPtransferRS;
                                                    updateDB(userslist);
                                                    return "tfail";
                                                }
                                            }
                                        }
                                    }
                                    int to_user = Get_ToAccount(Convert.ToInt32(tfVM.PhoneNumber));
                                    if (userslist[i].TransactionList == null)
                                    {
                                        List<Transaction> transactionlist = new List<Transaction>();
                                        transactionlist.Add(new Transaction
                                        {
                                            To_AccountNumber = Convert.ToInt32(userslist[to_user].AccountsList[0].AccountNumber),
                                            From_AccountNumber = Convert.ToInt32(tfVM.From_AccountNumber),
                                            Amount = tfVM.TransferAmount,
                                            Currency = tfVM.TransferCurrency,
                                            TimeSent = DateTime.Now,
                                        });
                                        userslist[i].TransactionList = transactionlist;
                                    }
                                    else
                                    {
                                        userslist[i].TransactionList.Add(new Transaction
                                        {
                                            To_AccountNumber = Convert.ToInt32(userslist[to_user].AccountsList[0].AccountNumber),
                                            From_AccountNumber = Convert.ToInt32(tfVM.From_AccountNumber),
                                            Amount = tfVM.TransferAmount,
                                            Currency = tfVM.TransferCurrency,
                                            TimeSent = DateTime.Now,
                                        });
                                    }
                                    string text = "You have sent " + tfVM.TransferCurrency + " " + tfVM.TransferAmount + " to " + recName + " on " + DateTime.Now.ToString("f");
                                    string OTPtype = teleContext.getOTPType(accesscode);
                                    if (OTPtype == "SMS")
                                    {
                                        int phoneno = Convert.ToInt32(teleContext.getPhoneNumber(accesscode));
                                        TwilioClient.Init(apiKey, apiSecret, accountSid);
                                        var message = MessageResource.Create(
                                        body: text,
                                        from: new Twilio.Types.PhoneNumber("+19377779542"),
                                        to: new Twilio.Types.PhoneNumber("+65" + phoneno));
                                    }
                                    else if (OTPtype == null || OTPtype == "Telegram")
                                    {
                                        sendMessage(Convert.ToString(userslist[i].TelegramChatID), text);
                                    }

                                }
                            }
                        }
                    }
                }

            }

            return "true";
        }
        public int Get_ToAccount(int phNo)
        {
            List<User> userslist = loginContext.retrieveUserList();
            for (int i = 0; i < userslist.Count; i++)
            {
                if (Convert.ToInt32(userslist[i].PhoneNumber) == phNo)
                {
                    return Convert.ToInt32(i);
                }
            }
            return 0;
        }
        public bool updateDB(List<User> userlist)
        {
            ifclient = new FireSharp.FirebaseClient(ifc);
            if (ifclient != null)
            {
                ifclient.Set("User/", userlist);
            }
            return true;
        }
    }
}
