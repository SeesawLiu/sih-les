namespace PrintClient
{
    partial class PrintClient
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.lblMessage = new System.Windows.Forms.Label();
            this.dgvSubscribed = new System.Windows.Forms.DataGridView();
            this.dgvtbcId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtbcSubType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtbcFlow = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtbcRegion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtbcUserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtbcPrinterName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtbcIsAutoPrint = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gpb_SubCondition = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblRegion = new System.Windows.Forms.Label();
            this.lblFlow = new System.Windows.Forms.Label();
            this.lblSubType = new System.Windows.Forms.Label();
            this.btnDelete = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lblWarning1 = new System.Windows.Forms.Label();
            this.btnPrintSelected = new System.Windows.Forms.Button();
            this.lblNoAutoPrint = new System.Windows.Forms.Label();
            this.lblWaitforPrint = new System.Windows.Forms.Label();
            this.dataGridViewUnPrinted = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewNoAutoPrint = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.dataGridViewPrintedList = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn14 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn15 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn16 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn17 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnRePrint = new System.Windows.Forms.Button();
            this.timerPrint = new System.Windows.Forms.Timer(this.components);
            this.timerShowData = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.cmbSubType = new System.Windows.Forms.ComboBox();
            this.tbFlow = new System.Windows.Forms.TextBox();
            this.tbRegion = new System.Windows.Forms.TextBox();
            this.tbPrinterName = new System.Windows.Forms.TextBox();
            this.btnChoosePrinter = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.ckIsAutoPrint = new System.Windows.Forms.CheckBox();
            this.labelSubsType = new System.Windows.Forms.Label();
            this.labelFlow = new System.Windows.Forms.Label();
            this.labelRegion = new System.Windows.Forms.Label();
            this.labelPrinter = new System.Windows.Forms.Label();
            this.tbUserName = new System.Windows.Forms.TextBox();
            this.labelUserName = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSubscribed)).BeginInit();
            this.gpb_SubCondition.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUnPrinted)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewNoAutoPrint)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPrintedList)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(2, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(611, 516);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lblMessage);
            this.tabPage1.Controls.Add(this.dgvSubscribed);
            this.tabPage1.Controls.Add(this.gpb_SubCondition);
            this.tabPage1.Controls.Add(this.btnDelete);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(603, 490);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "订阅打印";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point(499, 462);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(101, 12);
            this.lblMessage.TabIndex = 20;
            this.lblMessage.Text = "双击编辑订阅打印";
            // 
            // dgvSubscribed
            // 
            this.dgvSubscribed.AllowUserToDeleteRows = false;
            this.dgvSubscribed.AllowUserToResizeRows = false;
            this.dgvSubscribed.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            this.dgvSubscribed.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSubscribed.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgvtbcId,
            this.dgvtbcSubType,
            this.dgvtbcFlow,
            this.dgvtbcRegion,
            this.dgvtbcUserName,
            this.dgvtbcPrinterName,
            this.dgvtbcIsAutoPrint});
            this.dgvSubscribed.Location = new System.Drawing.Point(2, 244);
            this.dgvSubscribed.Name = "dgvSubscribed";
            this.dgvSubscribed.ReadOnly = true;
            this.dgvSubscribed.RowTemplate.Height = 23;
            this.dgvSubscribed.Size = new System.Drawing.Size(598, 207);
            this.dgvSubscribed.TabIndex = 1;
            this.dgvSubscribed.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvSubscribed_CellMouseDoubleClick);
            // 
            // dgvtbcId
            // 
            this.dgvtbcId.HeaderText = "Id";
            this.dgvtbcId.Name = "dgvtbcId";
            this.dgvtbcId.ReadOnly = true;
            this.dgvtbcId.Width = 40;
            // 
            // dgvtbcSubType
            // 
            this.dgvtbcSubType.FillWeight = 80F;
            this.dgvtbcSubType.HeaderText = "订阅类型";
            this.dgvtbcSubType.Name = "dgvtbcSubType";
            this.dgvtbcSubType.ReadOnly = true;
            this.dgvtbcSubType.Width = 80;
            // 
            // dgvtbcFlow
            // 
            this.dgvtbcFlow.FillWeight = 80F;
            this.dgvtbcFlow.HeaderText = "路线";
            this.dgvtbcFlow.Name = "dgvtbcFlow";
            this.dgvtbcFlow.ReadOnly = true;
            this.dgvtbcFlow.Width = 80;
            // 
            // dgvtbcRegion
            // 
            this.dgvtbcRegion.FillWeight = 80F;
            this.dgvtbcRegion.HeaderText = "区域";
            this.dgvtbcRegion.Name = "dgvtbcRegion";
            this.dgvtbcRegion.ReadOnly = true;
            this.dgvtbcRegion.Width = 80;
            // 
            // dgvtbcUserName
            // 
            this.dgvtbcUserName.FillWeight = 80F;
            this.dgvtbcUserName.HeaderText = "用户名";
            this.dgvtbcUserName.Name = "dgvtbcUserName";
            this.dgvtbcUserName.ReadOnly = true;
            this.dgvtbcUserName.Width = 80;
            // 
            // dgvtbcPrinterName
            // 
            this.dgvtbcPrinterName.FillWeight = 180F;
            this.dgvtbcPrinterName.HeaderText = "打印机";
            this.dgvtbcPrinterName.Name = "dgvtbcPrinterName";
            this.dgvtbcPrinterName.ReadOnly = true;
            this.dgvtbcPrinterName.Width = 180;
            // 
            // dgvtbcIsAutoPrint
            // 
            this.dgvtbcIsAutoPrint.FillWeight = 50F;
            this.dgvtbcIsAutoPrint.HeaderText = "自动打印";
            this.dgvtbcIsAutoPrint.Name = "dgvtbcIsAutoPrint";
            this.dgvtbcIsAutoPrint.ReadOnly = true;
            // 
            // gpb_SubCondition
            // 
            this.gpb_SubCondition.Controls.Add(this.labelUserName);
            this.gpb_SubCondition.Controls.Add(this.tbUserName);
            this.gpb_SubCondition.Controls.Add(this.labelPrinter);
            this.gpb_SubCondition.Controls.Add(this.labelRegion);
            this.gpb_SubCondition.Controls.Add(this.labelFlow);
            this.gpb_SubCondition.Controls.Add(this.labelSubsType);
            this.gpb_SubCondition.Controls.Add(this.ckIsAutoPrint);
            this.gpb_SubCondition.Controls.Add(this.label2);
            this.gpb_SubCondition.Controls.Add(this.btnAdd);
            this.gpb_SubCondition.Controls.Add(this.btnChoosePrinter);
            this.gpb_SubCondition.Controls.Add(this.tbPrinterName);
            this.gpb_SubCondition.Controls.Add(this.label1);
            this.gpb_SubCondition.Controls.Add(this.tbRegion);
            this.gpb_SubCondition.Controls.Add(this.tbFlow);
            this.gpb_SubCondition.Controls.Add(this.lblRegion);
            this.gpb_SubCondition.Controls.Add(this.lblFlow);
            this.gpb_SubCondition.Controls.Add(this.lblSubType);
            this.gpb_SubCondition.Controls.Add(this.cmbSubType);
            this.gpb_SubCondition.Location = new System.Drawing.Point(3, 6);
            this.gpb_SubCondition.Name = "gpb_SubCondition";
            this.gpb_SubCondition.Size = new System.Drawing.Size(594, 232);
            this.gpb_SubCondition.TabIndex = 0;
            this.gpb_SubCondition.TabStop = false;
            this.gpb_SubCondition.Text = "订阅打印条件";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(20, 176);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 14);
            this.label2.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(20, 135);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 14);
            this.label1.TabIndex = 7;
            // 
            // lblRegion
            // 
            this.lblRegion.AutoSize = true;
            this.lblRegion.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblRegion.Location = new System.Drawing.Point(20, 94);
            this.lblRegion.Name = "lblRegion";
            this.lblRegion.Size = new System.Drawing.Size(0, 14);
            this.lblRegion.TabIndex = 4;
            // 
            // lblFlow
            // 
            this.lblFlow.AutoSize = true;
            this.lblFlow.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblFlow.Location = new System.Drawing.Point(20, 59);
            this.lblFlow.Name = "lblFlow";
            this.lblFlow.Size = new System.Drawing.Size(0, 14);
            this.lblFlow.TabIndex = 3;
            // 
            // lblSubType
            // 
            this.lblSubType.AutoSize = true;
            this.lblSubType.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblSubType.Location = new System.Drawing.Point(20, 26);
            this.lblSubType.Name = "lblSubType";
            this.lblSubType.Size = new System.Drawing.Size(0, 14);
            this.lblSubType.TabIndex = 2;
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(112, 457);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(111, 23);
            this.btnDelete.TabIndex = 15;
            this.btnDelete.Text = "删除订阅打印";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lblWarning1);
            this.tabPage2.Controls.Add(this.btnPrintSelected);
            this.tabPage2.Controls.Add(this.lblNoAutoPrint);
            this.tabPage2.Controls.Add(this.lblWaitforPrint);
            this.tabPage2.Controls.Add(this.dataGridViewUnPrinted);
            this.tabPage2.Controls.Add(this.dataGridViewNoAutoPrint);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(603, 490);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "未打印文档";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // lblWarning1
            // 
            this.lblWarning1.AutoSize = true;
            this.lblWarning1.Location = new System.Drawing.Point(373, 465);
            this.lblWarning1.Name = "lblWarning1";
            this.lblWarning1.Size = new System.Drawing.Size(227, 12);
            this.lblWarning1.TabIndex = 7;
            this.lblWarning1.Text = "注意：非自动打印列表最多缓存500条记录";
            // 
            // btnPrintSelected
            // 
            this.btnPrintSelected.Location = new System.Drawing.Point(39, 460);
            this.btnPrintSelected.Name = "btnPrintSelected";
            this.btnPrintSelected.Size = new System.Drawing.Size(100, 23);
            this.btnPrintSelected.TabIndex = 6;
            this.btnPrintSelected.Text = "打印";
            this.btnPrintSelected.UseVisualStyleBackColor = true;
            this.btnPrintSelected.Click += new System.EventHandler(this.btnPrintSelected_Click);
            // 
            // lblNoAutoPrint
            // 
            this.lblNoAutoPrint.Location = new System.Drawing.Point(7, 230);
            this.lblNoAutoPrint.Name = "lblNoAutoPrint";
            this.lblNoAutoPrint.Size = new System.Drawing.Size(100, 15);
            this.lblNoAutoPrint.TabIndex = 5;
            this.lblNoAutoPrint.Text = "非自动打印列表";
            // 
            // lblWaitforPrint
            // 
            this.lblWaitforPrint.Location = new System.Drawing.Point(7, 4);
            this.lblWaitforPrint.Name = "lblWaitforPrint";
            this.lblWaitforPrint.Size = new System.Drawing.Size(100, 15);
            this.lblWaitforPrint.TabIndex = 4;
            this.lblWaitforPrint.Text = "待打印列表";
            // 
            // dataGridViewUnPrinted
            // 
            this.dataGridViewUnPrinted.AllowUserToDeleteRows = false;
            this.dataGridViewUnPrinted.AllowUserToResizeRows = false;
            this.dataGridViewUnPrinted.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            this.dataGridViewUnPrinted.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewUnPrinted.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.Column4,
            this.dataGridViewTextBoxColumn5});
            this.dataGridViewUnPrinted.Location = new System.Drawing.Point(0, 22);
            this.dataGridViewUnPrinted.MultiSelect = false;
            this.dataGridViewUnPrinted.Name = "dataGridViewUnPrinted";
            this.dataGridViewUnPrinted.ReadOnly = true;
            this.dataGridViewUnPrinted.RowTemplate.Height = 23;
            this.dataGridViewUnPrinted.Size = new System.Drawing.Size(600, 202);
            this.dataGridViewUnPrinted.TabIndex = 3;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "No";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Width = 80;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "订阅类型";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.Width = 80;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.HeaderText = "来源库位";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.Width = 80;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.HeaderText = "目的库位";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            this.dataGridViewTextBoxColumn4.Width = 80;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "用户名";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.Width = 80;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.HeaderText = "打印机";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.ReadOnly = true;
            this.dataGridViewTextBoxColumn5.Width = 180;
            // 
            // dataGridViewNoAutoPrint
            // 
            this.dataGridViewNoAutoPrint.AllowUserToDeleteRows = false;
            this.dataGridViewNoAutoPrint.AllowUserToResizeRows = false;
            this.dataGridViewNoAutoPrint.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            this.dataGridViewNoAutoPrint.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewNoAutoPrint.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.dataGridViewTextBoxColumn6,
            this.Column2,
            this.Column3,
            this.Column5,
            this.dataGridViewTextBoxColumn9});
            this.dataGridViewNoAutoPrint.Location = new System.Drawing.Point(0, 248);
            this.dataGridViewNoAutoPrint.MultiSelect = false;
            this.dataGridViewNoAutoPrint.Name = "dataGridViewNoAutoPrint";
            this.dataGridViewNoAutoPrint.ReadOnly = true;
            this.dataGridViewNoAutoPrint.RowTemplate.Height = 23;
            this.dataGridViewNoAutoPrint.Size = new System.Drawing.Size(600, 208);
            this.dataGridViewNoAutoPrint.TabIndex = 2;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "No";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 80;
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.HeaderText = "订阅类型";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.ReadOnly = true;
            this.dataGridViewTextBoxColumn6.Width = 80;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "来源库位";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Width = 80;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "目的库位";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.Width = 80;
            // 
            // Column5
            // 
            this.Column5.HeaderText = "用户名";
            this.Column5.Name = "Column5";
            this.Column5.ReadOnly = true;
            this.Column5.Width = 80;
            // 
            // dataGridViewTextBoxColumn9
            // 
            this.dataGridViewTextBoxColumn9.HeaderText = "打印机";
            this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
            this.dataGridViewTextBoxColumn9.ReadOnly = true;
            this.dataGridViewTextBoxColumn9.Width = 180;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.label3);
            this.tabPage3.Controls.Add(this.dataGridViewPrintedList);
            this.tabPage3.Controls.Add(this.btnRePrint);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(603, 490);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "已打印文档";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(397, 460);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(203, 12);
            this.label3.TabIndex = 9;
            this.label3.Text = "注意：已打印列表最多缓存500条记录";
            // 
            // dataGridViewPrintedList
            // 
            this.dataGridViewPrintedList.AllowUserToDeleteRows = false;
            this.dataGridViewPrintedList.AllowUserToResizeRows = false;
            this.dataGridViewPrintedList.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            this.dataGridViewPrintedList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPrintedList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn13,
            this.dataGridViewTextBoxColumn14,
            this.dataGridViewTextBoxColumn15,
            this.dataGridViewTextBoxColumn16,
            this.Column6,
            this.dataGridViewTextBoxColumn17});
            this.dataGridViewPrintedList.Location = new System.Drawing.Point(2, 3);
            this.dataGridViewPrintedList.MultiSelect = false;
            this.dataGridViewPrintedList.Name = "dataGridViewPrintedList";
            this.dataGridViewPrintedList.ReadOnly = true;
            this.dataGridViewPrintedList.RowTemplate.Height = 23;
            this.dataGridViewPrintedList.Size = new System.Drawing.Size(601, 442);
            this.dataGridViewPrintedList.TabIndex = 8;
            // 
            // dataGridViewTextBoxColumn13
            // 
            this.dataGridViewTextBoxColumn13.HeaderText = "No";
            this.dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
            this.dataGridViewTextBoxColumn13.ReadOnly = true;
            this.dataGridViewTextBoxColumn13.Width = 80;
            // 
            // dataGridViewTextBoxColumn14
            // 
            this.dataGridViewTextBoxColumn14.HeaderText = "订阅类型";
            this.dataGridViewTextBoxColumn14.Name = "dataGridViewTextBoxColumn14";
            this.dataGridViewTextBoxColumn14.ReadOnly = true;
            this.dataGridViewTextBoxColumn14.Width = 80;
            // 
            // dataGridViewTextBoxColumn15
            // 
            this.dataGridViewTextBoxColumn15.HeaderText = "来源库位";
            this.dataGridViewTextBoxColumn15.Name = "dataGridViewTextBoxColumn15";
            this.dataGridViewTextBoxColumn15.ReadOnly = true;
            this.dataGridViewTextBoxColumn15.Width = 80;
            // 
            // dataGridViewTextBoxColumn16
            // 
            this.dataGridViewTextBoxColumn16.HeaderText = "目的库位";
            this.dataGridViewTextBoxColumn16.Name = "dataGridViewTextBoxColumn16";
            this.dataGridViewTextBoxColumn16.ReadOnly = true;
            this.dataGridViewTextBoxColumn16.Width = 80;
            // 
            // Column6
            // 
            this.Column6.HeaderText = "用户名";
            this.Column6.Name = "Column6";
            this.Column6.ReadOnly = true;
            this.Column6.Width = 80;
            // 
            // dataGridViewTextBoxColumn17
            // 
            this.dataGridViewTextBoxColumn17.HeaderText = "打印机";
            this.dataGridViewTextBoxColumn17.Name = "dataGridViewTextBoxColumn17";
            this.dataGridViewTextBoxColumn17.ReadOnly = true;
            this.dataGridViewTextBoxColumn17.Width = 180;
            // 
            // btnRePrint
            // 
            this.btnRePrint.Location = new System.Drawing.Point(42, 455);
            this.btnRePrint.Name = "btnRePrint";
            this.btnRePrint.Size = new System.Drawing.Size(75, 23);
            this.btnRePrint.TabIndex = 1;
            this.btnRePrint.Text = "打印";
            this.btnRePrint.UseVisualStyleBackColor = true;
            this.btnRePrint.Click += new System.EventHandler(this.btnRePrint_Click);
            // 
            // timerPrint
            // 
            this.timerPrint.Enabled = true;
            this.timerPrint.Interval = 10000;
            this.timerPrint.Tick += new System.EventHandler(this.timerPrint_Tick);
            // 
            // timerShowData
            // 
            this.timerShowData.Enabled = true;
            this.timerShowData.Interval = 1000;
            this.timerShowData.Tick += new System.EventHandler(this.timerShowData_Tick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tabControl1);
            this.panel1.Location = new System.Drawing.Point(0, -1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(613, 519);
            this.panel1.TabIndex = 1;
            // 
            // cmbSubType
            // 
            this.cmbSubType.FormattingEnabled = true;
            this.cmbSubType.Location = new System.Drawing.Point(114, 26);
            this.cmbSubType.Name = "cmbSubType";
            this.cmbSubType.Size = new System.Drawing.Size(194, 20);
            this.cmbSubType.TabIndex = 1;
            // 
            // tbFlow
            // 
            this.tbFlow.Location = new System.Drawing.Point(114, 58);
            this.tbFlow.Name = "tbFlow";
            this.tbFlow.Size = new System.Drawing.Size(194, 21);
            this.tbFlow.TabIndex = 5;
            // 
            // tbRegion
            // 
            this.tbRegion.Location = new System.Drawing.Point(114, 93);
            this.tbRegion.Name = "tbRegion";
            this.tbRegion.Size = new System.Drawing.Size(194, 21);
            this.tbRegion.TabIndex = 6;
            // 
            // tbPrinterName
            // 
            this.tbPrinterName.Location = new System.Drawing.Point(114, 128);
            this.tbPrinterName.Name = "tbPrinterName";
            this.tbPrinterName.Size = new System.Drawing.Size(194, 21);
            this.tbPrinterName.TabIndex = 8;
            // 
            // btnChoosePrinter
            // 
            this.btnChoosePrinter.Location = new System.Drawing.Point(317, 126);
            this.btnChoosePrinter.Name = "btnChoosePrinter";
            this.btnChoosePrinter.Size = new System.Drawing.Size(106, 23);
            this.btnChoosePrinter.TabIndex = 9;
            this.btnChoosePrinter.Text = "选择打印机";
            this.btnChoosePrinter.UseVisualStyleBackColor = true;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(114, 196);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(96, 23);
            this.btnAdd.TabIndex = 10;
            this.btnAdd.Text = "保存订阅打印";
            this.btnAdd.UseVisualStyleBackColor = true;
            // 
            // ckIsAutoPrint
            // 
            this.ckIsAutoPrint.AutoSize = true;
            this.ckIsAutoPrint.Location = new System.Drawing.Point(116, 166);
            this.ckIsAutoPrint.Name = "ckIsAutoPrint";
            this.ckIsAutoPrint.Size = new System.Drawing.Size(96, 16);
            this.ckIsAutoPrint.TabIndex = 13;
            this.ckIsAutoPrint.Text = "是否自动打印";
            this.ckIsAutoPrint.UseVisualStyleBackColor = true;
            // 
            // labelSubsType
            // 
            this.labelSubsType.AutoSize = true;
            this.labelSubsType.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelSubsType.Location = new System.Drawing.Point(20, 29);
            this.labelSubsType.Name = "labelSubsType";
            this.labelSubsType.Size = new System.Drawing.Size(77, 14);
            this.labelSubsType.TabIndex = 16;
            this.labelSubsType.Text = "订阅类型：";
            // 
            // labelFlow
            // 
            this.labelFlow.AutoSize = true;
            this.labelFlow.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelFlow.Location = new System.Drawing.Point(20, 61);
            this.labelFlow.Name = "labelFlow";
            this.labelFlow.Size = new System.Drawing.Size(77, 14);
            this.labelFlow.TabIndex = 17;
            this.labelFlow.Text = "路    线：";
            // 
            // labelRegion
            // 
            this.labelRegion.AutoSize = true;
            this.labelRegion.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelRegion.Location = new System.Drawing.Point(20, 96);
            this.labelRegion.Name = "labelRegion";
            this.labelRegion.Size = new System.Drawing.Size(77, 14);
            this.labelRegion.TabIndex = 18;
            this.labelRegion.Text = "区    域：";
            // 
            // labelPrinter
            // 
            this.labelPrinter.AutoSize = true;
            this.labelPrinter.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelPrinter.Location = new System.Drawing.Point(22, 132);
            this.labelPrinter.Name = "labelPrinter";
            this.labelPrinter.Size = new System.Drawing.Size(77, 14);
            this.labelPrinter.TabIndex = 19;
            this.labelPrinter.Text = "打 印 机：";
            // 
            // tbUserName
            // 
            this.tbUserName.Location = new System.Drawing.Point(114, 75);
            this.tbUserName.Name = "tbUserName";
            this.tbUserName.Size = new System.Drawing.Size(194, 21);
            this.tbUserName.TabIndex = 20;
            this.tbUserName.Visible = false;
            // 
            // labelUserName
            // 
            this.labelUserName.AutoSize = true;
            this.labelUserName.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelUserName.Location = new System.Drawing.Point(20, 78);
            this.labelUserName.Name = "labelUserName";
            this.labelUserName.Size = new System.Drawing.Size(77, 14);
            this.labelUserName.TabIndex = 21;
            this.labelUserName.Text = "用 户 名：";
            this.labelUserName.Visible = false;
            // 
            // PrintClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 521);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.Name = "PrintClient";
            this.Text = "订阅打印监视器";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PrintClient_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSubscribed)).EndInit();
            this.gpb_SubCondition.ResumeLayout(false);
            this.gpb_SubCondition.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUnPrinted)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewNoAutoPrint)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPrintedList)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Timer timerPrint;
        private System.Windows.Forms.Button btnRePrint;
        private System.Windows.Forms.Timer timerShowData;
        private System.Windows.Forms.DataGridView dataGridViewNoAutoPrint;
        private System.Windows.Forms.DataGridView dataGridViewUnPrinted;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.DataGridView dgvSubscribed;
        private System.Windows.Forms.GroupBox gpb_SubCondition;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblRegion;
        private System.Windows.Forms.Label lblFlow;
        private System.Windows.Forms.Label lblSubType;
        private System.Windows.Forms.Label lblWaitforPrint;
        private System.Windows.Forms.Label lblNoAutoPrint;
        private System.Windows.Forms.Button btnPrintSelected;
        private System.Windows.Forms.DataGridView dataGridViewPrintedList;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Label lblWarning1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtbcId;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtbcSubType;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtbcFlow;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtbcRegion;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtbcUserName;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtbcPrinterName;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtbcIsAutoPrint;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn14;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn15;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn16;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn17;
        private System.Windows.Forms.Label labelUserName;
        private System.Windows.Forms.TextBox tbUserName;
        private System.Windows.Forms.Label labelPrinter;
        private System.Windows.Forms.Label labelRegion;
        private System.Windows.Forms.Label labelFlow;
        private System.Windows.Forms.Label labelSubsType;
        private System.Windows.Forms.CheckBox ckIsAutoPrint;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnChoosePrinter;
        private System.Windows.Forms.TextBox tbPrinterName;
        private System.Windows.Forms.TextBox tbRegion;
        private System.Windows.Forms.TextBox tbFlow;
        private System.Windows.Forms.ComboBox cmbSubType;
    }
}

