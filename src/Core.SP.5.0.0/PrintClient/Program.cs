using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PrintClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool createNew;
            using (System.Threading.Mutex m = new System.Threading.Mutex(true, Application.ProductName, out createNew))
            {
                if (createNew)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    var printClient = new PrintClient();
                    if (!printClient.IsServerOK)
                    {
                        return;
                    }
                    //Application.Run(new Login());
                    Application.Run(printClient);
                }
                else
                {
                    MessageBox.Show("Only one instance of this application is allowed!");
                }
            } 
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new PrintClient());
            //Application.Run(new Login());
        }
    }
}
