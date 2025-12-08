using System.Collections.Generic;

namespace VillainLairManager.Services
{
    /// <summary>
    /// Statistics data transfer object
    /// </summary>
    public class DashboardStatistics
    {
        public int TotalMinions { get; set; }
        public int HappyMinions { get; set; }
        public int GrumpyMinions { get; set; }
        public int BetrayalMinions { get; set; }

        public int TotalSchemes { get; set; }
        public int ActiveSchemes { get; set; }
        public double AverageSuccessLikelihood { get; set; }

        public decimal TotalMinionSalaries { get; set; }
        public decimal TotalBaseCosts { get; set; }
        public decimal TotalEquipmentCosts { get; set; }
        public decimal TotalMonthlyCost { get; set; }

        public List<string> Alerts { get; set; } = new List<string>();
    }

    /// <summary>
    /// Service interface for dashboard statistics and alerts
    /// </summary>
    public interface IStatisticsService
    {
        /// <summary>
        /// Calculates all dashboard statistics
        /// </summary>
        DashboardStatistics CalculateDashboardStatistics();

        /// <summary>
        /// Generates system alerts based on current state
        /// </summary>
        List<string> GenerateAlerts();
    }
}
