using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddressTransfer.BO
{
    public class Request_SMS
    {
        public int id { get; set; }
        public string phone { get; set; }
        public string messagebody { get; set; }
        public string datetime { get; set; }
        public int status { get; set; }
        public string sentmsg { get; set; }
        public decimal lat { get; set; }
        public decimal lon { get; set; }
    }
}
