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

        private string _OrderId;
        public string OrderId
        {
            get => _OrderId;
            set => _OrderId = value;
        }

    }
}
