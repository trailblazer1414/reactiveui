using DynamicData;
using DynamicData.Binding;
using GalaSoft.MvvmLight.Command;
using Microsoft.Extensions.Logging;
using SignalrClient.Container;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks.Dataflow;
using System.Windows.Input;

namespace SignalrClient.ViewModel
{

    public class OrderViewModel : AbstractNotifyPropertyChanged
    {

        /*
            The Order ViewModel is mainly for filtering and grid manipulation. I have flyout menus which shows unique values for each "filteritem" 
            SetupSubscription() is where I added all the subscriptions
            I was only able to add AddOrder and AddInitOrder signalr calls. In my code I get multiple updates on the order object OrderUpd and Order OtherUpd which i didnot add here. 
            I use _ordRelay Subject to throttle for 700ms and batch update on grid. This is where my grid hangs and slows the system down.
            I also remove orders after 5 minutes once they get inactive.
        */

        private CacheContainer _cache;
        private ReadOnlyObservableCollection<Order> _ordCollection;
        public ReadOnlyObservableCollection<Order> OrdCollection => _ordCollection;

        private SourceCache<Order, string> _ordCache = new SourceCache<Order, string>(ord => ord.OrderId);
        private ConcurrentDictionary<string, Order> _inactives = new ConcurrentDictionary<string, Order>();
        private IObservableCache<Order, string> _observableCache;
        private ILogger<OrderViewModel> _logger;
        private IDisposable _ordSub;
        private IDisposable _smblFiltSub;
        private IDisposable _algoFiltSub;
        private IDisposable _notlTotalSub;

        public HashSet<string> _selectedSmblFilt = new HashSet<string>();
        public HashSet<string> _selectedAlgoFilt = new HashSet<string>();

        private ReadOnlyObservableCollection<FilterItem> _smblCollection;
        public ReadOnlyObservableCollection<FilterItem> SmblCollection => _smblCollection;
        private ReadOnlyObservableCollection<FilterItem> _algoCollection;
        public ReadOnlyObservableCollection<FilterItem> AlgoCollection => _algoCollection;
        private Subject<Order> _ordRelaySub = new Subject<Order>();

        //for filters
        private bool _isAllSmblSel = true;
        public bool IsAllSmblSel
        {
            get { return _isAllSmblSel; }
            set
            {
                SetAndRaise(ref _isAllSmblSel, value);
            }
        }

        private bool _isAllAlgoSel = true;
        public bool IsAllAlgoSel
        {
            get { return _isAllAlgoSel; }
            set
            {
                //_isAllAlgoSel = value;
                //RaisePropertyChanged("IsAllAlgoSel");
                SetAndRaise(ref _isAllAlgoSel, value);
            }
        }

        private bool _isLive = true;
        public bool IsLive
        {
            get { return _isLive; }
            set
            {
                SetAndRaise(ref _isLive, value);
            }
        }

