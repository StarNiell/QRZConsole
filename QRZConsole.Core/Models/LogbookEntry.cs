using System;
using System.Xml.Serialization;

namespace QRZConsole.Core.Models
{
    /// <summary>One QSO row of the QRZ logbook.</summary>
    public class LogbookEntry
    {
        /// <summary>1-based position of the QSO in the logbook (depends on the current sort order).</summary>
        [XmlElement("position")]
        public int Position { get; set; }
        public string QRZRN { get; set; } = string.Empty;
        public DateTime QSODateTime { get; set; }
        public string Call { get; set; } = string.Empty;
        public string Band { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Mode { get; set; } = string.Empty;
        public string GridLocator { get; set; } = string.Empty;
        public string DXCC { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public bool Confirmed { get; set; }
        public bool LoTWSent { get; set; }
    }
}
