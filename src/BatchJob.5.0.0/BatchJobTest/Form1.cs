using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Castle.Windsor;
using Castle.Windsor.Installer;
using com.Sconit.Service.BatchJob;

namespace BatchJobTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IWindsorContainer container = new WindsorContainer();
            container.Install(Configuration.FromAppConfig());
            container.Install(FromAssembly.Named("com.Sconit.BatchJob.WindowsService"));
            IJobRunMgr jobRunMgr = container.Resolve<IJobRunMgr>();
            jobRunMgr.RunBatchJobs(container);
            container.Dispose();
        }
    }
}
