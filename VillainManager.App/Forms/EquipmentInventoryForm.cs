using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using VillainLairManager.Models;
using VillainLairManager.Repositories;
using VillainLairManager.Services;

namespace VillainLairManager.Forms
{
    /// <summary>
    /// Equipment inventory form using service layer for business logic
    /// </summary>
    public partial class EquipmentInventoryForm : Form
    {
        private DataGridView dgvEquipment;
        private Button btnAdd, btnEdit, btnDelete, btnMaintain, btnAssign;
        private BindingList<Equipment> equipmentBinding;
        private System.Windows.Forms.Timer degradeTimer;
        private readonly IEquipmentService _equipmentService;

        public EquipmentInventoryForm(IEquipmentService equipmentService)
        {
            _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            lblStub = new Label();
            dgvEquipment = new DataGridView();
            btnAdd = new Button();
            btnEdit = new Button();
            btnDelete = new Button();
            btnMaintain = new Button();
            btnAssign = new Button();

            SuspendLayout();

            // lblStub
            lblStub.Location = new Point(0, 0);
            lblStub.Name = "lblStub";
            lblStub.Size = new Size(100, 23);
            lblStub.TabIndex = 0;

            // dgvEquipment
            dgvEquipment.Location = new Point(12, 40);
            dgvEquipment.Size = new Size(600, 400);
            dgvEquipment.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvEquipment.ReadOnly = true;
            dgvEquipment.AutoGenerateColumns = true;
            dgvEquipment.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // <-- Add this line
            dgvEquipment.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom; // <-- Add this line


            // btnAdd
            btnAdd.Text = "Add";
            btnAdd.Location = new Point(630, 40);
            btnAdd.Size = new Size(120, 30);
            btnAdd.Click += BtnAdd_Click;

            // btnEdit
            btnEdit.Text = "Edit";
            btnEdit.Location = new Point(630, 80);
            btnEdit.Size = new Size(120, 30);
            btnEdit.Click += BtnEdit_Click;

            // btnDelete
            btnDelete.Text = "Delete";
            btnDelete.Location = new Point(630, 120);
            btnDelete.Size = new Size(120, 30);
            btnDelete.Click += BtnDelete_Click;

            // btnMaintain
            btnMaintain.Text = "Maintain";
            btnMaintain.Location = new Point(630, 160);
            btnMaintain.Size = new Size(120, 30);
            btnMaintain.Click += BtnMaintain_Click;

            // btnAssign
            btnAssign.Text = "Assign to Scheme";
            btnAssign.Location = new Point(630, 200);
            btnAssign.Size = new Size(120, 30);
            btnAssign.Click += BtnAssign_Click;

            // EquipmentInventoryForm
            ClientSize = new Size(884, 561);
            Controls.Add(lblStub);
            Controls.Add(dgvEquipment);
            Controls.Add(btnAdd);
            Controls.Add(btnEdit);
            Controls.Add(btnDelete);
            Controls.Add(btnMaintain);
            Controls.Add(btnAssign);
            Name = "EquipmentInventoryForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Equipment Inventory";
            Load += EquipmentInventoryForm_Load;
            ResumeLayout(false);
        }

