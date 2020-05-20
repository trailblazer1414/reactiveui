using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SignalrClient.ViewModel;

namespace SignalrClient.Container
{

    public class CacheContainer
    {

        public BroadcastBlock<Order> orderRelayBlock = new BroadcastBlock<Order>(null);
        public BroadcastBlock<List<Order>> relayInitOrderBlock = new BroadcastBlock<List<Order>>(null);
        public ActionBlock<Order> inactiveOrdBlock;
        public BufferBlock<OrderUpd> orderExecBufBlock = new BufferBlock<OrderUpd>(new ExecutionDataflowBlockOptions() { BoundedCapacity = 100000 });
        public BufferBlock<Order> orderRecvBlock = new BufferBlock<Order>(new ExecutionDataflowBlockOptions() { BoundedCapacity = 100000 });
        private ConcurrentDictionary<string, Order> _inactiveOrderMap = new ConcurrentDictionary<string, Order>();
        private ConcurrentDictionary<string, Order> _ordersMap = new ConcurrentDictionary<string, Order>();
        public ActionBlock<OrderUpd> orderTermBlock;
        public ActionBlock<OrderUpd> orderNewBlock;
        public BufferBlock<OrderOtherUpd> stratMsgBlock = new BufferBlock<OrderOtherUpd>(new ExecutionDataflowBlockOptions() { BoundedCapacity = 15000 });
        public Subject<OrderOtherUpd> updStrat = new Subject<OrderOtherUpd>();
        public Subject<OrderUpd> updExec = new Subject<OrderUpd>();
        public BufferBlock<List<Order>> initOrdersBlock = new BufferBlock<List<Order>>(new ExecutionDataflowBlockOptions() { BoundedCapacity = 10000 });

        private readonly IConfiguration _config;
        private readonly ILogger<CacheContainer> _logger;
        public CacheContainer(ILogger<CacheContainer> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            SetupBlocks();
        }

        private void AddOrder(Order ord)
        {
            try
            {
                orderRelayBlock.Post(ord);
                _ordersMap.TryAdd(ord.OrderId, ord);
                ord.IsActive = true;

            }

            catch (Exception ex)
            {
                _logger.LogError("Add Order Error:" + ex.Message);
            }
        }
        private void SetupBlocks()
        {
            inactiveOrdBlock = new ActionBlock<Order>(AddOrdertoInactive);
            stratMsgBlock.LinkTo(new ActionBlock<OrderOtherUpd>(x =>
            {
                updStrat.OnNext(x);
            }));

            orderRecvBlock.LinkTo(new ActionBlock<Order>(AddOrder));

            //
            updStrat.OnErrorResumeNext(this.updStrat).Buffer(TimeSpan.FromMilliseconds(750))
               .Subscribe((y) =>
               {
                   y.GroupBy(c => new { c.OrdId, c.Mode }).ToList().ForEach(x =>
                   {
                       var strat = x.Last();
                       UpdateOtherUpdates(strat);
                   });
               });
            //
            initOrdersBlock.LinkTo(new ActionBlock<List<Order>>(async x =>
            {
                try
                {

                    relayInitOrderBlock.Post(x);
                    LoadInitOrders(x);
                    //GetOrdersBySenders(x.Item1);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error in filterlist", ex);
                }
            }), new DataflowLinkOptions { PropagateCompletion = true });


            orderExecBufBlock.LinkTo(new ActionBlock<OrderUpd>(async ord =>
            {
                updExec.OnNext(ord);
            }));
            updExec.OnErrorResumeNext(this.updExec).Buffer(TimeSpan.FromMilliseconds(750))
               .Subscribe((openords) =>
               {
                   openords.GroupBy(c => new { c.Orderid, c.Status }).ToList().ForEach(x =>
                   {
                       var ord = x.Last();
                       switch (ord.Status)
                       {
                           case "NEW":
                           case "PENDING_REPL":
                               try
                               {
                                   orderNewBlock.SendAsync(ord);
                               }
                               catch (Exception ex)
                               {
                                   _logger.LogError("Error in sending New " + ex.Message);
                               }
                               break;
                           case "PART":
                           case "FILLED":
                               try
                               {
                                   this.UpdateExec(ord);
                               }
                               catch (Exception ex)
                               {
                                   _logger.LogError("Error in sending exec " + ex.Message);
                               }
                               break;
                           case "CXLD":
                           case "REJD":
                           case "CXLD_REJD":
                               try
                               {
                                   this.UpdateOrdTerm(ord);
                               }
                               catch (Exception ex)
                               {
                                   _logger.LogError("Error in sending term " + ex.Message);
                               }
                               break;
                           default:
                               break;
                       }

                   });
               });
        }

