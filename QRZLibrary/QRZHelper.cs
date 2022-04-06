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
        public static List<KeyValuePair<int, string>> dxccs;

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

        public static int GetIntByStringOrNegative(string input)
        {
            int ret = -1;

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

        public static List<KeyValuePair<int, string>> GetListDXCCByCountry(string country)
        {
            List<KeyValuePair<int, string>> ret;

            if (dxccs == null)
                SetDXCCList();

            ret = (from x in dxccs where x.Value.Contains(country.ToUpper()) select x).ToList();

            return ret;
        }

        public static string GetDXCCByCountry(string country)
        {
            string ret = string.Empty;

            if (dxccs == null)
                SetDXCCList();

            int tmp = (from x in dxccs where x.Value == country.ToUpper() select x.Key).FirstOrDefault();

            if (tmp > 0)
                ret = tmp.ToString();

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

        public static List<KeyValuePair<int, string>> SetDXCCList()
        {
            dxccs = new List<KeyValuePair<int, string>>();

            dxccs.Add(new KeyValuePair<int, string>(1, "CANADA"));
            dxccs.Add(new KeyValuePair<int, string>(2, "ABU AIL IS."));
            dxccs.Add(new KeyValuePair<int, string>(3, "AFGHANISTAN"));
            dxccs.Add(new KeyValuePair<int, string>(4, "AGALEGA & ST. BRANDON IS."));
            dxccs.Add(new KeyValuePair<int, string>(5, "ALAND IS."));
            dxccs.Add(new KeyValuePair<int, string>(6, "ALASKA"));
            dxccs.Add(new KeyValuePair<int, string>(7, "ALBANIA"));
            dxccs.Add(new KeyValuePair<int, string>(8, "ALDABRA"));
            dxccs.Add(new KeyValuePair<int, string>(9, "AMERICAN SAMOA"));
            dxccs.Add(new KeyValuePair<int, string>(10, "AMSTERDAM & ST. PAUL IS."));
            dxccs.Add(new KeyValuePair<int, string>(11, "ANDAMAN & NICOBAR IS."));
            dxccs.Add(new KeyValuePair<int, string>(12, "ANGUILLA"));
            dxccs.Add(new KeyValuePair<int, string>(13, "ANTARCTICA"));
            dxccs.Add(new KeyValuePair<int, string>(14, "ARMENIA"));
            dxccs.Add(new KeyValuePair<int, string>(15, "ASIATIC RUSSIA"));
            dxccs.Add(new KeyValuePair<int, string>(16, "NEW ZEALAND SUBANTARCTIC ISLANDS"));
            dxccs.Add(new KeyValuePair<int, string>(17, "AVES I."));
            dxccs.Add(new KeyValuePair<int, string>(18, "AZERBAIJAN"));
            dxccs.Add(new KeyValuePair<int, string>(19, "BAJO NUEVO"));
            dxccs.Add(new KeyValuePair<int, string>(20, "BAKER & HOWLAND IS."));
            dxccs.Add(new KeyValuePair<int, string>(21, "BALEARIC IS."));
            dxccs.Add(new KeyValuePair<int, string>(22, "PALAU"));
            dxccs.Add(new KeyValuePair<int, string>(23, "BLENHEIM REEF"));
            dxccs.Add(new KeyValuePair<int, string>(24, "BOUVET"));
            dxccs.Add(new KeyValuePair<int, string>(25, "BRITISH NORTH BORNEO"));
            dxccs.Add(new KeyValuePair<int, string>(26, "BRITISH SOMALILAND"));
            dxccs.Add(new KeyValuePair<int, string>(27, "BELARUS"));
            dxccs.Add(new KeyValuePair<int, string>(28, "CANAL ZONE"));
            dxccs.Add(new KeyValuePair<int, string>(29, "CANARY IS."));
            dxccs.Add(new KeyValuePair<int, string>(30, "CELEBE & MOLUCCA IS."));
            dxccs.Add(new KeyValuePair<int, string>(31, "C. KIRIBATI (BRITISH PHOENIX IS.)"));
            dxccs.Add(new KeyValuePair<int, string>(32, "CEUTA & MELILLA"));
            dxccs.Add(new KeyValuePair<int, string>(33, "CHAGOS IS."));
            dxccs.Add(new KeyValuePair<int, string>(34, "CHATHAM IS."));
            dxccs.Add(new KeyValuePair<int, string>(35, "CHRISTMAS I."));
            dxccs.Add(new KeyValuePair<int, string>(36, "CLIPPERTON I."));
            dxccs.Add(new KeyValuePair<int, string>(37, "COCOS I."));
            dxccs.Add(new KeyValuePair<int, string>(38, "COCOS (KEELING) IS."));
            dxccs.Add(new KeyValuePair<int, string>(39, "COMOROS"));
            dxccs.Add(new KeyValuePair<int, string>(40, "CRETE"));
            dxccs.Add(new KeyValuePair<int, string>(41, "CROZET I."));
            dxccs.Add(new KeyValuePair<int, string>(42, "DAMAO, DIU"));
            dxccs.Add(new KeyValuePair<int, string>(43, "DESECHEO I."));
            dxccs.Add(new KeyValuePair<int, string>(44, "DESROCHES"));
            dxccs.Add(new KeyValuePair<int, string>(45, "DODECANESE"));
            dxccs.Add(new KeyValuePair<int, string>(46, "EAST MALAYSIA"));
            dxccs.Add(new KeyValuePair<int, string>(47, "EASTER I."));
            dxccs.Add(new KeyValuePair<int, string>(48, "E. KIRIBATI (LINE IS.)"));
            dxccs.Add(new KeyValuePair<int, string>(49, "EQUATORIAL GUINEA"));
            dxccs.Add(new KeyValuePair<int, string>(50, "MEXICO"));
            dxccs.Add(new KeyValuePair<int, string>(51, "ERITREA"));
            dxccs.Add(new KeyValuePair<int, string>(52, "ESTONIA"));
            dxccs.Add(new KeyValuePair<int, string>(53, "ETHIOPIA"));
            dxccs.Add(new KeyValuePair<int, string>(54, "RUSSIA"));
            dxccs.Add(new KeyValuePair<int, string>(55, "FARQUHAR"));
            dxccs.Add(new KeyValuePair<int, string>(56, "FERNANDO DE NORONHA"));
            dxccs.Add(new KeyValuePair<int, string>(57, "FRENCH EQUATORIAL AFRICA"));
            dxccs.Add(new KeyValuePair<int, string>(58, "FRENCH INDO-CHINA"));
            dxccs.Add(new KeyValuePair<int, string>(59, "FRENCH WEST AFRICA"));
            dxccs.Add(new KeyValuePair<int, string>(60, "BAHAMAS"));
            dxccs.Add(new KeyValuePair<int, string>(61, "FRANZ JOSEF LAND"));
            dxccs.Add(new KeyValuePair<int, string>(62, "BARBADOS"));
            dxccs.Add(new KeyValuePair<int, string>(63, "FRENCH GUIANA"));
            dxccs.Add(new KeyValuePair<int, string>(64, "BERMUDA"));
            dxccs.Add(new KeyValuePair<int, string>(65, "BRITISH VIRGIN IS."));
            dxccs.Add(new KeyValuePair<int, string>(66, "BELIZE"));
            dxccs.Add(new KeyValuePair<int, string>(67, "FRENCH INDIA"));
            dxccs.Add(new KeyValuePair<int, string>(68, "KUWAIT/SAUDI ARABIA NEUTRAL ZONE"));
            dxccs.Add(new KeyValuePair<int, string>(69, "CAYMAN IS."));
            dxccs.Add(new KeyValuePair<int, string>(70, "CUBA"));
            dxccs.Add(new KeyValuePair<int, string>(71, "GALAPAGOS IS."));
            dxccs.Add(new KeyValuePair<int, string>(72, "DOMINICAN REPUBLIC"));
            dxccs.Add(new KeyValuePair<int, string>(74, "EL SALVADOR"));
            dxccs.Add(new KeyValuePair<int, string>(75, "GEORGIA"));
            dxccs.Add(new KeyValuePair<int, string>(76, "GUATEMALA"));
            dxccs.Add(new KeyValuePair<int, string>(77, "GRENADA"));
            dxccs.Add(new KeyValuePair<int, string>(78, "HAITI"));
            dxccs.Add(new KeyValuePair<int, string>(79, "GUADELOUPE"));
            dxccs.Add(new KeyValuePair<int, string>(80, "HONDURAS"));
            dxccs.Add(new KeyValuePair<int, string>(81, "GERMANY"));
            dxccs.Add(new KeyValuePair<int, string>(82, "JAMAICA"));
            dxccs.Add(new KeyValuePair<int, string>(84, "MARTINIQUE"));
            dxccs.Add(new KeyValuePair<int, string>(85, "BONAIRE, CURACAO"));
            dxccs.Add(new KeyValuePair<int, string>(86, "NICARAGUA"));
            dxccs.Add(new KeyValuePair<int, string>(88, "PANAMA"));
            dxccs.Add(new KeyValuePair<int, string>(89, "TURKS & CAICOS IS."));
            dxccs.Add(new KeyValuePair<int, string>(90, "TRINIDAD & TOBAGO"));
            dxccs.Add(new KeyValuePair<int, string>(91, "ARUBA"));
            dxccs.Add(new KeyValuePair<int, string>(93, "GEYSER REEF"));
            dxccs.Add(new KeyValuePair<int, string>(94, "ANTIGUA & BARBUDA"));
            dxccs.Add(new KeyValuePair<int, string>(95, "DOMINICA"));
            dxccs.Add(new KeyValuePair<int, string>(96, "MONTSERRAT"));
            dxccs.Add(new KeyValuePair<int, string>(97, "ST. LUCIA"));
            dxccs.Add(new KeyValuePair<int, string>(98, "ST. VINCENT"));
            dxccs.Add(new KeyValuePair<int, string>(99, "GLORIOSO IS."));
            dxccs.Add(new KeyValuePair<int, string>(100, "ARGENTINA"));
            dxccs.Add(new KeyValuePair<int, string>(101, "GOA"));
            dxccs.Add(new KeyValuePair<int, string>(102, "GOLD COAST, TOGOLAND"));
            dxccs.Add(new KeyValuePair<int, string>(103, "GUAM"));
            dxccs.Add(new KeyValuePair<int, string>(104, "BOLIVIA"));
            dxccs.Add(new KeyValuePair<int, string>(105, "GUANTANAMO BAY"));
            dxccs.Add(new KeyValuePair<int, string>(106, "GUERNSEY"));
            dxccs.Add(new KeyValuePair<int, string>(107, "GUINEA"));
            dxccs.Add(new KeyValuePair<int, string>(108, "BRAZIL"));
            dxccs.Add(new KeyValuePair<int, string>(109, "GUINEA-BISSAU"));
            dxccs.Add(new KeyValuePair<int, string>(110, "HAWAII"));
            dxccs.Add(new KeyValuePair<int, string>(111, "HEARD I."));
            dxccs.Add(new KeyValuePair<int, string>(112, "CHILE"));
            dxccs.Add(new KeyValuePair<int, string>(113, "IFNI"));
            dxccs.Add(new KeyValuePair<int, string>(114, "ISLE OF MAN"));
            dxccs.Add(new KeyValuePair<int, string>(115, "ITALIAN SOMALILAND"));
            dxccs.Add(new KeyValuePair<int, string>(116, "COLOMBIA"));
            dxccs.Add(new KeyValuePair<int, string>(117, "ITU HQ"));
            dxccs.Add(new KeyValuePair<int, string>(118, "JAN MAYEN"));
            dxccs.Add(new KeyValuePair<int, string>(119, "JAVA"));
            dxccs.Add(new KeyValuePair<int, string>(120, "ECUADOR"));
            dxccs.Add(new KeyValuePair<int, string>(122, "JERSEY"));
            dxccs.Add(new KeyValuePair<int, string>(123, "JOHNSTON I."));
            dxccs.Add(new KeyValuePair<int, string>(124, "JUAN DE NOVA, EUROPA"));
            dxccs.Add(new KeyValuePair<int, string>(125, "JUAN FERNANDEZ IS."));
            dxccs.Add(new KeyValuePair<int, string>(126, "KALININGRAD"));
            dxccs.Add(new KeyValuePair<int, string>(127, "KAMARAN IS."));
            dxccs.Add(new KeyValuePair<int, string>(128, "KARELO-FINNISH REPUBLIC"));
            dxccs.Add(new KeyValuePair<int, string>(129, "GUYANA"));
            dxccs.Add(new KeyValuePair<int, string>(130, "KAZAKHSTAN"));
            dxccs.Add(new KeyValuePair<int, string>(131, "KERGUELEN IS."));
            dxccs.Add(new KeyValuePair<int, string>(132, "PARAGUAY"));
            dxccs.Add(new KeyValuePair<int, string>(133, "KERMADEC IS."));
            dxccs.Add(new KeyValuePair<int, string>(134, "KINGMAN REEF"));
            dxccs.Add(new KeyValuePair<int, string>(135, "KYRGYZSTAN"));
            dxccs.Add(new KeyValuePair<int, string>(136, "PERU"));
            dxccs.Add(new KeyValuePair<int, string>(137, "REPUBLIC OF KOREA"));
            dxccs.Add(new KeyValuePair<int, string>(138, "KURE I."));
            dxccs.Add(new KeyValuePair<int, string>(139, "KURIA MURIA I."));
            dxccs.Add(new KeyValuePair<int, string>(140, "SURINAME"));
            dxccs.Add(new KeyValuePair<int, string>(141, "FALKLAND IS."));
            dxccs.Add(new KeyValuePair<int, string>(142, "LAKSHADWEEP IS."));
            dxccs.Add(new KeyValuePair<int, string>(143, "LAOS"));
            dxccs.Add(new KeyValuePair<int, string>(144, "URUGUAY"));
            dxccs.Add(new KeyValuePair<int, string>(145, "LATVIA"));
            dxccs.Add(new KeyValuePair<int, string>(146, "LITHUANIA"));
            dxccs.Add(new KeyValuePair<int, string>(147, "LORD HOWE I."));
            dxccs.Add(new KeyValuePair<int, string>(148, "VENEZUELA"));
            dxccs.Add(new KeyValuePair<int, string>(149, "AZORES"));
            dxccs.Add(new KeyValuePair<int, string>(150, "AUSTRALIA"));
            dxccs.Add(new KeyValuePair<int, string>(151, "MALYJ VYSOTSKIJ I."));
            dxccs.Add(new KeyValuePair<int, string>(152, "MACAO"));
            dxccs.Add(new KeyValuePair<int, string>(153, "MACQUARIE I."));
            dxccs.Add(new KeyValuePair<int, string>(154, "YEMEN ARAB REPUBLIC"));
            dxccs.Add(new KeyValuePair<int, string>(155, "MALAYA"));
            dxccs.Add(new KeyValuePair<int, string>(157, "NAURU"));
            dxccs.Add(new KeyValuePair<int, string>(158, "VANUATU"));
            dxccs.Add(new KeyValuePair<int, string>(159, "MALDIVES"));
            dxccs.Add(new KeyValuePair<int, string>(160, "TONGA"));
            dxccs.Add(new KeyValuePair<int, string>(161, "MALPELO I."));
            dxccs.Add(new KeyValuePair<int, string>(162, "NEW CALEDONIA"));
            dxccs.Add(new KeyValuePair<int, string>(163, "PAPUA NEW GUINEA"));
            dxccs.Add(new KeyValuePair<int, string>(164, "MANCHURIA"));
            dxccs.Add(new KeyValuePair<int, string>(165, "MAURITIUS"));
            dxccs.Add(new KeyValuePair<int, string>(166, "MARIANA IS."));
            dxccs.Add(new KeyValuePair<int, string>(167, "MARKET REEF"));
            dxccs.Add(new KeyValuePair<int, string>(168, "MARSHALL IS."));
            dxccs.Add(new KeyValuePair<int, string>(169, "MAYOTTE"));
            dxccs.Add(new KeyValuePair<int, string>(170, "NEW ZEALAND"));
            dxccs.Add(new KeyValuePair<int, string>(171, "MELLISH REEF"));
            dxccs.Add(new KeyValuePair<int, string>(172, "PITCAIRN I."));
            dxccs.Add(new KeyValuePair<int, string>(173, "MICRONESIA"));
            dxccs.Add(new KeyValuePair<int, string>(174, "MIDWAY I."));
            dxccs.Add(new KeyValuePair<int, string>(175, "FRENCH POLYNESIA"));
            dxccs.Add(new KeyValuePair<int, string>(176, "FIJI"));
            dxccs.Add(new KeyValuePair<int, string>(177, "MINAMI TORISHIMA"));
            dxccs.Add(new KeyValuePair<int, string>(178, "MINERVA REEF"));
            dxccs.Add(new KeyValuePair<int, string>(179, "MOLDOVA"));
            dxccs.Add(new KeyValuePair<int, string>(180, "MOUNT ATHOS"));
            dxccs.Add(new KeyValuePair<int, string>(181, "MOZAMBIQUE"));
            dxccs.Add(new KeyValuePair<int, string>(182, "NAVASSA I."));
            dxccs.Add(new KeyValuePair<int, string>(183, "NETHERLANDS BORNEO"));
            dxccs.Add(new KeyValuePair<int, string>(184, "NETHERLANDS NEW GUINEA"));
            dxccs.Add(new KeyValuePair<int, string>(185, "SOLOMON IS."));
            dxccs.Add(new KeyValuePair<int, string>(186, "NEWFOUNDLAND, LABRADOR"));
            dxccs.Add(new KeyValuePair<int, string>(187, "NIGER"));
            dxccs.Add(new KeyValuePair<int, string>(188, "NIUE"));
            dxccs.Add(new KeyValuePair<int, string>(189, "NORFOLK I."));
            dxccs.Add(new KeyValuePair<int, string>(190, "SAMOA"));
            dxccs.Add(new KeyValuePair<int, string>(191, "NORTH COOK IS."));
            dxccs.Add(new KeyValuePair<int, string>(192, "OGASAWARA"));
            dxccs.Add(new KeyValuePair<int, string>(193, "OKINAWA (RYUKYU IS.)"));
            dxccs.Add(new KeyValuePair<int, string>(194, "OKINO TORI-SHIMA"));
            dxccs.Add(new KeyValuePair<int, string>(195, "ANNOBON I."));
            dxccs.Add(new KeyValuePair<int, string>(196, "PALESTINE"));
            dxccs.Add(new KeyValuePair<int, string>(197, "PALMYRA & JARVIS IS."));
            dxccs.Add(new KeyValuePair<int, string>(198, "PAPUA TERRITORY"));
            dxccs.Add(new KeyValuePair<int, string>(199, "PETER 1 I."));
            dxccs.Add(new KeyValuePair<int, string>(200, "PORTUGUESE TIMOR"));
            dxccs.Add(new KeyValuePair<int, string>(201, "PRINCE EDWARD & MARION IS."));
            dxccs.Add(new KeyValuePair<int, string>(202, "PUERTO RICO"));
            dxccs.Add(new KeyValuePair<int, string>(203, "ANDORRA"));
            dxccs.Add(new KeyValuePair<int, string>(204, "REVILLAGIGEDO"));
            dxccs.Add(new KeyValuePair<int, string>(205, "ASCENSION I."));
            dxccs.Add(new KeyValuePair<int, string>(206, "AUSTRIA"));
            dxccs.Add(new KeyValuePair<int, string>(207, "RODRIGUEZ I."));
            dxccs.Add(new KeyValuePair<int, string>(208, "RUANDA-URUNDI"));
            dxccs.Add(new KeyValuePair<int, string>(209, "BELGIUM"));
            dxccs.Add(new KeyValuePair<int, string>(210, "SAAR"));
            dxccs.Add(new KeyValuePair<int, string>(211, "SABLE I."));
            dxccs.Add(new KeyValuePair<int, string>(212, "BULGARIA"));
            dxccs.Add(new KeyValuePair<int, string>(213, "SAINT MARTIN"));
            dxccs.Add(new KeyValuePair<int, string>(214, "CORSICA"));
            dxccs.Add(new KeyValuePair<int, string>(215, "CYPRUS"));
            dxccs.Add(new KeyValuePair<int, string>(216, "SAN ANDRES & PROVIDENCIA"));
            dxccs.Add(new KeyValuePair<int, string>(217, "SAN FELIX & SAN AMBROSIO"));
            dxccs.Add(new KeyValuePair<int, string>(218, "CZECHOSLOVAKIA"));
            dxccs.Add(new KeyValuePair<int, string>(219, "SAO TOME & PRINCIPE"));
            dxccs.Add(new KeyValuePair<int, string>(220, "SARAWAK"));
            dxccs.Add(new KeyValuePair<int, string>(221, "DENMARK"));
            dxccs.Add(new KeyValuePair<int, string>(222, "FAROE IS."));
            dxccs.Add(new KeyValuePair<int, string>(223, "ENGLAND"));
            dxccs.Add(new KeyValuePair<int, string>(224, "FINLAND"));
            dxccs.Add(new KeyValuePair<int, string>(225, "SARDINIA"));
            dxccs.Add(new KeyValuePair<int, string>(226, "SAUDI ARABIA/IRAQ NEUTRAL ZONE"));
            dxccs.Add(new KeyValuePair<int, string>(227, "FRANCE"));
            dxccs.Add(new KeyValuePair<int, string>(228, "SERRANA BANK & RONCADOR CAY"));
            dxccs.Add(new KeyValuePair<int, string>(229, "GERMAN DEMOCRATIC REPUBLIC"));
            dxccs.Add(new KeyValuePair<int, string>(230, "FEDERAL REPUBLIC OF GERMANY"));
            dxccs.Add(new KeyValuePair<int, string>(231, "SIKKIM"));
            dxccs.Add(new KeyValuePair<int, string>(232, "SOMALIA"));
            dxccs.Add(new KeyValuePair<int, string>(233, "GIBRALTAR"));
            dxccs.Add(new KeyValuePair<int, string>(234, "SOUTH COOK IS."));
            dxccs.Add(new KeyValuePair<int, string>(235, "SOUTH GEORGIA I."));
            dxccs.Add(new KeyValuePair<int, string>(236, "GREECE"));
            dxccs.Add(new KeyValuePair<int, string>(237, "GREENLAND"));
            dxccs.Add(new KeyValuePair<int, string>(238, "SOUTH ORKNEY IS."));
            dxccs.Add(new KeyValuePair<int, string>(239, "HUNGARY"));
            dxccs.Add(new KeyValuePair<int, string>(240, "SOUTH SANDWICH IS."));
            dxccs.Add(new KeyValuePair<int, string>(241, "SOUTH SHETLAND IS."));
            dxccs.Add(new KeyValuePair<int, string>(242, "ICELAND"));
            dxccs.Add(new KeyValuePair<int, string>(243, "PEOPLE'S DEMOCRATIC REP. OF YEMEN"));
            dxccs.Add(new KeyValuePair<int, string>(244, "SOUTHERN SUDAN"));
            dxccs.Add(new KeyValuePair<int, string>(245, "IRELAND"));
            dxccs.Add(new KeyValuePair<int, string>(246, "SOVEREIGN MILITARY ORDER OF MALTA"));
            dxccs.Add(new KeyValuePair<int, string>(247, "SPRATLY IS."));
            dxccs.Add(new KeyValuePair<int, string>(248, "ITALY"));
            dxccs.Add(new KeyValuePair<int, string>(249, "ST. KITTS & NEVIS"));
            dxccs.Add(new KeyValuePair<int, string>(250, "ST. HELENA"));
            dxccs.Add(new KeyValuePair<int, string>(251, "LIECHTENSTEIN"));
            dxccs.Add(new KeyValuePair<int, string>(252, "ST. PAUL I."));
            dxccs.Add(new KeyValuePair<int, string>(253, "ST. PETER & ST. PAUL ROCKS"));
            dxccs.Add(new KeyValuePair<int, string>(254, "LUXEMBOURG"));
            dxccs.Add(new KeyValuePair<int, string>(255, "ST. MAARTEN, SABA, ST. EUSTATIUS"));
            dxccs.Add(new KeyValuePair<int, string>(256, "MADEIRA IS."));
            dxccs.Add(new KeyValuePair<int, string>(257, "MALTA"));
            dxccs.Add(new KeyValuePair<int, string>(258, "SUMATRA"));
            dxccs.Add(new KeyValuePair<int, string>(259, "SVALBARD"));
            dxccs.Add(new KeyValuePair<int, string>(260, "MONACO"));
            dxccs.Add(new KeyValuePair<int, string>(261, "SWAN IS."));
            dxccs.Add(new KeyValuePair<int, string>(262, "TAJIKISTAN"));
            dxccs.Add(new KeyValuePair<int, string>(263, "NETHERLANDS"));
            dxccs.Add(new KeyValuePair<int, string>(264, "TANGIER"));
            dxccs.Add(new KeyValuePair<int, string>(265, "NORTHERN IRELAND"));
            dxccs.Add(new KeyValuePair<int, string>(266, "NORWAY"));
            dxccs.Add(new KeyValuePair<int, string>(267, "TERRITORY OF NEW GUINEA"));
            dxccs.Add(new KeyValuePair<int, string>(268, "TIBET"));
            dxccs.Add(new KeyValuePair<int, string>(269, "POLAND"));
            dxccs.Add(new KeyValuePair<int, string>(270, "TOKELAU IS."));
            dxccs.Add(new KeyValuePair<int, string>(271, "TRIESTE"));
            dxccs.Add(new KeyValuePair<int, string>(272, "PORTUGAL"));
            dxccs.Add(new KeyValuePair<int, string>(273, "TRINDADE & MARTIM VAZ IS."));
            dxccs.Add(new KeyValuePair<int, string>(274, "TRISTAN DA CUNHA & GOUGH I."));
            dxccs.Add(new KeyValuePair<int, string>(275, "ROMANIA"));
            dxccs.Add(new KeyValuePair<int, string>(276, "TROMELIN I."));
            dxccs.Add(new KeyValuePair<int, string>(277, "ST. PIERRE & MIQUELON"));
            dxccs.Add(new KeyValuePair<int, string>(278, "SAN MARINO"));
            dxccs.Add(new KeyValuePair<int, string>(279, "SCOTLAND"));
            dxccs.Add(new KeyValuePair<int, string>(280, "TURKMENISTAN"));
            dxccs.Add(new KeyValuePair<int, string>(281, "SPAIN"));
            dxccs.Add(new KeyValuePair<int, string>(282, "TUVALU"));
            dxccs.Add(new KeyValuePair<int, string>(283, "UK SOVEREIGN BASE AREAS ON CYPRUS"));
            dxccs.Add(new KeyValuePair<int, string>(284, "SWEDEN"));
            dxccs.Add(new KeyValuePair<int, string>(285, "VIRGIN IS."));
            dxccs.Add(new KeyValuePair<int, string>(286, "UGANDA"));
            dxccs.Add(new KeyValuePair<int, string>(287, "SWITZERLAND"));
            dxccs.Add(new KeyValuePair<int, string>(288, "UKRAINE"));
            dxccs.Add(new KeyValuePair<int, string>(289, "UNITED NATIONS HQ"));
            dxccs.Add(new KeyValuePair<int, string>(291, "UNITED STATES OF AMERICA"));
            dxccs.Add(new KeyValuePair<int, string>(292, "UZBEKISTAN"));
            dxccs.Add(new KeyValuePair<int, string>(293, "VIET NAM"));
            dxccs.Add(new KeyValuePair<int, string>(294, "WALES"));
            dxccs.Add(new KeyValuePair<int, string>(295, "VATICAN"));
            dxccs.Add(new KeyValuePair<int, string>(296, "SERBIA"));
            dxccs.Add(new KeyValuePair<int, string>(297, "WAKE I."));
            dxccs.Add(new KeyValuePair<int, string>(298, "WALLIS & FUTUNA IS."));
            dxccs.Add(new KeyValuePair<int, string>(299, "WEST MALAYSIA"));
            dxccs.Add(new KeyValuePair<int, string>(301, "W. KIRIBATI (GILBERT IS. )"));
            dxccs.Add(new KeyValuePair<int, string>(302, "WESTERN SAHARA"));
            dxccs.Add(new KeyValuePair<int, string>(303, "WILLIS I."));
            dxccs.Add(new KeyValuePair<int, string>(304, "BAHRAIN"));
            dxccs.Add(new KeyValuePair<int, string>(305, "BANGLADESH"));
            dxccs.Add(new KeyValuePair<int, string>(306, "BHUTAN"));
            dxccs.Add(new KeyValuePair<int, string>(307, "ZANZIBAR"));
            dxccs.Add(new KeyValuePair<int, string>(308, "COSTA RICA"));
            dxccs.Add(new KeyValuePair<int, string>(309, "MYANMAR"));
            dxccs.Add(new KeyValuePair<int, string>(312, "CAMBODIA"));
            dxccs.Add(new KeyValuePair<int, string>(315, "SRI LANKA"));
            dxccs.Add(new KeyValuePair<int, string>(318, "CHINA"));
            dxccs.Add(new KeyValuePair<int, string>(321, "HONG KONG"));
            dxccs.Add(new KeyValuePair<int, string>(324, "INDIA"));
            dxccs.Add(new KeyValuePair<int, string>(327, "INDONESIA"));
            dxccs.Add(new KeyValuePair<int, string>(330, "IRAN"));
            dxccs.Add(new KeyValuePair<int, string>(333, "IRAQ"));
            dxccs.Add(new KeyValuePair<int, string>(336, "ISRAEL"));
            dxccs.Add(new KeyValuePair<int, string>(339, "JAPAN"));
            dxccs.Add(new KeyValuePair<int, string>(342, "JORDAN"));
            dxccs.Add(new KeyValuePair<int, string>(344, "DEMOCRATIC PEOPLE'S REP. OF KOREA"));
            dxccs.Add(new KeyValuePair<int, string>(345, "BRUNEI DARUSSALAM"));
            dxccs.Add(new KeyValuePair<int, string>(348, "KUWAIT"));
            dxccs.Add(new KeyValuePair<int, string>(354, "LEBANON"));
            dxccs.Add(new KeyValuePair<int, string>(363, "MONGOLIA"));
            dxccs.Add(new KeyValuePair<int, string>(369, "NEPAL"));
            dxccs.Add(new KeyValuePair<int, string>(370, "OMAN"));
            dxccs.Add(new KeyValuePair<int, string>(372, "PAKISTAN"));
            dxccs.Add(new KeyValuePair<int, string>(375, "PHILIPPINES"));
            dxccs.Add(new KeyValuePair<int, string>(376, "QATAR"));
            dxccs.Add(new KeyValuePair<int, string>(378, "SAUDI ARABIA"));
            dxccs.Add(new KeyValuePair<int, string>(379, "SEYCHELLES"));
            dxccs.Add(new KeyValuePair<int, string>(381, "SINGAPORE"));
            dxccs.Add(new KeyValuePair<int, string>(382, "DJIBOUTI"));
            dxccs.Add(new KeyValuePair<int, string>(384, "SYRIA"));
            dxccs.Add(new KeyValuePair<int, string>(386, "TAIWAN"));
            dxccs.Add(new KeyValuePair<int, string>(387, "THAILAND"));
            dxccs.Add(new KeyValuePair<int, string>(390, "TURKEY"));
            dxccs.Add(new KeyValuePair<int, string>(391, "UNITED ARAB EMIRATES"));
            dxccs.Add(new KeyValuePair<int, string>(400, "ALGERIA"));
            dxccs.Add(new KeyValuePair<int, string>(401, "ANGOLA"));
            dxccs.Add(new KeyValuePair<int, string>(402, "BOTSWANA"));
            dxccs.Add(new KeyValuePair<int, string>(404, "BURUNDI"));
            dxccs.Add(new KeyValuePair<int, string>(406, "CAMEROON"));
            dxccs.Add(new KeyValuePair<int, string>(408, "CENTRAL AFRICA"));
            dxccs.Add(new KeyValuePair<int, string>(409, "CAPE VERDE"));
            dxccs.Add(new KeyValuePair<int, string>(410, "CHAD"));
            dxccs.Add(new KeyValuePair<int, string>(411, "COMOROS"));
            dxccs.Add(new KeyValuePair<int, string>(412, "REPUBLIC OF THE CONGO"));
            dxccs.Add(new KeyValuePair<int, string>(414, "DEMOCRATIC REPUBLIC OF THE CONGO"));
            dxccs.Add(new KeyValuePair<int, string>(416, "BENIN"));
            dxccs.Add(new KeyValuePair<int, string>(420, "GABON"));
            dxccs.Add(new KeyValuePair<int, string>(422, "THE GAMBIA"));
            dxccs.Add(new KeyValuePair<int, string>(424, "GHANA"));
            dxccs.Add(new KeyValuePair<int, string>(428, "COTE D'IVOIRE"));
            dxccs.Add(new KeyValuePair<int, string>(430, "KENYA"));
            dxccs.Add(new KeyValuePair<int, string>(432, "LESOTHO"));
            dxccs.Add(new KeyValuePair<int, string>(434, "LIBERIA"));
            dxccs.Add(new KeyValuePair<int, string>(436, "LIBYA"));
            dxccs.Add(new KeyValuePair<int, string>(438, "MADAGASCAR"));
            dxccs.Add(new KeyValuePair<int, string>(440, "MALAWI"));
            dxccs.Add(new KeyValuePair<int, string>(442, "MALI"));
            dxccs.Add(new KeyValuePair<int, string>(444, "MAURITANIA"));
            dxccs.Add(new KeyValuePair<int, string>(446, "MOROCCO"));
            dxccs.Add(new KeyValuePair<int, string>(450, "NIGERIA"));
            dxccs.Add(new KeyValuePair<int, string>(452, "ZIMBABWE"));
            dxccs.Add(new KeyValuePair<int, string>(453, "REUNION I."));
            dxccs.Add(new KeyValuePair<int, string>(454, "RWANDA"));
            dxccs.Add(new KeyValuePair<int, string>(456, "SENEGAL"));
            dxccs.Add(new KeyValuePair<int, string>(458, "SIERRA LEONE"));
            dxccs.Add(new KeyValuePair<int, string>(460, "ROTUMA I."));
            dxccs.Add(new KeyValuePair<int, string>(462, "SOUTH AFRICA"));
            dxccs.Add(new KeyValuePair<int, string>(464, "NAMIBIA"));
            dxccs.Add(new KeyValuePair<int, string>(466, "SUDAN"));
            dxccs.Add(new KeyValuePair<int, string>(468, "SWAZILAND"));
            dxccs.Add(new KeyValuePair<int, string>(470, "TANZANIA"));
            dxccs.Add(new KeyValuePair<int, string>(474, "TUNISIA"));
            dxccs.Add(new KeyValuePair<int, string>(478, "EGYPT"));
            dxccs.Add(new KeyValuePair<int, string>(480, "BURKINA FASO"));
            dxccs.Add(new KeyValuePair<int, string>(482, "ZAMBIA"));
            dxccs.Add(new KeyValuePair<int, string>(483, "TOGO"));
            dxccs.Add(new KeyValuePair<int, string>(488, "WALVIS BAY"));
            dxccs.Add(new KeyValuePair<int, string>(489, "CONWAY REEF"));
            dxccs.Add(new KeyValuePair<int, string>(490, "BANABA I. (OCEAN I.)"));
            dxccs.Add(new KeyValuePair<int, string>(492, "YEMEN"));
            dxccs.Add(new KeyValuePair<int, string>(493, "PENGUIN IS."));
            dxccs.Add(new KeyValuePair<int, string>(497, "CROATIA"));
            dxccs.Add(new KeyValuePair<int, string>(499, "SLOVENIA"));
            dxccs.Add(new KeyValuePair<int, string>(501, "BOSNIA-HERZEGOVINA"));
            dxccs.Add(new KeyValuePair<int, string>(502, "MACEDONIA"));
            dxccs.Add(new KeyValuePair<int, string>(503, "CZECH REPUBLIC"));
            dxccs.Add(new KeyValuePair<int, string>(504, "SLOVAK REPUBLIC"));
            dxccs.Add(new KeyValuePair<int, string>(505, "PRATAS I."));
            dxccs.Add(new KeyValuePair<int, string>(506, "SCARBOROUGH REEF"));
            dxccs.Add(new KeyValuePair<int, string>(507, "TEMOTU PROVINCE"));
            dxccs.Add(new KeyValuePair<int, string>(508, "AUSTRAL I."));
            dxccs.Add(new KeyValuePair<int, string>(509, "MARQUESAS IS."));
            dxccs.Add(new KeyValuePair<int, string>(510, "PALESTINE"));
            dxccs.Add(new KeyValuePair<int, string>(511, "TIMOR-LESTE"));
            dxccs.Add(new KeyValuePair<int, string>(512, "CHESTERFIELD IS."));
            dxccs.Add(new KeyValuePair<int, string>(513, "DUCIE I."));
            dxccs.Add(new KeyValuePair<int, string>(514, "MONTENEGRO"));
            dxccs.Add(new KeyValuePair<int, string>(515, "SWAINS I."));
            dxccs.Add(new KeyValuePair<int, string>(516, "SAINT BARTHELEMY"));
            dxccs.Add(new KeyValuePair<int, string>(517, "CURACAO"));
            dxccs.Add(new KeyValuePair<int, string>(518, "ST MAARTEN"));
            dxccs.Add(new KeyValuePair<int, string>(519, "SABA & ST. EUSTATIUS"));
            dxccs.Add(new KeyValuePair<int, string>(520, "BONAIRE"));
            dxccs.Add(new KeyValuePair<int, string>(521, "SOUTH SUDAN (REPUBLIC OF)"));
            dxccs.Add(new KeyValuePair<int, string>(522, "REPUBLIC OF KOSOVO"));

            return dxccs;
        }
    }
}
