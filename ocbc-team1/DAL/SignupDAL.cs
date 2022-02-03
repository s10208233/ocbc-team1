using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using MailKit.Net.Smtp;
using MimeKit;
using Newtonsoft.Json;
using ocbc_team1.Models;

namespace ocbc_team1.DAL
{
    public class SignupDAL
    {
        private LoginDAL loginContext = new LoginDAL();

        IFirebaseConfig ifc = new FirebaseConfig()
        {
            AuthSecret = "Sa3cJdieiAEHpAPK7Z243SRtpxia29x6gzwaoz1g",
            BasePath = "https://failsafefundtransfer-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };
        IFirebaseClient ifclient;

        public List<Card> retrieveCardList()
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

        public List<Card> retrieveShortCardList(List<Card> cardlist)
        {
            List<Card> shortCardList = new List<Card>();
            foreach (Card c in cardlist)
            {
                shortCardList.Add(new Card() { 
                    CardNumber=c.CardNumber.Substring(c.CardNumber.Length - 8),
                    CardPIN=c.CardPIN }
                );
                Console.WriteLine(c.CardNumber.Substring(c.CardNumber.Length - 8));
            }
            return shortCardList;
        }

        public void completeUserSignUp(SignUpViewModel signupform)
        {
            string accesscode = createAccessCode();
            createUser(signupform, accesscode);


            string createAccessCode()
            {
                List<string> existingacesscodelist = loginContext.existingAccessCodeList(loginContext.retrieveUserList());
                Random rnd = new Random();
                string newaccesscode = Convert.ToString(rnd.Next(000000, 999999));
                while (existingacesscodelist.Contains(newaccesscode))
                {
                    newaccesscode = Convert.ToString(rnd.Next(000000, 999999));
                }
                return newaccesscode;
            }

            void createUser(SignUpViewModel input, string accesscode)
            {
                //  User's first bank account value will be random
                Random rnd = new Random();
                int randombal = rnd.Next(15, 1500);

                ifclient = new FireSharp.FirebaseClient(ifc);
                List<User> userList = new List<User>();
                if (loginContext.retrieveUserList() != null)
                {
                    userList = loginContext.retrieveUserList();
                }

                List<BankAccount> bankaccountlist = new List<BankAccount>();
                int newbankaccountnumber = rnd.Next(000000000, 999999999);
                foreach (User u in userList)
                {
                    foreach (BankAccount ba in u.AccountsList)
                    {
                        if (ba.AccountNumber == newbankaccountnumber) { newbankaccountnumber = rnd.Next(000000000, 999999999); }
                    }
                }

                bankaccountlist.Add(new BankAccount()
                {
                    AccountNumber = newbankaccountnumber,
                    AccountType = "Savings",
                    AmountAvaliable = randombal,
                    AmountRemaining = randombal,
                    AccountCurrency = "SGD",
                    CreationDate = DateTime.Today
                });
                List<Transaction> transactionlist = new List<Transaction>();

                userList.Add(new User()
                {
                    AccessCode = accesscode,
                    CardNumber = input.CardNumber,
                    BankPIN = input.BankPIN,
                    IC = input.IC,
                    FirstName = input.FirstName,
                    LastName = input.LastName,
                    Email = input.Email,
                    PhoneNumber = input.PhoneNumber,
                    DateOfBirth = input.DateOfBirth,
                    AccountsList = bankaccountlist,
                    TransactionList = transactionlist
                });
                if (ifclient != null)
                {
                    ifclient.Set("User/", userList);
                    sendAccessCode(accesscode, input.Email, input.FirstName+" "+input.LastName);
                }
            }

            void sendAccessCode(string accesscode, string rEmail, string rName)
            {
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
