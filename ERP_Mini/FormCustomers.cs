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
    public partial class FormCustomers : DevExpress.XtraEditors.XtraForm
    {
        public FormCustomers()
        {
            InitializeComponent();
        }

        private void LoadCustomers()
        {
            try
            {
                gridControl1.DataSource = DataBaseHelper.GetCustomers();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Error loading customers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormCustomers_Load(object sender, EventArgs e)
        {
            LoadCustomers();

            ClearCustomerDetails(); 
            
        }

      
        

        private void btnAddCustomer_Click(object sender, EventArgs e)
        {
            string name = txtCustomerName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                XtraMessageBox.Show("Please enter the customer's name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }



            bool success = DataBaseHelper.AddCustomer(name, email, phone);
            if (success)
            {
                XtraMessageBox.Show("Customer added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadCustomers();
                gridView1.FocusedRowHandle = gridView1.RowCount - 1;
                
                ClearCustomerDetails();
            }
            else
            {
                XtraMessageBox.Show("Error adding customer. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearCustomerDetails()
        {
            txtCustomerID.Text = "";
            txtCustomerName.Text = "";
            txtEmail.Text = "";
            txtPhone.Text = "";
            lblSelectedCustomer.Text = "No Customer Selected";
        }

     

        private void btnNewCustomer_Click(object sender, EventArgs e)
        {
            
            gridView1.ClearSelection();
            ClearCustomerDetails();
        }

        

        private void btnEditCustomer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCustomerID.Text))
            {
                XtraMessageBox.Show("Please select a customer to edit.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            
            int id = int.Parse(txtCustomerID.Text);
            string name = txtCustomerName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();

            
            if (string.IsNullOrEmpty(name))
            {
                XtraMessageBox.Show("Customer name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            
            bool success = DataBaseHelper.UpdateCustomer(id, name, email, phone);
            if (success)
            {
                XtraMessageBox.Show("Customer updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadCustomers(); 
                ClearCustomerDetails(); 
            }
            else
            {
                XtraMessageBox.Show("Update failed. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSelectedCustomerFromGrid()
        {
            int rowHandle = gridView1.FocusedRowHandle;

            if (rowHandle >= 0 && !gridView1.IsGroupRow(rowHandle))
            {
                txtCustomerID.Text = gridView1.GetRowCellValue(rowHandle, "CustomerID")?.ToString();
                txtCustomerName.Text = gridView1.GetRowCellValue(rowHandle, "CustomerName")?.ToString();
                txtEmail.Text = gridView1.GetRowCellValue(rowHandle, "Email")?.ToString();
                txtPhone.Text = gridView1.GetRowCellValue(rowHandle, "Phone")?.ToString();

                lblSelectedCustomer.Text = $"Selected Customer: {txtCustomerName.Text}";
            }
            else
            {
                ClearCustomerDetails();
            }
        }

        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (gridView1.FocusedRowHandle >= 0)
            {
                LoadSelectedCustomerFromGrid();
            }
        }

        private void gridView1_Click(object sender, EventArgs e)
        {
            LoadSelectedCustomerFromGrid();
        }

        private void btnDeleteCustomer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCustomerID.Text))
            {
                XtraMessageBox.Show("Please select a customer to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = XtraMessageBox.Show("Are you sure you want to delete this customer?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
                return;

            int id = int.Parse(txtCustomerID.Text);

            bool success = DataBaseHelper.DeleteCustomer(id);
            if (success)
            {
                XtraMessageBox.Show("Customer deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadCustomers();
                ClearCustomerDetails();
            }
            else
            {
                XtraMessageBox.Show("Error deleting customer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textEdit1_EditValueChanged(object sender, EventArgs e)
        {
            string filterText = textEdit1.Text.Trim();

            if (string.IsNullOrWhiteSpace(filterText))
            {
                gridControl1.DataSource = DataBaseHelper.GetCustomers(); // Afișează toți
            }
            else
            {
                DataTable allCustomers = DataBaseHelper.GetCustomers();

                DataView view = new DataView(allCustomers);
                view.RowFilter = $"CustomerName LIKE '%{filterText}%' OR Email LIKE '%{filterText}%' OR Phone LIKE '%{filterText}%'";

                gridControl1.DataSource = view;
            }
        }
    }
}