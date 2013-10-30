using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FormTest.SessionReference;

namespace FormTest
{
    public partial class Main : Form
    {
        IpServiceClient ip = new IpServiceClient();
        public Main()
        {
            InitializeComponent();
        }

        private void btnSetName_Click(object sender, EventArgs e)
        {
            try
            {
                Hu i = ip.GetHu(this.tbUserName.Text.Trim());
                MessageBox.Show(i.HuIdk__BackingField);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnGetName_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show("orderMaster.OrderNo");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
