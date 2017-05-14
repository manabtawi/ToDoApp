using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using ToDoApp.REST;
using ToDoApp.REST.SMS;
using Microsoft.Rest;
using ToDoApp.Models;
using SMSAPI;

namespace ToDoApp.Utils
{
    public class SmsHelper
    {
        static public void SendShortCode(SmsModel model)
        {
            using (SMSAPIClient client = GetSMSAPIClient())
            {
                client.Create(new SMSAPI.Models.SmsModel(Settings.SMS_API_SENDER, "+" + model.to,model.body));
            };
        }

        public static SMSAPIClient GetSMSAPIClient()
        {
            ServiceClientCredentials credentials = new TokenCredentials("<bearer token>");
            var client = new SMSAPIClient(new Uri(Settings.SMS_API_ENDPOINT), credentials);
            return client;
        }

        static public string GetRandomCode()
        {
            Random random = new Random();
            return random.Next(1000, 9999).ToString();
        }
    }
}