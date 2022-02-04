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
        string accountSid = "AC33d8de9089a6d0c154358213b4772ebf";
        string apiKey = "SK754a190e66db43863ae52ebea4c88b82";
        string apiSecret = "GESQ4q7mWcypxwHAycBg8o2CaQdr0oaZ";

        IFirebaseConfig ifc = new FirebaseConfig()
        {
            AuthSecret = "Sa3cJdieiAEHpAPK7Z243SRtpxia29x6gzwaoz1g",
            BasePath = "https://failsafefundtransfer-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };
        IFirebaseClient ifclient;
        public List<BankAccount> getBankAccountList(string accesscode)
        {
            List<User> userlist = loginContext.retrieveUserList();
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
            catch (Exception e)
            {
                return false;
            }

        }

        public bool checkSenderFunds(string accesscode, string AccountNumber, double TransferAmount)
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
                            if (userslist[i].AccountsList[j].AmountAvaliable > TransferAmount && userslist[i].AccountsList[j].AmountRemaining > TransferAmount)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
        public void scheduledTransferFunds(TransferViewModel tfVM)
        {
            ifclient = new FireSharp.FirebaseClient(ifc);
            if (ifclient != null)
            {
                ifclient.Set("ScheduledTransaction/", tfVM);
            }
        }
        public string transferFunds(TransferViewModel tfVM, string accesscode)
        {
            tfVM.TransferAmount = Math.Round(tfVM.TransferAmount, 2);
            //if (tfVM.To_AccountNumber != null && tfVM.PhoneNumber != null) { Console.WriteLine("Two transfer type has been input, transfer canceled"); return; }
            List<User> userslist = loginContext.retrieveUserList();
            List<User> checkerlist = loginContext.retrieveUserList();
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
                            double Btransfer = userslist[i].AccountsList[j].AmountAvaliable;
                            double BtransferR = userslist[i].AccountsList[j].AmountRemaining;
                            userslist[i].AccountsList[j].AmountAvaliable += tfVM.TransferAmount;
                            //userslist[i].AccountsList[j].AmountRemaining += tfVM.TransferAmount;
                            updateDB(userslist);
                            List<User> checklist = loginContext.retrieveUserList();
                            if (checklist == null) { Console.WriteLine("checklist null, transfer failed"); return "ufail"; }
                            for (int k = 0; k < checklist.Count; k++)
                            {
                                for (int l = 0; l < checklist[l].AccountsList.Count; l++)
                                {
                                    if (Convert.ToString(checklist[k].AccountsList[l].AccountNumber) == tfVM.To_AccountNumber)
                                    {
                                        if (checklist[k].AccountsList[l].AmountAvaliable == Btransfer + tfVM.TransferAmount)
                                        {
                                            if (checklist[k].AccountsList[l].AmountRemaining == BtransferR + tfVM.TransferAmount)
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
                            updateDB(userslist);                          
                            recName = "";
                            recName = userslist[i].FirstName + " " + userslist[i].LastName;
                            string sName = getName(accesscode);
                            string text = "You have recieved " + "$" + tfVM.TransferAmount + " from " + sName + " on " + DateTime.Now.ToString("f");
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
                                    double BtransferS = userslist[i].AccountsList[j].AmountAvaliable;
                                    double BtransferRS = userslist[i].AccountsList[j].AmountRemaining;
                                    userslist[i].AccountsList[j].AmountAvaliable -= tfVM.TransferAmount;
                                    userslist[i].AccountsList[j].AmountRemaining -= tfVM.TransferAmount;
                                    updateDB(userslist);
                                    List<User> checklist = loginContext.retrieveUserList();
                                    for (int k = 0; k < checklist.Count; k++)
                                    {
                                        for (int l = 0; l < checklist[l].AccountsList.Count; l++)
                                        {
                                            if (Convert.ToString(checklist[k].AccountsList[l].AccountNumber) == tfVM.From_AccountNumber)
                                            {
                                                if (checklist[k].AccountsList[l].AmountAvaliable == BtransferS - tfVM.TransferAmount)
                                                {
                                                    if (checklist[k].AccountsList[l].AmountRemaining == BtransferRS - tfVM.TransferAmount)
                                                    {
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        userslist[pC].AccountsList[oC].AmountAvaliable = userslist[pC].AccountsList[oC].AmountAvaliable - tfVM.TransferAmount;
                                                        userslist[pC].AccountsList[oC].AmountRemaining = userslist[pC].AccountsList[oC].AmountRemaining - tfVM.TransferAmount;
                                                        userslist[i].AccountsList[j].AmountAvaliable = BtransferS;
                                                        userslist[i].AccountsList[j].AmountRemaining = BtransferRS;
                                                        updateDB(userslist);
                                                        return "tfail";
                                                    }
                                                }
                                                else
                                                {
                                                    userslist[pC].AccountsList[oC].AmountAvaliable = userslist[pC].AccountsList[oC].AmountAvaliable - tfVM.TransferAmount;
                                                    userslist[pC].AccountsList[oC].AmountRemaining = userslist[pC].AccountsList[oC].AmountRemaining - tfVM.TransferAmount;
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
                                    updateDB(userslist);                                    
                                    string text = "You have sent " + "$" + tfVM.TransferAmount + " to " + recName + " on " + DateTime.Now.ToString("f");
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
                        double Btransfer = userslist[i].AccountsList[0].AmountAvaliable;
                        double BtransferR = userslist[i].AccountsList[0].AmountRemaining;
                        userslist[i].AccountsList[0].AmountAvaliable += tfVM.TransferAmount;
                        userslist[i].AccountsList[0].AmountRemaining += tfVM.TransferAmount;
                        updateDB(userslist);
                        List<User> checklist = loginContext.retrieveUserList();
                        for (int k = 0; k < checklist.Count; k++)
                        {

                            if (checklist[k].PhoneNumber == tfVM.PhoneNumber)
                            {
                                if (checklist[k].AccountsList[0].AmountAvaliable == Btransfer + tfVM.TransferAmount)
                                {
                                    if (checklist[k].AccountsList[0].AmountAvaliable == BtransferR + tfVM.TransferAmount)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        userslist[i].AccountsList[0].AmountAvaliable = Btransfer;
                                        userslist[i].AccountsList[0].AmountRemaining = BtransferR;
                                        updateDB(userslist);
                                        return "tfail";
                                    }
                                }
                                else
                                {
                                    userslist[i].AccountsList[0].AmountAvaliable = Btransfer;
                                    userslist[i].AccountsList[0].AmountRemaining = BtransferR;
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
                                Currency = "SGD",
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
                                Currency = "SGD",
                                TimeSent = DateTime.Now,
                            });
                        }
                        updateDB(userslist);                       
                        recName = "";
                        recName = userslist[i].FirstName + " " + userslist[i].LastName;
                        string sName = getName(accesscode);
                        string text = "You have recieved " + "$" + tfVM.TransferAmount + " from " + sName + " on " + DateTime.Now.ToString("f");
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
                                    double BtransferS = userslist[i].AccountsList[j].AmountAvaliable;
                                    double BtransferRS = userslist[i].AccountsList[j].AmountRemaining;
                                    userslist[i].AccountsList[j].AmountAvaliable -= tfVM.TransferAmount;
                                    userslist[i].AccountsList[j].AmountRemaining -= tfVM.TransferAmount;
                                    updateDB(userslist);
                                    List<User> checklist = loginContext.retrieveUserList();
                                    for (int k = 0; k < checklist.Count; k++)
                                    {
                                        for (int l = 0; l < checklist[l].AccountsList.Count; l++)
                                        {
                                            if (Convert.ToString(checklist[k].AccountsList[l].AccountNumber) == tfVM.From_AccountNumber)
                                            {
                                                if (checklist[k].AccountsList[l].AmountAvaliable == BtransferS - tfVM.TransferAmount)
                                                {
                                                    if (checklist[k].AccountsList[l].AmountRemaining == BtransferRS - tfVM.TransferAmount)
                                                    {
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        userslist[ppC].AccountsList[0].AmountAvaliable = userslist[ppC].AccountsList[0].AmountAvaliable - tfVM.TransferAmount;
                                                        userslist[ppC].AccountsList[0].AmountRemaining = userslist[ppC].AccountsList[0].AmountRemaining - tfVM.TransferAmount;
                                                        userslist[i].AccountsList[j].AmountAvaliable = BtransferS;
                                                        userslist[i].AccountsList[j].AmountRemaining = BtransferRS;
                                                        updateDB(userslist);
                                                        return "tfail";
                                                    }
                                                }
                                                else
                                                {
                                                    userslist[ppC].AccountsList[0].AmountAvaliable = userslist[ppC].AccountsList[0].AmountAvaliable - tfVM.TransferAmount;
                                                    userslist[ppC].AccountsList[0].AmountRemaining = userslist[ppC].AccountsList[0].AmountRemaining - tfVM.TransferAmount;
                                                    userslist[i].AccountsList[j].AmountAvaliable = BtransferS;
                                                    userslist[i].AccountsList[j].AmountRemaining = BtransferRS;
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
                                            Currency = "SGD",
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
                                            Currency = "SGD",
                                            TimeSent = DateTime.Now,
                                        });
                                    }
                                    updateDB(userslist);                                   
                                    string text = "You have sent " + "$" + tfVM.TransferAmount + " to " + recName + " on " + DateTime.Now.ToString("f");
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
            List<User> userlist = uList();
            if (detectFailsafe(accesscode, checkerlist, tfVM.From_AccountNumber, userlist) == true)
            {
                return "true";
            }
            else
            {                              
                return "false";
            }

                           
            
        }
        public List<User> uList()
        {
            List<User> userList = new List<User>();
            ifclient = new FireSharp.FirebaseClient(ifc);
            if (ifclient != null)
            {
                FirebaseResponse firebaseresponse = ifclient.Get("User");
                userList = firebaseresponse.ResultAs<List<User>>();
            }
            return userList;
            
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
        
        public bool detectFailsafe(string accesscode, List<User> checkerlist, string faccn, List<User> userlist)
        {
            double amt = 0;
            string tacc = "";
            double BSTransfer = 0;
            double BSTransferR = 0;
            double BRTransfer = 0;
            double BRTransferR = 0;
            int pC = 0;
            int oC = 0;
            int pCC = 0;
            int oCC = 0;            
            for (int i = 0; i < userlist.Count; i++)
            {
                if (userlist[i].AccessCode == accesscode)
                {
                    int tc = userlist[i].TransactionList.Count;
                    int tcc = tc - 1;
                    for (int j = 0; j < tc; j++)
                    {
                        if (Convert.ToString(userlist[i].TransactionList[tcc].From_AccountNumber) == faccn)
                        {
                            amt = userlist[i].TransactionList[tcc].Amount;
                            tacc = Convert.ToString(userlist[i].TransactionList[tcc].To_AccountNumber);
                        }
                    }
                    for (int j = 0; j < checkerlist[i].AccountsList.Count; j++)
                    {
                        if (Convert.ToString(checkerlist[i].AccountsList[j].AccountNumber) == faccn)
                        {
                            BSTransfer = checkerlist[i].AccountsList[j].AmountAvaliable;
                            BSTransferR = checkerlist[i].AccountsList[j].AmountRemaining;
                        }                       

                    }
                    
                }
            }
            for (int i = 0; i < userlist.Count; i++)
            {
                for (int j = 0; j < checkerlist[i].AccountsList.Count; j++)
                {
                    if (Convert.ToString(checkerlist[i].AccountsList[j].AccountNumber) == tacc)
                    {
                        BRTransfer = checkerlist[i].AccountsList[j].AmountAvaliable;
                        BRTransferR = checkerlist[i].AccountsList[j].AmountRemaining;
                    }
                }
            }
            for (int p = 0; p < userlist.Count; p++)
            {
                for (int o = 0; o < userlist[p].AccountsList.Count; o++)
                {
                    if (Convert.ToString(userlist[p].AccountsList[o].AccountNumber) == tacc)
                    {
                        pCC = p;
                        oCC = o;
                    }
                }
            }
            for (int p = 0; p < userlist.Count; p++)
            {
                for (int o = 0; o < userlist[p].AccountsList.Count; o++)
                {
                    if (Convert.ToString(userlist[p].AccountsList[o].AccountNumber) == faccn)
                    {
                        pC = p;
                        oC = o;
                    }
                }
            }
            for (int i = 0; i < userlist.Count; i++)
            {
                if (userlist[i].AccessCode == accesscode)
                {
                    for (int j = 0; j < userlist[i].AccountsList.Count; j++)
                    {
                        if (Convert.ToString(userlist[i].AccountsList[j].AccountNumber) == faccn)
                        {
                            if (userlist[i].AccountsList[j].AmountAvaliable == BSTransfer - amt)
                            {
                                if (userlist[i].AccountsList[j].AmountRemaining == BSTransferR - amt)
                                {
                                    continue;
                                }
                                else
                                {
                                    userlist[pCC].AccountsList[oCC].AmountAvaliable = BRTransfer;
                                    userlist[pCC].AccountsList[oCC].AmountRemaining = BRTransferR;
                                    userlist[i].AccountsList[j].AmountAvaliable = BSTransfer;
                                    userlist[i].AccountsList[j].AmountRemaining = BSTransferR;
                                    for (int l = 0; l < userlist.Count; l++)
                                    {
                                        if (userlist[l].AccessCode == accesscode)
                                        {
                                            int tc = userlist[l].TransactionList.Count;
                                            for (int p = 0; p < tc; p++)
                                            {
                                                if (Convert.ToString(userlist[l].TransactionList.Last().From_AccountNumber) == faccn)
                                                {
                                                   userlist[l].TransactionList.RemoveAt(userlist[l].TransactionList.Count - 1);
                                                }
                                            }
                                        }
                                    }
                                    int tc2 = userlist[pCC].TransactionList.Count;
                                    userlist[pCC].TransactionList.RemoveAt(userlist[pCC].TransactionList.Count - 1);
                                    updateDB(userlist);
                                    for (int ii = 0; ii < userlist.Count; ii++)
                                    {
                                        for (int jj = 0; jj < userlist[ii].AccountsList.Count; jj++)
                                        {
                                            if (Convert.ToString(userlist[ii].AccountsList[jj].AccountNumber) == tacc)
                                            {
                                                string sName = getName(accesscode);
                                                string text = "Your previous transfer of " + "$" + amt + " recieved from " + sName + " on " + DateTime.Now.ToString("f") + " has been reverted due to an error";
                                                string OTPtype = teleContext.getOTPType(accesscode);
                                                if (OTPtype == "SMS")
                                                {
                                                    TwilioClient.Init(apiKey, apiSecret, accountSid);
                                                    var message = MessageResource.Create(
                                                    body: text,
                                                    from: new Twilio.Types.PhoneNumber("+19377779542"),
                                                    to: new Twilio.Types.PhoneNumber("+65" + userlist[ii].PhoneNumber));
                                                }
                                                else if (OTPtype == null || OTPtype == "Telegram")
                                                {
                                                    sendMessage(Convert.ToString(userlist[ii].TelegramChatID), text);
                                                }
                                            }
                                        }
                                    }
                                    for (int ii = 0; ii < userlist.Count; ii++)
                                    {
                                        for (int jj = 0; jj < userlist[ii].AccountsList.Count; jj++)
                                        {
                                            if (Convert.ToString(userlist[ii].AccountsList[jj].AccountNumber) == faccn)
                                            {
                                                string sName = getName(accesscode);
                                                string text = "Your previous transfer of " + "$" + amt + " on " + DateTime.Now.ToString("f") + " has been reverted due to an error";
                                                string OTPtype = teleContext.getOTPType(accesscode);
                                                if (OTPtype == "SMS")
                                                {
                                                    TwilioClient.Init(apiKey, apiSecret, accountSid);
                                                    var message = MessageResource.Create(
                                                    body: text,
                                                    from: new Twilio.Types.PhoneNumber("+19377779542"),
                                                    to: new Twilio.Types.PhoneNumber("+65" + userlist[ii].PhoneNumber));
                                                }
                                                else if (OTPtype == null || OTPtype == "Telegram")
                                                {
                                                    sendMessage(Convert.ToString(userlist[ii].TelegramChatID), text);
                                                }
                                            }
                                        }
                                    }
                                    return false;
                                }
                            }
                            else
                            {
                                userlist[pCC].AccountsList[oCC].AmountAvaliable = BRTransfer;
                                userlist[pCC].AccountsList[oCC].AmountRemaining = BRTransferR;
                                userlist[i].AccountsList[j].AmountAvaliable = BSTransfer;
                                userlist[i].AccountsList[j].AmountRemaining = BSTransferR;
                                for (int l = 0; l < userlist.Count; l++)
                                {
                                    if (userlist[l].AccessCode == accesscode)
                                    {
                                        int tc = userlist[l].TransactionList.Count;
                                        for (int p = 0; p < tc; p++)
                                        {
                                            if (Convert.ToString(userlist[l].TransactionList.Last().From_AccountNumber) == faccn)
                                            {
                                                userlist[l].TransactionList.RemoveAt(userlist[l].TransactionList.Count - 1);
                                            }
                                        }
                                    }
                                }
                                int tc2 = userlist[pCC].TransactionList.Count;
                                userlist[pCC].TransactionList.RemoveAt(userlist[pCC].TransactionList.Count - 1);
                                updateDB(userlist);
                                for (int ii = 0; ii < userlist.Count; ii++)
                                {
                                    for (int jj = 0; jj < userlist[ii].AccountsList.Count; jj++)
                                    {
                                        if (Convert.ToString(userlist[ii].AccountsList[jj].AccountNumber) == tacc)
                                        {
                                            string sName = getName(accesscode);
                                            string text = "Your previous transfer of " + "$" + amt + " recieved from " + sName + " on " + DateTime.Now.ToString("f") + " has been reverted due to an error";
                                            string OTPtype = teleContext.getOTPType(accesscode);
                                            if (OTPtype == "SMS")
                                            {
                                                TwilioClient.Init(apiKey, apiSecret, accountSid);
                                                var message = MessageResource.Create(
                                                body: text,
                                                from: new Twilio.Types.PhoneNumber("+19377779542"),
                                                to: new Twilio.Types.PhoneNumber("+65" + userlist[ii].PhoneNumber));
                                            }
                                            else if (OTPtype == null || OTPtype == "Telegram")
                                            {
                                                sendMessage(Convert.ToString(userlist[ii].TelegramChatID), text);
                                            }
                                        }
                                    }
                                }
                                for (int ii = 0; ii < userlist.Count; ii++)
                                {
                                    for (int jj = 0; jj < userlist[ii].AccountsList.Count; jj++)
                                    {
                                        if (Convert.ToString(userlist[ii].AccountsList[jj].AccountNumber) == faccn)
                                        {
                                            string sName = getName(accesscode);
                                            string text = "Your previous transfer of " + "$" + amt + " on " + DateTime.Now.ToString("f") + " has been reverted due to an error";
                                            string OTPtype = teleContext.getOTPType(accesscode);
                                            if (OTPtype == "SMS")
                                            {
                                                TwilioClient.Init(apiKey, apiSecret, accountSid);
                                                var message = MessageResource.Create(
                                                body: text,
                                                from: new Twilio.Types.PhoneNumber("+19377779542"),
                                                to: new Twilio.Types.PhoneNumber("+65" + userlist[ii].PhoneNumber));
                                            }
                                            else if (OTPtype == null || OTPtype == "Telegram")
                                            {
                                                sendMessage(Convert.ToString(userlist[ii].TelegramChatID), text);
                                            }
                                        }
                                    }
                                }
                                return false;
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < userlist.Count; i++)
            {
                for (int j = 0; j < userlist[i].AccountsList.Count; j++)
                {
                    if (Convert.ToString(userlist[i].AccountsList[j].AccountNumber) == tacc)
                    {
                        if (userlist[i].AccountsList[j].AmountAvaliable == BRTransfer + amt)
                        {
                            if (userlist[i].AccountsList[j].AmountRemaining == BRTransferR + amt)
                            {
                                continue;
                            }
                            else
                            {
                                userlist[i].AccountsList[j].AmountAvaliable = BRTransfer;
                                userlist[i].AccountsList[j].AmountRemaining = BRTransferR;
                                userlist[pC].AccountsList[oC].AmountAvaliable = BSTransfer;
                                userlist[pC].AccountsList[oC].AmountRemaining = BSTransferR;
                                for (int l = 0; l < userlist.Count; l++)
                                {
                                    if (userlist[l].AccessCode == accesscode)
                                    {
                                        int tc = userlist[l].TransactionList.Count;
                                        for (int p = 0; p < tc; p++)
                                        {
                                            if (Convert.ToString(userlist[l].TransactionList.Last().From_AccountNumber) == faccn)
                                            {
                                                userlist[l].TransactionList.RemoveAt(userlist[l].TransactionList.Count - 1);
                                            }
                                        }
                                    }
                                }
                                int tc2 = userlist[pC].TransactionList.Count;
                                userlist[pC].TransactionList.RemoveAt(userlist[pC].TransactionList.Count - 1);
                                updateDB(userlist);
                                for (int ii = 0; ii < userlist.Count; ii++)
                                {
                                    for (int jj = 0; jj < userlist[ii].AccountsList.Count; jj++)
                                    {
                                        if (Convert.ToString(userlist[ii].AccountsList[jj].AccountNumber) == tacc)
                                        {
                                            string sName = getName(accesscode);
                                            string text = "Your previous transfer of " + "$" + amt + " recieved from " + sName + " on " + DateTime.Now.ToString("f") + " has been reverted due to an error";
                                            string OTPtype = teleContext.getOTPType(accesscode);
                                            if (OTPtype == "SMS")
                                            {
                                                TwilioClient.Init(apiKey, apiSecret, accountSid);
                                                var message = MessageResource.Create(
                                                body: text,
                                                from: new Twilio.Types.PhoneNumber("+19377779542"),
                                                to: new Twilio.Types.PhoneNumber("+65" + userlist[ii].PhoneNumber));
                                            }
                                            else if (OTPtype == null || OTPtype == "Telegram")
                                            {
                                                sendMessage(Convert.ToString(userlist[ii].TelegramChatID), text);
                                            }
                                        }
                                    }
                                }
                                for (int ii = 0; ii < userlist.Count; ii++)
                                {
                                    for (int jj = 0; jj < userlist[ii].AccountsList.Count; jj++)
                                    {
                                        if (Convert.ToString(userlist[ii].AccountsList[jj].AccountNumber) == faccn)
                                        {
                                            string sName = getName(accesscode);
                                            string text = "Your previous transfer of " + "$" + amt + " on " + DateTime.Now.ToString("f") + " has been reverted due to an error";
                                            string OTPtype = teleContext.getOTPType(accesscode);
                                            if (OTPtype == "SMS")
                                            {
                                                TwilioClient.Init(apiKey, apiSecret, accountSid);
                                                var message = MessageResource.Create(
                                                body: text,
                                                from: new Twilio.Types.PhoneNumber("+19377779542"),
                                                to: new Twilio.Types.PhoneNumber("+65" + userlist[ii].PhoneNumber));
                                            }
                                            else if (OTPtype == null || OTPtype == "Telegram")
                                            {
                                                sendMessage(Convert.ToString(userlist[ii].TelegramChatID), text);
                                            }
                                        }
                                    }
                                }
                                return false;
                            }
                        }
                        else
                        {
                            userlist[i].AccountsList[j].AmountAvaliable = BRTransfer;
                            userlist[i].AccountsList[j].AmountRemaining = BRTransferR;
                            userlist[pC].AccountsList[oC].AmountAvaliable = BSTransfer;
                            userlist[pC].AccountsList[oC].AmountRemaining = BSTransferR;
                            for (int l = 0; l < userlist.Count; l++)
                            {
                                if (userlist[l].AccessCode == accesscode)
                                {
                                    int tc = userlist[l].TransactionList.Count;
                                    for (int p = 0; p < tc; p++)
                                    {
                                        if (Convert.ToString(userlist[l].TransactionList.Last().From_AccountNumber) == faccn)
                                        {
                                            userlist[l].TransactionList.RemoveAt(userlist[l].TransactionList.Count - 1);
                                        }
                                    }
                                }
                            }
                            int tc2 = userlist[pC].TransactionList.Count;
                            userlist[pC].TransactionList.RemoveAt(userlist[pC].TransactionList.Count - 1);
                            updateDB(userlist);
                            for (int ii = 0; ii < userlist.Count; ii++)
                            {
                                for (int jj = 0; jj < userlist[ii].AccountsList.Count; jj++)
                                {
                                    if (Convert.ToString(userlist[ii].AccountsList[jj].AccountNumber) == tacc)
                                    {
                                        string sName = getName(accesscode);
                                        string text = "Your previous transfer of " + "$" + amt + " recieved from " + sName + " on " + DateTime.Now.ToString("f") + " has been reverted due to an error";
                                        string OTPtype = teleContext.getOTPType(accesscode);
                                        if (OTPtype == "SMS")
                                        {
                                            TwilioClient.Init(apiKey, apiSecret, accountSid);
                                            var message = MessageResource.Create(
                                            body: text,
                                            from: new Twilio.Types.PhoneNumber("+19377779542"),
                                            to: new Twilio.Types.PhoneNumber("+65" + userlist[ii].PhoneNumber));
                                        }
                                        else if (OTPtype == null || OTPtype == "Telegram")
                                        {
                                            sendMessage(Convert.ToString(userlist[ii].TelegramChatID), text);
                                        }
                                    }
                                }
                            }
                            for (int ii = 0; ii < userlist.Count; ii++)
                            {
                                for (int jj = 0; jj < userlist[ii].AccountsList.Count; jj++)
                                {
                                    if (Convert.ToString(userlist[ii].AccountsList[jj].AccountNumber) == faccn)
                                    {
                                        string sName = getName(accesscode);
                                        string text = "Your previous transfer of " + "$" + amt + " on " + DateTime.Now.ToString("f") + " has been reverted due to an error";
                                        string OTPtype = teleContext.getOTPType(accesscode);
                                        if (OTPtype == "SMS")
                                        {
                                            TwilioClient.Init(apiKey, apiSecret, accountSid);
                                            var message = MessageResource.Create(
                                            body: text,
                                            from: new Twilio.Types.PhoneNumber("+19377779542"),
                                            to: new Twilio.Types.PhoneNumber("+65" + userlist[ii].PhoneNumber));
                                        }
                                        else if (OTPtype == null || OTPtype == "Telegram")
                                        {
                                            sendMessage(Convert.ToString(userlist[ii].TelegramChatID), text);
                                        }
                                    }
                                }
                            }
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
