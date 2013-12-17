namespace com.Sconit.SmartDevice
{
    partial class UCLotNoScan
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
            this.tbBarCode = new System.Windows.Forms.TextBox();
            this.lblMessage = new System.Windows.Forms.Label();
            this.lblBarCodeInfo = new System.Windows.Forms.Label();
            this.lblWo = new System.Windows.Forms.Label();
            this.lblTraceCodeInfo = new System.Windows.Forms.Label();
            this.lblFlow = new System.Windows.Forms.Label();
            this.lblOpRefInfo = new System.Windows.Forms.Label();
            this.lblSeq = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbBarCode
            // 
            this.tbBarCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbBarCode.Location = new System.Drawing.Point(3, 1);
            this.tbBarCode.MaxLength = 50;
            this.tbBarCode.Name = "tbBarCode";
            this.tbBarCode.Size = new System.Drawing.Size(231, 23);
            this.tbBarCode.TabIndex = 119;
            this.tbBarCode.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbBarCode_KeyUp);
            // 
            // lblMessage
            // 
            this.lblMessage.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular);
            this.lblMessage.ForeColor = System.Drawing.Color.Red;
            this.lblMessage.Location = new System.Drawing.Point(0, 34);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(234, 31);
            this.lblMessage.Text = "请扫描工位";
            // 
            // lblBarCodeInfo
            // 
            this.lblBarCodeInfo.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular);
            this.lblBarCodeInfo.Location = new System.Drawing.Point(47, 104);
            this.lblBarCodeInfo.Name = "lblBarCodeInfo";
            this.lblBarCodeInfo.Size = new System.Drawing.Size(173, 16);
            // 
            // lblWo
            // 
            this.lblWo.Location = new System.Drawing.Point(7, 104);
            this.lblWo.Name = "lblWo";
            this.lblWo.Size = new System.Drawing.Size(41, 18);
            this.lblWo.Text = "批号:";
            // 
            // lblTraceCodeInfo
            // 
            this.lblTraceCodeInfo.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular);
            this.lblTraceCodeInfo.Location = new System.Drawing.Point(47, 84);
            this.lblTraceCodeInfo.Name = "lblTraceCodeInfo";
            this.lblTraceCodeInfo.Size = new System.Drawing.Size(170, 18);
            // 
            // lblFlow
            // 
            this.lblFlow.Location = new System.Drawing.Point(0, 84);
            this.lblFlow.Name = "lblFlow";
            this.lblFlow.Size = new System.Drawing.Size(54, 18);
            this.lblFlow.Text = "Van号:";
            // 
            // lblOpRefInfo
            // 
            this.lblOpRefInfo.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular);
            this.lblOpRefInfo.Location = new System.Drawing.Point(47, 65);
            this.lblOpRefInfo.Name = "lblOpRefInfo";
            this.lblOpRefInfo.Size = new System.Drawing.Size(170, 18);
            // 
            // lblSeq
            // 
            this.lblSeq.Location = new System.Drawing.Point(7, 65);
            this.lblSeq.Name = "lblSeq";
            this.lblSeq.Size = new System.Drawing.Size(41, 18);
            this.lblSeq.Text = "工位:";
            // 
            // UCLotNoScan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.lblBarCodeInfo);
            this.Controls.Add(this.lblWo);
            this.Controls.Add(this.lblTraceCodeInfo);
            this.Controls.Add(this.lblFlow);
            this.Controls.Add(this.lblOpRefInfo);
            this.Controls.Add(this.lblSeq);
            this.Controls.Add(this.tbBarCode);
            this.Controls.Add(this.lblMessage);
            this.Name = "UCLotNoScan";
            this.Size = new System.Drawing.Size(240, 320);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TextBox tbBarCode;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Label lblBarCodeInfo;
        private System.Windows.Forms.Label lblWo;
        private System.Windows.Forms.Label lblTraceCodeInfo;
        private System.Windows.Forms.Label lblFlow;
        private System.Windows.Forms.Label lblOpRefInfo;
        private System.Windows.Forms.Label lblSeq;
    }
}