        private void EquipmentInventoryForm_Load(object sender, EventArgs e)
        {
            // Load equipment using service
            var equipmentList = _equipmentService.GetAllEquipment();
            equipmentBinding = new BindingList<Equipment>(equipmentList);
            dgvEquipment.DataSource = equipmentBinding;

            // Timer for condition degradation - using service for business logic
            degradeTimer = new System.Windows.Forms.Timer { Interval = 60000 }; // 1 minute
            degradeTimer.Tick += (s, args) =>
            {
                foreach (var eq in equipmentBinding)
                {
                    _equipmentService.DegradeCondition(eq);
                }
                equipmentBinding.ResetBindings();
            };
            degradeTimer.Start();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // Use service to create new equipment with default values
            var result = _equipmentService.CreateEquipment(
                "New Equipment",
                "Gadget",
                0,
                0,
                false,
                null);

            if (result.success)
            {
                equipmentBinding.Add(_equipmentService.GetAllEquipment()[^1]);
            }
            else
            {
                ShowError(result.message);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            var eq = GetSelectedEquipment();
            if (eq == null) return;

            var controls = CreateEditDialog(eq);
            using var form = CreateDialogForm("Edit Equipment", controls.form, controls.btnOk, controls.btnCancel);

            if (form.ShowDialog() == DialogResult.OK)
            {
                if (!TryParseEquipmentInput(controls.txtName, controls.txtCategory, controls.txtPurchasePrice,
                    controls.txtMaintenanceCost, controls.txtStoredAtBaseId, out var parsed))
                {
                    MessageBox.Show("Invalid input. Please check your entries.", "Edit Equipment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                eq.Name = parsed.name;
                eq.Category = parsed.category;
                eq.PurchasePrice = parsed.purchasePrice;
                eq.MaintenanceCost = parsed.maintenanceCost;
                eq.RequiresSpecialist = controls.chkRequiresSpecialist.Checked;
                eq.StoredAtBaseId = parsed.storedAtBaseId;

                var result = _equipmentService.UpdateEquipment(eq);
                if (!result.success)
                {
                    ShowError(result.message);
                }
                equipmentBinding.ResetBindings();
            }
        }




        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var eq = GetSelectedEquipment();
            if (eq == null) return;

            var result = _equipmentService.DeleteEquipment(eq.EquipmentId);
            if (result.success)
            {
                equipmentBinding.Remove(eq);
            }
            else
            {
                ShowError(result.message);
            }
        }

        private void BtnMaintain_Click(object sender, EventArgs e)
        {
            var eq = GetSelectedEquipment();
            if (eq == null) return;

            var result = _equipmentService.PerformMaintenance(eq);
            ShowOperationResult(result.success, result.message, "Maintenance Complete");
            equipmentBinding.ResetBindings();
        }

        private void BtnAssign_Click(object sender, EventArgs e)
        {
            var eq = GetSelectedEquipment();
            if (eq == null) return;

            var schemeIdStr = Prompt("Assign to Scheme (ID):", eq.AssignedToSchemeId?.ToString() ?? "");
            if (int.TryParse(schemeIdStr, out int schemeId))
            {
                var result = _equipmentService.AssignToScheme(eq, schemeId);
                ShowOperationResult(result.success, result.message, "Success");
                equipmentBinding.ResetBindings();
            }
        }

        private static string Prompt(string text, string defaultValue)
        {
            using var form = new Form();
            var label = new Label { Left = 10, Top = 10, Text = text, AutoSize = true };
            var textBox = new TextBox { Left = 10, Top = 35, Width = 260, Text = defaultValue };
            var buttonOk = new Button { Text = "OK", Left = 110, Width = 80, Top = 65, DialogResult = DialogResult.OK };
            form.ClientSize = new Size(280, 100);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk });
            form.AcceptButton = buttonOk;
            return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }

        /// <summary>
        /// Helper method to get selected equipment from grid
        /// </summary>
        private Equipment GetSelectedEquipment()
        {
            return dgvEquipment.CurrentRow?.DataBoundItem as Equipment;
        }

