using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalrServer
{
    public class Subscriber : IHostedService
    {
        private readonly ILogger<Subscriber> _logger;
        private readonly IConfigurationRoot _config;
        private IHubContext<RelayHub, IRelayHubClient> _hubContext;
        private List<Tuple<string, double>> _symbls = new List<Tuple<string, double>>() {
                                                new Tuple<string, double>("FB", 227.86),
                                                new Tuple<string, double>("AAPL", 318.85),
                                                new Tuple<string, double>("AMZN", 2480.06),
                                                new Tuple<string, double>("NFLX", 450.33),
                                                new Tuple<string, double>("GOOG", 1401.70),
                                                new Tuple<string, double>("MSFT", 101.70)
                                                };
        private int[] _ordsize = new int[] { 2000, 5000, 2500, 10000, 1200, 10500 };
        private int[] _execs = new int[] { 100, 500, 250, 150, 120, 10 };
        private double[] _pxs = new double[] { 228.5, 300.76, 2480.55, 454.25, 125.55, 1099.10 };
        private string[] _algo = new string[] { "GM", "VWAP", "TWAP", "REV", "FN", "AB" };
        private string[] _rsns = new string[] { "BAD", "NOPE", "WHAT", "OCCUR", "THIS", "SICK" };
        private string[] _stats = new string[] { "NEW", "PART", "FILLED", "CXLD", "REJD", "PART", "FILLED", "PART", "PART" };
        private string[] _side = new string[] { "B", "S", "SS" };
        private CacheContainer _cache;

        public Subscriber(ILogger<Subscriber> logger, IConfigurationRoot config, IHubContext<RelayHub, IRelayHubClient> hubContext, CacheContainer cache)
        {
            _logger = logger;
            _config = config;
            _hubContext = hubContext;
            _cache = cache;
        }

        private void MakeConnection()
        {
            
            Observable.Interval(TimeSpan.FromMilliseconds(107)).Subscribe((y) =>
            {
                var order = GetRandomOrder();
                if(_cache.OrderCollection.TryAdd(order.OrderId, order))
                    _hubContext.Clients.All.SendOrder(order);
               
            });

            Observable.Interval(TimeSpan.FromMilliseconds(229)).Subscribe((y) =>
            {
                var order = GetRandomOrder();
                if (_cache.OrderCollection.TryAdd(order.OrderId, order))
                    _hubContext.Clients.All.SendOrder(order);
               
            });

            Observable.Interval(TimeSpan.FromMilliseconds(397)).Subscribe((y) =>
            {
                var ordUp = GetRandomOrderOtherUpd();
                _hubContext.Clients.All.SendOrderOtherUpd(ordUp);
            });

            Observable.Interval(TimeSpan.FromMilliseconds(297)).Subscribe((y) =>
            {
                var ordUp = GetRandomOrderOtherUpd();
                _hubContext.Clients.All.SendOrderOtherUpd(ordUp);
            });

            Observable.Interval(TimeSpan.FromMilliseconds(229)).Subscribe((y) =>
            {
                var order = GetRandomOrderUpd();
                _cache.UpdateOrder(order);
                _hubContext.Clients.All.SendOrderUpd(order);

            });

            Observable.Interval(TimeSpan.FromMilliseconds(179)).Subscribe((y) =>
            {
                var order = GetRandomOrderUpd();
                _cache.UpdateOrder(order);
                _hubContext.Clients.All.SendOrderUpd(order);

            });

            Observable.Interval(TimeSpan.FromMilliseconds(259)).Subscribe((y) =>
            {
                var order = GetRandomOrderUpd();
                _cache.UpdateOrder(order);
                _hubContext.Clients.All.SendOrderUpd(order);
            });

            Observable.Interval(TimeSpan.FromMilliseconds(339)).Subscribe((y) =>
            {
                var order = GetRandomOrderUpd();
                _cache.UpdateOrder(order);
                _hubContext.Clients.All.SendOrderUpd(order);

            });

            Observable.Interval(TimeSpan.FromMilliseconds(239)).Subscribe((y) =>
            {
                var order = GetRandomOrderOtherUpd();
                _hubContext.Clients.All.SendOrderOtherUpd(order);
            });
        }

       
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(()=> { MakeConnection(); });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }


        private Order GetRandomOrder()
        {
            var random = new Random();
            var order = new Order()
            {
                OrderId = Guid.NewGuid().ToString(),
                OrdTime = DateTime.UtcNow
            };

            int index = random.Next(_symbls.Count);
            order.Smbl = _symbls[index].Item1;
            order.LimitPrice = _symbls[index].Item2;
            order.OrdSize = _ordsize[random.Next(_ordsize.Count())];
            order.Algo = _algo[random.Next(_algo.Count())];
            order.Side = _side[random.Next(_side.Count())];
            return order;
        }
        private OrderUpd GetRandomOrderUpd()
        {
            var random = new Random();
            if (_cache.OrderCollection.Count == 0)
                return null;
            int index = random.Next(_cache.OrderCollection.Count);
            string od = _cache.OrderCollection.Keys.ToList()[index];
            var order = new OrderUpd()
            {
                Orderid = od,
                ExecShares = _execs[random.Next(_execs.Count())],
                ExecPrice = _pxs[random.Next(_pxs.Count())],
                CxlReason = _rsns[random.Next(_rsns.Count())],
                ExecTime = DateTime.Now.ToShortTimeString(),
                Status = _stats[random.Next(_stats.Count())],
            };

            
            return order;
        }

        private OrderOtherUpd GetRandomOrderOtherUpd()
        {
            var random = new Random();
            int index = random.Next(_cache.OrderCollection.Count);
            string od = _cache.OrderCollection.Keys.ToList()[index];
            var order = new OrderOtherUpd()
            {
                OrdId = od,
                LastMkt = _rsns[random.Next(_rsns.Count())],
                Mode = "START",
                FreeText = "Quick brown fox",
                StartAlg = _algo[random.Next(_algo.Count())],
                UpdTime = DateTime.Now,
                StartSub = DateTime.Now.ToShortTimeString()
            };


            return order;
        }

    }

}
