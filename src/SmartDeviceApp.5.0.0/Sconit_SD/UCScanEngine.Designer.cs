namespace com.Sconit.SmartDevice
{
    partial class UCScanEngine
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
            this.lblEngineInfo = new System.Windows.Forms.Label();
            this.lblEngine = new System.Windows.Forms.Label();
            this.lblTraceCodeInfo = new System.Windows.Forms.Label();
            this.lblTraceCode = new System.Windows.Forms.Label();
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
            this.lblMessage.Size = new System.Drawing.Size(234, 47);
            this.lblMessage.Text = "请先扫描Van号，在扫描发动机条码";
            // 
            // lblEngineInfo
            // 
            this.lblEngineInfo.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular);
            this.lblEngineInfo.Location = new System.Drawing.Point(78, 101);
            this.lblEngineInfo.Name = "lblEngineInfo";
            this.lblEngineInfo.Size = new System.Drawing.Size(142, 16);
            // 
            // lblEngine
            // 
            this.lblEngine.Location = new System.Drawing.Point(1, 101);
            this.lblEngine.Name = "lblEngine";
            this.lblEngine.Size = new System.Drawing.Size(79, 18);
            this.lblEngine.Text = "发动机号:";
            // 
            // lblTraceCodeInfo
            // 
            this.lblTraceCodeInfo.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular);
            this.lblTraceCodeInfo.Location = new System.Drawing.Point(78, 81);
            this.lblTraceCodeInfo.Name = "lblTraceCodeInfo";
            this.lblTraceCodeInfo.Size = new System.Drawing.Size(139, 18);
            // 
            // lblTraceCode
            // 
            this.lblTraceCode.Location = new System.Drawing.Point(23, 81);
            this.lblTraceCode.Name = "lblTraceCode";
            this.lblTraceCode.Size = new System.Drawing.Size(48, 18);
            this.lblTraceCode.Text = "Van号:";
            // 
            // UCScanEngine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.lblEngineInfo);
            this.Controls.Add(this.lblEngine);
            this.Controls.Add(this.lblTraceCodeInfo);
            this.Controls.Add(this.lblTraceCode);
            this.Controls.Add(this.tbBarCode);
            this.Controls.Add(this.lblMessage);
            this.Name = "UCScanEngine";
            this.Size = new System.Drawing.Size(240, 320);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TextBox tbBarCode;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Label lblEngineInfo;
        private System.Windows.Forms.Label lblEngine;
        private System.Windows.Forms.Label lblTraceCodeInfo;
        private System.Windows.Forms.Label lblTraceCode;
    }
}
