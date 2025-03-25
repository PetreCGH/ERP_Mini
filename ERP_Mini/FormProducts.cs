using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace ERP_Mini
{
    public partial class FormProducts : DevExpress.XtraEditors.XtraForm
    {
        public FormProducts()
        {
            InitializeComponent();
            this.Load += FormProducts_Load;
        }

        private void LoadProducts()
        {
            gridControl1.DataSource = DataBaseHelper.GetProducts();
            gridView1.FocusedRowHandle = DevExpress.XtraGrid.GridControl.InvalidRowHandle;
        }

        private void FormProducts_Load(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            string productName = txtProductName.Text.Trim();
            decimal price = txtPrice.Value;
            int stock = (int)txtStock.Value;

            if (string.IsNullOrEmpty(productName) || price < 0 || stock < 0)
            {
                XtraMessageBox.Show("Please enter valid product details!", "Validation Error",MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;

            }

            bool succees = DataBaseHelper.AddProduct(productName, price, stock);

            if (succees)
            {
                XtraMessageBox.Show("Product added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadProducts();
                ClearFields();
            }
            else
            {
                XtraMessageBox.Show("Error adding product. Please try again!","Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearFields()
        {
            txtProductName.Text = "";
            txtPrice.Value = 0;
            txtStock.Value = 0;

        }

        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (gridView1.GetFocusedRowCellValue("ProductID") != null)
            {
                object productNameObj = gridView1.GetFocusedRowCellValue("Name");
                object priceObj = gridView1.GetFocusedRowCellValue("Price");
                object stockObj = gridView1.GetFocusedRowCellValue("Stock");

                txtProductName.Text = productNameObj != null ? productNameObj.ToString() : "";
                txtPrice.Value = priceObj != null ? Convert.ToDecimal(priceObj) : 0;
                txtStock.Value = stockObj != null ? Convert.ToInt32(stockObj) : 0;
            }
        }

        private void btnEditProduct_Click(object sender, EventArgs e)
        {
            if (gridView1.FocusedRowHandle < 0 || gridView1.GetFocusedRowCellValue("ProductID") == null)
            {
                XtraMessageBox.Show("Please select a product to edit!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int productId = Convert.ToInt32(gridView1.GetFocusedRowCellValue("ProductID"));
            string productName = txtProductName.Text.Trim();
            decimal price = txtPrice.Value;
            int stock = (int)txtStock.Value;

            
            if (string.IsNullOrEmpty(productName) || price < 0 || stock < 0)
            {
                XtraMessageBox.Show("Please enter valid product details!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            
            bool success = DataBaseHelper.EditProduct(productId, productName, price, stock);

            if (success)
            {
                XtraMessageBox.Show("Product updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadProducts(); 
                ClearFields();  
            }
            else
            {
                XtraMessageBox.Show("Error updating product. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnDeleteProduct_Click(object sender, EventArgs e)
        {
            int rowHandle = gridView1.FocusedRowHandle;
            if (rowHandle < 0)
            {
                XtraMessageBox.Show("No product selected! Please select a product.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            
            object productIdObj = gridView1.GetFocusedRowCellValue("ProductID");
            if (productIdObj == null)
            {
                XtraMessageBox.Show("Error retrieving product ID!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int productId = Convert.ToInt32(productIdObj);

            
            DialogResult result = XtraMessageBox.Show("Are you sure you want to delete this product?",
                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                bool success = DataBaseHelper.DeleteProduct(productId);
                if (success)
                {
                    XtraMessageBox.Show("Product deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadProducts(); 
                }
                else
                {
                    XtraMessageBox.Show("Error deleting product. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}