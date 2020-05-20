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
    }
}
