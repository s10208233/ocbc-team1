using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using ocbc_team1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.DAL
{
    public class GiftDAL
    {
        private SignupDAL singupContext = new SignupDAL();
        private LoginDAL loginContext = new LoginDAL();
        private TransactionDAL transactionContext = new TransactionDAL();

        IFirebaseConfig ifc = new FirebaseConfig()
        {
            AuthSecret = "Sa3cJdieiAEHpAPK7Z243SRtpxia29x6gzwaoz1g",
            BasePath = "https://failsafefundtransfer-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };
        IFirebaseClient ifclient;

        public Dictionary<string,Gift> RetrieveGiftDictionary()
        {
            Dictionary<string, Gift> giftdictionary = null;
            ifclient = new FireSharp.FirebaseClient(ifc);
            try
            {
                FirebaseResponse firebaseresponse = ifclient.Get("Gifts");
                giftdictionary = firebaseresponse.ResultAs<Dictionary<string, Gift>>();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error retrieving gift dictionary, {e.Message}");
            }
            return giftdictionary;
        }

        public List<Gift> RetrieveGiftList()
        {
            List<Gift> giftlist = null;
            try
            {
                foreach (var row in RetrieveGiftDictionary())
                {
                    giftlist.Add(new Gift()
                    {
                        Sender = row.Value.Sender,
                        Receipient = row.Value.Receipient,
                        transaction = row.Value.transaction,
                        Received = row.Value.Received,
                        sticker_src = row.Value.sticker_src
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error retrieving gift list, {e.Message}");
            }
            return giftlist;
        }

        //  Opening a gift
        public async Task OpenGift(string key)
        {
            try
            {
                Dictionary<string, Gift> giftdictionary = RetrieveGiftDictionary();
                //  Add Money To Receipient
                List<User> userlist = loginContext.retrieveUserList();
                //  for each user in userlist, check  if user == receipient
                for (int u=0; u<userlist.Count; u++)
                {
                    if (userlist[u].AccessCode == giftdictionary[key].Receipient.AccessCode)
                    {
                        // for each bankaccount in accountlist, check if accountnumber == recipient account number
                        for (int ba = 0; ba <userlist[u].AccountsList.Count; ba++)
                        {
                            if (userlist[u].AccountsList[ba].AccountNumber == giftdictionary[key].transaction.To_AccountNumber)
                            {
                                userlist[u].AccountsList[ba].AmountAvaliable += giftdictionary[key].transaction.Amount;
                                userlist[u].AccountsList[ba].AmountRemaining += giftdictionary[key].transaction.Amount;
                                giftdictionary[key].transaction.Amount = Math.Round(giftdictionary[key].transaction.Amount, 2);
                                if (userlist[u].TransactionList == null)
                                {
                                    List<Transaction> transactionlist = new List<Transaction>();
                                    transactionlist.Add(giftdictionary[key].transaction);
                                    userlist[u].TransactionList = transactionlist;
                                }
                                else
                                {

                                    userlist[u].TransactionList.Add(giftdictionary[key].transaction);
                                }
                            }
                        }
                    }
                }
                //  Update bool Gift.Received 
                giftdictionary[key].Received = true;
                //  Send Telegram Notification
                await transactionContext.sendMessage(Convert.ToString(giftdictionary[key].Sender.TelegramChatID), $"You have opened a gift from {giftdictionary[key].Sender.FirstName +" "+ giftdictionary[key].Sender.LastName}, ${giftdictionary[key].transaction.Amount} has been transferred to your account {giftdictionary[key].transaction.To_AccountNumber}");
                await transactionContext.sendMessage(Convert.ToString(giftdictionary[key].Sender.TelegramChatID), $"{giftdictionary[key].Receipient.FirstName +" "+ giftdictionary[key].Receipient.LastName} has opened and received your gift of ${giftdictionary[key].transaction.Amount} has been transferred to their account.");
                //  Update FireBase
                ifclient = new FireSharp.FirebaseClient(ifc);
                if (ifclient != null)
                {
                    ifclient.Set("User/", userlist);
                    ifclient.Set("Gifts/", giftdictionary);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to open gift, {e.Message}");
            }
        }

        //  Sending a gift
        public async Task SendGift(Gift gift)
        {
            try
            {
                //  Deduct Money From Sender
                List<User> userlist = loginContext.retrieveUserList();
                for (int u = 0; u< userlist.Count; u++)
                {
                    if (userlist[u].AccessCode == gift.Sender.AccessCode)
                    {
                        for (int ba = 0; ba < userlist[u].AccountsList.Count; ba++)
                        {
                            if (userlist[u].AccountsList[ba].AccountNumber == gift.transaction.From_AccountNumber)
                            {
                                userlist[u].AccountsList[ba].AmountAvaliable -= gift.transaction.Amount;
                                userlist[u].AccountsList[ba].AmountRemaining -= gift.transaction.Amount;
                                gift.transaction.Amount = Math.Round(gift.transaction.Amount, 2);
                                if (userlist[u].TransactionList == null)
                                {
                                    List<Transaction> transactionlist = new List<Transaction>();
                                    transactionlist.Add(gift.transaction);
                                    userlist[u].TransactionList = transactionlist;
                                }
                                else
                                {
                                    userlist[u].TransactionList.Add(gift.transaction);
                                }
                            }
                        }
                    }
                }
                //  Send Telegram Notification
                await transactionContext.sendMessage(Convert.ToString(gift.Sender.TelegramChatID), $"You have sent a gift ${gift.transaction.Amount} from {gift.transaction.From_AccountNumber} to {gift.Receipient.FirstName +" "+ gift.Receipient.LastName}");
                await transactionContext.sendMessage(Convert.ToString(gift.Receipient.TelegramChatID), $"{gift.Sender.FirstName +" "+ gift.Sender.LastName} has sent ${gift.transaction.Amount} to you, check your gift inbox to claim this amount.");
                //  Update FireBase
                ifclient = new FireSharp.FirebaseClient(ifc);
                if (ifclient != null)
                {
                    ifclient.Set("User/", userlist);
                    ifclient.Push("Gifts/", gift);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to create/send gift, {e.Message}");
            }
        }

    }
}
