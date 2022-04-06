using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRZLibrary.Classes
{
    public class LogbookEntry
    {
        public int position { get; set; }
        public string QRZRN { get; set; }
        public DateTime QSODateTime { get; set; }
        public string Call { get; set; }
        public string Band { get; set; }
        public string Frequency { get; set; }
        public string Mode { get; set; }
        public string GridLocator { get; set; }
        public string DXCC { get; set; }
        public string Country { get; set; }
        public string OperatorName { get; set; }
        public string Comments { get; set; }
        public bool Confirmed { get; set;  }
    }
}
