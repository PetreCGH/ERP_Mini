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
    public partial class FormDashboard : DevExpress.XtraEditors.XtraForm
    {
        public FormDashboard()
        {
            InitializeComponent();
        }

        private void btnProducts_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            FormProducts formProducts = new FormProducts();
            formProducts.Show();
        }

        private void btnCustomers_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            FormCustomers formCustomers = new FormCustomers();
            formCustomers.Show();
        }

        private void btnOrders_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            FormOrders formOrders = new FormOrders();
            formOrders.Show():
        }
    }
}
