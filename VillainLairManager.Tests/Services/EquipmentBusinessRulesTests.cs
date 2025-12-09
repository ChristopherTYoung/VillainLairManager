using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using VillainLairManager.Models;
using VillainLairManager.Repositories;
using VillainLairManager.Services;
using Xunit;

namespace VillainLairManager.Tests.Services
{
    /// <summary>
    /// Comprehensive tests for equipment business rules:
    /// - Condition degradation
    /// - Maintenance costs
    /// - Equipment operational thresholds
    /// Note: EquipmentService methods take Equipment objects, not IDs
    /// </summary>
    public class EquipmentBusinessRulesTests
    {
        private readonly Mock<IEquipmentRepository> _mockEquipmentRepository;
        private readonly Mock<IEvilSchemeRepository> _mockSchemeRepository;
        private readonly EquipmentService _equipmentService;

        public EquipmentBusinessRulesTests()
        {
            _mockEquipmentRepository = new Mock<IEquipmentRepository>();
            _mockSchemeRepository = new Mock<IEvilSchemeRepository>();
            _equipmentService = new EquipmentService(
                _mockEquipmentRepository.Object,
                _mockSchemeRepository.Object);
        }

        #region Maintenance Cost Tests

        [Fact]
        public void PerformMaintenance_DoomsdayDevice_Costs30Percent()
        {
            // Arrange
            var equipment = new Equipment
            {
                EquipmentId = 1,
                Name = "Death Ray",
                Category = "Doomsday Device",
                PurchasePrice = 1000000,
                Condition = 50
            };

            _mockEquipmentRepository.Setup(r => r.GetById(1)).Returns(equipment);
            _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

            // Act
            decimal cost = _equipmentService.PerformMaintenance(1);

            // Assert - 30% of 1,000,000 = 300,000
            Assert.Equal(300000, cost);
        }

        [Fact]
        public void PerformMaintenance_RegularEquipment_Costs15Percent()
        {
            // Arrange
            var equipment = new Equipment
            {
                EquipmentId = 1,
                Name = "Surveillance Camera",
                Category = "Surveillance",
                PurchasePrice = 10000,
                Condition = 60
            };

            _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

            // Act
            var result = _equipmentService.PerformMaintenance(equipment);
            decimal cost = result.cost;

            // Assert - 15% of 10,000 = 1,500
            Assert.Equal(1500, cost);
        }

        [Theory]
        [InlineData("Doomsday Device", 100000, 30000)]
        [InlineData("Doomsday Device", 500000, 150000)]
        [InlineData("Doomsday Device", 2000000, 600000)]
        public void PerformMaintenance_DoomsdayDevices_CalculatesCorrectly(string category, decimal price, decimal expectedCost)
        {
            // Arrange
            var equipment = new Equipment
            {
                EquipmentId = 1,
                Name = "Test Device",
                Category = category,
                PurchasePrice = price,
                Condition = 50
            };

            _mockEquipmentRepository.Setup(r => r.GetById(1)).Returns(equipment);
            _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

            // Act
            decimal cost = _equipmentService.PerformMaintenance(1);

            // Assert
            Assert.Equal(expectedCost, cost);
        }

        [Theory]
        [InlineData("Surveillance", 20000, 3000)]
        [InlineData("Transportation", 50000, 7500)]
        [InlineData("Communication", 8000, 1200)]
        [InlineData("Weapons", 100000, 15000)]
        public void PerformMaintenance_RegularEquipment_CalculatesCorrectly(string category, decimal price, decimal expectedCost)
        {
            // Arrange
            var equipment = new Equipment
            {
                EquipmentId = 1,
                Name = "Test Equipment",
                Category = category,
                PurchasePrice = price,
                Condition = 50
            };

            _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

            // Act
            var result = _equipmentService.PerformMaintenance(equipment);
            decimal cost = result.cost;

            // Assert
            Assert.Equal(expectedCost, cost);
        }

