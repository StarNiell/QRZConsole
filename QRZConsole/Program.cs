using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QRZConsole
{
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // .NET (Core) changed the default font to Segoe UI 9pt: keep the .NET Framework one
            // so that the layout designed with AutoScaleMode.Font does not get rescaled.
            Application.SetDefaultFont(new System.Drawing.Font("Microsoft Sans Serif", 8.25F));
            Application.Run(new frmMain());
        }
    }
}
