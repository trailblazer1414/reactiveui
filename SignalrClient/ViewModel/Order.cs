using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData.Binding;

namespace SignalrClient.ViewModel
{

    public class Order : AbstractNotifyPropertyChanged, IDisposable, IEquatable<Order>
    {
        private ISubject<bool> _isActiveChangedSubject = new ReplaySubject<bool>();

        public IObservable<bool> IsActiveChanged => _isActiveChangedSubject.AsObservable();

        private ISubject<bool> _isReadyToRemovedSubject = new ReplaySubject<bool>();

        public IObservable<bool> IsReadyToBeRemovedChanged => _isReadyToRemovedSubject.AsObservable();


        public double _PctDone;
        public double PctDone
        {
            get => _PctDone;
            set => SetAndRaise(ref _PctDone, value);
        }

        private double _AvgPriceCum;
        public double AvgPriceCum
        {
            get => _AvgPriceCum;
            set => SetAndRaise(ref _AvgPriceCum, value);
        }

        private long _leaves;
        public long Leaves
        {
            get => _leaves;
            set => SetAndRaise(ref _leaves, value);
        }

        private string _Smbl;
        public string Smbl
        {
            get => _Smbl;
            set => _Smbl = value;
        }
        private DateTime _OrdTime;
        public DateTime OrdTime
        {
            get => _OrdTime;
            set => _OrdTime = value;
        }

        private string _Side;
        public string Side
        {
            get => _Side;
            set => _Side = value;
        }

        private long _OrdSize;
        public long OrdSize
        {
            get => _OrdSize;
            set => _OrdSize = value;
        }

        private double _LimitPrice;
        public double LimitPrice
        {
            get => _LimitPrice;
            set => _LimitPrice = value;
        }

        private string _Algo;
        public string Algo
        {
            get => _Algo;
            set => _Algo = value;
        }

        private double _Notl;
        public double Notl
        {
            get => _Notl;
            set => SetAndRaise(ref _Notl, value);
        }

        private double _ExecNotl;
        public double ExecNotl
        {
            get => _ExecNotl;
            set => SetAndRaise(ref _ExecNotl, value);
        }

        private DateTime _ExecTime;
        public DateTime ExecTime
        {
            get => _ExecTime;
            set => SetAndRaise(ref _ExecTime, value);
        }

        private string _Status;
        public string Status
        {
            get => _Status;
            set => SetAndRaise(ref _Status, value);
        }


        private string _Rsn;
        public string Rsn
        {
            get => _Rsn;
            set => SetAndRaise(ref _Rsn, value);
        }

        private bool _IsActive;
        public bool IsActive
        {
            get => _IsActive;
            set
            {
                _IsActive = value;
                if (!_IsActive)
                    _isActiveChangedSubject.OnNext(_IsActive);
            }
        }

        private int _ExecShrsCum;
        public int ExecSharesCum
        {
            get => _ExecShrsCum;
            set => SetAndRaise(ref _ExecShrsCum, value);
        }

        private bool _IsReadyToRemove;
        public bool IsReadyToRemove
        {
            get => _IsReadyToRemove;
            set
            {
                _IsReadyToRemove = value;
                _isReadyToRemovedSubject.OnNext(_IsReadyToRemove);
            }
        }

        private string _OrderId;
        public string OrderId
        {
            get => _OrderId;
            set => _OrderId = value;
        }

        private string _StartAlgo;
        public string StartAlgo
        {
            get => _StartAlgo;
            set => SetAndRaise(ref _StartAlgo, value);
        }

        private string _StartSub;
        public string StartSub
        {
            get => _StartSub;
            set => SetAndRaise(ref _StartSub, value);
        }

        private string _FreeText;
        public string FreeText
        {
            get => _FreeText;
            set => SetAndRaise(ref _FreeText, value);
        }

        public bool Equals(Order obj)
        {
            return obj != null && obj.OrderId.Equals(this.OrderId, StringComparison.OrdinalIgnoreCase);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed.
        private bool disposed = false;
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    ClearAll();
                }
            }
            disposed = true;
        }

        ~Order()
        {
            this.Dispose(false);
        }

        private void ClearAll()
        {
            _isActiveChangedSubject.OnCompleted();
        }

        #endregion
    }

}