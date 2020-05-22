using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace SignalrServer
{
    public class CacheContainer
    {
        public ConcurrentDictionary<string, Order> OrderCollection = new ConcurrentDictionary<string, Order>();

        public void UpdateOrder(OrderUpd or)
        {
            if (OrderCollection.TryGetValue(or.Orderid, out Order ord))
            {
                ord.AvgPriceCum = or.ExecPrice;
                ord.ExecSharesCum = or.ExecShares;
                ord.Status = or.Status;
                ord.Rsn = or.CxlReason;

                if (ord.ExecSharesCum == ord.OrdSize || or.Status == "FILLED")
                {
                    ord.IsActive = false;
                    ord.Status = "FILLED";
                }
            }
        }
    }
}
