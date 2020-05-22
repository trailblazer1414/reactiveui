using System;
using System.Collections.Generic;
using System.Text;

namespace SignalrServer
{
    public class Order
    {
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

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
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

        private int _ExecShrsCum;
        public int ExecSharesCum
        {
            get => _ExecShrsCum;
            set => _ExecShrsCum = value;
        }

        private double _AvgPriceCum;
        public double AvgPriceCum
        {
            get => _AvgPriceCum;
            set => _AvgPriceCum = value;
        }

        private string _OrderId;
        public string OrderId
        {
            get => _OrderId;
            set => _OrderId = value;
        }

        private string _Status;
        public string Status
        {
            get => _Status;
            set => _Status = value;
        }

        private string _Rsn;
        public string Rsn
        {
            get => _Rsn;
            set => _Rsn = value;
        }

    }
}
