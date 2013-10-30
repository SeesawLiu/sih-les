using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SubTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Threading.Thread.Sleep(1000);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Subscriber());
        }
    }
}
