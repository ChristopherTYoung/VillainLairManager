using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VillainLairManager.Models;
using VillainLairManager.Utils;
using VillainLairManager.Repositories;

namespace VillainLairManager.Forms
{
    /// <summary>
    /// Main dashboard form with navigation and statistics
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly IMinionRepository _minionRepository;
        private readonly IEvilSchemeRepository _schemeRepository;
        private readonly ISecretBaseRepository _baseRepository;
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly Func<MinionManagementForm> _createMinionForm;
        private readonly Func<EquipmentInventoryForm> _createEquipmentForm;
        private readonly Func<SchemeManagementForm> _createSchemeForm;
        private readonly Func<BaseManagementForm> _createBaseForm;

        public MainForm(
            IMinionRepository minionRepository,
            IEvilSchemeRepository schemeRepository,
            ISecretBaseRepository baseRepository,
            IEquipmentRepository equipmentRepository,
            Func<MinionManagementForm> createMinionForm,
            Func<EquipmentInventoryForm> createEquipmentForm,
            Func<SchemeManagementForm> createSchemeForm,
            Func<BaseManagementForm> createBaseForm)
        {
            _minionRepository = minionRepository ?? throw new ArgumentNullException(nameof(minionRepository));
            _schemeRepository = schemeRepository ?? throw new ArgumentNullException(nameof(schemeRepository));
            _baseRepository = baseRepository ?? throw new ArgumentNullException(nameof(baseRepository));
            _equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
            _createMinionForm = createMinionForm ?? throw new ArgumentNullException(nameof(createMinionForm));
            _createEquipmentForm = createEquipmentForm ?? throw new ArgumentNullException(nameof(createEquipmentForm));
            _createSchemeForm = createSchemeForm ?? throw new ArgumentNullException(nameof(createSchemeForm));
            _createBaseForm = createBaseForm ?? throw new ArgumentNullException(nameof(createBaseForm));
            
            InitializeComponent();
            LoadStatistics();
        }

        private void btnMinions_Click(object sender, EventArgs e)
        {
            OpenForm(_createMinionForm());
        }

        private void btnSchemes_Click(object sender, EventArgs e)
        {
            OpenForm(_createSchemeForm());
        }

        private void btnBases_Click(object sender, EventArgs e)
        {
            OpenForm(_createBaseForm());
        }

        private void btnEquipment_Click(object sender, EventArgs e)
        {
            OpenForm(_createEquipmentForm());
        }

        private void OpenForm(Form form)
        {
            form.ShowDialog();
            LoadStatistics(); // Refresh after closing child form
        }

        // Business logic in UI layer (anti-pattern)
        // This calculation is duplicated from models
        private void LoadStatistics()
        {
            var minions = _minionRepository.GetAll();
            var schemes = _schemeRepository.GetAll();
            var bases = _baseRepository.GetAll();
            var equipment = _equipmentRepository.GetAll();

            // Minion statistics with duplicated mood calculation
            int happyCount = 0, grumpyCount = 0, betrayalCount = 0;
            foreach (var minion in minions)
            {
                // Mood calculation duplicated from Minion.UpdateMood() (anti-pattern)
                if (minion.LoyaltyScore > 70)
                    happyCount++;
                else if (minion.LoyaltyScore < 40)
                    betrayalCount++;
                else
                    grumpyCount++;
            }

            lblMinionStats.Text = $"Minions: {minions.Count} total | Happy: {happyCount} | Grumpy: {grumpyCount} | Plotting Betrayal: {betrayalCount}";

            // Scheme statistics with duplicated success calculation
            var activeSchemes = schemes.Where(s => s.Status == "Active").ToList();
            double avgSuccess = 0;
            if (activeSchemes.Any())
            {
                // Success likelihood calculation duplicated here (anti-pattern)
                foreach (var scheme in activeSchemes)
                {
                    // This is also in EvilScheme.CalculateSuccessLikelihood() - duplication!
                    int success = scheme.CalculateSuccessLikelihood();
                    avgSuccess += success;
                }
                avgSuccess /= activeSchemes.Count;
            }

            lblSchemeStats.Text = $"Evil Schemes: {schemes.Count} total | Active: {activeSchemes.Count} | Avg Success Likelihood: {avgSuccess:F1}%";

            // Cost calculation (business logic in UI)
            decimal totalMinionSalaries = 0;
            foreach (var minion in minions)
            {
                totalMinionSalaries += minion.SalaryDemand;
            }

            decimal totalBaseCosts = 0;
            foreach (var baseObj in bases)
            {
                totalBaseCosts += baseObj.MonthlyMaintenanceCost;
            }

            decimal totalEquipmentCosts = 0;
            foreach (var equip in equipment)
            {
                totalEquipmentCosts += equip.MaintenanceCost;
            }

            decimal totalMonthlyCost = totalMinionSalaries + totalBaseCosts + totalEquipmentCosts;

            lblCostStats.Text = $"Monthly Costs: Minions: ${totalMinionSalaries:N0} | Bases: ${totalBaseCosts:N0} | Equipment: ${totalEquipmentCosts:N0} | TOTAL: ${totalMonthlyCost:N0}";

            // Alerts (more business logic in UI)
            var alerts = "";

            // Low loyalty alert
            var lowLoyaltyMinions = minions.Where(m => m.LoyaltyScore < 40).Count();
            if (lowLoyaltyMinions > 0)
            {
                alerts += $"⚠ Warning: {lowLoyaltyMinions} minions have low loyalty and may betray you! ";
            }

            // Broken equipment alert
            var brokenEquipment = equipment.Where(e => e.Condition < 20).Count();
            if (brokenEquipment > 0)
            {
                alerts += $"⚠ {brokenEquipment} equipment items are broken! ";
            }

            // Over budget schemes
            var overBudgetSchemes = schemes.Where(s => s.CurrentSpending > s.Budget).Count();
            if (overBudgetSchemes > 0)
            {
                alerts += $"⚠ {overBudgetSchemes} schemes are over budget! ";
            }

            lblAlerts.Text = string.IsNullOrEmpty(alerts) ? "✓ All systems operational" : alerts;
        }
    }
}