        public ICommand FiltAlgoCommand
        {
            get
            {
                return new RelayCommand(() =>
                {

                    this.IsAlgoFilterOpen = !this.IsAlgoFilterOpen;


                });
            }
        }
        public ICommand FiltSmblCommand
        {
            get
            {
                return new RelayCommand(() =>
                {

                    this.IsSmblFilterOpen = !this.IsSmblFilterOpen;


                });
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetAndRaise(ref _searchText, value);
        }

        //filters
        private double _execSharesTotal;
        public double ExecSharesTotal
        {
            get { return _execSharesTotal; }
            set
            {

                SetAndRaise(ref _execSharesTotal, value);
            }
        }

        private double _execNotlTotal;
        public double ExecNotlTotal
        {
            get { return _execNotlTotal; }
            set
            {

                SetAndRaise(ref _execNotlTotal, value);
            }
        }

        private double _ordsizeTotal;
        public double OrdsizeTotal
        {
            get { return _ordsizeTotal; }
            set
            {

                SetAndRaise(ref _ordsizeTotal, value);
            }
        }

        private double _notlTotal;
        public double NotlTotal
        {
            get { return _notlTotal; }
            set
            {

                SetAndRaise(ref _notlTotal, value);
            }
        }

        private double _leavesTotal;
        public double LeavesTotal
        {
            get { return _leavesTotal; }
            set
            {

                SetAndRaise(ref _leavesTotal, value);
            }
        }


        public OrderViewModel(ILogger<OrderViewModel> logger, CacheContainer cache)
        {
            _observableCache = _ordCache.Connect().AsObservableCache();
            _logger = logger;
            _cache = cache;
            SetupSubscription();

            // Get individual order
            var getDataBlock = new TransformBlock<Order, Order>(x =>
            {
                try
                {
                    _ordRelaySub.OnNext(x);
                    x.IsActiveChanged.Subscribe(c =>
                    {
                        Observable.Interval(TimeSpan.FromMinutes(2)).Subscribe((y) =>
                        {
                            if (!c)
                            {
                                x.IsReadyToRemove = true;
                                this._inactives.TryAdd(x.OrderId, x);
                            }
                        });
                    });
                    return x;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                return null;
            });

            // broadcast to all open order VMs
            _cache.orderRelayBlock.LinkTo(getDataBlock, new DataflowLinkOptions { PropagateCompletion = true });

            //Get all orders which are in the server as a List
            _cache.relayInitOrderBlock.LinkTo(new ActionBlock<List<Order>>(ords =>
            {

                try
                {

                    var actOrds = ords;
                    // use TPL to update sourcecache 
                    AddBatchOrdersToCache(actOrds.ToList());

                }
                catch (Exception ex)
                {
                    _logger.LogError("Error in Init list of orders", ex);
                }
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 }), new DataflowLinkOptions { PropagateCompletion = true }); ;

            //Buffer for 700ms to see if i get a large batch of orders
            this._ordRelaySub.OnErrorResumeNext(this._ordRelaySub).Buffer(TimeSpan.FromMilliseconds(700))
               .Subscribe((y) =>
               {

                   try
                   {
                       if (y.Count() > 0)
                       {
                           if (y.Count() > 200)
                           {
                               // This is where my grid hangs.
                               _ordCache.Edit(ords => { ords.AddOrUpdate(y); });
                           }
                           else
                           {
                               _ordCache.AddOrUpdate(y);
                           }
                       }
                   }
                   catch (Exception ex)
                   {
                       _logger.LogError("Batch CustOrder : Auto Catch", ex);
                   }

               },
               (e) => _logger.LogError("Batch CustOrder : Auto Catch", e));
        }

        private void AddBatchOrdersToCache(List<Order> actOrds)
        {
            var setDataBlock = new ActionBlock<Order>(x =>
            {
                _ordCache.AddOrUpdate(x);
            });

            var getDataBlock = new TransformBlock<Order, Order>(c =>
            {
                try
                {

                    c.IsActiveChanged.Throttle(TimeSpan.FromMinutes(2)).Subscribe(x =>
                    {
                        if (!x)
                        {
                            c.IsReadyToRemove = true;
                            this._inactives.TryAdd(c.OrderId, c);
                        }
                    });

                    return c;

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                return null;
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 10 });
            foreach (Order od in actOrds)
            {
                getDataBlock.Post(od);
            }
            getDataBlock.LinkTo(setDataBlock, new DataflowLinkOptions { PropagateCompletion = true });
            getDataBlock.Complete();
            
        }


        // Used to filter the grid
        private bool _isFilterChanged = false;
        public bool IsFilterChanged
        {
            get => _isFilterChanged;
            set => SetAndRaise(ref _isFilterChanged, value);

        }

        private bool _isAlgoFilterOpen = false;
        public bool IsAlgoFilterOpen
        {
            get { return _isAlgoFilterOpen; }
            set
            {
                //_isAlgoFilterOpen = value;
                //RaisePropertyChanged("IsAlgoFilterOpen");
                SetAndRaise(ref _isAlgoFilterOpen, value);
            }
        }
        private bool _isSmblFilterOpen = false;
        public bool IsSmblFilterOpen
        {
            get { return _isSmblFilterOpen; }
            set
            {
                //_isUrgFilterOpen = value;
                //RaisePropertyChanged("IsUrgFilterOpen");
                SetAndRaise(ref _isSmblFilterOpen, value);
            }
        }

        public ICommand CloseFiltCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //_pagedOrderList.Filter = null;
                    this.IsAlgoFilterOpen = false;
                    this.IsSmblFilterOpen = false;
                    
                    if (this.IsFilterChanged)
                    {
                        this.IsFilterChanged = false;
                    }
                });
            }
        }


        private object LoadTotals(IReadOnlyCollection<Order> items)
        {
            this.ExecSharesTotal = items.Sum(p => p.ExecSharesCum);
            this.ExecNotlTotal = items.Sum(p => p.ExecNotl);
            this.OrdsizeTotal = items.Sum(p => p.OrdSize);
            this.LeavesTotal = items.Sum(p => p.Leaves);
            this.NotlTotal = items.Sum(p => p.Notl);
            return null;
        }

        private Func<Order, bool> SearchFilter(string searchText)
        {
            if (string.IsNullOrEmpty(this.SearchText)) return trade => true;

            return t => (t.Smbl.Equals(searchText, StringComparison.OrdinalIgnoreCase)
                            || t.Algo.Equals(searchText, StringComparison.OrdinalIgnoreCase)
                            );
        }

        private Func<Order, bool> GridSelectFilter(bool isfilter)
        {
            if (this.IsAllAlgoSel && this.IsAllSmblSel)
                return t => true;
            else
            {
                return ord => FilterGridForSmbl(ord) && FilterGridForAlgo(ord);
            }
        }
        public bool FilterGridForSmbl(Order dec)
        {
            //if (!this.IsSelectedMonikerFilters)
            if (this.IsAllSmblSel)
                return true;
            return (this._selectedSmblFilt.Contains(dec.Smbl ?? ""));
        }

