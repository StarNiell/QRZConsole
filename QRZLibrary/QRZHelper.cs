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

    }
}
