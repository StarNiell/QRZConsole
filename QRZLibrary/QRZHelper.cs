using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            HtmlElementCollection els = parent.GetElementsByTagName(tagname);

            foreach (HtmlElement el in els)
            {
                if (el.GetAttribute("className") == classname)
                {
                    return el;
                }
            }

            return null;
        }

        public static HtmlElement GetElementContainsValue(HtmlElement parent, string value)
        {
            foreach (HtmlElement el in parent.Children)
            {
                if (el.InnerText != null && el.InnerText.Contains(value)) {
                    return el;
                }
            }
            return null;
        }

    }
}
