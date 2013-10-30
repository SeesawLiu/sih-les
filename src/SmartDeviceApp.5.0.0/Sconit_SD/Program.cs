using System;

using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

namespace com.Sconit.SmartDevice
{
    static class Program
    {

        const string appName = "SmartDevice";
        const int ALREADY_EXISTS = 183;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {
            startTicks = Environment.TickCount;
            Application.Run(new MainForm());

            //using (AppExecutionManager execMgr = new AppExecutionManager(appName))
            //{
            //    if (execMgr.IsFirstInstance)
            //        Application.Run(new MainForm());
            //}
        }
        internal static int startTicks;
    }
}