using System;
using System.Windows.Forms;
using System.Drawing;
using com.Sconit.SmartDevice.SmartDeviceRef;

namespace com.Sconit.SmartDevice
{
    public partial class MainForm : Form
    {
        public delegate void ModuleSelectHandler(CodeMaster.TerminalPermission module);
        public delegate void LoginHandler(string userCode, string password);
        public delegate void ExitHandler();
        public delegate void ModuleSelectExitHandler();

        private UCLogin ucLogin;
        private UCModuleSelect ucModuleSelect;
        //private UCShip ucShip;
        private UCReceive ucReceive;
        private UCHuStatus ucHuStatus;
        private UCTransfer ucTransfer;
        private UCPickList ucPickList;
        private UCPickUp ucPickUp;
        private UCPutAway ucPutAway;
        private UCReceiveAnJiHu ucReceiveAnJiHu;
        //private UCRecAnJiSeqOrder ucRecAnJiSeqOrder;

        private UCPack ucPack;
        private UCRePack ucRePack;
        private UCUnPack ucUnPack;
        private UCInspect ucInspect;
        private UCJudgeInspect ucJudgeInspect;
        //private UCReject ucReject;

        //private UCStockTaking ucStockTaking;
        private UCPickListOnline ucPickListOnline;
        private UCCabTransfer ucCabTransfer;
        //private UCReuse ucReuse;

        //生产
        private UCMaterialIn ucMaterialIn;
        private UCAnDon ucAnDon;
        private UCChassisOnline ucChassisOnline;
        private AssemblyOffline ucAssemblyOffline;

        private User user;
        //private UserPreference userPreference;

        SmartDeviceService smartDeviceService;

        public MainForm()
        {
            InitializeComponent();
            LoadUCLogin();
            smartDeviceService = new SmartDeviceService();
        }

        private void LoadUCLogin()
        {
            try
            {
                if (this.plMain.Controls.Count > 0)
                {
                    this.plMain.Controls.RemoveAt(0);
                }
                this.ucLogin = new UCLogin();
                //
                this.ucLogin.LoginEvent += new LoginHandler(this.ProcessLoginEvent);
                this.ucLogin.ExitEvent += new ExitHandler(this.ProcessExitEvent);

                this.plMain.Controls.Add(this.ucLogin);

            }
            catch (Exception ex)
            {
                Utility.ShowMessageBox("网络故障!" + ex.Message);
                Application.Exit();
            }
        }

        private void ProcessLoginEvent(string userCode, string password)
        {
            try
            {
                User user = smartDeviceService.GetUser(userCode);
                if (user != null && user.Password.ToLower() == Utility.Md5(password))
                {
                    this.user = user;
                    this.SwitchModule(CodeMaster.TerminalPermission.M_Switch);
                }
                else
                {
                    Utility.ShowMessageBox("登陆失败");
                    this.ucLogin.InitialLogin();
                    this.user = null;
                }
            }
            catch (Exception ex)
            {
                Utility.ShowMessageBox(ex.Message);
                this.ucLogin.InitialLogin();
                this.user = null;
            }
        }

        private void ProcessExitEvent()
        {
            this.Dispose(true);
        }

