using System;
using System.Linq;
using System.Windows.Forms;
using VillainLairManager.Models;
using VillainLairManager.Utils;
using VillainLairManager.Repositories;

namespace VillainLairManager.Forms
{
    /// <summary>
    /// Minion management form with dependency injection
    /// </summary>
    public partial class MinionManagementForm : Form
    {
        private DataGridView dgvMinions;
        private TextBox txtName, txtSkillLevel, txtSalary, txtLoyalty;
        private ComboBox cboSpecialty, cboBase, cboScheme, cboMood;
        private Button btnAdd, btnUpdate, btnDelete, btnRefresh;
        private Label lblName, lblSkillLevel, lblSpecialty, lblSalary, lblLoyalty, lblMood, lblBase, lblScheme;
        private ConfigManager _config = ConfigManager.Instance;
        private readonly IMinionRepository _minionRepository;
        private readonly ISecretBaseRepository _baseRepository;
        private readonly IEvilSchemeRepository _schemeRepository;

        public MinionManagementForm(IMinionRepository minionRepository, ISecretBaseRepository baseRepository, IEvilSchemeRepository schemeRepository)
        {
            _minionRepository = minionRepository ?? throw new ArgumentNullException(nameof(minionRepository));
            _baseRepository = baseRepository ?? throw new ArgumentNullException(nameof(baseRepository));
            _schemeRepository = schemeRepository ?? throw new ArgumentNullException(nameof(schemeRepository));
            
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

            // Validate specialty using ValidationHelper and configuration
            string specialty = cboSpecialty.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(specialty) || !ValidationHelper.IsValidSpecialty(specialty))
            {
                MessageBox.Show($"Invalid specialty! Must be one of: {string.Join(", ", _config.ValidSpecialties)}", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Parse and validate skill level using ValidationHelper
            if (!int.TryParse(txtSkillLevel.Text, out int skillLevel))
            {
                MessageBox.Show("Skill level must be a number!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidationHelper.IsValidSkillLevel(skillLevel))
            {
                MessageBox.Show($"Skill level must be between {_config.SkillLevelRange.Min} and {_config.SkillLevelRange.Max}!", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            // Parse loyalty using configuration default
            if (!int.TryParse(txtLoyalty.Text, out int loyalty))
            {
                loyalty = _config.DefaultLoyaltyScore;
            }

            if (!ValidationHelper.IsValidLoyalty(loyalty))
            {
                MessageBox.Show($"Loyalty must be between {_config.LoyaltyScoreRange.Min} and {_config.LoyaltyScoreRange.Max}!", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Business logic for mood determination using configuration thresholds
            string mood = cboMood.SelectedItem?.ToString() ?? _config.DefaultMoodStatus;
            if (string.IsNullOrEmpty(mood))
            {
                if (loyalty > _config.HighLoyaltyThreshold)
                    mood = _config.MoodHappy;
                else if (loyalty < _config.LowLoyaltyThreshold)
                    mood = _config.MoodBetrayal;
                else
                    mood = _config.MoodGrumpy;
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
                _minionRepository.Insert(minion);
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

            // Validate specialty using ValidationHelper
            string specialty = cboSpecialty.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(specialty) || !ValidationHelper.IsValidSpecialty(specialty))
            {
                MessageBox.Show($"Invalid specialty! Must be one of: {string.Join(", ", _config.ValidSpecialties)}", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Parse skill level with ValidationHelper
            if (!int.TryParse(txtSkillLevel.Text, out int skillLevel) || !ValidationHelper.IsValidSkillLevel(skillLevel))
            {
                MessageBox.Show($"Skill level must be between {_config.SkillLevelRange.Min} and {_config.SkillLevelRange.Max}!", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Parse salary with inline validation (duplicated)
            if (!decimal.TryParse(txtSalary.Text, out decimal salary) || salary < 0)
            {
                MessageBox.Show("Salary must be a positive number!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Parse loyalty with ValidationHelper
            if (!int.TryParse(txtLoyalty.Text, out int loyalty) || !ValidationHelper.IsValidLoyalty(loyalty))
            {
                MessageBox.Show($"Loyalty must be between {_config.LoyaltyScoreRange.Min} and {_config.LoyaltyScoreRange.Max}!", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Business logic for mood using configuration
            string mood = cboMood.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(mood))
            {
                if (loyalty > _config.HighLoyaltyThreshold)
                    mood = _config.MoodHappy;
                else if (loyalty < _config.LowLoyaltyThreshold)
                    mood = _config.MoodBetrayal;
                else
                    mood = _config.MoodGrumpy;
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
                _minionRepository.Update(minion);
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
                    _minionRepository.Delete(minionId);
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
            dgvMinions.DataSource = null;
            dgvMinions.DataSource = _minionRepository.GetAll();

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
            // Load specialty list from configuration
            cboSpecialty.Items.Clear();
            cboSpecialty.Items.AddRange(_config.ValidSpecialties.Cast<object>().ToArray());

            // Load mood list from configuration
            cboMood.Items.Clear();
            cboMood.Items.AddRange(_config.ValidMoodStatuses.Cast<object>().ToArray());

            // Load bases from database
            cboBase.Items.Clear();
            cboBase.Items.Add("(None)");
            var bases = _baseRepository.GetAll();
            foreach (var b in bases)
            {
                cboBase.Items.Add($"{b.BaseId}: {b.Name}");
            }

            // Load schemes from database
            cboScheme.Items.Clear();
            cboScheme.Items.Add("(None)");
            var schemes = _schemeRepository.GetAll();
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
            txtLoyalty.Text = _config.DefaultLoyaltyScore.ToString();
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
            dgvMinions.SelectionChanged += dgvMinions_SelectionChanged;

            // Labels and TextBoxes
            int labelX = 20;
            int controlX = 150;
            int startY = 440;
            int rowHeight = 35;

            lblName = new Label { Text = "Name:", Location = new System.Drawing.Point(labelX, startY), Size = new System.Drawing.Size(120, 20) };
            txtName = new TextBox { Location = new System.Drawing.Point(controlX, startY), Size = new System.Drawing.Size(200, 20) };

            lblSkillLevel = new Label { Text = "Skill Level (1-10):", Location = new System.Drawing.Point(labelX, startY + rowHeight), Size = new System.Drawing.Size(120, 20) };
            txtSkillLevel = new TextBox { Location = new System.Drawing.Point(controlX, startY + rowHeight), Size = new System.Drawing.Size(200, 20) };

            lblSpecialty = new Label { Text = "Specialty:", Location = new System.Drawing.Point(labelX, startY + rowHeight * 2), Size = new System.Drawing.Size(120, 20) };
            cboSpecialty = new ComboBox { Location = new System.Drawing.Point(controlX, startY + rowHeight * 2), Size = new System.Drawing.Size(200, 20), DropDownStyle = ComboBoxStyle.DropDownList };

            lblSalary = new Label { Text = "Salary Demand:", Location = new System.Drawing.Point(labelX, startY + rowHeight * 3), Size = new System.Drawing.Size(120, 20) };
            txtSalary = new TextBox { Location = new System.Drawing.Point(controlX, startY + rowHeight * 3), Size = new System.Drawing.Size(200, 20) };

            // Second column
            int labelX2 = 400;
            int controlX2 = 530;

            lblLoyalty = new Label { Text = "Loyalty (0-100):", Location = new System.Drawing.Point(labelX2, startY), Size = new System.Drawing.Size(120, 20) };
            txtLoyalty = new TextBox { Location = new System.Drawing.Point(controlX2, startY), Size = new System.Drawing.Size(200, 20), Text = "50" };

            lblMood = new Label { Text = "Mood:", Location = new System.Drawing.Point(labelX2, startY + rowHeight), Size = new System.Drawing.Size(120, 20) };
            cboMood = new ComboBox { Location = new System.Drawing.Point(controlX2, startY + rowHeight), Size = new System.Drawing.Size(200, 20), DropDownStyle = ComboBoxStyle.DropDownList };

            lblBase = new Label { Text = "Assigned Base:", Location = new System.Drawing.Point(labelX2, startY + rowHeight * 2), Size = new System.Drawing.Size(120, 20) };
            cboBase = new ComboBox { Location = new System.Drawing.Point(controlX2, startY + rowHeight * 2), Size = new System.Drawing.Size(200, 20), DropDownStyle = ComboBoxStyle.DropDownList };

            lblScheme = new Label { Text = "Assigned Scheme:", Location = new System.Drawing.Point(labelX2, startY + rowHeight * 3), Size = new System.Drawing.Size(120, 20) };
            cboScheme = new ComboBox { Location = new System.Drawing.Point(controlX2, startY + rowHeight * 3), Size = new System.Drawing.Size(200, 20), DropDownStyle = ComboBoxStyle.DropDownList };

            // Buttons
            int buttonY = startY + rowHeight * 4 + 10;
            btnAdd = new Button { Text = "Add Minion", Location = new System.Drawing.Point(20, buttonY), Size = new System.Drawing.Size(120, 30) };
            btnAdd.Click += btnAdd_Click;

            btnUpdate = new Button { Text = "Update Minion", Location = new System.Drawing.Point(150, buttonY), Size = new System.Drawing.Size(120, 30) };
            btnUpdate.Click += btnUpdate_Click;

            btnDelete = new Button { Text = "Delete Minion", Location = new System.Drawing.Point(280, buttonY), Size = new System.Drawing.Size(120, 30) };
            btnDelete.Click += btnDelete_Click;

            btnRefresh = new Button { Text = "Refresh", Location = new System.Drawing.Point(410, buttonY), Size = new System.Drawing.Size(120, 30) };
            btnRefresh.Click += btnRefresh_Click;

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
