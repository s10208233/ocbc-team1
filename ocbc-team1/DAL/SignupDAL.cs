using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using ocbc_team1.Models;

namespace ocbc_team1.DAL
{
    public class SignupDAL
    {
        IFirebaseConfig ifc = new FirebaseConfig()
        {
            AuthSecret = "Sa3cJdieiAEHpAPK7Z243SRtpxia29x6gzwaoz1g",
            BasePath = "https://failsafefundtransfer-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };
        IFirebaseClient ifclient;

        public List<Card> CardList()
        {
            List<Card> cardlist = new List<Card>();
            ifclient = new FireSharp.FirebaseClient(ifc);
            if (ifclient != null)
            {
                    FirebaseResponse firebaseresponse = ifclient.Get("Card");
                    cardlist = firebaseresponse.ResultAs<List<Card>>();
            }
            return cardlist;
        }
        public bool If_SignupCardExist(Card c)
        {
            if (CardList().Contains(c));
            return true;
        }

        public bool If_UserHasCard(User u)
        {

            return true;
        }

        public void SignupUser(User u)
        {
            ifclient = new FireSharp.FirebaseClient(ifc);
            if (ifclient != null)
            {
                var setter = ifclient.Set("User/"+u.AccessCode, u);
            }
        }
    }
}
