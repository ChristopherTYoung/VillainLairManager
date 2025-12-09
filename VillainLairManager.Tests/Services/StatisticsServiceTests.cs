using Moq;
using System;
using System.Collections.Generic;
using VillainLairManager.Models;
using VillainLairManager.Services;
using Xunit;

namespace VillainLairManager.Tests.Services
{
    /// <summary>
    /// Tests for StatisticsService business logic
    /// </summary>
    public class StatisticsServiceTests
    {
        private readonly Mock<IMinionService> _mockMinionService;
        private readonly Mock<ISchemeService> _mockSchemeService;
        private readonly Mock<IBaseService> _mockBaseService;
        private readonly Mock<IEquipmentService> _mockEquipmentService;
        private readonly StatisticsService _statisticsService;

        public StatisticsServiceTests()
        {
            _mockMinionService = new Mock<IMinionService>();
            _mockSchemeService = new Mock<ISchemeService>();
            _mockBaseService = new Mock<IBaseService>();
            _mockEquipmentService = new Mock<IEquipmentService>();
            
            _statisticsService = new StatisticsService(
                _mockMinionService.Object,
                _mockSchemeService.Object,
                _mockBaseService.Object,
                _mockEquipmentService.Object);
        }

        [Fact]
        public void CalculateDashboardStatistics_WithNoData_ReturnsZeroStatistics()
        {
            // Arrange
            _mockMinionService.Setup(s => s.GetAllMinions()).Returns(new List<Minion>());
            _mockSchemeService.Setup(s => s.GetAllSchemes()).Returns(new List<EvilScheme>());
            _mockSchemeService.Setup(s => s.GetActiveSchemes()).Returns(new List<EvilScheme>());
            _mockBaseService.Setup(s => s.GetAllBases()).Returns(new List<SecretBase>());
            _mockEquipmentService.Setup(s => s.GetAllEquipment()).Returns(new List<Equipment>());

            // Act
            var stats = _statisticsService.CalculateDashboardStatistics();

            // Assert
            Assert.Equal(0, stats.TotalMinions);
            Assert.Equal(0, stats.HappyMinions);
            Assert.Equal(0, stats.GrumpyMinions);
            Assert.Equal(0, stats.BetrayalMinions);
            Assert.Equal(0, stats.TotalSchemes);
            Assert.Equal(0, stats.ActiveSchemes);
            Assert.Equal(0, stats.TotalMinionSalaries);
            Assert.Equal(0, stats.TotalBaseCosts);
            Assert.Equal(0, stats.TotalEquipmentCosts);
            Assert.Equal(0, stats.TotalMonthlyCost);
        }

        [Fact]
        public void CalculateDashboardStatistics_WithMixedMoods_CountsCorrectly()
        {
            // Arrange
            var minions = new List<Minion>
            {
                new Minion { MinionId = 1, Name = "Happy Minion", LoyaltyScore = 80, SalaryDemand = 5000 },
                new Minion { MinionId = 2, Name = "Grumpy Minion", LoyaltyScore = 50, SalaryDemand = 6000 },
                new Minion { MinionId = 3, Name = "Betraying Minion", LoyaltyScore = 20, SalaryDemand = 7000 }
            };

            _mockMinionService.Setup(s => s.GetAllMinions()).Returns(minions);
            _mockMinionService.Setup(s => s.CalculateMood(80)).Returns("Happy");
            _mockMinionService.Setup(s => s.CalculateMood(50)).Returns("Grumpy");
            _mockMinionService.Setup(s => s.CalculateMood(20)).Returns("Plotting Betrayal");
            _mockSchemeService.Setup(s => s.GetAllSchemes()).Returns(new List<EvilScheme>());
            _mockSchemeService.Setup(s => s.GetActiveSchemes()).Returns(new List<EvilScheme>());
            _mockBaseService.Setup(s => s.GetAllBases()).Returns(new List<SecretBase>());
            _mockEquipmentService.Setup(s => s.GetAllEquipment()).Returns(new List<Equipment>());

            // Act
            var stats = _statisticsService.CalculateDashboardStatistics();

            // Assert
            Assert.Equal(3, stats.TotalMinions);
            Assert.Equal(1, stats.HappyMinions);
            Assert.Equal(1, stats.GrumpyMinions);
            Assert.Equal(1, stats.BetrayalMinions);
            Assert.Equal(18000, stats.TotalMinionSalaries);
        }

        [Fact]
        public void CalculateDashboardStatistics_WithActiveSchemes_CalculatesAverageSuccess()
        {
            // Arrange
            var activeSchemes = new List<EvilScheme>
            {
                new EvilScheme { SchemeId = 1, Status = "Active" },
                new EvilScheme { SchemeId = 2, Status = "Active" }
            };

            _mockMinionService.Setup(s => s.GetAllMinions()).Returns(new List<Minion>());
            _mockSchemeService.Setup(s => s.GetAllSchemes()).Returns(activeSchemes);
            _mockSchemeService.Setup(s => s.GetActiveSchemes()).Returns(activeSchemes);
            _mockSchemeService.Setup(s => s.CalculateAverageSuccess(activeSchemes)).Returns(65.5);
            _mockBaseService.Setup(s => s.GetAllBases()).Returns(new List<SecretBase>());
            _mockEquipmentService.Setup(s => s.GetAllEquipment()).Returns(new List<Equipment>());

            // Act
            var stats = _statisticsService.CalculateDashboardStatistics();

            // Assert
            Assert.Equal(2, stats.TotalSchemes);
            Assert.Equal(2, stats.ActiveSchemes);
            Assert.Equal(65.5, stats.AverageSuccessLikelihood);
        }

