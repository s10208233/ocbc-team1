using FireSharp.Config;
using FireSharp.Interfaces;
using ocbc_team1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;

namespace ocbc_team1.DAL
{
    public class TransactionDAL
    {
        static TelegramBotClient Bot = new TelegramBotClient("2106855009:AAEVAKqEbNj6W7GeZoOLkgmF8XgsL7ZvG2o");
        private LoginDAL loginContext = new LoginDAL();
        private string recName = "";

        IFirebaseConfig ifc = new FirebaseConfig()
        {
            AuthSecret = "Sa3cJdieiAEHpAPK7Z243SRtpxia29x6gzwaoz1g",
            BasePath = "https://failsafefundtransfer-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };
        IFirebaseClient ifclient;
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
        public void transferFunds(TransferViewModel tfVM, string accesscode)
        {
            tfVM.TransferAmount = Math.Round(tfVM.TransferAmount, 2);
            //if (tfVM.To_AccountNumber != null && tfVM.PhoneNumber != null) { Console.WriteLine("Two transfer type has been input, transfer canceled"); return; }
            List<User> userslist = loginContext.retrieveUserList();
            if (userslist == null) { Console.WriteLine("uselist null, transfer failed"); return; }
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
                            userslist[i].AccountsList[j].AmountAvaliable += tfVM.TransferAmount;
                            userslist[i].AccountsList[j].AmountRemaining += tfVM.TransferAmount;
                            if (userslist[i].TransactionList == null)
                            {
                                List<Transaction> transactionlist = new List<Transaction>();
                                transactionlist.Add(new Transaction
                                {
                                    To_AccountNumber = Convert.ToInt32(tfVM.To_AccountNumber),
                                    From_AccountNumber = Convert.ToInt32(tfVM.From_AccountNumber),
                                    Amount = tfVM.TransferAmount,
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
                                    TimeSent = DateTime.Now,
                                });
                            }
                            recName = "";
                            recName = userslist[i].FirstName + " " + userslist[i].LastName;
                            string sName = getName(accesscode);
                            string text = "You have recieved " + "$" + tfVM.TransferAmount + " from " + sName + " on " + DateTime.Now.ToString("f");
                            sendMessage(Convert.ToString(userslist[i].TelegramChatID), text);



                        }
                    }
                }
                // Sender
                for (int i = 0; i < userslist.Count; i++)
                {
                    if (userslist[i].AccessCode == accesscode)
                    {
                        for (int j = 0; j < userslist[i].AccountsList.Count; j++)
                        {
                            if (Convert.ToString(userslist[i].AccountsList[j].AccountNumber) == tfVM.From_AccountNumber)
                            {
                                if (userslist[i].AccountsList[j].AmountAvaliable > tfVM.TransferAmount && userslist[i].AccountsList[j].AmountRemaining > tfVM.TransferAmount)
                                {
                                    userslist[i].AccountsList[j].AmountAvaliable -= tfVM.TransferAmount;
                                    userslist[i].AccountsList[j].AmountRemaining -= tfVM.TransferAmount;
                                    if (userslist[i].TransactionList == null)
                                    {
                                        List<Transaction> transactionlist = new List<Transaction>();
                                        transactionlist.Add(new Transaction
                                        {
                                            To_AccountNumber = Convert.ToInt32(tfVM.To_AccountNumber),
                                            From_AccountNumber = Convert.ToInt32(tfVM.From_AccountNumber),
                                            Amount = tfVM.TransferAmount,
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
                                            TimeSent = DateTime.Now,
                                        });
                                    }
                                    string text = "You have sent " + "$" + tfVM.TransferAmount + " to " + recName + " on " + DateTime.Now.ToString("f");
                                    sendMessage(Convert.ToString(userslist[i].TelegramChatID), text);
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

                        userslist[i].AccountsList[0].AmountAvaliable += tfVM.TransferAmount;
                        userslist[i].AccountsList[0].AmountRemaining += tfVM.TransferAmount;
                        if (userslist[i].TransactionList == null)
                        {
                            List<Transaction> transactionlist = new List<Transaction>();
                            transactionlist.Add(new Transaction
                            {
                                To_AccountNumber = Convert.ToInt32(userslist[i].AccountsList[0].AccountNumber),
                                From_AccountNumber = Convert.ToInt32(tfVM.From_AccountNumber),
                                Amount = tfVM.TransferAmount,
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
                                TimeSent = DateTime.Now,
                            });
                        }
                        recName = "";
                        recName = userslist[i].FirstName + " " + userslist[i].LastName;
                        string sName = getName(accesscode);
                        string text = "You have recieved " + "$" + tfVM.TransferAmount + " from " + sName + " on " + DateTime.Now.ToString("f");
                        sendMessage(Convert.ToString(userslist[i].TelegramChatID), text);

                    }
                }
                for (int i = 0; i < userslist.Count; i++)
                {
                    if (userslist[i].AccessCode == accesscode)
                    {
                        for (int j = 0; j < userslist[i].AccountsList.Count; j++)
                        {
                            if (Convert.ToString(userslist[i].AccountsList[j].AccountNumber) == tfVM.From_AccountNumber)
                            {
                                if (userslist[i].AccountsList[j].AmountAvaliable > tfVM.TransferAmount && userslist[i].AccountsList[j].AmountRemaining > tfVM.TransferAmount)
                                {
                                    userslist[i].AccountsList[j].AmountAvaliable -= tfVM.TransferAmount;
                                    userslist[i].AccountsList[j].AmountRemaining -= tfVM.TransferAmount;
                                    int to_user = Get_ToAccount(Convert.ToInt32(tfVM.PhoneNumber));
                                    if (userslist[i].TransactionList == null)
                                    {
                                        List<Transaction> transactionlist = new List<Transaction>();
                                        transactionlist.Add(new Transaction
                                        {
                                            To_AccountNumber = Convert.ToInt32(userslist[to_user].AccountsList[0].AccountNumber),
                                            From_AccountNumber = Convert.ToInt32(tfVM.From_AccountNumber),
                                            Amount = tfVM.TransferAmount,
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
                                            TimeSent = DateTime.Now,
                                        });
                                    }
                                    string text = "You have sent " + "$" + tfVM.TransferAmount + " to " + recName + " on " + DateTime.Now.ToString("f");
                                    sendMessage(Convert.ToString(userslist[i].TelegramChatID), text);

                                }
                            }
                        }
                    }
                }

            }
            

            // update firebase
            ifclient = new FireSharp.FirebaseClient(ifc);
            if (ifclient != null)
            {
                ifclient.Set("User/", userslist);
            }
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
    }
}
