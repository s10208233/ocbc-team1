using FireSharp.Config;
using FireSharp.Interfaces;
using ocbc_team1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.DAL
{
    public class NewBankAccountDAL
    {
        private LoginDAL loginContext = new LoginDAL();

        IFirebaseConfig ifc = new FirebaseConfig()
        {
            AuthSecret = "Sa3cJdieiAEHpAPK7Z243SRtpxia29x6gzwaoz1g",
            BasePath = "https://failsafefundtransfer-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };
        IFirebaseClient ifclient;

        public void createNewBankAccount(NewBankAccountViewModel nbaVM, string accesscode)
        {
            List<User> userslist = loginContext.retrieveUserList();
            if (userslist == null) { Console.WriteLine("uselist null, transfer failed"); return; }

            for (int i = 0; i < userslist.Count; i++)
            {
                if (userslist[i].AccessCode == accesscode)
                {
                    Random rnd = new Random();

                    int newbankaccountnumber = rnd.Next(000000000, 999999999);
                    foreach (User u in loginContext.retrieveUserList())
                    {
                        foreach (BankAccount ba in u.AccountsList)
                        {
                            if (ba.AccountNumber == newbankaccountnumber) { newbankaccountnumber = rnd.Next(000000000, 999999999); }
                        }
                    }

                    userslist[i].AccountsList.Add(new BankAccount()
                    {
                        AccountNumber = newbankaccountnumber,
                        AccountType = nbaVM.AccountType,
                        AmountAvaliable = nbaVM.AmountRemaining,
                        AmountRemaining = nbaVM.AmountRemaining,
                        AccountCurrency = "SGD",
                        CreationDate = DateTime.Today
                    });
                }
            }
            // update firebase
            ifclient = new FireSharp.FirebaseClient(ifc);
            if (ifclient != null)
            {
                ifclient.Set("User/", userslist);
            }
        }
    }
}