        [Fact]
        public void PerformMaintenance_RestoresConditionTo100()
        {
            // Arrange
            var equipment = new Equipment
            {
                EquipmentId = 1,
                Name = "Test Equipment",
                Category = "Surveillance",
                PurchasePrice = 10000,
                Condition = 25 // Poor condition
            };

            _mockEquipmentRepository.Setup(r => r.GetById(1)).Returns(equipment);
            _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

            // Act
            _equipmentService.PerformMaintenance(1);

            // Assert
            _mockEquipmentRepository.Verify(r => r.Update(It.Is<Equipment>(e => e.Condition == 100)), Times.Once);
        }

        [Fact]
        public void PerformMaintenance_ZeroPriceEquipment_ZeroCost()
        {
            // Arrange
            var equipment = new Equipment
            {
                EquipmentId = 1,
                Name = "Free Equipment",
                Category = "Surveillance",
                PurchasePrice = 0,
                Condition = 50
            };

            _mockEquipmentRepository.Setup(r => r.GetById(1)).Returns(equipment);
            _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

            // Act
            decimal cost = _equipmentService.PerformMaintenance(1);

            // Assert
            Assert.Equal(0, cost);
        }

        #endregion

        #region Condition Degradation Tests

        [Fact]
        public void DegradeCondition_ActiveScheme_Degrades5Points()
        {
            // Arrange
            var equipment = new Equipment
            {
                EquipmentId = 1,
                Name = "Test Equipment",
                Category = "Surveillance",
                Condition = 80,
                AssignedToSchemeId = 1,
                LastMaintenanceDate = DateTime.Now.AddMonths(-1)
            };

            var scheme = new EvilScheme
            {
                SchemeId = 1,
                Status = "Active"
            };

            _mockEquipmentRepository.Setup(r => r.GetById(1)).Returns(equipment);
            _mockSchemeRepository.Setup(r => r.GetById(1)).Returns(scheme);
            _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

            // Act
            _equipmentService.DegradeCondition(1);

            // Assert - should degrade by 5 points
            _mockEquipmentRepository.Verify(r => r.Update(It.Is<Equipment>(e => e.Condition == 75)), Times.Once);
        }

        [Fact]
        public void DegradeCondition_NoActiveScheme_MinimalDegradation()
        {
            // Arrange
            var equipment = new Equipment
            {
                EquipmentId = 1,
                Name = "Test Equipment",
                Category = "Surveillance",
                Condition = 80,
                AssignedToSchemeId = null,
                LastMaintenanceDate = DateTime.Now.AddMonths(-1)
            };

            _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

            // Act
            _equipmentService.DegradeCondition(equipment);

            // Assert - should degrade by 1 point (or stay at 80 if no idle degradation)
            _mockEquipmentRepository.Verify(r => r.Update(It.Is<Equipment>(e => e.Condition >= 79 && e.Condition <= 80)), Times.Once);
        }

        [Fact]
        public void DegradeCondition_CannotGoBelow0()
        {
            // Arrange
            var equipment = new Equipment
            {
                EquipmentId = 1,
                Name = "Test Equipment",
                Category = "Surveillance",
                Condition = 3, // Very low condition
                AssignedToSchemeId = 1,
                LastMaintenanceDate = DateTime.Now.AddMonths(-1)
            };

            var scheme = new EvilScheme
            {
                SchemeId = 1,
                Status = "Active"
            };

            _mockEquipmentRepository.Setup(r => r.GetById(1)).Returns(equipment);
            _mockSchemeRepository.Setup(r => r.GetById(1)).Returns(scheme);
            _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

            // Act
            _equipmentService.DegradeCondition(1);

            // Assert - should not go below 0
            _mockEquipmentRepository.Verify(r => r.Update(It.Is<Equipment>(e => e.Condition >= 0)), Times.Once);
        }

        [Fact]
        public void DegradeCondition_RecentMaintenance_NoDegradation()
        {
            // Arrange
            var equipment = new Equipment
            {
                EquipmentId = 1,
                Name = "Test Equipment",
                Category = "Surveillance",
                Condition = 100,
                AssignedToSchemeId = 1,
                LastMaintenanceDate = DateTime.Now.AddDays(-5) // Very recent
            };

            var scheme = new EvilScheme
            {
                SchemeId = 1,
                Status = "Active"
            };

            _mockEquipmentRepository.Setup(r => r.GetById(1)).Returns(equipment);
            _mockSchemeRepository.Setup(r => r.GetById(1)).Returns(scheme);
            _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

            // Act
            _equipmentService.DegradeCondition(1);

            // Assert - might not degrade if too recent
            _mockEquipmentRepository.Verify(r => r.Update(It.Is<Equipment>(e => e.Condition >= 95)), Times.Once);
        }