        [Fact]
        public void CalculateDashboardStatistics_WithCosts_SumsTotalCorrectly()
        {
            // Arrange
            var minions = new List<Minion>
            {
                new Minion { SalaryDemand = 5000 },
                new Minion { SalaryDemand = 6000 }
            };
            var bases = new List<SecretBase>
            {
                new SecretBase { MonthlyMaintenanceCost = 10000 },
                new SecretBase { MonthlyMaintenanceCost = 15000 }
            };
            var equipment = new List<Equipment>
            {
                new Equipment { MaintenanceCost = 2000 },
                new Equipment { MaintenanceCost = 3000 }
            };

            _mockMinionService.Setup(s => s.GetAllMinions()).Returns(minions);
            _mockMinionService.Setup(s => s.CalculateMood(It.IsAny<int>())).Returns("Grumpy");
            _mockSchemeService.Setup(s => s.GetAllSchemes()).Returns(new List<EvilScheme>());
            _mockSchemeService.Setup(s => s.GetActiveSchemes()).Returns(new List<EvilScheme>());
            _mockBaseService.Setup(s => s.GetAllBases()).Returns(bases);
            _mockEquipmentService.Setup(s => s.GetAllEquipment()).Returns(equipment);

            // Act
            var stats = _statisticsService.CalculateDashboardStatistics();

            // Assert
            Assert.Equal(11000, stats.TotalMinionSalaries);
            Assert.Equal(25000, stats.TotalBaseCosts);
            Assert.Equal(5000, stats.TotalEquipmentCosts);
            Assert.Equal(41000, stats.TotalMonthlyCost);
        }

        [Fact]
        public void GenerateAlerts_WithLowLoyaltyMinions_GeneratesWarning()
        {
            // Arrange
            var minions = new List<Minion>
            {
                new Minion { LoyaltyScore = 20 },
                new Minion { LoyaltyScore = 30 }
            };

            _mockMinionService.Setup(s => s.GetAllMinions()).Returns(minions);
            _mockSchemeService.Setup(s => s.GetAllSchemes()).Returns(new List<EvilScheme>());
            _mockEquipmentService.Setup(s => s.GetAllEquipment()).Returns(new List<Equipment>());

            // Act
            var alerts = _statisticsService.GenerateAlerts();

            // Assert
            Assert.Contains(alerts, a => a.Contains("low loyalty"));
        }

        [Fact]
        public void GenerateAlerts_WithBrokenEquipment_GeneratesWarning()
        {
            // Arrange
            var equipment = new List<Equipment>
            {
                new Equipment { Condition = 10 },
                new Equipment { Condition = 15 }
            };

            _mockMinionService.Setup(s => s.GetAllMinions()).Returns(new List<Minion>());
            _mockSchemeService.Setup(s => s.GetAllSchemes()).Returns(new List<EvilScheme>());
            _mockEquipmentService.Setup(s => s.GetAllEquipment()).Returns(equipment);
            _mockEquipmentService.Setup(s => s.IsBroken(It.IsAny<Equipment>())).Returns(true);

            // Act
            var alerts = _statisticsService.GenerateAlerts();

            // Assert
            Assert.Contains(alerts, a => a.Contains("broken"));
        }

        [Fact]
        public void GenerateAlerts_WithOverBudgetSchemes_GeneratesWarning()
        {
            // Arrange
            var schemes = new List<EvilScheme>
            {
                new EvilScheme { Budget = 100000, CurrentSpending = 120000 }
            };

            _mockMinionService.Setup(s => s.GetAllMinions()).Returns(new List<Minion>());
            _mockSchemeService.Setup(s => s.GetAllSchemes()).Returns(schemes);
            _mockSchemeService.Setup(s => s.IsOverBudget(It.IsAny<EvilScheme>())).Returns(true);
            _mockEquipmentService.Setup(s => s.GetAllEquipment()).Returns(new List<Equipment>());

            // Act
            var alerts = _statisticsService.GenerateAlerts();

            // Assert
            Assert.Contains(alerts, a => a.Contains("over budget"));
        }

        [Fact]
        public void GenerateAlerts_WithNoIssues_ReturnsOperationalMessage()
        {
            // Arrange
            _mockMinionService.Setup(s => s.GetAllMinions()).Returns(new List<Minion>());
            _mockSchemeService.Setup(s => s.GetAllSchemes()).Returns(new List<EvilScheme>());
            _mockEquipmentService.Setup(s => s.GetAllEquipment()).Returns(new List<Equipment>());

            // Act
            var alerts = _statisticsService.GenerateAlerts();

            // Assert
            Assert.Contains(alerts, a => a.Contains("operational"));
        }
    }
}
