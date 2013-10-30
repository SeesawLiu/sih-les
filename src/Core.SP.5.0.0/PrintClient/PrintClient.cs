using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;
using Castle.Core.Logging;
using Castle.Windsor;
using com.Sconit.PrintModel;
using com.Sconit.PrintModel.INV;
using com.Sconit.PrintModel.ORD;
using com.Sconit.Utility.Report;
using com.Sconit.Utility.Report.Operator;
using com.Sconit.Service;
using com.Sconit.Services.SP;
using System.Xml.Serialization;
using System.IO;

namespace PrintClient
{
    public partial class PrintClient : Form
    {
        #region 静态全局变量
        private static IWindsorContainer container;
        private static Int32 maxId = 0;
        private static IList<ClientData> noAutoPrintDataList = new List<ClientData>();
        private static IList<ClientData> printedDataList = new List<ClientData>();
        private static IList<ClientData> unPrintedDataList = new List<ClientData>();

        private static IList<SubscriberData> autoPrintSubList = new List<SubscriberData>();
        private static IList<SubscriberData> noAutoPrintSubList = new List<SubscriberData>();
        private static string connstr = ConfigurationManager.ConnectionStrings["OLEDBConnection"].ToString();
        private static string templatePath = ConfigurationManager.AppSettings["TemplatePath"].ToString();
        private static string currentMachineMac = Utility.WMIGetMACString()[0];
        #endregion

        #region 私有成员变量/属性
        private ILogger logger { get; set; }
        private IReportGen reportGen { get; set; }

        private string userName, password;
        private OleDbConnection conn = new OleDbConnection(connstr);
        #endregion

        #region 公有成员变量/属性
        public Boolean IsServerOK { get; set; }
        #endregion

        #region 窗体方法
        public PrintClient()
        {
            container = new WindsorContainer();
            container.Install(Castle.Windsor.Installer.Configuration.FromAppConfig());
            container.Install(Castle.Windsor.Installer.FromAssembly.This());
            IsServerOK = this.OpenHeartBeatToServer();
            InitializeComponent();
            LoadSubList();
            Inti();
        }

