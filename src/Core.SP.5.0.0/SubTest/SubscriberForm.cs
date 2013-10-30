using System;
using System.Configuration;
using System.ServiceModel;
using System.Windows.Forms;
using com.Sconit.Service;
using com.Sconit.Services.SP;
using com.Sconit.PrintModel;
using com.Sconit.PrintModel.ORD;


namespace SubTest
{
 
    public partial class Subscriber : Form
    {
        ISubscription _proxy;
        static int _eventCount;
        string _endpoint = string.Empty;

        public Subscriber()
        {
            InitializeComponent();

            _endpoint = ConfigurationManager.AppSettings["EndpointAddress"];
            MakeProxy(_endpoint, this);
            _eventCount = 0;
            txtTopicName.Text = "asdf";
        }   


        public void MakeProxy(string EndpoindAddress, object callbackinstance)
        {
            NetTcpBinding netTcpbinding = new NetTcpBinding(SecurityMode.None);
            EndpointAddress endpointAddress = new EndpointAddress(EndpoindAddress);
            InstanceContext context = new InstanceContext(callbackinstance);        

            DuplexChannelFactory<ISubscription> channelFactory = new DuplexChannelFactory<ISubscription>(new InstanceContext(this), netTcpbinding, endpointAddress);
            _proxy  = channelFactory.CreateChannel();
        }

        void OnSubscribe(object sender, EventArgs e)
        {
            try
            {
                string topicName = txtTopicName.Text.Trim();
                if (string.IsNullOrEmpty(topicName))
                {
                    MessageBox.Show("Please Enter a Topic Name");
                    return;
                }
                //_proxy.Subscribe(topicName);
                //_proxy.Subscribe(1002, "RM-WIP1", "WIP1");
                ((Button)sender).Visible = false;
                button2.Visible = true;
            }
            catch
            {

            }
        }

        void OnUnSubscribe(object sender, EventArgs e)
        {
            string topicName = txtTopicName.Text.Trim();
            if (string.IsNullOrEmpty(topicName))
            {
                MessageBox.Show("Please Enter a Topic Name");
                return;
            }
            ((Button)sender).Visible = false;
            button1.Visible = true;
            //_proxy.Subscribe(1, "flow", "partfrom");
            //_proxy.UnSubscribe(topicName);
        }



        private void btnClearAstaListView_Click(object sender, EventArgs e)
        {
            lstEvents.Items.Clear();
        }

        #region IMyEvents Members


        public void Publish(PrintBase o)
        {
            if (o != null)
            {
                if (o.GetType() == typeof(PrintOrderMaster))
                {
                    PrintOrderMaster orderMaster = (PrintOrderMaster)o;
                    int itemNum = (lstEvents.Items.Count < 1) ? 0 : lstEvents.Items.Count;
                    lstEvents.Items.Add(itemNum.ToString());
                    lstEvents.Items[itemNum].SubItems.AddRange(new string[] { orderMaster.OrderNo.ToString(), orderMaster.PartyFrom });
                    _eventCount += 1;
                    txtAstaEventCount.Text = _eventCount.ToString();
                }

            }
        }

        //public void Publish(OrderMaster e, String topicName)
        //{
        //    if (e != null)
        //    {
        //        int itemNum = (lstEvents.Items.Count < 1) ? 0 : lstEvents.Items.Count;
        //        lstEvents.Items.Add(itemNum.ToString());
        //        lstEvents.Items[itemNum].SubItems.AddRange(new string[] { e.OrderNo.ToString(), e.PartyFrom });
        //        _eventCount += 1;
        //        txtAstaEventCount.Text = _eventCount.ToString();

        //    }
        //}

        #endregion
    }

}