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
                    if (Convert.ToString(userslist[i].AccountsList[j].AccountNumber) == tfVM.To_AccountNumber)
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

            //  Recipient
            for (int i = 0; i < userslist.Count; i++)
            {
                for (int j = 0; j < userslist[i].AccountsList.Count; j++)
                {
                    if (Convert.ToString(userslist[i].AccountsList[j].AccountNumber) == tfVM.To_AccountNumber)
                    {

                        userslist[i].AccountsList[j].AmountAvaliable += tfVM.TransferAmount;
                        userslist[i].AccountsList[j].AmountRemaining += tfVM.TransferAmount;

                    }
                }
            }
             // Sender
            for (int i=0; i < userslist.Count; i++)
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
                            }
                        }
                    }
                }
            }
            ifclient = new FireSharp.FirebaseClient(ifc);
            // update firebase
            if (ifclient != null)
            {
                ifclient.Set("User/", userslist);
            }
        }

    }
}