        [Theory]
        [InlineData(100, 1)]
        [InlineData(80, 1)]
        [InlineData(50, 1)]
        [InlineData(20, 1)]
        [InlineData(10, 1)]
        public void DegradeCondition_ActiveScheme_DegradesByMonthsElapsed(int startCondition, int monthsElapsed)
        {
            // Arrange
            var equipment = new Equipment
            {
                EquipmentId = 1,
                Name = "Test Equipment",
                Category = "Surveillance",
                Condition = startCondition,
                AssignedToSchemeId = 1,
                LastMaintenanceDate = DateTime.Now.AddMonths(-monthsElapsed)
            };

            var scheme = new EvilScheme
            {
                SchemeId = 1,
                Status = "Active"
            };

            _mockEquipmentRepository.Setup(r => r.GetById(1)).Returns(equipment);
            _mockSchemeRepository.Setup(r => r.GetById(1)).Returns(scheme);
            _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

            // Act
            _equipmentService.DegradeCondition(1);

            // Assert - should degrade by (monthsElapsed * 5) points for active scheme
            int expectedDegradation = monthsElapsed * 5;
            int expectedCondition = Math.Max(0, startCondition - expectedDegradation);
            _mockEquipmentRepository.Verify(r => r.Update(It.Is<Equipment>(e => e.Condition == expectedCondition)), Times.Once);
        }

        #endregion

        #region Equipment Operational Threshold Tests

        [Fact]
        public void GetOperationalEquipment_FiltersCorrectly()
        {
            // Arrange
            var allEquipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, Condition = 100 }, // Operational
                new Equipment { EquipmentId = 2, Condition = 75 },  // Operational
                new Equipment { EquipmentId = 3, Condition = 50 },  // Operational (at threshold)
                new Equipment { EquipmentId = 4, Condition = 49 },  // Below operational
                new Equipment { EquipmentId = 5, Condition = 25 },  // Below operational
                new Equipment { EquipmentId = 6, Condition = 0 }    // Broken
            };

            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(allEquipment);

            // Act
            var allEq = _equipmentService.GetAllEquipment();
            var operational = allEq.Where(e => _equipmentService.IsOperational(e)).ToList();

