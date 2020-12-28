using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API
{
    public class Constants
    {
        private IConfiguration configuration;

        public Constants(IConfiguration configuration)
        {
            this.configuration = configuration;
            Web_IP = configuration["Constants:Web_IP"];
            Web_Port = configuration["Constants:Web_Port"];
            Web_Protocol = configuration["Constants:Web_Protocol"];
        }
        public string Web_IP;
        public string Web_Port;
        public string Web_Protocol;

        public string Web_App_Address
        {
            get => Web_Protocol + "://" + Web_IP + ":" + Web_Port;
        }
    }
}
