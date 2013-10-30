namespace com.Sconit.SmartDevice
{
    partial class UCProductOrderOnline
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnOrder = new System.Windows.Forms.Button();
            this.tbBarCode = new System.Windows.Forms.TextBox();
            this.lblBarCode = new System.Windows.Forms.Label();
            this.lblStartTimeInfo = new System.Windows.Forms.Label();
            this.lblStartTime = new System.Windows.Forms.Label();
            this.lblWoInfo = new System.Windows.Forms.Label();
            this.lblWo = new System.Windows.Forms.Label();
            this.lblVANInfo = new System.Windows.Forms.Label();
            this.lblVAN = new System.Windows.Forms.Label();
            this.lblFlowInfo = new System.Windows.Forms.Label();
            this.lblFlow = new System.Windows.Forms.Label();
            this.lblMessage = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnOrder
            // 
            this.btnOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOrder.Location = new System.Drawing.Point(200, 1);
            this.btnOrder.Name = "btnOrder";
            this.btnOrder.Size = new System.Drawing.Size(40, 20);
            this.btnOrder.TabIndex = 116;
            this.btnOrder.Text = "确定";
            this.btnOrder.Click += new System.EventHandler(this.btnOrder_Click);
            this.btnOrder.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbBarCode_KeyUp);
            // 
            // tbBarCode
            // 
            this.tbBarCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbBarCode.Location = new System.Drawing.Point(43, 0);
            this.tbBarCode.MaxLength = 50;
            this.tbBarCode.Name = "tbBarCode";
            this.tbBarCode.Size = new System.Drawing.Size(151, 27);
            this.tbBarCode.TabIndex = 115;
            this.tbBarCode.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbBarCode_KeyUp);
            // 
            // lblBarCode
            // 
            this.lblBarCode.Location = new System.Drawing.Point(6, 1);
            this.lblBarCode.Name = "lblBarCode";
            this.lblBarCode.Size = new System.Drawing.Size(151, 23);
            this.lblBarCode.Text = "条码:";
            // 
            // lblStartTimeInfo
            // 
            this.lblStartTimeInfo.Location = new System.Drawing.Point(77, 108);
            this.lblStartTimeInfo.Name = "lblStartTimeInfo";
            this.lblStartTimeInfo.Size = new System.Drawing.Size(162, 16);
            this.lblStartTimeInfo.Text = "2011-1-1 11:11";
            // 
            // lblStartTime
            // 
            this.lblStartTime.Location = new System.Drawing.Point(1, 108);
            this.lblStartTime.Name = "lblStartTime";
            this.lblStartTime.Size = new System.Drawing.Size(70, 18);
            this.lblStartTime.Text = "上线日期:";
            // 
            // lblWoInfo
            // 
            this.lblWoInfo.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular);
            this.lblWoInfo.Location = new System.Drawing.Point(43, 71);
            this.lblWoInfo.Name = "lblWoInfo";
            this.lblWoInfo.Size = new System.Drawing.Size(179, 16);
            this.lblWoInfo.Text = "123456789012345678901234567890";
            // 
            // lblWo
            // 
            this.lblWo.Location = new System.Drawing.Point(2, 71);
            this.lblWo.Name = "lblWo";
            this.lblWo.Size = new System.Drawing.Size(41, 18);
            this.lblWo.Text = "工单:";
            // 
            // lblVANInfo
            // 
            this.lblVANInfo.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular);
            this.lblVANInfo.Location = new System.Drawing.Point(43, 89);
            this.lblVANInfo.Name = "lblVANInfo";
            this.lblVANInfo.Size = new System.Drawing.Size(179, 16);
            this.lblVANInfo.Text = "123456789012345678901234567890";
            // 
            // lblVAN
            // 
            this.lblVAN.Location = new System.Drawing.Point(3, 89);
            this.lblVAN.Name = "lblVAN";
            this.lblVAN.Size = new System.Drawing.Size(40, 18);
            this.lblVAN.Text = "VAN :";
            // 
            // lblFlowInfo
            // 
            this.lblFlowInfo.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular);
            this.lblFlowInfo.Location = new System.Drawing.Point(43, 51);
            this.lblFlowInfo.Name = "lblFlowInfo";
            this.lblFlowInfo.Size = new System.Drawing.Size(176, 18);
            this.lblFlowInfo.Text = "A";
            // 
            // lblFlow
            // 
            this.lblFlow.Location = new System.Drawing.Point(2, 51);
            this.lblFlow.Name = "lblFlow";
            this.lblFlow.Size = new System.Drawing.Size(41, 18);
            this.lblFlow.Text = "产线:";
            // 
            // lblMessage
            // 
            this.lblMessage.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular);
            this.lblMessage.ForeColor = System.Drawing.Color.Red;
            this.lblMessage.Location = new System.Drawing.Point(0, 24);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(234, 16);
            this.lblMessage.Text = "123456789012345678901234567890";
            // 
            // UCProductOrderOnline
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.lblStartTimeInfo);
            this.Controls.Add(this.lblStartTime);
            this.Controls.Add(this.lblWoInfo);
            this.Controls.Add(this.lblWo);
            this.Controls.Add(this.lblVANInfo);
            this.Controls.Add(this.lblVAN);
            this.Controls.Add(this.lblFlowInfo);
            this.Controls.Add(this.lblFlow);
            this.Controls.Add(this.btnOrder);
            this.Controls.Add(this.tbBarCode);
            this.Controls.Add(this.lblBarCode);
            this.Name = "UCProductOrderOnline";
            this.Size = new System.Drawing.Size(240, 320);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOrder;
        public System.Windows.Forms.TextBox tbBarCode;
        private System.Windows.Forms.Label lblBarCode;
        private System.Windows.Forms.Label lblStartTimeInfo;
        private System.Windows.Forms.Label lblStartTime;
        private System.Windows.Forms.Label lblWoInfo;
        private System.Windows.Forms.Label lblWo;
        private System.Windows.Forms.Label lblVANInfo;
        private System.Windows.Forms.Label lblVAN;
        private System.Windows.Forms.Label lblFlowInfo;
        private System.Windows.Forms.Label lblFlow;
        private System.Windows.Forms.Label lblMessage;
    }
}
