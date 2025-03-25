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
using DevExpress.XtraGrid;

namespace ERP_Mini
{
    public partial class FormOrders : DevExpress.XtraEditors.XtraForm
    {
        public FormOrders()
        {
            InitializeComponent();
        }

        private void LoadOrders()
        {
            try
            {
                gridOrders.DataSource = DataBaseHelper.GetOrders();
                gridViewOrders.Columns["OrderID"].AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                gridViewOrders.Columns["CustomerID"].AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                gridViewOrders.Columns["OrderDate"].AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                gridViewOrders.Columns["TotalAmount"].AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far; 
                gridViewOrders.Columns["OrderStatus"].AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Error loading orders: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int currentOrderId = -1;

        private void FormOrders_Load(object sender, EventArgs e)
        {
            LoadOrders();
            LoadCustomers();
            LoadStatuses();
            PopulateProductColumn();
            dgvOrderDetails.CellValueChanged += dgvOrderDetails_CellValueChanged;
            dgvOrderDetails.EditingControlShowing += dgvOrderDetails_EditingControlShowing;
            gridViewOrders.DoubleClick += gridViewOrders_DoubleClick;
            gridViewOrders.FocusedRowChanged += gridViewOrders_FocusedRowChanged;
            dgvOrderDetails.Columns["QuantityColumn"].ValueType = typeof(int);
            dgvOrderDetails.CellContentClick += dgvOrderDetails_CellContentClick;
        }

        private void txtSearchOrders_EditValueChanged(object sender, EventArgs e)
        {
            string filterText = txtSearchOrders.Text.Trim();

            gridViewOrders.ActiveFilter.Clear();

            if (!string.IsNullOrEmpty(filterText))
            {
                gridViewOrders.ActiveFilterString =
                    $"[OrderStatus] LIKE '%{filterText}%' OR " +
                    $"[OrderDate] LIKE '%{filterText}%'";
            }
        }

        private void PopulateProductColumn()
        {
            DataTable products = DataBaseHelper.GetProductListForOrder();


            var productCol = dgvOrderDetails.Columns["ProductColumn"] as DataGridViewComboBoxColumn;

            if (productCol != null)
            {
                productCol.DataSource = products;
                productCol.DisplayMember = "Name";
                productCol.ValueMember = "ProductID";
            }

            dgvOrderDetails.Columns["QuantityColumn"].ValueType = typeof(int);
        }

        private void dgvOrderDetails_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dgvOrderDetails.Rows[e.RowIndex];


                if (e.ColumnIndex == dgvOrderDetails.Columns["ProductColumn"].Index)
                {
                    if (row.Cells["ProductColumn"].Value != null)
                    {
                        int productId = Convert.ToInt32(row.Cells["ProductColumn"].Value);
                        decimal price = DataBaseHelper.GetProductPrice(productId);

                        row.Cells["PriceColumn"].Value = price;
                        row.Cells["QuantityColumn"].Value = 1;
                        row.Cells["SubtotalColumn"].Value = price * 1;
                    }
                }


                if (e.ColumnIndex == dgvOrderDetails.Columns["QuantityColumn"].Index)
                {
                    if (row.Cells["QuantityColumn"].Value != null && row.Cells["PriceColumn"].Value != null)
                    {
                        int qty = Convert.ToInt32(row.Cells["QuantityColumn"].Value);
                        decimal price = Convert.ToDecimal(row.Cells["PriceColumn"].Value);
                        row.Cells["SubtotalColumn"].Value = qty * price;
                    }
                }
            }

            UpdateTotal();
        }

