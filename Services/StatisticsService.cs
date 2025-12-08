using System;
using System.Collections.Generic;
using System.Linq;
using VillainLairManager.Utils;

namespace VillainLairManager.Services
{
    /// <summary>
    /// Service class for calculating dashboard statistics and generating alerts
    /// Extracted from MainForm to centralize business logic
    /// </summary>
    public class StatisticsService : IStatisticsService
    {
        private readonly IMinionService _minionService;
        private readonly ISchemeService _schemeService;
        private readonly IBaseService _baseService;
        private readonly IEquipmentService _equipmentService;
        private readonly ConfigManager _config;

        public StatisticsService(
            IMinionService minionService,
            ISchemeService schemeService,
            IBaseService baseService,
            IEquipmentService equipmentService)
        {
            _minionService = minionService ?? throw new ArgumentNullException(nameof(minionService));
            _schemeService = schemeService ?? throw new ArgumentNullException(nameof(schemeService));
            _baseService = baseService ?? throw new ArgumentNullException(nameof(baseService));
            _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
            _config = ConfigManager.Instance;
        }

        /// <summary>
        /// Calculates comprehensive dashboard statistics
        /// Business logic extracted from MainForm.LoadStatistics()
        /// </summary>
        public DashboardStatistics CalculateDashboardStatistics()
        {
            var stats = new DashboardStatistics();

            // Get all data
            var minions = _minionService.GetAllMinions();
            var schemes = _schemeService.GetAllSchemes();
            var bases = _baseService.GetAllBases();
            var equipment = _equipmentService.GetAllEquipment();

            // Minion statistics - categorize by mood
            foreach (var minion in minions)
            {
                stats.TotalMinions++;

                // Use consistent mood calculation from service
                string calculatedMood = _minionService.CalculateMood(minion.LoyaltyScore);
                
                if (calculatedMood == _config.MoodHappy)
                    stats.HappyMinions++;
                else if (calculatedMood == _config.MoodBetrayal)
                    stats.BetrayalMinions++;
                else
                    stats.GrumpyMinions++;
            }

            // Scheme statistics
            stats.TotalSchemes = schemes.Count;
            var activeSchemes = _schemeService.GetActiveSchemes();
            stats.ActiveSchemes = activeSchemes.Count;

            // Calculate average success likelihood using service
            if (activeSchemes.Any())
            {
                stats.AverageSuccessLikelihood = _schemeService.CalculateAverageSuccess(activeSchemes);
            }

            // Cost calculations
            stats.TotalMinionSalaries = minions.Sum(m => m.SalaryDemand);
            stats.TotalBaseCosts = bases.Sum(b => b.MonthlyMaintenanceCost);
            stats.TotalEquipmentCosts = equipment.Sum(e => e.MaintenanceCost);
            stats.TotalMonthlyCost = stats.TotalMinionSalaries + stats.TotalBaseCosts + stats.TotalEquipmentCosts;

            // Generate alerts
            stats.Alerts = GenerateAlerts();

            return stats;
        }

        /// <summary>
        /// Generates system alerts based on current state
        /// Business logic extracted from MainForm.LoadStatistics()
        /// </summary>
        public List<string> GenerateAlerts()
        {
            var alerts = new List<string>();

            var minions = _minionService.GetAllMinions();
            var schemes = _schemeService.GetAllSchemes();
            var equipment = _equipmentService.GetAllEquipment();

            // Low loyalty alert
            var lowLoyaltyMinions = minions.Count(m => m.LoyaltyScore < _config.LowLoyaltyThreshold);
            if (lowLoyaltyMinions > 0)
            {
                alerts.Add($"⚠ Warning: {lowLoyaltyMinions} minions have low loyalty and may betray you!");
            }

            // Broken equipment alert
            var brokenEquipment = equipment.Count(e => _equipmentService.IsBroken(e));
            if (brokenEquipment > 0)
            {
                alerts.Add($"⚠ {brokenEquipment} equipment items are broken!");
            }

            // Over budget schemes alert
            var overBudgetSchemes = schemes.Count(s => _schemeService.IsOverBudget(s));
            if (overBudgetSchemes > 0)
            {
                alerts.Add($"⚠ {overBudgetSchemes} schemes are over budget!");
            }

            // No alerts message
            if (!alerts.Any())
            {
                alerts.Add("✓ All systems operational");
            }

            return alerts;
        }
    }
}
