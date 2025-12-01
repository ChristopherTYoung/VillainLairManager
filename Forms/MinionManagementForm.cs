using System;
using System.Windows.Forms;
using VillainLairManager.Models;
using VillainLairManager.Utils;

namespace VillainLairManager.Forms
{
    /// <summary>
    /// Minion management form - STUB for students to implement
    /// Should contain CRUD operations with business logic in event handlers
    /// </summary>
    public partial class MinionManagementForm : Form
    {
        private DataGridView dgvMinions;
        private TextBox txtName, txtSkillLevel, txtSalary;
        private ComboBox cboSpecialty, cboBase, cboScheme;
        private Button btnAdd, btnUpdate, btnDelete, btnRefresh;
        public MinionManagementForm()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Validation logic directly in event handler (anti-pattern)
            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Name is required!");
                return;
            }

            // Hardcoded specialty validation (duplicates ValidationHelper)
            string specialty = cboSpecialty.SelectedItem?.ToString();
            if (!ValidationHelper.IsValidSpecialty(specialty)) // unfinished conditional for specialty
            {

                MessageBox.Show("Invalid specialty!");
                return;
            }

            // Direct database call from UI (anti-pattern)
            var minion = new Minion
            {
                Name = txtName.Text,
                SkillLevel = int.Parse(txtSkillLevel.Text),
                Specialty = specialty,
                LoyaltyScore = 50, // Hardcoded default
                SalaryDemand = decimal.Parse(txtSalary.Text),
                MoodStatus = "Grumpy",
                LastMoodUpdate = DateTime.Now
            };

            DatabaseHelper.InsertMinion(minion);
            RefreshGrid();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Name is required!");
                return;
            }



        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshGrid();
        }
        private void RefreshGrid()
        {
            // Direct database call
            dgvMinions.DataSource = null;
            dgvMinions.DataSource = DatabaseHelper.GetAllMinions();
        }

        private void InitializeComponent()
        {
            this.Text = "Minion Management";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var lblStub = new Label
            {
                Text = "TODO: Implement Minion Management Form\n\n" +
                       "Requirements:\n" +
                       "- DataGridView showing all minions\n" +
                       "- Text boxes for: Name, Specialty, Skill Level, Salary\n" +
                       "- ComboBox for Base assignment\n" +
                       "- ComboBox for Scheme assignment\n" +
                       "- Buttons: Add, Update, Delete, Refresh\n" +
                       "- All validation logic in button click handlers (anti-pattern)\n" +
                       "- Direct database calls from event handlers (anti-pattern)\n" +
                       "- Loyalty calculation duplicated here (anti-pattern)",
                Location = new System.Drawing.Point(50, 50),
                Size = new System.Drawing.Size(800, 400),
                Font = new System.Drawing.Font("Arial", 10)
            };
            txtName = new TextBox();
            txtSkillLevel = new TextBox();
            txtSalary = new TextBox();

            this.Controls.Add(txtName);
            this.Controls.Add(txtSkillLevel);
            this.Controls.Add(txtSalary);
            this.Controls.Add(lblStub);
        }
    }
}
