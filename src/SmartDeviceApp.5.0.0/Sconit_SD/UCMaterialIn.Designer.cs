namespace com.Sconit.SmartDevice
{
    partial class UCMaterialIn
    {
        ///// <summary> 
        ///// Required designer variable.
        ///// </summary>
        //private System.ComponentModel.IContainer components = null;

        ///// <summary> 
        ///// Clean up any resources being used.
        ///// </summary>
        ///// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        //protected void Dispose(bool disposing)
        //{
        //    if (disposing && (components != null))
        //    {
        //        components.Dispose();
        //    }
        //    //base.Dispose(disposing);
        //}

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbBarCode = new System.Windows.Forms.TextBox();
            this.lblMessage = new System.Windows.Forms.Label();
            this.lblVANInfo = new System.Windows.Forms.Label();
            this.lblVAN = new System.Windows.Forms.Label();
            this.LBScanhuList = new System.Windows.Forms.ListBox();
            this.lblBarCode = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbBarCode
            // 
            this.tbBarCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbBarCode.Location = new System.Drawing.Point(41, 0);
            this.tbBarCode.MaxLength = 50;
            this.tbBarCode.Name = "tbBarCode";
            this.tbBarCode.Size = new System.Drawing.Size(183, 27);
            this.tbBarCode.TabIndex = 115;
            this.tbBarCode.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbBarCode_KeyUp);
            // 
            // lblMessage
            // 
            this.lblMessage.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular);
            this.lblMessage.ForeColor = System.Drawing.Color.Red;
            this.lblMessage.Location = new System.Drawing.Point(3, 30);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(234, 16);
            this.lblMessage.Text = "123456789012345678901234567890";
            // 
            // lblVANInfo
            // 
            this.lblVANInfo.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular);
            this.lblVANInfo.Location = new System.Drawing.Point(45, 57);
            this.lblVANInfo.Name = "lblVANInfo";
            this.lblVANInfo.Size = new System.Drawing.Size(179, 16);
            this.lblVANInfo.Text = "123456789012345678901234567890";
            // 
            // lblVAN
            // 
            this.lblVAN.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular);
            this.lblVAN.Location = new System.Drawing.Point(4, 57);
            this.lblVAN.Name = "lblVAN";
            this.lblVAN.Size = new System.Drawing.Size(41, 18);
            this.lblVAN.Text = "VAN:";
            // 
            // LBScanhuList
            // 
            this.LBScanhuList.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.LBScanhuList.Location = new System.Drawing.Point(4, 84);
            this.LBScanhuList.Name = "LBScanhuList";
            this.LBScanhuList.Size = new System.Drawing.Size(233, 194);
            this.LBScanhuList.TabIndex = 194;
            // 
            // lblBarCode
            // 
            this.lblBarCode.Location = new System.Drawing.Point(4, 1);
            this.lblBarCode.Name = "lblBarCode";
            this.lblBarCode.Size = new System.Drawing.Size(151, 23);
            this.lblBarCode.Text = "条码:";
            // 
            // UCMaterialIn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.LBScanhuList);
            this.Controls.Add(this.lblVANInfo);
            this.Controls.Add(this.lblVAN);
            this.Controls.Add(this.tbBarCode);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.lblBarCode);
            this.Name = "UCMaterialIn";
            this.Size = new System.Drawing.Size(240, 320);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblMessage;
        public System.Windows.Forms.TextBox tbBarCode;
        private System.Windows.Forms.Label lblVANInfo;
        private System.Windows.Forms.Label lblVAN;
        private System.Windows.Forms.ListBox LBScanhuList;
        private System.Windows.Forms.Label lblBarCode;


    }
}
