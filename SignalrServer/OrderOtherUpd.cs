using System;
using System.Collections.Generic;
using System.Text;

namespace SignalrServer
{
    public class OrderOtherUpd
    {
        private string _OrdId;
        public string OrdId
        {
            get => _OrdId;
            set => _OrdId = value;
        }

        public string StartSub { get; set; }
        public string StartAlg { get; set; }
        public string LastMkt { get; set; }

        public string Mode { get; set; }
        public string FreeText { get; set; }

        public DateTime UpdTime { get; set; }

    }
}
