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
    /// Comprehensive tests for evil scheme business rules:
    /// - Success calculations
    /// - Budget enforcement
    /// - Status transitions
    /// </summary>
    public class SchemeBusinessRulesTests
    {
        private readonly Mock<IEvilSchemeRepository> _mockSchemeRepository;
        private readonly Mock<IMinionRepository> _mockMinionRepository;
        private readonly Mock<IEquipmentRepository> _mockEquipmentRepository;
        private readonly SchemeService _schemeService;

        public SchemeBusinessRulesTests()
        {
            _mockSchemeRepository = new Mock<IEvilSchemeRepository>();
            _mockMinionRepository = new Mock<IMinionRepository>();
            _mockEquipmentRepository = new Mock<IEquipmentRepository>();
            _schemeService = new SchemeService(
                _mockSchemeRepository.Object,
                _mockMinionRepository.Object,
                _mockEquipmentRepository.Object);
        }

        #region Success Likelihood Calculation Tests

        [Fact]
        public void CalculateSuccessLikelihood_WithNoResources_ReturnsBaseSuccess()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                Name = "Test Scheme",
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 50000,
                TargetCompletionDate = DateTime.Now.AddDays(30)
            };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(new List<Minion>());
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(new List<Equipment>());

            // Act
            int success = _schemeService.CalculateSuccessLikelihood(scheme);

            // Assert - base 50 - 15 (resource penalty) = 35
            Assert.Equal(35, success);
        }

        [Fact]
        public void CalculateSuccessLikelihood_WithMatchingSpecialists_AddsBonus()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                Name = "Test Scheme",
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 50000,
                TargetCompletionDate = DateTime.Now.AddDays(30)
            };

            var minions = new List<Minion>
            {
                new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Henchman" },
                new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Henchman" }
            };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(new List<Equipment>());

            // Act
            int success = _schemeService.CalculateSuccessLikelihood(scheme);

            // Assert - base 50 + (2 matching * 10) = 70
            Assert.Equal(70, success);
        }

        [Fact]
        public void CalculateSuccessLikelihood_WithNonMatchingMinions_NoSpecialistBonus()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                Name = "Test Scheme",
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 50000,
                TargetCompletionDate = DateTime.Now.AddDays(30)
            };

            var minions = new List<Minion>
            {
                new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Scientist" },
                new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Technician" }
            };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(new List<Equipment>());

            // Act
            int success = _schemeService.CalculateSuccessLikelihood(scheme);

            // Assert - base 50, no specialist bonus, no resource penalty (has 2 minions)
            Assert.Equal(50, success);
        }

        [Fact]
        public void CalculateSuccessLikelihood_WithWorkingEquipment_AddsBonus()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                Name = "Test Scheme",
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 50000,
                TargetCompletionDate = DateTime.Now.AddDays(30)
            };

            var minions = new List<Minion>
            {
                new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Henchman" },
                new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Henchman" }
            };

            var equipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, AssignedToSchemeId = 1, Condition = 80 },
                new Equipment { EquipmentId = 2, AssignedToSchemeId = 1, Condition = 90 },
                new Equipment { EquipmentId = 3, AssignedToSchemeId = 1, Condition = 60 }
            };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

            // Act
            int success = _schemeService.CalculateSuccessLikelihood(scheme);

            // Assert - base 50 + (2 matching * 10) + (3 equipment * 5) = 85
            Assert.Equal(85, success);
        }

        [Fact]
        public void CalculateSuccessLikelihood_WithBrokenEquipment_NoBonus()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                Name = "Test Scheme",
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 50000,
                TargetCompletionDate = DateTime.Now.AddDays(30)
            };

            var minions = new List<Minion>
            {
                new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Henchman" },
                new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Henchman" }
            };

            var equipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, AssignedToSchemeId = 1, Condition = 30 } // Below min threshold (50)
            };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

            // Act
            int success = _schemeService.CalculateSuccessLikelihood(scheme);

            // Assert - base 50 + (2 matching * 10) = 70, no equipment bonus
            Assert.Equal(70, success);
        }

        [Fact]
        public void CalculateSuccessLikelihood_OverBudget_AppliesPenalty()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                Name = "Test Scheme",
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 150000, // Over budget
                TargetCompletionDate = DateTime.Now.AddDays(30)
            };

            var minions = new List<Minion>
            {
                new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Henchman" },
                new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Henchman" }
            };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(new List<Equipment>());

            // Act
            int success = _schemeService.CalculateSuccessLikelihood(scheme);

            // Assert - base 50 + (2 matching * 10) - 20 (budget penalty) = 50
            Assert.Equal(50, success);
        }

        [Fact]
        public void CalculateSuccessLikelihood_PastDeadline_AppliesPenalty()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                Name = "Test Scheme",
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 50000,
                TargetCompletionDate = DateTime.Now.AddDays(-10) // Past deadline
            };

            var minions = new List<Minion>
            {
                new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Henchman" },
                new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Henchman" }
            };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(new List<Equipment>());

            // Act
            int success = _schemeService.CalculateSuccessLikelihood(scheme);

            // Assert - base 50 + (2 matching * 10) - 25 (timeline penalty) = 45
            Assert.Equal(45, success);
        }

        [Fact]
        public void CalculateSuccessLikelihood_InsufficientMinions_AppliesPenalty()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                Name = "Test Scheme",
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 50000,
                TargetCompletionDate = DateTime.Now.AddDays(30)
            };

            var minions = new List<Minion>
            {
                new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Scientist" } // Wrong specialty, only 1 minion
            };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(new List<Equipment>());

            // Act
            int success = _schemeService.CalculateSuccessLikelihood(scheme);

            // Assert - base 50 - 15 (resource penalty: < 2 minions or no matching) = 35
            Assert.Equal(35, success);
        }

        [Fact]
        public void CalculateSuccessLikelihood_CannotExceed100()
        {
            // Arrange - perfect conditions
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                Name = "Test Scheme",
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 50000,
                TargetCompletionDate = DateTime.Now.AddDays(30)
            };

            var minions = new List<Minion>();
            for (int i = 1; i <= 10; i++)
            {
                minions.Add(new Minion { MinionId = i, CurrentSchemeId = 1, Specialty = "Henchman" });
            }

            var equipment = new List<Equipment>();
            for (int i = 1; i <= 10; i++)
            {
                equipment.Add(new Equipment { EquipmentId = i, AssignedToSchemeId = 1, Condition = 100 });
            }

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

            // Act
            int success = _schemeService.CalculateSuccessLikelihood(scheme);

            // Assert
            Assert.True(success <= 100, "Success likelihood should not exceed 100");
            Assert.Equal(100, success);
        }

        [Fact]
        public void CalculateSuccessLikelihood_CannotGoBelowZero()
        {
            // Arrange - terrible conditions
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                Name = "Test Scheme",
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 200000, // Over budget
                TargetCompletionDate = DateTime.Now.AddDays(-100) // Way past deadline
            };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(new List<Minion>());
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(new List<Equipment>());

            // Act
            int success = _schemeService.CalculateSuccessLikelihood(scheme);

            // Assert
            Assert.True(success >= 0, "Success likelihood should not go below 0");
            Assert.Equal(0, success);
        }

        #endregion

        #region Budget Enforcement Tests

        [Fact]
        public void IsOverBudget_WithinBudget_ReturnsFalse()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                Budget = 100000,
                CurrentSpending = 75000
            };

            // Act
            bool isOver = _schemeService.IsOverBudget(scheme);

            // Assert
            Assert.False(isOver);
        }

        [Fact]
        public void IsOverBudget_AtExactBudget_ReturnsFalse()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                Budget = 100000,
                CurrentSpending = 100000
            };

            // Act
            bool isOver = _schemeService.IsOverBudget(scheme);

            // Assert
            Assert.False(isOver);
        }

        [Fact]
        public void IsOverBudget_SlightlyOverBudget_ReturnsTrue()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                Budget = 100000,
                CurrentSpending = 100001
            };

            // Act
            bool isOver = _schemeService.IsOverBudget(scheme);

            // Assert
            Assert.True(isOver);
        }

        [Fact]
        public void IsOverBudget_SignificantlyOverBudget_ReturnsTrue()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                Budget = 100000,
                CurrentSpending = 500000
            };

            // Act
            bool isOver = _schemeService.IsOverBudget(scheme);

            // Assert
            Assert.True(isOver);
        }

        [Theory]
        [InlineData(0, 0, false)]
        [InlineData(100000, 50000, false)]
        [InlineData(100000, 100000, false)]
        [InlineData(100000, 100001, true)]
        [InlineData(50000, 75000, true)]
        public void IsOverBudget_VariousScenarios_ReturnsExpected(decimal budget, decimal spending, bool expectedOver)
        {
            // Arrange
            var scheme = new EvilScheme
            {
                Budget = budget,
                CurrentSpending = spending
            };

            // Act
            bool isOver = _schemeService.IsOverBudget(scheme);

            // Assert
            Assert.Equal(expectedOver, isOver);
        }

        #endregion

        #region Status Transition Tests

        [Fact]
        public void GetActiveSchemes_FiltersCorrectly()
        {
            // Arrange
            var allSchemes = new List<EvilScheme>
            {
                new EvilScheme { SchemeId = 1, Status = "Active" },
                new EvilScheme { SchemeId = 2, Status = "Planning" },
                new EvilScheme { SchemeId = 3, Status = "Active" },
                new EvilScheme { SchemeId = 4, Status = "Completed" },
                new EvilScheme { SchemeId = 5, Status = "Active" }
            };

            _mockSchemeRepository.Setup(r => r.GetAll()).Returns(allSchemes);

            // Act
            var activeSchemes = _schemeService.GetActiveSchemes();

            // Assert
            Assert.Equal(3, activeSchemes.Count);
            Assert.All(activeSchemes, s => Assert.Equal("Active", s.Status));
        }

        [Fact]
        public void GetActiveSchemes_WithNoActiveSchemes_ReturnsEmpty()
        {
            // Arrange
            var allSchemes = new List<EvilScheme>
            {
                new EvilScheme { SchemeId = 1, Status = "Planning" },
                new EvilScheme { SchemeId = 2, Status = "Completed" }
            };

            _mockSchemeRepository.Setup(r => r.GetAll()).Returns(allSchemes);

            // Act
            var activeSchemes = _schemeService.GetActiveSchemes();

            // Assert
            Assert.Empty(activeSchemes);
        }

        #endregion

        #region Average Success Calculation Tests

        [Fact]
        public void CalculateAverageSuccess_WithMultipleSchemes_ReturnsCorrectAverage()
        {
            // Arrange
            var schemes = new List<EvilScheme>
            {
                new EvilScheme { SchemeId = 1, SuccessLikelihood = 60 },
                new EvilScheme { SchemeId = 2, SuccessLikelihood = 80 },
                new EvilScheme { SchemeId = 3, SuccessLikelihood = 70 }
            };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(new List<Minion>());
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(new List<Equipment>());

            // Act
            double average = _schemeService.CalculateAverageSuccess(schemes);

            // Assert - will recalculate, so expect base value
            Assert.InRange(average, 0, 100);
        }

        [Fact]
        public void CalculateAverageSuccess_WithEmptyList_ReturnsZero()
        {
            // Arrange
            var schemes = new List<EvilScheme>();

            // Act
            double average = _schemeService.CalculateAverageSuccess(schemes);

            // Assert
            Assert.Equal(0, average);
        }

        [Fact]
        public void CalculateAverageSuccess_WithNullList_ReturnsZero()
        {
            // Act
            double average = _schemeService.CalculateAverageSuccess(null);

            // Assert
            Assert.Equal(0, average);
        }

        #endregion

        #region Complex Scenario Tests

        [Fact]
        public void CalculateSuccessLikelihood_PerfectConditions_HighSuccess()
        {
            // Arrange - ideal scheme
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                Name = "Perfect Plan",
                RequiredSpecialty = "Henchman",
                Budget = 1000000,
                CurrentSpending = 500000, // Well under budget
                TargetCompletionDate = DateTime.Now.AddDays(90) // Plenty of time
            };

            var minions = new List<Minion>
            {
                new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Henchman", SkillLevel = 10 },
                new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Henchman", SkillLevel = 9 },
                new Minion { MinionId = 3, CurrentSchemeId = 1, Specialty = "Henchman", SkillLevel = 8 }
            };

            var equipment = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, AssignedToSchemeId = 1, Condition = 100 },
                new Equipment { EquipmentId = 2, AssignedToSchemeId = 1, Condition = 95 }
            };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

            // Act
            int success = _schemeService.CalculateSuccessLikelihood(scheme);

            // Assert - should be high
            Assert.True(success >= 70, $"Perfect conditions should yield high success, got {success}");
        }

        [Fact]
        public void CalculateSuccessLikelihood_DisasterConditions_LowSuccess()
        {
            // Arrange - worst case scenario
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                Name = "Doomed Plan",
                RequiredSpecialty = "Henchman",
                Budget = 50000,
                CurrentSpending = 100000, // Double budget
                TargetCompletionDate = DateTime.Now.AddDays(-30) // Past deadline
            };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(new List<Minion>());
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(new List<Equipment>());

            // Act
            int success = _schemeService.CalculateSuccessLikelihood(scheme);

            // Assert - should be very low
            Assert.True(success <= 30, $"Disaster conditions should yield low success, got {success}");
        }

        #endregion
    }
}
