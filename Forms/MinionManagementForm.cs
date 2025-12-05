using System;
using System.Linq;
using System.Windows.Forms;
using VillainLairManager.Models;
using VillainLairManager.Utils;

namespace VillainLairManager.Forms
{
    /// <summary>
    /// Minion management form with business logic in event handlers (anti-pattern)
    /// Contains CRUD operations with validation and business rules mixed into UI
    /// </summary>
    public partial class MinionManagementForm : Form
    {
        private DataGridView dgvMinions;
        private TextBox txtName, txtSkillLevel, txtSalary, txtLoyalty;
        private ComboBox cboSpecialty, cboBase, cboScheme, cboMood;
        private Button btnAdd, btnUpdate, btnDelete, btnRefresh;
        private Label lblName, lblSkillLevel, lblSpecialty, lblSalary, lblLoyalty, lblMood, lblBase, lblScheme;

        public MinionManagementForm()
        {
            InitializeComponent();
            LoadComboBoxData();
            RefreshGrid();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Validation logic directly in event handler (anti-pattern)
            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Name is required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Hardcoded specialty validation (duplicates ValidationHelper)
            string specialty = cboSpecialty.SelectedItem?.ToString();
            if (specialty != "Hacking" && specialty != "Explosives" && 
                specialty != "Disguise" && specialty != "Combat" && 
                specialty != "Engineering" && specialty != "Piloting")
            {
                MessageBox.Show("Invalid specialty! Must be one of: Hacking, Explosives, Disguise, Combat, Engineering, Piloting", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Parse and validate skill level (duplicates ValidationHelper logic)
            if (!int.TryParse(txtSkillLevel.Text, out int skillLevel))
            {
                MessageBox.Show("Skill level must be a number!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (skillLevel < 1 || skillLevel > 10)
            {
                MessageBox.Show("Skill level must be between 1 and 10!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Parse and validate salary
            if (!decimal.TryParse(txtSalary.Text, out decimal salary))
            {
                MessageBox.Show("Salary must be a valid number!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (salary < 0)
            {
                MessageBox.Show("Salary cannot be negative!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Parse loyalty with hardcoded validation (anti-pattern)
            if (!int.TryParse(txtLoyalty.Text, out int loyalty))
            {
                loyalty = 50; // Hardcoded default
            }

            if (loyalty < 0 || loyalty > 100)
            {
                MessageBox.Show("Loyalty must be between 0 and 100!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Business logic for mood determination (duplicated from Minion model - anti-pattern)
            string mood = cboMood.SelectedItem?.ToString() ?? "Grumpy";
            if (string.IsNullOrEmpty(mood))
            {
                if (loyalty > 70)
                    mood = "Happy";
                else if (loyalty < 40)
                    mood = "Plotting Betrayal";
                else
                    mood = "Grumpy";
            }

            // Get base and scheme assignments
            int? baseId = null;
            int? schemeId = null;

            if (cboBase.SelectedItem != null && cboBase.SelectedIndex > 0)
            {
                var baseItem = cboBase.SelectedItem.ToString();
                baseId = int.Parse(baseItem.Split(':')[0].Trim());
            }

            if (cboScheme.SelectedItem != null && cboScheme.SelectedIndex > 0)
            {
                var schemeItem = cboScheme.SelectedItem.ToString();
                schemeId = int.Parse(schemeItem.Split(':')[0].Trim());
            }

            // Direct database call from UI (anti-pattern)
            var minion = new Minion
            {
                Name = txtName.Text,
                SkillLevel = skillLevel,
                Specialty = specialty,
                LoyaltyScore = loyalty,
                SalaryDemand = salary,
                CurrentBaseId = baseId,
                CurrentSchemeId = schemeId,
                MoodStatus = mood,
                LastMoodUpdate = DateTime.Now
            };

            try
            {
                DatabaseHelper.InsertMinion(minion);
                MessageBox.Show("Minion added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshGrid();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding minion: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Check if a minion is selected
            if (dgvMinions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a minion to update!", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validation logic (duplicated from btnAdd_Click - anti-pattern)
            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Name is required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Hardcoded specialty validation again (more duplication)
            string specialty = cboSpecialty.SelectedItem?.ToString();
            if (specialty != "Hacking" && specialty != "Explosives" && 
                specialty != "Disguise" && specialty != "Combat" && 
                specialty != "Engineering" && specialty != "Piloting")
            {
                MessageBox.Show("Invalid specialty!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Parse skill level with inline validation (duplicated)
            if (!int.TryParse(txtSkillLevel.Text, out int skillLevel) || skillLevel < 1 || skillLevel > 10)
            {
                MessageBox.Show("Skill level must be between 1 and 10!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Parse salary with inline validation (duplicated)
            if (!decimal.TryParse(txtSalary.Text, out decimal salary) || salary < 0)
            {
                MessageBox.Show("Salary must be a positive number!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Parse loyalty with inline validation (duplicated)
            if (!int.TryParse(txtLoyalty.Text, out int loyalty) || loyalty < 0 || loyalty > 100)
            {
                MessageBox.Show("Loyalty must be between 0 and 100!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Business logic for mood (duplicated again - anti-pattern)
            string mood = cboMood.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(mood))
            {
                // Hardcoded business rule (duplicates Minion.UpdateMood)
                if (loyalty > 70)
                    mood = "Happy";
                else if (loyalty < 40)
                    mood = "Plotting Betrayal";
                else
                    mood = "Grumpy";
            }

            // Get base and scheme assignments
            int? baseId = null;
            int? schemeId = null;

            if (cboBase.SelectedItem != null && cboBase.SelectedIndex > 0)
            {
                var baseItem = cboBase.SelectedItem.ToString();
                baseId = int.Parse(baseItem.Split(':')[0].Trim());
            }

            if (cboScheme.SelectedItem != null && cboScheme.SelectedIndex > 0)
            {
                var schemeItem = cboScheme.SelectedItem.ToString();
                schemeId = int.Parse(schemeItem.Split(':')[0].Trim());
            }

            // Get minion ID from selected row
            int minionId = (int)dgvMinions.SelectedRows[0].Cells["MinionId"].Value;

            // Direct database call from UI (anti-pattern)
            var minion = new Minion
            {
                MinionId = minionId,
                Name = txtName.Text,
                SkillLevel = skillLevel,
                Specialty = specialty,
                LoyaltyScore = loyalty,
                SalaryDemand = salary,
                CurrentBaseId = baseId,
                CurrentSchemeId = schemeId,
                MoodStatus = mood,
                LastMoodUpdate = DateTime.Now
            };

            try
            {
                DatabaseHelper.UpdateMinion(minion);
                MessageBox.Show("Minion updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshGrid();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating minion: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Check if a minion is selected
            if (dgvMinions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a minion to delete!", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get minion details for confirmation
            string minionName = dgvMinions.SelectedRows[0].Cells["Name"].Value.ToString();
            int minionId = (int)dgvMinions.SelectedRows[0].Cells["MinionId"].Value;

            // Confirmation dialog
            var result = MessageBox.Show(
                $"Are you sure you want to delete minion '{minionName}'? This action cannot be undone!",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // Direct database call from UI (anti-pattern)
                    DatabaseHelper.DeleteMinion(minionId);
                    MessageBox.Show("Minion deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshGrid();
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting minion: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshGrid();
            LoadComboBoxData();
            MessageBox.Show("Data refreshed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RefreshGrid()
        {
            // Direct database call (anti-pattern)
            dgvMinions.DataSource = null;
            dgvMinions.DataSource = DatabaseHelper.GetAllMinions();

            // Format grid columns
            if (dgvMinions.Columns.Count > 0)
            {
                dgvMinions.Columns["MinionId"].HeaderText = "ID";
                dgvMinions.Columns["Name"].HeaderText = "Name";
                dgvMinions.Columns["SkillLevel"].HeaderText = "Skill";
                dgvMinions.Columns["Specialty"].HeaderText = "Specialty";
                dgvMinions.Columns["LoyaltyScore"].HeaderText = "Loyalty";
                dgvMinions.Columns["SalaryDemand"].HeaderText = "Salary Demand";
                dgvMinions.Columns["CurrentBaseId"].HeaderText = "Base ID";
                dgvMinions.Columns["CurrentSchemeId"].HeaderText = "Scheme ID";
                dgvMinions.Columns["MoodStatus"].HeaderText = "Mood";
                dgvMinions.Columns["LastMoodUpdate"].HeaderText = "Last Updated";

                dgvMinions.Columns["SalaryDemand"].DefaultCellStyle.Format = "C";
            }
        }

        private void LoadComboBoxData()
        {
            // Direct database calls from UI (anti-pattern)
            // Hardcoded specialty list (duplicates ValidationHelper and Minion model)
            cboSpecialty.Items.Clear();
            cboSpecialty.Items.AddRange(new object[] { "Hacking", "Explosives", "Disguise", "Combat", "Engineering", "Piloting" });

            // Hardcoded mood list (another anti-pattern)
            cboMood.Items.Clear();
            cboMood.Items.AddRange(new object[] { "Happy", "Grumpy", "Plotting Betrayal", "Exhausted" });

            // Load bases from database
            cboBase.Items.Clear();
            cboBase.Items.Add("(None)");
            var bases = DatabaseHelper.GetAllBases();
            foreach (var b in bases)
            {
                cboBase.Items.Add($"{b.BaseId}: {b.Name}");
            }

            // Load schemes from database
            cboScheme.Items.Clear();
            cboScheme.Items.Add("(None)");
            var schemes = DatabaseHelper.GetAllSchemes();
            foreach (var s in schemes)
            {
                cboScheme.Items.Add($"{s.SchemeId}: {s.Name}");
            }
        }

        private void dgvMinions_SelectionChanged(object sender, EventArgs e)
        {
            // Load selected minion into form fields
            if (dgvMinions.SelectedRows.Count > 0)
            {
                var row = dgvMinions.SelectedRows[0];
                txtName.Text = row.Cells["Name"].Value?.ToString();
                txtSkillLevel.Text = row.Cells["SkillLevel"].Value?.ToString();
                txtSalary.Text = row.Cells["SalaryDemand"].Value?.ToString();
                txtLoyalty.Text = row.Cells["LoyaltyScore"].Value?.ToString();

                // Set specialty combo box
                string specialty = row.Cells["Specialty"].Value?.ToString();
                cboSpecialty.SelectedItem = specialty;

                // Set mood combo box
                string mood = row.Cells["MoodStatus"].Value?.ToString();
                cboMood.SelectedItem = mood;

                // Set base combo box
                var baseId = row.Cells["CurrentBaseId"].Value;
                if (baseId != null && baseId != DBNull.Value)
                {
                    for (int i = 0; i < cboBase.Items.Count; i++)
                    {
                        if (cboBase.Items[i].ToString().StartsWith(baseId.ToString() + ":"))
                        {
                            cboBase.SelectedIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    cboBase.SelectedIndex = 0;
                }

                // Set scheme combo box
                var schemeId = row.Cells["CurrentSchemeId"].Value;
                if (schemeId != null && schemeId != DBNull.Value)
                {
                    for (int i = 0; i < cboScheme.Items.Count; i++)
                    {
                        if (cboScheme.Items[i].ToString().StartsWith(schemeId.ToString() + ":"))
                        {
                            cboScheme.SelectedIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    cboScheme.SelectedIndex = 0;
                }
            }
        }

        private void ClearFields()
        {
            txtName.Clear();
            txtSkillLevel.Clear();
            txtSalary.Clear();
            txtLoyalty.Text = "50"; // Hardcoded default
            cboSpecialty.SelectedIndex = -1;
            cboMood.SelectedIndex = -1;
            cboBase.SelectedIndex = 0;
            cboScheme.SelectedIndex = 0;
            dgvMinions.ClearSelection();
        }

        private void InitializeComponent()
        {
            this.Text = "Minion Management";
            this.Size = new System.Drawing.Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // DataGridView
            dgvMinions = new DataGridView
            {
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(900, 400),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            txtName = new TextBox();
            txtSkillLevel = new TextBox();
            txtSalary = new TextBox();
            //var button = new Button() { OnClick(btnAdd_Click()); };

            // Add all controls
            this.Controls.Add(dgvMinions);
            this.Controls.Add(lblName);
            this.Controls.Add(txtName);
            this.Controls.Add(lblSkillLevel);
            this.Controls.Add(txtSkillLevel);
            this.Controls.Add(lblSpecialty);
            this.Controls.Add(cboSpecialty);
            this.Controls.Add(lblSalary);
            this.Controls.Add(txtSalary);
            this.Controls.Add(lblLoyalty);
            this.Controls.Add(txtLoyalty);
            this.Controls.Add(lblMood);
            this.Controls.Add(cboMood);
            this.Controls.Add(lblBase);
            this.Controls.Add(cboBase);
            this.Controls.Add(lblScheme);
            this.Controls.Add(cboScheme);
            this.Controls.Add(btnAdd);
            this.Controls.Add(btnUpdate);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnRefresh);
        }
    }
}
