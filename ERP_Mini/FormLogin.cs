using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace ERP_Mini
{
    public partial class FormLogin : DevExpress.XtraEditors.XtraForm
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUserName.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "Username and password cannot be empty!";
                lblError.Visible = true;
                return;
            }

            if (DataBaseHelper.AuthenticateUser(username, password))
            {
                XtraMessageBox.Show("Login successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                
                FormDashboard dashboard = new FormDashboard();
                this.Hide();
                dashboard.ShowDialog();
                this.Close();
            }
            else
            {
                lblError.Text = "Invalid username or password!";
                lblError.Visible = true;
            }
        }
    }
}