        public void LoadInitOrders(List<Order> ordersList)
        {
            try
            {
                var getDataBlock = new TransformBlock<Order, Order>(delta =>
                {
                    try
                    {
                        if (!delta.IsActive)
                        {
                            inactiveOrdBlock.Post(delta);
                        }
                        else if (!_ordersMap.ContainsKey(delta.OrderId))
                        {
                            _ordersMap.TryAdd(delta.OrderId, delta);

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                    return delta;
                }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded });


                foreach (Order od in ordersList)
                {
                    getDataBlock.Post(od);
                }


                //getDataBlock.Complete();
                //getDataBlock.Completion.Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in Init Order Load:", ex);
            }
        }

        private void AddOrdertoInactive(Order item)
        {
            try
            {
                Order remItem;
                _ordersMap.TryRemove(item.OrderId, out remItem);
                _inactiveOrderMap.TryAdd(item.OrderId, item);
                // figure out how and when to remocve completely from UI system
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ading to inactive cache: " + ex.Message);
            }
        }

        public void UpdateExec(OrderUpd orderexec)
        {
            try
            {
                Order ord;
                if (_ordersMap.TryGetValue(orderexec.Orderid, out ord))
                {
                    ProcessOrderExec(orderexec, ord);
                }
                else if (_inactiveOrderMap.TryGetValue(orderexec.Orderid, out ord))
                {
                    ProcessOrderExec(orderexec, ord);
                }
                else
                {
                    //orderExecBufBlock.Post(orderexec);
                    _logger.LogError("MISSING ORDER for exec." + orderexec.Orderid);

                }

            }
            catch (Exception ex)
            {
                _logger.LogError("Exec Upd Error:" + ex.Message);
            }
        }
        public void ProcessOrderExec(OrderUpd ex, Order ord)
        {
            if (ord.IsActive)
            {
                if (ord.ExecSharesCum <= ex.ExecShares)
                {
                    ord.AvgPriceCum = ex.ExecPrice;
                    ord.ExecSharesCum = ex.ExecShares;
                }
            }

            if (ord.ExecSharesCum == ord.OrdSize || ex.Status == "FILLED")
            {
                ord.IsActive = false;
                inactiveOrdBlock.Post(ord);
                ord.Status = "FILLED";
            }
        }

        public void UpdateOrdTerm(OrderUpd orderexec)
        {
            try
            {
                Order ord;
                if (_ordersMap.TryGetValue(orderexec.Orderid, out ord))
                {
                    ord.Status = orderexec.Status;
                    ord.IsActive = false;
                    inactiveOrdBlock.Post(ord);
                }
                else if (_ordersMap.TryGetValue(orderexec.RefOrderId, out ord))
                {
                    ord.Status = orderexec.Status;
                    ord.IsActive = false;
                    inactiveOrdBlock.Post(ord);
                }
                else
                {
                    //_logger.LogError("MISSING ORDER for cxl." + orderexec.Orderid);
                    //orderExecBufBlock.Post(orderexec);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Terminate Error:" + ex.Message);
            }
        }

        public void UpdateOtherUpdates(OrderOtherUpd otherupd)
        {
            try
            {
                Order ord;
                if (_ordersMap.TryGetValue(otherupd.OrdId, out ord))
                {
                    ProcessOrderWithStratMsg(otherupd, ord);
                }
                else if (_inactiveOrderMap.TryGetValue(otherupd.OrdId, out ord))
                {
                    ProcessOrderWithStratMsg(otherupd, ord);
                }
                else
                {
                    _logger.LogError("MISSING ORDER for strat." + otherupd.OrdId + ":" + otherupd.UpdTime);
                    //if (!_PendingIOrder.ContainsKey(strat.OrdId))
                    //{
                    //    _PendingIOrder.AddOrUpdate(strat.OrdId, new Queue<IOrder>(), (k, v) => { return v; });
                    //}
                    //_PendingIOrder[strat.OrdId].Enqueue(strat);
                    //stratMsgBlock.Post(strat);
                }

                //_stratMsgMap.AddOrUpdate(strat.OrdId, strat, (key, oldVal) => oldVal = strat);
            }
            catch (Exception ex)
            {
                _logger.LogError("Strat Upd Error:" + ex.Message);
            }

            //await stratMsgBlock.SendAsync(strat);

        }

        private void ProcessOrderWithStratMsg(OrderOtherUpd strat, Order ord)
        {
            //ord.StratDetails = msg;
            if (ord.IsActive)
            {
                ord.Status = strat.Mode;
            }
            ord.StartAlgo = strat.StartAlg;
            ord.StartSub = strat.StartSub;
            ord.FreeText = strat.FreeText;
            ord.ExecTime = strat.UpdTime;
        }
    }
}