        private void dgvOrderDetails_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dgvOrderDetails.CurrentCell.ColumnIndex == dgvOrderDetails.Columns["QuantityColumn"].Index)
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress -= OnlyAllowDigits;
                    tb.KeyPress += OnlyAllowDigits;
                }
            }
        }
        private void OnlyAllowDigits(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void UpdateTotal()
        {
            decimal total = 0;

            foreach (DataGridViewRow row in dgvOrderDetails.Rows)
            {
                if (row.Cells["SubtotalColumn"].Value != null)
                {
                    total += Convert.ToDecimal(row.Cells["SubtotalColumn"].Value);
                }
            }

            lblTotal.Text = total.ToString("C"); 
        }

        private void ResetOrderForm()
        {
            currentOrderId = -1;
            cmbCustomer.SelectedIndex = -1;
            cmbStatus.SelectedIndex = -1;
            dtOrderDate.Value = DateTime.Now;
            lblTotal0.Text = "$0.00";
            dgvOrderDetails.Rows.Clear();
        }

        private void btnNewOrder_Click(object sender, EventArgs e)
        {
            ResetOrderForm();
        }

        private void btnSaveOrder_Click(object sender, EventArgs e)
        {
            if (cmbCustomer.SelectedIndex == -1 || cmbStatus.SelectedIndex == -1 || dgvOrderDetails.Rows.Count == 0)
            {
                MessageBox.Show("Please fill all required fields and add at least one product.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            
            int customerId = Convert.ToInt32(cmbCustomer.SelectedValue);
            DateTime orderDate = dtOrderDate.Value;
            string status = cmbStatus.SelectedItem.ToString();
            decimal totalAmount = 0;

            List<(int ProductID, int Quantity, decimal Price)> orderItems = new List<(int, int, decimal)>();

            foreach (DataGridViewRow row in dgvOrderDetails.Rows)
            {
                if (row.IsNewRow) continue;

                int productId = Convert.ToInt32(row.Cells["ProductColumn"].Value);
                int quantity = Convert.ToInt32(row.Cells["QuantityColumn"].Value);
                decimal price = Convert.ToDecimal(row.Cells["PriceColumn"].Value);
                totalAmount += quantity * price;

                orderItems.Add((productId, quantity, price));
            }

            
            try
            {
                if (currentOrderId > 0)
                {
                    
                    bool updated = DataBaseHelper.UpdateOrder(currentOrderId, customerId, orderDate, status, totalAmount, orderItems);
                    if (updated)
                        MessageBox.Show($"Order #{currentOrderId} updated successfully!", "Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show("Failed to update the order.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    
                    int newOrderId = DataBaseHelper.SaveOrder(customerId, orderDate, status, totalAmount, orderItems);
                    MessageBox.Show($"Order #{newOrderId} saved successfully!", "Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                ResetOrderForm();
                LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving order:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCustomers()
        {
            DataTable dt = DataBaseHelper.GetCustomers();

            cmbCustomer.DataSource = dt;
            cmbCustomer.DisplayMember = "CustomerName";
            cmbCustomer.ValueMember = "CustomerID";
            cmbCustomer.SelectedIndex = -1;
        }

        private void LoadStatuses()
        {
            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("Pending");
            cmbStatus.Items.Add("Processing");
            cmbStatus.Items.Add("Shipped");
            cmbStatus.Items.Add("Delivered");
            cmbStatus.Items.Add("Cancelled");

            cmbStatus.SelectedIndex = -1; 
        }

        private void btnDeleteOrder_Click(object sender, EventArgs e)
        {
            if (currentOrderId <= 0)
            {
                MessageBox.Show("No order selected for deletion.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this order?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
                return;

            bool success = DataBaseHelper.DeleteOrder(currentOrderId);
            if (success)
            {
                MessageBox.Show("Order deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ResetOrderForm();
                LoadOrders();
                currentOrderId = -1;
            }
            else
            {
                MessageBox.Show("Failed to delete order.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gridViewOrders_DoubleClick(object sender, EventArgs e)
        {
            if (gridViewOrders.FocusedRowHandle >= 0)
            {
                currentOrderId = Convert.ToInt32(gridViewOrders.GetFocusedRowCellValue("OrderID"));
                int customerId = Convert.ToInt32(gridViewOrders.GetFocusedRowCellValue("CustomerID"));
                DateTime orderDate = Convert.ToDateTime(gridViewOrders.GetFocusedRowCellValue("OrderDate"));
                string status = gridViewOrders.GetFocusedRowCellValue("OrderStatus").ToString();

                
                cmbCustomer.SelectedValue = customerId;
                dtOrderDate.Value = orderDate;
                cmbStatus.SelectedItem = status;

                LoadOrderItems(currentOrderId);

                
                xtraTabControl1.SelectedTabPage = xtraTabPage2;
            }
        }

        private void LoadOrderItems(int orderId)
        {
            dgvOrderDetails.Rows.Clear();

            DataTable dt = DataBaseHelper.GetOrderDetails(orderId);

            foreach (DataRow dr in dt.Rows)
            {
                int rowIndex = dgvOrderDetails.Rows.Add();

                dgvOrderDetails.Rows[rowIndex].Cells["ProductColumn"].Value = dr["ProductID"];
                dgvOrderDetails.Rows[rowIndex].Cells["QuantityColumn"].Value = dr["Quantity"];
                dgvOrderDetails.Rows[rowIndex].Cells["PriceColumn"].Value = dr["Price"];

                int qty = Convert.ToInt32(dr["Quantity"]);
                decimal price = Convert.ToDecimal(dr["Price"]);
                dgvOrderDetails.Rows[rowIndex].Cells["SubtotalColumn"].Value = qty * price;
            }

            UpdateTotal();
        }

        private void gridViewOrders_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (gridViewOrders.FocusedRowHandle >= 0)
            {
                currentOrderId = Convert.ToInt32(gridViewOrders.GetRowCellValue(e.FocusedRowHandle, "OrderID"));
                int customerId = Convert.ToInt32(gridViewOrders.GetRowCellValue(e.FocusedRowHandle, "CustomerID"));
                DateTime orderDate = Convert.ToDateTime(gridViewOrders.GetRowCellValue(e.FocusedRowHandle, "OrderDate"));
                string status = gridViewOrders.GetRowCellValue(e.FocusedRowHandle, "OrderStatus").ToString();

                cmbCustomer.SelectedValue = customerId;
                dtOrderDate.Value = orderDate;
                cmbStatus.SelectedItem = status;

                LoadOrderItems(currentOrderId);

                xtraTabControl1.SelectedTabPage = xtraTabPage2;
            }
        }

        private void dgvOrderDetails_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvOrderDetails.Columns[e.ColumnIndex].Name == "DeleteColumn")
            {
                dgvOrderDetails.Rows.RemoveAt(e.RowIndex);
                UpdateTotal(); 
            }
        }
    }    
    }

