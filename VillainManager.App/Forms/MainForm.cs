using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VillainLairManager.Models;
using VillainLairManager.Utils;
using VillainLairManager.Repositories;
using VillainLairManager.Services;

namespace VillainLairManager.Forms
{
    /// <summary>
    /// Main dashboard form with navigation and statistics
    /// Business logic extracted to StatisticsService
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly IStatisticsService _statisticsService;
        private readonly Func<MinionManagementForm> _createMinionForm;
        private readonly Func<EquipmentInventoryForm> _createEquipmentForm;
        private readonly Func<SchemeManagementForm> _createSchemeForm;
        private readonly Func<BaseManagementForm> _createBaseForm;

        public MainForm(
            IStatisticsService statisticsService,
            Func<MinionManagementForm> createMinionForm,
            Func<EquipmentInventoryForm> createEquipmentForm,
            Func<SchemeManagementForm> createSchemeForm,
            Func<BaseManagementForm> createBaseForm)
        {
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
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

        // Statistics now calculated by service (business logic extracted from UI)
        private void LoadStatistics()
        {
            // Use service to calculate all statistics - no business logic in UI
            var stats = _statisticsService.CalculateDashboardStatistics();

            // Display minion statistics
            lblMinionStats.Text = $"Minions: {stats.TotalMinions} total | Happy: {stats.HappyMinions} | Grumpy: {stats.GrumpyMinions} | Plotting Betrayal: {stats.BetrayalMinions}";

            // Display scheme statistics
            lblSchemeStats.Text = $"Evil Schemes: {stats.TotalSchemes} total | Active: {stats.ActiveSchemes} | Avg Success Likelihood: {stats.AverageSuccessLikelihood:F1}%";

            // Display cost statistics
            lblCostStats.Text = $"Monthly Costs: Minions: ${stats.TotalMinionSalaries:N0} | Bases: ${stats.TotalBaseCosts:N0} | Equipment: ${stats.TotalEquipmentCosts:N0} | TOTAL: ${stats.TotalMonthlyCost:N0}";

            // Display alerts
            lblAlerts.Text = string.Join(" ", stats.Alerts);
        }
    }
}
