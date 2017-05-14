using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ToDoApp.Utils
{
    public class Settings
    {
        public static string SMS_API_ENDPOINT = ConfigurationManager.AppSettings["SMS_API_ENDPOINT"];
        public static string SMS_API_SENDER = ConfigurationManager.AppSettings["SMS_API_SENDER"];
    }
}