        public void LoadSubList()
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(List<SubscriberData>));
                Stream stream = new FileStream(System.AppDomain.CurrentDomain.BaseDirectory+"SubInfo.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                List<SubscriberData> subscriberDataList = xs.Deserialize(stream) as List<SubscriberData>;//xs.Deserialize(fs) as List<Peoson>;
                
                if (subscriberDataList.Count > 0)
                {
                    foreach (var subscriberData in subscriberDataList)
                    {
                        if (subscriberData.IsAutoPrint)
                        {
                            autoPrintSubList.Add(subscriberData);
                        }
                        else
                        {
                            noAutoPrintSubList.Add(subscriberData);
                        }
                        DataGridViewRow row = (DataGridViewRow)dgvSubscribed.Rows[0].Clone();
                        row.Cells[0].Value = maxId++;
                        row.Cells[1].Value = Utility.GetEnumDescription(typeof(DocumentsType), subscriberData.SubscribeType);
                        row.Cells[2].Value = subscriberData.Flow;
                        row.Cells[3].Value = subscriberData.Region;
                        row.Cells[4].Value = subscriberData.UserName;
                        row.Cells[5].Value = subscriberData.PrinterName;
                        row.Cells[6].Value = subscriberData.IsAutoPrint;
                        dgvSubscribed.Rows.Add(row);
                    }
                }
                stream.Dispose();
            }
            catch
            {
 
            }
        }

        void Inti()
        {
            string[] enumNames = System.Enum.GetNames(typeof(DocumentsType));
            foreach (var item in enumNames)
            {
                cmbSubType.Items.Add(new KeyValuePair<string, string>(item, Utility.GetEnumDescription(typeof(DocumentsType),item)));
            }
            cmbSubType.DisplayMember = "Value";
            cmbSubType.ValueMember = "Key";
            cmbSubType.SelectedIndex = 0;
            cmbSubType.SelectedIndexChanged += new System.EventHandler(cmbSubType_SelectedIndexChanged);
            btnChoosePrinter.Click += new EventHandler(btnChoosePrinter_Click);
            btnAdd.Click += new EventHandler(btnAdd_Click);
            if (!File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "SubInfo.xml"))
            {
                File.Create(System.AppDomain.CurrentDomain.BaseDirectory + "SubInfo.xml");
            }
        }

        void cmbSubType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            DocumentsType currType = (DocumentsType)Enum.Parse(typeof(DocumentsType), ((KeyValuePair<string,string>)cmbSubType.SelectedItem).Key, false);
            if (currType == DocumentsType.CloneHu)
            {
                this.labelFlow.Visible = false;
                this.tbFlow.Visible = false;
                this.labelRegion.Visible = false;
                this.tbRegion.Visible = false;
                this.labelUserName.Visible = true;
                this.tbUserName.Visible = true;
            }
            else 
            {
                this.labelFlow.Visible = true;
                this.tbFlow.Visible = true;
                this.labelRegion.Visible = true;
                this.tbRegion.Visible = true;
                this.labelUserName.Visible = false;
                this.tbUserName.Visible = false;
            }
        }

        private Boolean OpenHeartBeatToServer()
        {
            try
            {
                Publishing publishing = new Publishing("HeartBeat", "", false);
                ISubscription proxy = CreateProxy(publishing);
                proxy.Subscribe(currentMachineMac, "", "", "", "");
                return true;

            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                if (ex is EndpointNotFoundException)
                {
                    MessageBox.Show("服务器连接不上", "服务器错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(ex.Message, "服务器错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            //currentMachineMac
        }

        private void CloseHeartBeatToServer()
        {
            try
            {
                Publishing publishing = new Publishing("HeartBeat", "", false);
                ISubscription proxy = CreateProxy(publishing);
                proxy.UnSubscribe(currentMachineMac, "", "", "", "");
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                if (ex is EndpointNotFoundException)
                {
                    MessageBox.Show("服务器连接不上", "服务器错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(ex.Message, "服务器错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            //currentMachineMac
        }

        private void PrintClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Login login = new Login();
            //login.loginEvent += new Login.loginDelegate(GetUser);
            //login.ShowDialog();
            //SconitWs.SecurityService securityService = new SconitWs.SecurityService();
            //if (!securityService.VerifyUserPassword(this.userName,this.password))
            //{
            //    return;
            //}

            //using (OleDbConnection conn = new OleDbConnection(connstr))
            //{
            //    conn.Open();
            //    OleDbCommand cmd = new OleDbCommand("select * from T_Config", conn);

            //    OleDbDataReader reader = cmd.ExecuteReader();
            //    while (reader.Read())
            //    {
            //        Publishing publishing = new Publishing(reader[5].ToString(), reader[1].ToString(), reader.GetBoolean(6));
            //        ISubscription proxy = CreateProxy(publishing);
            //        proxy.UnSubscribe(reader[6].ToString(), reader[1].ToString(), reader[2].ToString(), reader[3].ToString(), reader[4].ToString());
            //    }
            //}
            foreach (var sub in noAutoPrintSubList)
            {
                //proxy.UnSubscribe(sub.GUID, sub.SubscribeType, sub.Flow, sub.Region, sub.UserName);
                Publishing publishing = new Publishing(sub.PrinterName, sub.SubscribeType, sub.IsAutoPrint);
                ISubscription proxy = CreateProxy(publishing);
                proxy.UnSubscribe(sub.GUID, sub.SubscribeType, sub.Flow, sub.Region, sub.UserName);
            }
            foreach (var sub in autoPrintSubList)
            {
                //proxy.UnSubscribe(sub.GUID, sub.SubscribeType, sub.Flow, sub.Region, sub.UserName);
                Publishing publishing = new Publishing(sub.PrinterName, sub.SubscribeType, sub.IsAutoPrint);
                ISubscription proxy = CreateProxy(publishing);
                proxy.UnSubscribe(sub.GUID, sub.SubscribeType, sub.Flow, sub.Region, sub.UserName);
            }

            this.CloseHeartBeatToServer();
        }

        void btnAdd_Click(object sender, EventArgs e)
        {
            string subscribeType = ((KeyValuePair<string, string>)cmbSubType.SelectedItem).Key;
            string flow = tbFlow.Text.Trim();
            string region = tbRegion.Text.Trim();
            string userName = tbUserName.Text.Trim();
            string printerName = tbPrinterName.Text.Trim();
            string GUID = Guid.NewGuid() + currentMachineMac;
            Boolean isAutoPrint = ckIsAutoPrint.Checked;

            if (string.IsNullOrEmpty(flow) && string.IsNullOrEmpty(region) && string.IsNullOrEmpty(userName))
            {
                MessageBox.Show("请输入订阅条件。", "用户输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            //try
            //{
            //    conn.Open();
            //    OleDbCommand cmd = new OleDbCommand("insert into T_Config(SubType,Flow,Region,PrinterName,Guid)values(" + subscribeType.ToString() + ",'" + flow + "','" + region + "','" + printerName + "','" + GUID + "')", conn);
            //    cmd.ExecuteNonQuery();
            //    cmd = new OleDbCommand("select max(id) from T_Config", conn);
            //    Id = (int)cmd.ExecuteScalar();
                
            //}
            //catch (Exception ex)
            //{
            //    logger = container.Resolve<ILogger>();
            //    logger.Error(ex.Message);
            //}
            if (autoPrintSubList.Any(o => o.SubscribeType == subscribeType && o.Flow == flow && o.Region == region
                && o.UserName == userName))
            {
                MessageBox.Show("该订阅条件已存在，请修改。", "用户输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = (DataGridViewRow)dgvSubscribed.Rows[0].Clone();
            row.Cells[0].Value = maxId++;
            row.Cells[1].Value = Utility.GetEnumDescription(typeof(DocumentsType), ((KeyValuePair<string, string>)cmbSubType.SelectedItem).Key); 
            row.Cells[2].Value = flow;
            row.Cells[3].Value = region;
            row.Cells[4].Value = userName;
            row.Cells[5].Value = printerName;
            row.Cells[6].Value = isAutoPrint;
            dgvSubscribed.Rows.Add(row);
            if (isAutoPrint)
            {
                autoPrintSubList.Add(new SubscriberData { SubscribeType = ((KeyValuePair<string, string>)cmbSubType.SelectedItem).Key, Flow = flow, Region = region, PrinterName = printerName, IsAutoPrint = isAutoPrint, GUID = GUID });
            }
            else
            {
                noAutoPrintSubList.Add(new SubscriberData { SubscribeType = ((KeyValuePair<string, string>)cmbSubType.SelectedItem).Key, Flow = flow, Region = region, PrinterName = printerName, IsAutoPrint = isAutoPrint, GUID = GUID });
            }
            Publishing publishing = new Publishing(printerName,((KeyValuePair<string, string>)cmbSubType.SelectedItem).Key, isAutoPrint);
            ISubscription proxy = CreateProxy(publishing);
            proxy.Subscribe(GUID, subscribeType, flow, region, userName);

            List<SubscriberData> subscriberList = new List<SubscriberData>();
            subscriberList.AddRange(autoPrintSubList);
            subscriberList.AddRange(noAutoPrintSubList);
            XmlSerializer xs = new XmlSerializer(typeof(List<SubscriberData>));

            Stream stream = new FileStream(System.AppDomain.CurrentDomain.BaseDirectory + "SubInfo.xml", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            xs.Serialize(stream, subscriberList);
            stream.Close();

            this.cmbSubType.Text = "";
            this.tbFlow.Text = "";
            this.tbRegion.Text = "";
            this.tbUserName.Text = "";
            this.tbPrinterName.Text = "";
            
        }

        private void dgvSubscribed_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (!dgvSubscribed.CurrentRow.IsNewRow)
            {
                //如果第4个单元格有值则是条码订阅，否则则是其他订阅
                if (this.dgvSubscribed.CurrentRow.Cells[4].Value!=null && !string.IsNullOrEmpty(this.dgvSubscribed.CurrentRow.Cells[4].Value.ToString()))
                {
                    this.labelFlow.Visible = false;
                    this.tbFlow.Visible = false;
                    this.labelRegion.Visible = false;
                    this.tbRegion.Visible = false;
                    this.labelUserName.Visible = true;
                    this.tbUserName.Visible = true;
                }
                else 
                {
                    this.labelFlow.Visible = true;
                    this.tbFlow.Visible = true;
                    this.labelRegion.Visible = true;
                    this.tbRegion.Visible = true;
                    this.labelUserName.Visible = false;
                    this.tbUserName.Visible = false;
                }
 
                this.cmbSubType.Text = this.dgvSubscribed.CurrentRow.Cells[1].Value.ToString();
                this.tbFlow.Text = this.dgvSubscribed.CurrentRow.Cells[2].Value == null ? string.Empty : this.dgvSubscribed.CurrentRow.Cells[2].Value.ToString();
                this.tbRegion.Text = this.dgvSubscribed.CurrentRow.Cells[3].Value == null ? string.Empty : this.dgvSubscribed.CurrentRow.Cells[3].Value.ToString();
                this.tbUserName.Text = this.dgvSubscribed.CurrentRow.Cells[4].Value == null ? string.Empty : this.dgvSubscribed.CurrentRow.Cells[4].Value.ToString();
                this.tbPrinterName.Text = this.dgvSubscribed.CurrentRow.Cells[5].Value == null ? string.Empty : this.dgvSubscribed.CurrentRow.Cells[5].Value.ToString();
                if ((bool)this.dgvSubscribed.CurrentRow.Cells[6].Value)
                {
                    this.ckIsAutoPrint.Checked = true;
                } 
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            //using (OleDbConnection conn = new OleDbConnection(connstr))
            //{
            //    conn.Open();
            //    OleDbCommand cmd = new OleDbCommand("delete from T_Config where Id=" + this.dgvSubscribed.CurrentRow.Cells[0].Value, conn);
            //    int rowCount = cmd.ExecuteNonQuery();
            //}
            //this.dgvSubscribed.Rows.RemoveAt(this.dgvSubscribed.CurrentRow.Index);

            var sub = new SubscriberData();
            if ((bool)this.dgvSubscribed.CurrentRow.Cells[6].Value)
            {
                sub = autoPrintSubList.FirstOrDefault(o => o.SubscribeType == this.dgvSubscribed.CurrentRow.Cells[1].Value.ToString()
                    && o.Flow == this.dgvSubscribed.CurrentRow.Cells[2].Value.ToString()
                    && o.Region == this.dgvSubscribed.CurrentRow.Cells[3].Value.ToString());
                    //&& o.UserName == (this.dgvSubscribed.CurrentRow.Cells[4].Value == null ? string.Empty : this.dgvSubscribed.CurrentRow.Cells[4].Value.ToString()));
                autoPrintSubList.Remove(sub);
            }
            else
            {
                sub = noAutoPrintSubList.FirstOrDefault(o => o.SubscribeType == this.dgvSubscribed.CurrentRow.Cells[1].Value.ToString()
                    && o.Flow == this.dgvSubscribed.CurrentRow.Cells[2].Value.ToString()
                    && o.Region == this.dgvSubscribed.CurrentRow.Cells[3].Value.ToString());
                    //&& o.UserName == (this.dgvSubscribed.CurrentRow.Cells[4].Value == null ? string.Empty : this.dgvSubscribed.CurrentRow.Cells[4].Value.ToString()));
                noAutoPrintSubList.Remove(sub);
            }
            this.dgvSubscribed.Rows.RemoveAt(this.dgvSubscribed.CurrentRow.Index);
            Publishing publishing = new Publishing("HeartBeat", "", false);
            ISubscription proxy = CreateProxy(publishing);
            proxy.UnSubscribe(sub.GUID, sub.SubscribeType, sub.Flow, sub.Region, sub.UserName);

            List<SubscriberData> subscriberList = new List<SubscriberData>();
            subscriberList.AddRange(autoPrintSubList);
            subscriberList.AddRange(noAutoPrintSubList);
            XmlSerializer xs = new XmlSerializer(typeof(List<SubscriberData>));

            Stream stream = new FileStream(System.AppDomain.CurrentDomain.BaseDirectory + "SubInfo.xml", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            xs.Serialize(stream, subscriberList);
            stream.Close();

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            
        }

        void btnChoosePrinter_Click(object sender, EventArgs e)
        {
            PrintDialog printDlg = new PrintDialog();
            
            if (DialogResult.OK == printDlg.ShowDialog())
            {
                tbPrinterName.Text = printDlg.PrinterSettings.PrinterName;
            }
        }

        private void btnRePrint_Click(object sender, EventArgs e)
        {
            if (!dataGridViewPrintedList.CurrentRow.IsNewRow)
            {
                string orderNo = this.dataGridViewPrintedList.CurrentRow.Cells[0].Value.ToString();
                string printerName = this.dataGridViewPrintedList.CurrentRow.Cells[5].Value.ToString();
                //string subscribeType = Enum.Parse(typeof(DocumentsType), ((KeyValuePair<string, string>)cmbSubType.SelectedItem).Key, false);
                string subscribeType = Utility.GetEnumValue(typeof(DocumentsType), this.dataGridViewPrintedList.CurrentRow.Cells[1].Value.ToString());
                try
                {
                    if (subscribeType.Contains("ORD"))
                    {
                        ClientData clientData = printedDataList.Single(p => p.No == orderNo); //printedDataList.Single(p => p.No == orderNo);
                        string printFile = CreatePrintFile(((PrintOrderMaster)clientData.PrintData).OrderTemplate, (PrintOrderMaster)clientData.PrintData, ((PrintOrderMaster)clientData.PrintData).OrderDetails);
                        Utility.PrintOrder(printFile, this.dataGridViewNoAutoPrint.CurrentRow.Cells[4].Value.ToString(), this);
                    }
                    else if (subscribeType.Contains("ASN"))
                    {
                        ClientData clientData = printedDataList.Single(p => p.No == orderNo); 
                        string printFile = CreatePrintFile(((PrintIpMaster)clientData.PrintData).AsnTemplate, (PrintIpMaster)clientData.PrintData, ((PrintIpMaster)clientData.PrintData).IpDetails);
                        Utility.PrintOrder(printFile, this.dataGridViewNoAutoPrint.CurrentRow.Cells[4].Value.ToString(), this);
                    }
                    else if (subscribeType.Contains("PIK"))
                    {
                        ClientData clientData = printedDataList.Single(p => p.No == orderNo);
                        string printFile = CreatePrintFile("PickList.xls", (PrintPickListMaster)clientData.PrintData, ((PrintPickListMaster)clientData.PrintData).PickListDetails);
                        Utility.PrintOrder(printFile, this.dataGridViewNoAutoPrint.CurrentRow.Cells[4].Value.ToString(), this);
                    }
                    else if (subscribeType.Contains("SEQ"))
                    {
                        ClientData clientData = printedDataList.Single(p => p.No == orderNo); 
                        string printFile = CreatePrintFile("Seq.xls", (PrintSequenceMaster)clientData.PrintData, ((PrintSequenceMaster)clientData.PrintData).SequenceDetails);
                        Utility.PrintOrder(printFile, this.dataGridViewNoAutoPrint.CurrentRow.Cells[4].Value.ToString(), this);
                    }
                    else if (subscribeType.Contains("CloneHu"))
                    {
                        ClientData clientData = printedDataList.Single(p => p.No == orderNo);
                        string printFile = CreatePrintFile("BarCode.xls", (PrintHu)clientData.PrintData, ((PrintHu)clientData.PrintData).CreateUserName);
                        Utility.PrintOrder(printFile, this.dataGridViewNoAutoPrint.CurrentRow.Cells[4].Value.ToString(), this);
                    }
                }
                catch (Exception ex)
                {
                    logger = container.Resolve<ILogger>();
                    logger.Error(ex.Message);
                }
            }
        }

        private void btnPrintSelected_Click(object sender, EventArgs e)
        {
            if (!dataGridViewNoAutoPrint.CurrentRow.IsNewRow)
            {
                string orderNo = this.dataGridViewNoAutoPrint.CurrentRow.Cells[0].Value.ToString();
                string printerName = this.dataGridViewNoAutoPrint.CurrentRow.Cells[5].Value.ToString();
                string subscribeType = Utility.GetEnumValue(typeof(DocumentsType), this.dataGridViewPrintedList.CurrentRow.Cells[1].Value.ToString());
                try
                {
                    if (subscribeType.Contains("ORD"))
                    {
                        ClientData clientData = printedDataList.Single(p => p.No == orderNo); //printedDataList.Single(p => p.No == orderNo);
                        string printFile = CreatePrintFile(((PrintOrderMaster)clientData.PrintData).OrderTemplate, (PrintOrderMaster)clientData.PrintData, ((PrintOrderMaster)clientData.PrintData).OrderDetails);
                        Utility.PrintOrder(printFile, this.dataGridViewNoAutoPrint.CurrentRow.Cells[4].Value.ToString(), this);
                    }
                    else if (subscribeType.Contains("ASN"))
                    {
                        ClientData clientData = printedDataList.Single(p => p.No == orderNo);
                        string printFile = CreatePrintFile(((PrintIpMaster)clientData.PrintData).AsnTemplate, (PrintIpMaster)clientData.PrintData, ((PrintIpMaster)clientData.PrintData).IpDetails);
                        Utility.PrintOrder(printFile, this.dataGridViewNoAutoPrint.CurrentRow.Cells[4].Value.ToString(), this);
                    }
                    else if (subscribeType.Contains("PIK"))
                    {
                        ClientData clientData = printedDataList.Single(p => p.No == orderNo);
                        string printFile = CreatePrintFile("PickList.xls", (PrintPickListMaster)clientData.PrintData, ((PrintPickListMaster)clientData.PrintData).PickListDetails);
                        Utility.PrintOrder(printFile, this.dataGridViewNoAutoPrint.CurrentRow.Cells[4].Value.ToString(), this);
                    }
                    else if (subscribeType.Contains("SEQ"))
                    {
                        ClientData clientData = printedDataList.Single(p => p.No == orderNo);
                        string printFile = CreatePrintFile("Seq.xls", (PrintSequenceMaster)clientData.PrintData, ((PrintSequenceMaster)clientData.PrintData).SequenceDetails);
                        Utility.PrintOrder(printFile, this.dataGridViewNoAutoPrint.CurrentRow.Cells[4].Value.ToString(), this);
                    }
                    else if (subscribeType.Contains("CloneHu"))
                    {
                        ClientData clientData = printedDataList.Single(p => p.No == orderNo);
                        string printFile = CreatePrintFile("BarCode.xls", (PrintHu)clientData.PrintData, ((PrintHu)clientData.PrintData).CreateUserName);
                        Utility.PrintOrder(printFile, this.dataGridViewNoAutoPrint.CurrentRow.Cells[4].Value.ToString(), this);
                    }
                }
                catch (Exception ex)
                {
                    logger = container.Resolve<ILogger>();
                    logger.Error(ex.Message);
                }
            }
        }
        #endregion

        #region 打印显示数据
        private void timerPrint_Tick(object sender, EventArgs e)
        {
            if (unPrintedDataList.Count > 0)
            {
                string printFile = "";
                ClientData clientData = unPrintedDataList.OrderBy(u => u.PublishDateTime).First();
                RepPurchaseOrderOperator rpoo = new RepPurchaseOrderOperator();
                IList<object> data = new List<object>();

                foreach (var item in unPrintedDataList)
                {
                    if (clientData.PrintData.GetType() == typeof(PrintOrderMaster))
                    {
                        printFile = CreatePrintFile(((PrintOrderMaster)clientData.PrintData).OrderTemplate, (PrintOrderMaster)clientData.PrintData, ((PrintOrderMaster)clientData.PrintData).OrderDetails);
                    }
                    else if (clientData.PrintData.GetType() == typeof(PrintIpMaster))
                    {
                        printFile = CreatePrintFile(((PrintIpMaster)clientData.PrintData).AsnTemplate, (PrintIpMaster)clientData.PrintData, ((PrintIpMaster)clientData.PrintData).IpDetails);
                    }
                    else if (clientData.PrintData.GetType() == typeof(PrintPickListMaster))
                    {
                        printFile = CreatePrintFile("PickList.xls", (PrintPickListMaster)clientData.PrintData, ((PrintPickListMaster)clientData.PrintData).PickListDetails);
                    }
                    else if (clientData.PrintData.GetType() == typeof(PrintSequenceMaster))
                    {
                        printFile = CreatePrintFile("Seq.xls", (PrintSequenceMaster)clientData.PrintData, ((PrintSequenceMaster)clientData.PrintData).SequenceDetails);
                    }
                    else if (clientData.PrintData.GetType() == typeof(PrintReceiptMaster))
                    {
                        printFile = CreatePrintFile(((PrintReceiptMaster)clientData.PrintData).ReceiptTemplate, (PrintReceiptMaster)clientData.PrintData, ((PrintReceiptMaster)clientData.PrintData).ReceiptDetails);
                    }
                    else if (clientData.PrintData.GetType() == typeof(PrintHu))
                    {
                        printFile = CreatePrintFile("BarCode.xls", (PrintHu)clientData.PrintData, ((PrintHu)clientData.PrintData).CreateUserName);
                    }
                    lock (printedDataList)
                    {
                        //clientData.PrintData = null;
                        printedDataList.Add(clientData);
                    }

                    try
                    {
                        string printerName = item.PrinterName;
                        Utility.PrintOrder(printFile, printerName, this);
                        item.isPrinted = true;
                    }
                    catch (Exception ex)
                    {
                        logger = container.Resolve<ILogger>();
                        logger.Error(ex.Message);
                    }
                }

                lock (unPrintedDataList)
                {
                    unPrintedDataList = unPrintedDataList.Where(p => p.isPrinted == false).ToList();
                }
            }
        }

        private void timerShowData_Tick(object sender, EventArgs e)
        {
            if (printedDataList.Count > 500)
            {
                printedDataList = printedDataList.OrderByDescending(p => p.PublishDateTime).ToList();
                printedDataList = printedDataList.Take(250).ToList();
            }

            #region 绑定未自动打印列表
            IList<ClientData> noShowList = noAutoPrintDataList.Where(i => i.isPrinted == false && i.hasShowed == false).ToList();
            foreach (var item in noShowList)
            {
                DataGridViewRow row = (DataGridViewRow)dataGridViewNoAutoPrint.Rows[0].Clone();
                row.Cells[0].Value = item.No;
                row.Cells[1].Value = Utility.GetEnumDescription(typeof(DocumentsType), item.SubscribeType); //(DocumentsType)Enum.Parse(typeof(DocumentsType), item.SubscribeType.ToString(), false);
                row.Cells[2].Value = item.PartFrom;
                row.Cells[3].Value = item.PartTo;
                row.Cells[4].Value = item.UserName;
                row.Cells[5].Value = item.PrinterName;
                dataGridViewNoAutoPrint.Rows.Add(row);
                item.hasShowed = true;
            }
            #endregion

            #region 绑定打印监控
            dataGridViewUnPrinted.Rows.Clear();
            foreach (var item in unPrintedDataList)
            {
                DataGridViewRow row = (DataGridViewRow)dataGridViewUnPrinted.Rows[0].Clone();
                row.Cells[0].Value = item.No;
                row.Cells[1].Value = Utility.GetEnumDescription(typeof(DocumentsType), item.SubscribeType);//(DocumentsType)Enum.Parse(typeof(DocumentsType), item.SubscribeType.ToString(), false);
                row.Cells[2].Value = item.PartFrom;
                row.Cells[3].Value = item.PartTo;
                row.Cells[4].Value = item.UserName;
                row.Cells[5].Value = item.PrinterName;
                dataGridViewUnPrinted.Rows.Add(row);
            }

            IList<ClientData> unShowPrintedList = printedDataList.Where(p => p.hasShowed == false).ToList();
            //dataGridViewPrinted.Rows.Clear();
            foreach (var item in printedDataList)
            {
                if (!item.hasShowed)
                {
                    DataGridViewRow row = (DataGridViewRow)dataGridViewPrintedList.Rows[0].Clone();
                    row.Cells[0].Value = item.No;
                    row.Cells[1].Value = Utility.GetEnumDescription(typeof(DocumentsType), item.SubscribeType);//(DocumentsType)Enum.Parse(typeof(DocumentsType), item.SubscribeType.ToString(), false);
                    row.Cells[2].Value = item.PartFrom;
                    row.Cells[3].Value = item.PartTo;
                    row.Cells[4].Value = item.UserName;
                    row.Cells[5].Value = item.PrinterName;
                    dataGridViewPrintedList.Rows.Add(row);
                    item.hasShowed = true;
                }
            }
            #endregion
        }
        #endregion

        #region 订阅回调类
        public class Publishing : IPublishing
        {
            private string _printerName;
            private string _subscriberType;
            private bool _isAutoPrint;

            public Publishing(string printerName, string subscriberType, bool isAutoPrint)
            {
                _printerName = printerName;
                _isAutoPrint = isAutoPrint;
                _subscriberType = subscriberType;
            }

            public void Publish(PrintBase o)
            {
                if (o.GetType() == typeof(PrintBase))
                {
                    return;
                }
                if (o.GetType() == typeof(PrintOrderMaster))
                {
                    ClientData clientData;
                    PrintOrderMaster orderMaster = (PrintOrderMaster)o;
                    if (_isAutoPrint)
                    {
                        clientData = new ClientData{
                            No = orderMaster.OrderNo,
                            SubscribeType = _subscriberType,
                            PartFrom = orderMaster.PartyFrom,
                            PartTo = orderMaster.PartyTo,
                            PrinterName = _printerName,
                            hasShowed = false,
                            isPrinted = false,
                            PublishDateTime = DateTime.Now,
                            PrintData = orderMaster
                        };
                        lock (unPrintedDataList)
                        {
                            unPrintedDataList.Add(clientData);
                        }
                    }
                    else
                    {
                        clientData = new ClientData
                        {
                            No = orderMaster.OrderNo,
                            SubscribeType = _subscriberType,
                            PartFrom = orderMaster.PartyFrom,
                            PartTo = orderMaster.PartyTo,
                            PrinterName = _printerName,
                            hasShowed = false,
                            isPrinted = false,
                            PublishDateTime = DateTime.Now,
                            PrintData = orderMaster
                        };
                        lock (noAutoPrintDataList)
                        {
                            noAutoPrintDataList.Add(clientData);
                        }
                    }
                }
                else if(o.GetType() == typeof(PrintReceiptMaster))
                {
                    ClientData clientData;
                    PrintReceiptMaster receiptMaster = (PrintReceiptMaster)o;
                    if (_isAutoPrint)
                    {
                        clientData = new ClientData{
                            No = receiptMaster.ReceiptNo,
                            SubscribeType = _subscriberType,
                            PartFrom = receiptMaster.PartyFrom,
                            PartTo = receiptMaster.PartyTo,
                            PrinterName = _printerName,
                            hasShowed = false,
                            isPrinted = false,
                            PublishDateTime = DateTime.Now,
                            PrintData = receiptMaster
                        };
                        lock (unPrintedDataList)
                        {
                            unPrintedDataList.Add(clientData);
                        }
                    }
                    else
                    {
                        clientData = new ClientData
                        {
                            No = receiptMaster.ReceiptNo,
                            SubscribeType = _subscriberType,
                            PartFrom = receiptMaster.PartyFrom,
                            PartTo = receiptMaster.PartyTo,
                            PrinterName = _printerName,
                            hasShowed = false,
                            isPrinted = false,
                            PublishDateTime = DateTime.Now,
                            PrintData = receiptMaster
                        };
                        lock (noAutoPrintDataList)
                        {
                            noAutoPrintDataList.Add(clientData);
                        }
                    }
                }
                else if (o.GetType() == typeof(PrintIpMaster))
                {
                    ClientData clientData;
                    PrintIpMaster ipMaster = (PrintIpMaster)o;
                    if (_isAutoPrint)
                    {
                        clientData = new ClientData
                        {
                            No = ipMaster.IpNo,
                            SubscribeType = _subscriberType,
                            PartFrom = ipMaster.PartyFrom,
                            PartTo = ipMaster.PartyTo,
                            PrinterName = _printerName,
                            hasShowed = false,
                            isPrinted = false,
                            PublishDateTime = DateTime.Now,
                            PrintData = ipMaster
                        };
                        lock (unPrintedDataList)
                        {
                            unPrintedDataList.Add(clientData);
                        }
                    }
                    else
                    {
                        clientData = new ClientData
                        {
                            No = ipMaster.IpNo,
                            SubscribeType = _subscriberType,
                            PartFrom = ipMaster.PartyFrom,
                            PartTo = ipMaster.PartyTo,
                            PrinterName = _printerName,
                            hasShowed = false,
                            isPrinted = false,
                            PublishDateTime = DateTime.Now,
                            PrintData = ipMaster
                        };
                        lock (noAutoPrintDataList)
                        {
                            noAutoPrintDataList.Add(clientData);
                        }
                    }
                }
                else if (o.GetType() == typeof(PrintPickListMaster))
                {
                    ClientData clientData;
                    PrintPickListMaster pickListMaster = (PrintPickListMaster)o;
                    if (_isAutoPrint)
                    {
                        clientData = new ClientData
                        {
                            No = pickListMaster.PickListNo,
                            SubscribeType = _subscriberType,
                            PartFrom = pickListMaster.PartyFrom,
                            PartTo = pickListMaster.PartyTo,
                            PrinterName = _printerName,
                            hasShowed = false,
                            isPrinted = false,
                            PublishDateTime = DateTime.Now,
                            PrintData = pickListMaster
                        };
                        lock (unPrintedDataList)
                        {
                            unPrintedDataList.Add(clientData);
                        }
                    }
                    else
                    {
                        clientData = new ClientData
                        {
                            No = pickListMaster.PickListNo,
                            SubscribeType = _subscriberType,
                            PartFrom = pickListMaster.PartyFrom,
                            PartTo = pickListMaster.PartyTo,
                            PrinterName = _printerName,
                            hasShowed = false,
                            isPrinted = false,
                            PublishDateTime = DateTime.Now,
                            PrintData = pickListMaster
                        };
                        lock (noAutoPrintDataList)
                        {
                            noAutoPrintDataList.Add(clientData);
                        }
                    }
                }
                else if (o.GetType() == typeof(PrintSequenceMaster))
                {
                    ClientData clientData;
                    PrintSequenceMaster sequenceMaster = (PrintSequenceMaster)o;
                    if (_isAutoPrint)
                    {
                        clientData = new ClientData
                        {
                            No = sequenceMaster.SequenceNo,
                            SubscribeType = _subscriberType,
                            PartFrom = sequenceMaster.PartyFrom,
                            PartTo = sequenceMaster.PartyTo,
                            PrinterName = _printerName,
                            hasShowed = false,
                            isPrinted = false,
                            PublishDateTime = DateTime.Now,
                            PrintData = sequenceMaster
                        };
                        lock (unPrintedDataList)
                        {
                            unPrintedDataList.Add(clientData);
                        }
                    }
                    else
                    {
                        clientData = new ClientData
                        {
                            No = sequenceMaster.SequenceNo,
                            SubscribeType = _subscriberType,
                            PartFrom = sequenceMaster.PartyFrom,
                            PartTo = sequenceMaster.PartyTo,
                            PrinterName = _printerName,
                            hasShowed = false,
                            isPrinted = false,
                            PublishDateTime = DateTime.Now,
                            PrintData = sequenceMaster
                        };
                        lock (noAutoPrintDataList)
                        {
                            noAutoPrintDataList.Add(clientData);
                        }
                    }
                }
                else if (o.GetType() == typeof(PrintHu))
                {
                    ClientData clientData;
                    PrintHu hu = (PrintHu)o;
                    if (_isAutoPrint)
                    {
                        clientData = new ClientData
                        {
                            No = hu.HuId,
                            SubscribeType = _subscriberType,
                            UserName = hu.CreateUserName,
                            PrinterName = _printerName,
                            hasShowed = false,
                            isPrinted = false,
                            PublishDateTime = DateTime.Now,
                            PrintData = hu
                        };
                        lock (unPrintedDataList)
                        {
                            unPrintedDataList.Add(clientData);
                        }
                    }
                    else
                    {
                        clientData = new ClientData
                        {
                            No = hu.HuId,
                            SubscribeType = _subscriberType,
                            UserName = hu.CreateUserName,
                            PrinterName = _printerName,
                            hasShowed = false,
                            isPrinted = false,
                            PublishDateTime = DateTime.Now,
                            PrintData = hu
                        };
                        lock (noAutoPrintDataList)
                        {
                            noAutoPrintDataList.Add(clientData);
                        }
                    }
                }
            }
        }
        #endregion

        #region 内部调用函数
        private ISubscription CreateProxy(object callbackinstance)
        {
            string EndpoindAddress = ConfigurationManager.AppSettings["EndpointAddress"];
            NetTcpBinding netTcpbinding = new NetTcpBinding(SecurityMode.None);
            netTcpbinding.ReceiveTimeout = TimeSpan.FromHours(24);
            EndpointAddress endpointAddress = new EndpointAddress(EndpoindAddress);
            InstanceContext context = new InstanceContext(callbackinstance);

            DuplexChannelFactory<ISubscription> channelFactory = new DuplexChannelFactory<ISubscription>(context, netTcpbinding, endpointAddress);
            return channelFactory.CreateChannel();
        }

        private string CreatePrintFile(string templateFileName, object master, object details)
        {
            IList<object> data = new List<object>();
            data.Add(master);
            data.Add(details);
            reportGen = container.Resolve<IReportGen>("ReportGen.Util");
            string templatefolder = System.AppDomain.CurrentDomain.BaseDirectory+"\\Template\\";
            reportGen.SetTemplateFolder(templatefolder);
            return reportGen.WriteToFile(templateFileName, data);
        }

        private void GetUser(string userName, string password)
        {
            this.userName = userName;
            this.password = password;
        }
        #endregion

    }
}
