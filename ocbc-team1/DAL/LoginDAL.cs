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

        public List<TestUser> LoginList()
        {
            List<TestUser> userList = new List<TestUser>();
            ifclient = new FireSharp.FirebaseClient(ifc);
            if (ifclient != null)
            {
                FirebaseResponse firebaseresponse = ifclient.Get("User");
                userList = firebaseresponse.ResultAs<List<TestUser>>();
            }
            return userList;
        }
        //public List<TestUser> LoginList()
        //{
        //    List<TestUser> loginList = new List<TestUser>();
        //    ifclient = new FireSharp.FirebaseClient(ifc);
        //    if (ifclient != null)
        //    {
        //        FirebaseResponse firebaseresponse = ifclient.Get("User");
        //        Dictionary<string, TestUser> data = JsonConvert.DeserializeObject<Dictionary<string, TestUser>>(firebaseresponse.Body.ToString());
        //        foreach (var d in data)
        //        {
        //            loginList.Add(
        //            new TestUser
        //            {
        //                AccessCode = d.Value.AccessCode,
        //                Email = d.Value.Email,
        //                FirstName = d.Value.FirstName,
        //                LastName = d.Value.LastName,
        //                CardNumber = d.Value.CardNumber,
        //                BankPIN = d.Value.BankPIN,
        //                IC = d.Value.IC,
        //                PhoneNumber = d.Value.PhoneNumber

        //            }
        //        );
        //        }
        //    }
        //    return loginList;
        //}

    }
}
