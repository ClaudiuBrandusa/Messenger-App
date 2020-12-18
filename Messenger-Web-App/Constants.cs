using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_Web_App
{
    public class Constants
    {
        private IConfiguration configuration;

        public Constants(IConfiguration configuration)
        {
            this.configuration = configuration;
            API_IP = configuration["Constants:API_IP"];
            API_Port = configuration["Constants:API_Port"];
            API_Protocol = configuration["Constants:API_Protocol"];
            Login_Path = configuration["Constants:Login_Path"];
            Register_Path = configuration["Constants:Register_Path"];
            Logout_Path = configuration["Constants:Logout_Path"];

        }

        public string API_IP;
        public string API_Port;
        public string API_Protocol;

        public string API_Address
        {
            get => API_Protocol + "://" + API_IP + ":" + API_Port;
        }

        public string Login_Path;
        public string Login_Endpoint
        {
            get => API_Address + "/" + Login_Path;
        }

        public string Register_Path;
        public string Register_Endpoint
        {
            get => API_Address + "/" + Register_Path;
        }

        public string Logout_Path;
        public string Logout_Endpoint
        {
            get => API_Address + "/" + Logout_Path;
        }
    }
}
