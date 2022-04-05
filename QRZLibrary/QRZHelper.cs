using QRZLibrary.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace QRZLibrary
{
    public static class QRZHelper
    {
        public static void initWBEdge11()
        {
            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
               @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION",
               true))
            {
                string app = $"{Assembly.GetEntryAssembly().GetName().Name}.exe";
                key.SetValue(app, 11001, Microsoft.Win32.RegistryValueKind.DWord);
                key.Close();
            }
        }

        public static HtmlElement GetElementByTagAndClassName(HtmlElement parent, string tagname, string classname)
        {
            if (parent != null)
            {
                HtmlElementCollection els = parent.GetElementsByTagName(tagname);

                foreach (HtmlElement el in els)
                {
                    if (el.GetAttribute("className") == classname)
                    {
                        return el;
                    }
                }
            }

            return null;
        }

        public static HtmlElement GetElementContainsValue(HtmlElement parent, string value, string classname = "")
        {
            foreach (HtmlElement el in parent.Children)
            {
                if (el.InnerText != null && el.InnerText.Contains(value) && (classname == "" || el.GetAttribute("className") == classname)) {
                    return el;
                }
            }
            return null;
        }

        public static HtmlElement GetLastChildren(HtmlElement parent, string tagname = "")
        {
            HtmlElement ret = null;
            foreach (HtmlElement el in parent.Children)
            {
                if (tagname == "" || el.TagName == tagname)
                    ret = el;
            }
            return ret;
        }

        public static int GetIntByString(string input)
        {
            int ret = 0;

            if (int.TryParse(input, out int tmp))
                ret = tmp;

            return ret;
        }

        public static DateTime GetDateTimeByString(string input)
        {
            DateTime ret = DateTime.MinValue;

            if (DateTime.TryParse(input, out DateTime tmp))
                ret = tmp;

            return ret;
        }

        public static string SerializeLogbookEntryCollection(List<LogbookEntry> collection)
        {
            string ret = string.Empty;

            var aSerializer = new XmlSerializer(typeof(List<LogbookEntry>));
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            aSerializer.Serialize(sw, new List<LogbookEntry>(collection)); // pass an instance of A
            string xmlResult = sw.GetStringBuilder().ToString();
            ret = xmlResult;
            return ret;
        }

        public static List<BandFrequencyRange> GetBandFrequencies()
        {
            List<BandFrequencyRange> bfr = new List<BandFrequencyRange>();

            bfr.Add(new BandFrequencyRange("2190m", 0.1357, 0.1378));
            bfr.Add(new BandFrequencyRange("560m", 0.501, 0.504));
            bfr.Add(new BandFrequencyRange("160m", 1.800, 2.000));
            bfr.Add(new BandFrequencyRange("80m", 3.500, 4.000));
            bfr.Add(new BandFrequencyRange("60m", 5.06, 5.45));
            bfr.Add(new BandFrequencyRange("40m", 7.000, 7.300));
            bfr.Add(new BandFrequencyRange("30m", 10.100, 10.150));
            bfr.Add(new BandFrequencyRange("20m", 14.000, 14.350));
            bfr.Add(new BandFrequencyRange("17m", 18.068, 18.168));
            bfr.Add(new BandFrequencyRange("15m", 21.000, 21.450));
            bfr.Add(new BandFrequencyRange("12m", 24.890, 24.990));
            bfr.Add(new BandFrequencyRange("10m", 28.000, 29.700));
            bfr.Add(new BandFrequencyRange("6m", 50, 54));
            bfr.Add(new BandFrequencyRange("4m", 70, 71));
            bfr.Add(new BandFrequencyRange("2m", 144, 148));
            bfr.Add(new BandFrequencyRange("1.25m", 222, 225));
            bfr.Add(new BandFrequencyRange("70cm", 420, 450));
            bfr.Add(new BandFrequencyRange("33cm", 902, 928));
            bfr.Add(new BandFrequencyRange("23cm", 1240, 1300));
            bfr.Add(new BandFrequencyRange("13cm", 2300, 2450));
            bfr.Add(new BandFrequencyRange("9cm", 3300, 3500));
            bfr.Add(new BandFrequencyRange("6cm", 5650, 5925));
            bfr.Add(new BandFrequencyRange("3cm", 10000, 10500));
            bfr.Add(new BandFrequencyRange("1.25cm", 24000, 24250));
            bfr.Add(new BandFrequencyRange("6mm", 47000, 47200));
            bfr.Add(new BandFrequencyRange("4mm", 75500, 81000));
            bfr.Add(new BandFrequencyRange("2.5mm", 119980, 141020));
            bfr.Add(new BandFrequencyRange("2mm", 142000, 149000));
            bfr.Add(new BandFrequencyRange("1mm", 241000, 250000));
            bfr.Add(new BandFrequencyRange("630m", 0.472, 0.479));
            bfr.Add(new BandFrequencyRange("8m", 40.000, 45.000));
            bfr.Add(new BandFrequencyRange("5m", 54.000001, 69.900));

            return bfr;
        }


        public static List<string> GetModes()
        {
            List<string> m = new List<string>();

            m.Add("AM");
            m.Add("ARDOP");
            m.Add("ATV");
            m.Add("C4FM");
            m.Add("CHIP");
            m.Add("CHIP128");
            m.Add("CHIP64");
            m.Add("CLO");
            m.Add("CONTESTI");
            m.Add("CW");
            m.Add("PCW");
            m.Add("DIGITALVOICE");
            m.Add("DOMINO");
            m.Add("DOM-M");
            m.Add("DOM11");
            m.Add("DOM16");
            m.Add("DOM22");
            m.Add("DOM4");
            m.Add("DOM44");
            m.Add("DOM5");
            m.Add("DOM8");
            m.Add("DOM88");
            m.Add("DOMINOEX");
            m.Add("DOMINOF");
            m.Add("DSTAR");
            m.Add("FAX");
            m.Add("FM");
            m.Add("FreeDV");
            m.Add("FSK441");
            m.Add("FT8");
            m.Add("HELL");
            m.Add("FMHELL");
            m.Add("FSKHELL");
            m.Add("HELL80");
            m.Add("HELLX5");
            m.Add("HELLX9");
            m.Add("HFSK");
            m.Add("PSKHELL");
            m.Add("SLOWHELL");
            m.Add("ISCAT");
            m.Add("ISCAT-A");
            m.Add("ISCAT-B");
            m.Add("JT4");
            m.Add("JT4A");
            m.Add("JT4B");
            m.Add("JT4C");
            m.Add("JT4D");
            m.Add("JT4E");
            m.Add("JT4F");
            m.Add("JT4G");
            m.Add("JT44");
            m.Add("JT65");
            m.Add("JT65A");
            m.Add("JT65B");
            m.Add("JT65B2");
            m.Add("JT65C");
            m.Add("JT65C2");
            m.Add("JT6M");
            m.Add("JT9");
            m.Add("JT9-1");
            m.Add("JT9-10");
            m.Add("JT9-2");
            m.Add("JT9-30");
            m.Add("JT9-5");
            m.Add("JT9A");
            m.Add("JT9B");
            m.Add("JT9C");
            m.Add("JT9D");
            m.Add("JT9E");
            m.Add("JT9EFAST");
            m.Add("JT9F");
            m.Add("JT9FFAST");
            m.Add("JT9G");
            m.Add("JT9GFAST");
            m.Add("JT9H");
            m.Add("JT9HFAST");
            m.Add("MFSK");
            m.Add("FSQCALL");
            m.Add("FST4");
            m.Add("FST4W");
            m.Add("FT4");
            m.Add("JS8");
            m.Add("JTMS");
            m.Add("MFSK11");
            m.Add("MFSK128");
            m.Add("MFSK128L");
            m.Add("MFSK16");
            m.Add("MFSK22");
            m.Add("MFSK31");
            m.Add("MFSK32");
            m.Add("MFSK4");
            m.Add("MFSK64");
            m.Add("MFSK64L");
            m.Add("MFSK8");
            m.Add("Q65");
            m.Add("MSK144");
            m.Add("MT63");
            m.Add("OLIVIA");
            m.Add("OLIVIA16/1000");
            m.Add("OLIVIA16/500");
            m.Add("OLIVIA32/1000");
            m.Add("OLIVIA4/125");
            m.Add("OLIVIA4/250");
            m.Add("OLIVIA8/250");
            m.Add("OLIVIA8/500");
            m.Add("OPERA");
            m.Add("OPERA-BEACON");
            m.Add("OPERA-QSO");
            m.Add("PAC");
            m.Add("PAC2");
            m.Add("PAC3");
            m.Add("PAC4");
            m.Add("PAX");
            m.Add("PAX2");
            m.Add("PKT");
            m.Add("PSK");
            m.Add("8PSK1000");
            m.Add("8PSK1000F");
            m.Add("8PSK1200F");
            m.Add("8PSK125");
            m.Add("8PSK125F");
            m.Add("8PSK125FL");
            m.Add("8PSK250");
            m.Add("8PSK250F");
            m.Add("8PSK250FL");
            m.Add("8PSK500");
            m.Add("8PSK500F");
            m.Add("FSK31");
            m.Add("PSK10");
            m.Add("PSK1000");
            m.Add("PSK1000C2");
            m.Add("PSK1000R");
            m.Add("PSK1000RC2");
            m.Add("PSK125");
            m.Add("PSK125C12");
            m.Add("PSK125R");
            m.Add("PSK125RC10");
            m.Add("PSK125RC12");
            m.Add("PSK125RC16");
            m.Add("PSK125RC4");
            m.Add("PSK125RC5");
            m.Add("PSK250");
            m.Add("PSK250C6");
            m.Add("PSK250R");
            m.Add("PSK250RC2");
            m.Add("PSK250RC3");
            m.Add("PSK250RC5");
            m.Add("PSK250RC6");
            m.Add("PSK250RC7");
            m.Add("PSK31");
            m.Add("PSK500");
            m.Add("PSK500C2");
            m.Add("PSK500C4");
            m.Add("PSK500R");
            m.Add("PSK500RC2");
            m.Add("PSK500RC3");
            m.Add("PSK500RC4");
            m.Add("PSK63");
            m.Add("PSK63F");
            m.Add("PSK63RC10");
            m.Add("PSK63RC20");
            m.Add("PSK63RC32");
            m.Add("PSK63RC4");
            m.Add("PSK63RC5");
            m.Add("PSK800C2");
            m.Add("PSK800RC2");
            m.Add("PSKAM10");
            m.Add("PSKAM31");
            m.Add("PSKAM50");
            m.Add("PSKFEC31");
            m.Add("QPSK125");
            m.Add("QPSK250");
            m.Add("QPSK31");
            m.Add("QPSK500");
            m.Add("QPSK63");
            m.Add("SIM31");
            m.Add("PSK2K");
            m.Add("Q15");
            m.Add("QRA64");
            m.Add("QRA64A");
            m.Add("QRA64B");
            m.Add("QRA64C");
            m.Add("QRA64D");
            m.Add("QRA64E");
            m.Add("ROS");
            m.Add("ROS-EME");
            m.Add("ROS-HF");
            m.Add("ROS-MF");
            m.Add("RTTY");
            m.Add("ASCI");
            m.Add("RTTYM");
            m.Add("SSB");
            m.Add("LSB");
            m.Add("USB");
            m.Add("SSTV");
            m.Add("T10");
            m.Add("THOR");
            m.Add("THOR-M");
            m.Add("THOR100");
            m.Add("THOR11");
            m.Add("THOR16");
            m.Add("THOR22");
            m.Add("THOR25X4");
            m.Add("THOR4");
            m.Add("THOR5");
            m.Add("THOR50X1");
            m.Add("THOR50X2");
            m.Add("THOR8");
            m.Add("THRB");
            m.Add("THRBX");
            m.Add("THRBX1");
            m.Add("THRBX2");
            m.Add("THRBX4");
            m.Add("THROB1");
            m.Add("THROB2");
            m.Add("THROB4");
            m.Add("TOR");
            m.Add("AMTORFEC");
            m.Add("GTOR");
            m.Add("NAVTEX");
            m.Add("SITORB");
            m.Add("V4");
            m.Add("VOI");
            m.Add("WINMOR");
            m.Add("WSPR");

            return m;
        }

    }
}
