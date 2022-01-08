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
using MailKit.Net.Smtp;
using MimeKit;

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

        // not in use yet
        // UPDATE DO NOT USE retrieveUserDictionary
        //public Dictionary<string, User> retrieveUserDictionary()
        //{
        //    try
        //    {
        //        ifclient = new FireSharp.FirebaseClient(ifc);
        //        if (ifclient != null)
        //        {
        //            FirebaseResponse firebaseresponse = ifclient.Get("User");
        //            if (firebaseresponse == null) { return null; }
        //            Dictionary<string, User> userdictionary = firebaseresponse.ResultAs<Dictionary<string, User>>();
        //            if (userdictionary == null) { return null; }
        //            return userdictionary;
        //        }
        //        return null;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine($"Error Occured, {e.Message}");
        //        return null;
        //    }
        //}

        public List<User> retrieveUserList()
        {
            try
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
            catch(Exception e)
            {
                return null;
            }
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
        public void ResendAccessCode(string rEmail)
        {
            List<User> userslist = retrieveUserList();
            for (int i = 0; i < userslist.Count; i++)
            {
                if (userslist[i].Email == rEmail)
                {
                    string rName = userslist[i].FirstName + userslist[i].LastName;
                    string accesscode = userslist[i].AccessCode;

                    MimeMessage message = new MimeMessage();
                    message.From.Add(new MailboxAddress("OCBC Team 1", "ocbcteam1@gmail.com"));
                    message.To.Add(MailboxAddress.Parse(rEmail));
                    message.Subject = "OCBC Account Access Code";
                    message.Body = new TextPart("html")
                    {
                        Text = "Welcome " + rName + "<br>Thank You For Signing Up at OCBC Bank." +
                        "<br>Here is Your Access Code" +
                        "<br>You will need this code to Log In" +
                        "<br>Access Code: " + accesscode

                    };
                    string emailAddress = "ocbcteam1@gmail.com";
                    string password = "ocbcteam1";
                    SmtpClient client = new SmtpClient();
                    try
                    {
                        client.Connect("smtp.gmail.com", 465, true);
                        client.Authenticate(emailAddress, password);
                        client.Send(message);

                        Console.WriteLine("Email Sent!.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        client.Disconnect(true);
                        client.Dispose();
                    }
                }
            }
        }
    }
}