        public bool FilterGridForAlgo(Order dec)
        {
            //if (!this.IsSelectedAlgoFilters)
            if (this.IsAllAlgoSel)
                return true;
            return (this._selectedAlgoFilt.Contains(dec.Algo ?? ""));
        }

        private void SetupSubscription()
        {
            var filter = this.WhenValueChanged(t => t.IsFilterChanged)
                .Throttle(TimeSpan.FromMilliseconds(250))
               .Select(GridSelectFilter);

            var srchfilter = this.WhenValueChanged(t => t.SearchText)
               .Throttle(TimeSpan.FromMilliseconds(250))
               .Select(SearchFilter);

            var sortComparer = SortExpressionComparer<Order>.Descending(p => p.OrdTime);

            _smblFiltSub = _observableCache.Connect().DistinctValues(trade => trade.Smbl)
                           .Transform(x => new FilterItem(FilterItem.FilterCategory.Symbol, x, this.IsAllSmblSel, _selectedSmblFilt))
                           .AutoRefresh()
                           .Sort(SortExpressionComparer<FilterItem>.Ascending(t => t.Description), SortOptimisations.ComparesImmutableValuesOnly, 25)
                           .ObserveOnDispatcher()
                           .Bind(out _smblCollection)
                           .Subscribe();

            _algoFiltSub = _observableCache.Connect().DistinctValues(trade => trade.Algo)
                          .Transform(x => new FilterItem(FilterItem.FilterCategory.Algo, x, this.IsAllAlgoSel, _selectedSmblFilt))
                          .AutoRefresh()
                          .Sort(SortExpressionComparer<FilterItem>.Ascending(t => t.Description), SortOptimisations.ComparesImmutableValuesOnly, 25)
                          .ObserveOnDispatcher()
                          .Bind(out _algoCollection)
                          .Subscribe();

            _notlTotalSub = _observableCache.Connect().Throttle(TimeSpan.FromMilliseconds(1000))
                           .Filter(filter)
                           .Filter(srchfilter)
                           .ToCollection().Select(items => LoadTotals(items))
                           .Subscribe();


            _ordSub = _observableCache.Connect()
                   .AutoRefresh()
                   .Filter(filter)
                   .Filter(srchfilter)
                   .Sort(sortComparer, SortOptimisations.ComparesImmutableValuesOnly, 25)
                   .ObserveOnDispatcher()
                   .Bind(out _ordCollection)
                   .DisposeMany()
                   .Subscribe();

            Observable.Interval(TimeSpan.FromMinutes(5)).Subscribe((y) =>
            {
                var removelist = _inactives.Values;

                removelist.ToList().ForEach(x =>
                {
                    _ordCache.Remove(x);
                    _inactives.TryRemove(x.OrderId, out Order c);
                });
            });

        }

        private void AlterFilterSelection(bool allfiltSelection, ReadOnlyObservableCollection<FilterItem> filters, HashSet<string> filtSet)
        {
            if (allfiltSelection)
            {
                foreach (var fil in filters)
                {
                    fil.IsSelected = true;
                    filtSet.Add(fil.Description);
                }
            }
            else
            {
                filtSet.Clear();
                foreach (var fil in filters)
                {
                    fil.IsSelected = false;
                }
            }
            _isFilterChanged = true;
        }

        public ICommand SelAllFiltCommand
        {
            get
            {
                return new RelayCommand<string>(ord =>
                {
                    switch (ord)
                    {
                        case "All Smbls":
                            {
                                AlterFilterSelection(this.IsAllSmblSel, _smblCollection, _selectedSmblFilt);
                            }
                            break;
                        case "All Algos":
                            {
                                AlterFilterSelection(this.IsAllAlgoSel, _algoCollection, _selectedAlgoFilt);
                            }
                            break;
                        default:
                            break;
                    }
                });
            }
        }

        public ICommand ValidateFiltCommand
        {
            get
            {
                return new RelayCommand<FilterItem>(sel =>
                {
                    switch (sel.FilterCategoryItem)
                    {
                        case FilterItem.FilterCategory.Symbol:
                            {
                                if (sel.IsSelected)
                                    _selectedSmblFilt.Add(sel.Description);
                                else
                                {
                                    this.IsAllSmblSel = false;
                                    _selectedSmblFilt.Remove(sel.Description);
                                }
                                _isFilterChanged = true;

                            }
                            break;
                        case FilterItem.FilterCategory.Algo:
                            {
                                if (sel.IsSelected)
                                    _selectedAlgoFilt.Add(sel.Description);
                                else
                                {
                                    this.IsAllAlgoSel = false;
                                    _selectedAlgoFilt.Remove(sel.Description);
                                }

                                _isFilterChanged = true;

                            }
                            break;
                        default:
                            _isFilterChanged = false;
                            break;
                    }
                });
            }
        }
    }
}