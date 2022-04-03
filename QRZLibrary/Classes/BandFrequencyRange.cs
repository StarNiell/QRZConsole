using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRZLibrary.Classes
{
    public class BandFrequencyRange
    {
        public string Band { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public BandFrequencyRange(string band, double min, double max)
        {
            Band = band;
            Min = min;
            Max = max;
        }
    }
}
