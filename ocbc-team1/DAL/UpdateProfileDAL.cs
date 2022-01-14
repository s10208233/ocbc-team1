using CloudinaryDotNet;
using FireSharp.Config;
using FireSharp.Interfaces;
using ocbc_team1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.DAL
{
    public class UpdateProfileDAL
    {
        SignupDAL signupcontext = new SignupDAL();
        LoginDAL logincontext = new LoginDAL();
        IFirebaseConfig ifc = new FirebaseConfig()
        {
            AuthSecret = "Sa3cJdieiAEHpAPK7Z243SRtpxia29x6gzwaoz1g",
            BasePath = "https://failsafefundtransfer-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };
        IFirebaseClient ifclient;

        public void UpdateUserProfile(string accesscode, UpdateUserProfileForm form)
        {
            List<User> userlist = new List<User>();
            if (logincontext.retrieveUserList() != null)
            {
                userlist = logincontext.retrieveUserList();
            }
            foreach (User u in userlist)
            {
                if (u.AccessCode == accesscode)
                {
                    if (u.ProfilePicStrIdentifier != null && u.ProfilePicStrIdentifier != form.ProfilePic_StringIdentifier)
                    {
                        Cloudinary cloudinary = new Cloudinary(new Account { ApiKey = "493755983692144", ApiSecret = "9lJhZP0e5XiDFDmY-yCFwwpE9vA", Cloud = "ocbcteam1" });
                        cloudinary.DeleteResourcesByTag(u.ProfilePicStrIdentifier);
                    }
                    u.ProfilePicURL = form.ProfilePic_Url;
                    u.ProfilePicStrIdentifier = form.ProfilePic_StringIdentifier;
                    u.Email = form.Email;
                    u.PhoneNumber = form.PhoneNumber;
                }
            }
            ifclient = new FireSharp.FirebaseClient(ifc);
            if (ifclient != null)
            {
                ifclient.Set("User/", userlist);
            }
        }

        public void DeleteProfilePicture(string accesscode)
        {
            List<User> userlist = new List<User>();
            if (logincontext.retrieveUserList() != null)
            {
                userlist = logincontext.retrieveUserList();
            }
            foreach (User u in userlist)
            {
                if (u.AccessCode == accesscode)
                {
                    u.ProfilePicURL = null;
                    u.ProfilePicStrIdentifier = null;
                }
            }
            ifclient = new FireSharp.FirebaseClient(ifc);
            if (ifclient != null)
            {
                ifclient.Set("User/", userlist);
            }
        }
    }
}
