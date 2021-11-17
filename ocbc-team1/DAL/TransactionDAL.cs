using FireSharp.Config;
using FireSharp.Interfaces;
using ocbc_team1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.DAL
{
    public class TransactionDAL
    {
        private LoginDAL loginContext = new LoginDAL();

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

        public void transferFunds(TransferViewModel tfVM, string accesscode)
        {
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
