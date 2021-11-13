using ocbc_team1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.DAL
{
    public class TransactionDAL
    {
        LoginDAL loginContext = new LoginDAL();
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
    }
}