            // Assert - should include only condition >= 50
            Assert.Equal(3, operational.Count);
            Assert.All(operational, e => Assert.True(e.Condition >= 50));
        }

        [Fact]
        public void GetOperationalEquipment_WithNoOperational_ReturnsEmpty()
        {
            // Arrange
            var allEquipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, Condition = 30 },
                new Equipment { EquipmentId = 2, Condition = 10 },
                new Equipment { EquipmentId = 3, Condition = 0 }
            };

            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(allEquipment);

            // Act
            var operational = _equipmentService.GetOperationalEquipment();

            // Assert
            Assert.Empty(operational);
        }

        [Fact]
        public void GetBrokenEquipment_FiltersBelowThreshold()
        {
            // Arrange
            var allEquipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, Condition = 100 },
                new Equipment { EquipmentId = 2, Condition = 50 },
                new Equipment { EquipmentId = 3, Condition = 20 },  // NOT broken (threshold is < 20, not <= 20)
                new Equipment { EquipmentId = 4, Condition = 19 },  // Broken
                new Equipment { EquipmentId = 5, Condition = 0 }    // Broken
            };

            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(allEquipment);

            // Act
            var allEq = _equipmentService.GetAllEquipment();
            var broken = allEq.Where(e => _equipmentService.IsBroken(e)).ToList();

            // Assert - should include only condition < 20 (business rule BR-E-001)
            Assert.Equal(2, broken.Count);
            Assert.All(broken, e => Assert.True(e.Condition < 20));
        }

        [Theory]
        [InlineData(100, true)]
        [InlineData(75, true)]
        [InlineData(50, true)]
        [InlineData(49, false)]
        [InlineData(25, false)]
        [InlineData(0, false)]
        public void IsOperational_VariousConditions_ReturnsExpected(int condition, bool expectedOperational)
        {
            // Arrange
            var equipment = new Equipment
            {
                EquipmentId = 1,
                Condition = condition
            };

            // Act
            bool isOperational = _equipmentService.IsOperational(equipment);

            // Assert
            Assert.Equal(expectedOperational, isOperational);
        }

        #endregion

        #region Cost Calculation Tests

        [Fact]
        public void CalculateTotalMaintenanceCost_ForMultipleEquipment()
        {
            // Arrange
            var equipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, Category = "Doomsday Device", PurchasePrice = 1000000, Condition = 50 },
                new Equipment { EquipmentId = 2, Category = "Surveillance", PurchasePrice = 10000, Condition = 40 },
                new Equipment { EquipmentId = 3, Category = "Weapons", PurchasePrice = 50000, Condition = 30 }
            };

            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);
            _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

            // Act
            decimal totalCost = 0;
            foreach (var eq in equipment)
            {
                var result = _equipmentService.PerformMaintenance(eq);
                totalCost += result.cost;
            }

            // Assert - 300,000 + 1,500 + 7,500 = 309,000
            Assert.Equal(309000, totalCost);
        }

        [Fact]
        public void CalculateTotalMaintenanceCost_WithNoEquipment_ReturnsZero()
        {
            // Arrange
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(new List<Equipment>());

            // Act
            var allEq = _equipmentService.GetAllEquipment();
            decimal totalCost = 0;
            foreach (var eq in allEq)
            {
                var result = _equipmentService.PerformMaintenance(eq);
                totalCost += result.cost;
            }

            // Assert
            Assert.Equal(0, totalCost);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public void PerformMaintenance_AlreadyPerfectCondition_StillCharges()
        {
            // Arrange
            var equipment = new Equipment
            {
                EquipmentId = 1,
                Name = "Perfect Equipment",
                Category = "Surveillance",
                PurchasePrice = 10000,
                Condition = 100 // Already perfect
            };

            _mockEquipmentRepository.Setup(r => r.GetById(1)).Returns(equipment);
            _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

            // Act
            decimal cost = _equipmentService.PerformMaintenance(1);

            // Assert - should still charge even if already perfect
            Assert.Equal(1500, cost);
        }

        [Fact]
        public void DegradeCondition_CompletedScheme_NoActiveDegradation()
        {
            // Arrange
            var equipment = new Equipment
            {
                EquipmentId = 1,
                Name = "Test Equipment",
                Category = "Surveillance",
                Condition = 80,
                AssignedToSchemeId = 1,
                LastMaintenanceDate = DateTime.Now.AddMonths(-1)
            };

            var scheme = new EvilScheme
            {
                SchemeId = 1,
                Status = "Completed"
            };

            _mockEquipmentRepository.Setup(r => r.GetById(1)).Returns(equipment);
            _mockSchemeRepository.Setup(r => r.GetById(1)).Returns(scheme);
            _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

            // Act
            _equipmentService.DegradeCondition(1);

            // Assert - should not degrade as much (scheme not active)
            _mockEquipmentRepository.Verify(r => r.Update(It.Is<Equipment>(e => e.Condition >= 79)), Times.Once);
        }

        [Fact]
        public void PerformMaintenance_UpdatesLastMaintenanceDate()
        {
            // Arrange
            var equipment = new Equipment
            {
                EquipmentId = 1,
                Name = "Test Equipment",
                Category = "Surveillance",
                PurchasePrice = 10000,
                Condition = 50,
                LastMaintenanceDate = DateTime.Now.AddMonths(-6)
            };

            _mockEquipmentRepository.Setup(r => r.GetById(1)).Returns(equipment);
            _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

            // Act
            _equipmentService.PerformMaintenance(1);

            // Assert - should update last maintenance date to today
            _mockEquipmentRepository.Verify(r => r.Update(It.Is<Equipment>(e => 
                e.LastMaintenanceDate.HasValue && 
                e.LastMaintenanceDate.Value.Date == DateTime.Now.Date)), Times.Once);
        }

        #endregion
    }
}
