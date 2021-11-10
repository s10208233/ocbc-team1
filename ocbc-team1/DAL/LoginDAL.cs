using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using ocbc_team1.Models;
using Newtonsoft.Json;

namespace ocbc_team1.DAL
{
    public class LoginDAL
    {
        IFirebaseConfig ifc = new FirebaseConfig()
        {
            AuthSecret = "Sa3cJdieiAEHpAPK7Z243SRtpxia29x6gzwaoz1g",
            BasePath = "https://failsafefundtransfer-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };
        IFirebaseClient ifclient;

        public List<User> retrieveUserList()
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
        public User retrieveUserByAccesscode(string accesscode)
        {
            ifclient = new FireSharp.FirebaseClient(ifc);
            if (ifclient != null)
            {
                FirebaseResponse firebaseresponse = ifclient.Get("User/" + accesscode);
                User u = firebaseresponse.ResultAs<User>();
                return u;
            }
            return null;
        }
        public List<string> existingAccessCodeList(List<User> userlist)
        {
            List<string> accesscodelist = new List<string>();
            if (userlist != null)
            {
                foreach (User u in userlist)
                {
                    accesscodelist.Add(u.AccessCode);
                }
            }
            return accesscodelist;
        }

    }
}
