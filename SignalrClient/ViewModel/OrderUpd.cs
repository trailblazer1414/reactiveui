using System;
using System.Collections.Generic;
using System.Text;

namespace SignalrClient.ViewModel
{
    public struct OrderUpd 
    {
        public double ExecPrice { get; set; }
        public int ExecShares { get; set; }
        public string ExecTime { get; set; }
        public string Orderid { get; set; }
        public string RefOrderId { get; set; }
        public string Status { get; set; }
        public string POrdId { get; set; }
        public string CxlReason { get; set; }
        
    }
}