using FireSharp.Config;
using FireSharp.Interfaces;
using ocbc_team1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.DAL
{
    public class TelegramDAL
    {
        private LoginDAL loginContext = new LoginDAL();

        IFirebaseConfig ifc = new FirebaseConfig()
        {
            AuthSecret = "Sa3cJdieiAEHpAPK7Z243SRtpxia29x6gzwaoz1g",
            BasePath = "https://failsafefundtransfer-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };
        IFirebaseClient ifclient;

        //  Functions starts here
        public void setTelegramChatId(string accesscode, int newchatid)
        {
            if (loginContext.retrieveUserList() != null) 
            {
                List<User> userlist = loginContext.retrieveUserList();
                for (int i = 0; i < userlist.Count; i++)
                {
                    if (userlist[i].AccessCode == accesscode)
                    {
                        userlist[i].TelegramChatID = newchatid;
                    }
                }
                if (ifclient != null)
                {
                    ifclient.Set("User/", userlist);
                }
            }
            Console.WriteLine("setTelegramChatID failed, no existing userlist");
        }

        public int? getTelegramChatId(string accesscode)
        {
            if (loginContext.retrieveUserList() != null)
            {
                foreach (User u in loginContext.retrieveUserList())
                {
                    if (u.AccessCode == accesscode)
                    {
                        if (u.TelegramChatID != null)
                        {
                            return u.TelegramChatID;
                        }
                        else
                        {
                            Console.WriteLine(accesscode + " telegram chatID does not exist");
                            return null;
                        }
                    }
                   
                }
                
            }
            Console.WriteLine("getTelegramChatID failed, no existing userlist");
            return null;
        }


    }
}