        private void SwitchModule(CodeMaster.TerminalPermission module)
        {
            //Rectangle rect = Screen.PrimaryScreen.WorkingArea;
            //int width = rect.Width;
            //int height = this.Height;

            if (module == CodeMaster.TerminalPermission.M_Switch)
            {
                if (this.user != null)
                {
                    this.ucModuleSelect = new UCModuleSelect(this.user);
                    this.ucModuleSelect.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                    this.ucModuleSelect.ModuleSelectExitEvent += new ModuleSelectExitHandler(this.LoadUCLogin);
                    this.AddModule(this.ucModuleSelect);
                    this.Text = "模块选择_Sconit_SD";
                }
                else
                {
                    this.ucModuleSelect.ModuleSelectExitEvent += new ModuleSelectExitHandler(this.LoadUCLogin);
                    this.LoadUCLogin();
                }
            }
            else if (module == CodeMaster.TerminalPermission.Client_OrderShip)
            {
                UCShip ucShip = UCShip.GetUCShip(user);
                ucShip.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucShip);
                ucShip.tbBarCode.Focus();
                this.Text = "发货";
            }
            else if (module == CodeMaster.TerminalPermission.Client_Receive)
            {
                UCReceive ucReceive = UCReceive.GetUCReceive(this.user);
                ucReceive.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucReceive);
                ucReceive.tbBarCode.Focus();
                this.Text = "供应商收货";
            }
            else if (module == CodeMaster.TerminalPermission.Client_Transfer)
            {
                UCTransfer ucTransfer = UCTransfer.GetUCTransfer(this.user);
                ucTransfer.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucTransfer);
                ucTransfer.tbBarCode.Focus();
                this.Text = "移库";
            }
            else if (module == CodeMaster.TerminalPermission.Client_PickList)
            {
                UCPick ucPickList = new UCPick(this.user);
                ucPickList.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucPickList);
                ucPickList.tbBarCode.Focus();
                this.Text = "拣货";
            }
            else if (module == CodeMaster.TerminalPermission.Client_QuickPick)
            {
                UCQuickPick ucQuickPick = new UCQuickPick(this.user);
                ucQuickPick.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucQuickPick);
                ucQuickPick.tbBarCode.Focus();
                this.Text = "快速拣货";
            }
            else if (module == CodeMaster.TerminalPermission.Client_PickListShip)
            {
                UCPickShip UCPickListShip = new UCPickShip(this.user);
                UCPickListShip.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(UCPickListShip);
                //UCPickListShip.tbBarCode.Focus();
                this.Text = "拣货发货";
            }
            else if (module == CodeMaster.TerminalPermission.Client_PutAway)
            {
                UCPutAway ucPutAway = UCPutAway.GetUCPutAway(this.user);
                ucPutAway.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucPutAway);
                ucPutAway.tbBarCode.Focus();
                this.Text = "上架";
            }
            else if (module == CodeMaster.TerminalPermission.Client_Pickup)
            {
                var ucPickUp = UCPickUp.GetUCPickUp(this.user);
                ucPickUp.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucPickUp);
                this.Text = "下架";
            }
            else if (module == CodeMaster.TerminalPermission.Client_ReceiveSQ)
            {
                var ucSQReceive = UCSQReceive.GetUCSQReceive(this.user);
                ucSQReceive.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucSQReceive);
                this.Text = "双桥条码收货";
            }
            else if (module == CodeMaster.TerminalPermission.Client_AnDon)
            {
                UCAnDon ucAnDon = UCAnDon.GetUCAnDon(this.user);
                ucAnDon.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                AddModule(ucAnDon);
                ucAnDon.tbBarCode.Focus();
                this.Text = "按灯";
                //this.ucDevanning.Height = height;
            }
            else if (module == CodeMaster.TerminalPermission.Client_StockTaking)
            {
                UCStockTaking ucStockTaking = UCStockTaking.GetUCStockTaking(this.user);
                ucStockTaking.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucStockTaking);
                ucStockTaking.tbBarCode.Focus();
                this.Text = "盘点";
            }

            else if (module == CodeMaster.TerminalPermission.Client_MaterialIn)
            {
                UCMaterialIn ucMaterialIn = UCMaterialIn.GetUCMaterialIn(this.user);
                ucMaterialIn.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucMaterialIn);
                ucMaterialIn.tbBarCode.Focus();
                this.Text = "关键件追溯";
            }
            else if (module == CodeMaster.TerminalPermission.Client_ForceMaterialIn)
            {
                var ucForceMaterialIn = UCForceMaterialIn.UCForceMaterialIns(this.user);
                ucForceMaterialIn.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucForceMaterialIn);
                ucForceMaterialIn.tbBarCode.Focus();
                this.Text = "车架追溯";
            }
            else if (module == CodeMaster.TerminalPermission.Client_ForceScanMaterialIn)
            {
                var ucScanForceMaterialIn = UCScanForceMaterialIn.UCScanForceMaterialIns(this.user);
                ucScanForceMaterialIn.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucScanForceMaterialIn);
                ucScanForceMaterialIn.tbBarCode.Focus();
                this.Text = "强制扫描";
            }
            else if (module == CodeMaster.TerminalPermission.Client_Qualify)
            {
                var ucJudgeInspect = new UCJudgeInspect(this.user, JudgeResult.Qualified);
                ucJudgeInspect.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucJudgeInspect);
                ucJudgeInspect.tbBarCode.Focus();
                this.Text = "合格";
            }
            else if (module == CodeMaster.TerminalPermission.Client_Reject)
            {
                var ucJudgeInspect = new UCJudgeInspect(this.user, JudgeResult.Rejected);
                this.ucJudgeInspect.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucJudgeInspect);
                ucJudgeInspect.tbBarCode.Focus();
                this.Text = "不合格";
            }
            else if (module == CodeMaster.TerminalPermission.Client_RePack)
            {
                var ucRePack = new UCRePack(this.user);
                ucRePack.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucRePack);
                ucRePack.tbBarCode.Focus();
                this.Text = "翻箱";
            }
            else if (module == CodeMaster.TerminalPermission.Client_UnPack)
            {
                var ucUnPack = new UCUnPack(this.user);
                ucUnPack.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucUnPack);
                ucUnPack.tbBarCode.Focus();
                this.Text = "拆箱";
            }
            else if (module == CodeMaster.TerminalPermission.Client_Pack)
            {
                var ucPack = new UCPack(this.user);
                ucPack.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucPack);
                ucPack.tbBarCode.Focus();
                this.Text = "装箱";
            }
            else if (module == CodeMaster.TerminalPermission.Client_Inspect)
            {
                var ucInspect = new UCInspect(this.user);
                ucInspect.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucInspect);
                ucInspect.tbBarCode.Focus();
                this.Text = "报验";
            }
            else if (module == CodeMaster.TerminalPermission.Client_WorkerWaste)
            {
                var ucWorkerWaste = new UCWorkerWaste(this.user);
                ucWorkerWaste.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucWorkerWaste);
                ucWorkerWaste.tbBarCode.Focus();
                this.Text = "工废";
            }
            else if (module == CodeMaster.TerminalPermission.Client_PickListOnline)
            {
                var ucPickListOnline = new UCPickListOnline(this.user);
                ucPickListOnline.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucPickListOnline);
                ucPickListOnline.tbBarCode.Focus();
                this.Text = "拣货单上线";
            }
            else if (module == CodeMaster.TerminalPermission.Client_HuStatus)
            {
                var ucHuStatus = new UCHuStatus(this.user);
                ucHuStatus.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucHuStatus);
                ucHuStatus.tbBarCode.Focus();
                this.Text = "条码状态";
            }
            else if (module == CodeMaster.TerminalPermission.Client_CabOnline)
            {
                UCCabOnline ucCabOnline = UCCabOnline.GetUCCabOnline(this.user);
                ucCabOnline.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucCabOnline);
                ucCabOnline.tbBarCode.Focus();
                this.Text = "驾驶室上线";
            }
            else if (module == CodeMaster.TerminalPermission.Client_ProductionOnline)
            {
                UCProductOrderOnline ucProductOrderOnline = UCProductOrderOnline.GetUCProductOrderOnline(this.user);
                ucProductOrderOnline.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucProductOrderOnline);
                ucProductOrderOnline.tbBarCode.Focus();
                this.Text = "上线";
            }
            else if (module == CodeMaster.TerminalPermission.Client_VanOrderReceive)
            {
                AssemblyOffline ucAssemblyOffline = AssemblyOffline.GetAssemblyOffline(user);
                ucAssemblyOffline.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucAssemblyOffline);
                ucAssemblyOffline.tbBarCode.Focus();
                this.Text = "整车入库";
            }
            else if (module == CodeMaster.TerminalPermission.Client_ReceiveAnJiHu)
            {
                UCReceiveAnJiHu ucReceiveAnJiHu = UCReceiveAnJiHu.GetUCReceiveAnJiHu(user);
                ucReceiveAnJiHu.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucReceiveAnJiHu);
                ucReceiveAnJiHu.tbBarCode.Focus();
                this.Text = "安吉条码收货";
            }
            else if (module == CodeMaster.TerminalPermission.Client_ReceiveAnJiSeq)
            {
                UCRecAnJiSeqOrder ucRecAnJiSeqOrder = UCRecAnJiSeqOrder.GetUCRecAnJiSeqOrder(user);
                ucRecAnJiSeqOrder.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucRecAnJiSeqOrder);
                ucRecAnJiSeqOrder.tbBarCode.Focus();
                this.Text = "安吉出库单收货";
            }
            else if (module == CodeMaster.TerminalPermission.Client_SeqPack)
            {
                //UCSeqPack ucSeqPack = UCSeqPack.GetUCSeqPack(this.user);
                //ucSeqPack.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                //this.AddModule(ucSeqPack);
                //ucSeqPack.tbBarCode.Focus();
                //this.Text = "排序装箱";
            }
            else if (module == CodeMaster.TerminalPermission.Client_MiscInOut)
            {
                UCMisInOut ucMisInOut = UCMisInOut.GetUCMisInOut(this.user);
                ucMisInOut.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucMisInOut);
                ucMisInOut.tbBarCode.Focus();
                this.Text = "计划外出入库";
            }
            else if (module == CodeMaster.TerminalPermission.Client_HuClone)
            {
                UCHuClone ucHuClone = UCHuClone.GetUCHuClone(this.user);
                ucHuClone.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucHuClone);
                ucHuClone.tbBarCode.Focus();
                this.Text = "条码克隆";
            }
            else if (module == CodeMaster.TerminalPermission.Client_MaterialReturn)
            {
                UCMaterialReturn ucMaterialReturn = new UCMaterialReturn(this.user);
                ucMaterialReturn.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucMaterialReturn);
                ucMaterialReturn.tbBarCode.Focus();
                this.Text = "退料";
            }
            else if (module == CodeMaster.TerminalPermission.Client_SeqCancel)
            {
                //UCSeqCancel ucSeqCancel = UCSeqCancel.GetUCSeqCancel(this.user);
                //ucSeqCancel.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                //this.AddModule(ucSeqCancel);
                //ucSeqCancel.tbBarCode.Focus();
                //this.Text = "排序装箱取消";
            }
            else if (module == CodeMaster.TerminalPermission.Client_SeqShip)
            {
                //UCSeqShip ucSeqShip = UCSeqShip.GetUCSeqShip(this.user);
                //ucSeqShip.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                //this.AddModule(ucSeqShip);
                //ucSeqShip.tbBarCode.Focus();
                //this.Text = "排序装箱发货";
            }
            else if (module == CodeMaster.TerminalPermission.Client_QuickSeqShip)
            {
                //UCQuickSeqShip ucQuickSeqShip = UCQuickSeqShip.GetUCQuickSeqShip(this.user);
                //ucQuickSeqShip.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                //this.AddModule(ucQuickSeqShip);
                //ucQuickSeqShip.tbBarCode.Focus();
                //this.Text = "排序单快速发货";
            }
            else if (module == CodeMaster.TerminalPermission.Client_SubAssemblyOffLine)
            {
                UCSubAssemblyOffLine ucSubAssemblyOffLine = UCSubAssemblyOffLine.GetUCSubAssemblyOffLine(this.user);
                ucSubAssemblyOffLine.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucSubAssemblyOffLine);
                ucSubAssemblyOffLine.tbBarCode.Focus();
                this.Text = "分装生产单下线";
            }
            else if (module == CodeMaster.TerminalPermission.Client_Freeze)
            {
                UCFreeze ucFreeze = UCFreeze.GetUCFreeze(this.user);
                ucFreeze.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucFreeze);
                ucFreeze.tbBarCode.Focus();
                this.Text = "库存冻结";
            }
            else if (module == CodeMaster.TerminalPermission.Client_UnFreeze)
            {
                UCUnFreeze ucUnFreeze = UCUnFreeze.GetUCUnFreeze(this.user);
                ucUnFreeze.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucUnFreeze);
                ucUnFreeze.tbBarCode.Focus();
                this.Text = "库存冻结";
            }
            else if (module == CodeMaster.TerminalPermission.Client_TransKeyScan)
            {
                UCTransKeyScan ucTransKeyScan = new UCTransKeyScan(this.user);
                ucTransKeyScan.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucTransKeyScan);
                ucTransKeyScan.tbBarCode.Focus();
                this.Text = "变速器关键件扫描";
            }
            else if (module == CodeMaster.TerminalPermission.Client_ForceRecive)
            {
                UCForceReceive ucForceReceive = new UCForceReceive(this.user);
                ucForceReceive.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucForceReceive);
                ucForceReceive.tbBarCode.Focus();
                this.Text = "强制收货";
            }
            else if (module == CodeMaster.TerminalPermission.Client_QuickReturn)
            {
                UCQuickReturn ucQuickReturn = new UCQuickReturn(this.user);
                ucQuickReturn.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucQuickReturn);
                ucQuickReturn.tbBarCode.Focus();
                this.Text = "快速退库";
            }
            else if (module == CodeMaster.TerminalPermission.Client_CabTransfer)
            {
                UCCabTransfer ucCabTransfer = UCCabTransfer.GetUCCabTransfer(user);
                ucCabTransfer.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucCabTransfer);
                ucCabTransfer.tbBarCode.Focus();
                this.Text = "驾驶室移库";
            }
            else if (module == CodeMaster.TerminalPermission.Client_LotNoScan)
            {
                UCLotNoScan ucLotNoScan = UCLotNoScan.GetUCLotNoScan(this.user);
                ucLotNoScan.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
                this.AddModule(ucLotNoScan);
                ucLotNoScan.tbBarCode.Focus();
                this.Text = "批号管理";
            }
           
            //else if (moduleType == BusinessConstants.ModuleType.Reuse)
            //{
            //    //this.ucReuse = new UCReuse(this.user, moduleType);
            //    this.ucReuse.ModuleSelectionEvent += new ModuleSelectHandler(this.SwitchModule);
            //    this.SwitchModule(this.ucReuse);
            //    this.Text = "材料回用";
            //    this.ucReuse.InitialAll();
            //    this.ucReuse.Width = width;
            //    //this.ucHuStatus.Height = height;
            //}
        }

        private void AddModule(UserControl userControl)
        {
            userControl.Location = new System.Drawing.Point(0, 0);
            userControl.Size = new System.Drawing.Size(238, 320);
            this.plMain.Controls.RemoveAt(0);
            this.plMain.Controls.Add(userControl);
            this.Activate();
            userControl.Focus();
        }
    }

}