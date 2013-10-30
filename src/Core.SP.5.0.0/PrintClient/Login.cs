using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PrintClient
{
    public partial class Login : Form
    {
        public delegate void loginDelegate(string userName, string userPassword);
        public event loginDelegate loginEvent;

        public Login()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            SconitWs.SecurityService securityService = new SconitWs.SecurityService();
            bool isHavePermission = securityService.VerifyUserPassword(tbUserCode.Text.Trim(), tbPassword.Text.Trim());
            if (isHavePermission)
            {
                if (loginEvent != null)
                {
                    loginEvent(tbUserCode.Text.Trim(), tbPassword.Text.Trim());
                    this.Close();
                    return;
                }
                this.Visible = false;
                PrintClient printClient = new PrintClient();
                printClient.Show();
            }
            else
            {
                lblMessage.Visible = true;
                lblMessage.Text = "用户名或者密码错误";
            }
        }
    }
}