        /// <summary>
        /// Helper method to show error messages
        /// </summary>
        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Helper method to show operation results with custom success title
        /// </summary>
        private void ShowOperationResult(bool success, string message, string successTitle)
        {
            if (success)
            {
                MessageBox.Show(message, successTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                ShowError(message);
            }
        }

        /// <summary>
        /// Creates a dialog form with standard settings
        /// </summary>
        private Form CreateDialogForm(string title, Control[] controls, Button okButton, Button cancelButton)
        {
            var form = new Form
            {
                Text = title,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(320, 270),
                MaximizeBox = false,
                MinimizeBox = false,
                AcceptButton = okButton,
                CancelButton = cancelButton
            };
            form.Controls.AddRange(controls);
            return form;
        }

        /// <summary>
        /// Creates edit dialog controls for equipment
        /// </summary>
        private (Control[] form, TextBox txtName, TextBox txtCategory, TextBox txtPurchasePrice, 
                 TextBox txtMaintenanceCost, CheckBox chkRequiresSpecialist, TextBox txtStoredAtBaseId, 
                 Button btnOk, Button btnCancel) CreateEditDialog(Equipment eq)
        {
            var lblName = new Label { Text = "Name:", Left = 10, Top = 20, Width = 100 };
            var txtName = new TextBox { Left = 120, Top = 18, Width = 180, Text = eq.Name };

            var lblCategory = new Label { Text = "Category:", Left = 10, Top = 55, Width = 100 };
            var txtCategory = new TextBox { Left = 120, Top = 53, Width = 180, Text = eq.Category };

            var lblPurchasePrice = new Label { Text = "Purchase Price:", Left = 10, Top = 90, Width = 100 };
            var txtPurchasePrice = new TextBox { Left = 120, Top = 88, Width = 180, Text = eq.PurchasePrice.ToString() };

            var lblMaintenanceCost = new Label { Text = "Maintenance Cost:", Left = 10, Top = 125, Width = 100 };
            var txtMaintenanceCost = new TextBox { Left = 120, Top = 123, Width = 180, Text = eq.MaintenanceCost.ToString() };

            var lblRequiresSpecialist = new Label { Text = "Requires Specialist:", Left = 10, Top = 160, Width = 100 };
            var chkRequiresSpecialist = new CheckBox { Left = 120, Top = 158, Width = 20, Checked = eq.RequiresSpecialist };

            var lblStoredAtBaseId = new Label { Text = "Stored At Base Id:", Left = 10, Top = 195, Width = 100 };
            var txtStoredAtBaseId = new TextBox { Left = 120, Top = 193, Width = 180, Text = eq.StoredAtBaseId?.ToString() ?? "" };

            var btnOk = new Button { Text = "OK", Left = 120, Width = 80, Top = 230, DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Cancel", Left = 220, Width = 80, Top = 230, DialogResult = DialogResult.Cancel };

            var controls = new Control[] {
                lblName, txtName,
                lblCategory, txtCategory,
                lblPurchasePrice, txtPurchasePrice,
                lblMaintenanceCost, txtMaintenanceCost,
                lblRequiresSpecialist, chkRequiresSpecialist,
                lblStoredAtBaseId, txtStoredAtBaseId,
                btnOk, btnCancel
            };

            return (controls, txtName, txtCategory, txtPurchasePrice, txtMaintenanceCost, 
                    chkRequiresSpecialist, txtStoredAtBaseId, btnOk, btnCancel);
        }

        /// <summary>
        /// Parses and validates equipment input fields
        /// </summary>
        private bool TryParseEquipmentInput(TextBox txtName, TextBox txtCategory, TextBox txtPurchasePrice,
            TextBox txtMaintenanceCost, TextBox txtStoredAtBaseId,
            out (string name, string category, decimal purchasePrice, decimal maintenanceCost, int? storedAtBaseId) parsed)
        {
            parsed = default;

            if (string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtCategory.Text) ||
                !decimal.TryParse(txtPurchasePrice.Text, out decimal purchasePrice) ||
                !decimal.TryParse(txtMaintenanceCost.Text, out decimal maintenanceCost) ||
                (!string.IsNullOrWhiteSpace(txtStoredAtBaseId.Text) && !int.TryParse(txtStoredAtBaseId.Text, out _)))
            {
                return false;
            }

            parsed = (
                txtName.Text,
                txtCategory.Text,
                purchasePrice,
                maintenanceCost,
                string.IsNullOrWhiteSpace(txtStoredAtBaseId.Text) ? null : int.Parse(txtStoredAtBaseId.Text)
            );

            return true;
        }

        private Label lblStub;
    }
}
