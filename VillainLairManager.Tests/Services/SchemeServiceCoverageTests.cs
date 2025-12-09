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
    /// Additional coverage tests for SchemeService operations
    /// </summary>
    public class SchemeServiceCoverageTests
    {
        private readonly Mock<IEvilSchemeRepository> _mockSchemeRepository;
        private readonly Mock<IMinionRepository> _mockMinionRepository;
        private readonly Mock<IEquipmentRepository> _mockEquipmentRepository;
        private readonly SchemeService _schemeService;

        public SchemeServiceCoverageTests()
        {
            _mockSchemeRepository = new Mock<IEvilSchemeRepository>();
            _mockMinionRepository = new Mock<IMinionRepository>();
            _mockEquipmentRepository = new Mock<IEquipmentRepository>();
            _schemeService = new SchemeService(
                _mockSchemeRepository.Object,
                _mockMinionRepository.Object,
                _mockEquipmentRepository.Object);
        }

        [Fact]
        public void UpdateSuccessLikelihood_CalculatesAndPersists()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                Name = "Test Scheme",
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 50000,
                TargetCompletionDate = DateTime.Now.AddDays(30),
                Status = "Active"
            };

            var minions = new List<Minion>
            {
                new Minion { CurrentSchemeId = 1, Specialty = "Henchman" }
            };

            var equipment = new List<Equipment>
            {
                new Equipment { AssignedToSchemeId = 1, Condition = 80 }
            };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);
            _mockSchemeRepository.Setup(r => r.Update(It.IsAny<EvilScheme>()));

            // Act
            _schemeService.UpdateSuccessLikelihood(scheme);

            // Assert
            Assert.True(scheme.SuccessLikelihood > 50); // Should have bonuses
            _mockSchemeRepository.Verify(r => r.Update(scheme), Times.Once);
        }

        [Fact]
        public void UpdateSuccessLikelihood_WithNullScheme_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _schemeService.UpdateSuccessLikelihood(null));
        }

        [Fact]
        public void IsOverBudget_WhenOverBudget_ReturnsTrue()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                Budget = 100000,
                CurrentSpending = 150000
            };

            // Act
            var result = _schemeService.IsOverBudget(scheme);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsOverBudget_WhenUnderBudget_ReturnsFalse()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                Budget = 100000,
                CurrentSpending = 80000
            };

            // Act
            var result = _schemeService.IsOverBudget(scheme);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsOverBudget_WithNullScheme_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _schemeService.IsOverBudget(null));
        }

        [Fact]
        public void GetAllSchemes_CallsRepository()
        {
            // Arrange
            var schemes = new List<EvilScheme>
            {
                new EvilScheme { SchemeId = 1, Name = "Scheme 1" },
                new EvilScheme { SchemeId = 2, Name = "Scheme 2" }
            };
            _mockSchemeRepository.Setup(r => r.GetAll()).Returns(schemes);

            // Act
            var result = _schemeService.GetAllSchemes();

            // Assert
            Assert.Equal(2, result.Count);
            _mockSchemeRepository.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public void GetSchemeById_CallsRepository()
        {
            // Arrange
            var scheme = new EvilScheme { SchemeId = 1, Name = "Test Scheme" };
            _mockSchemeRepository.Setup(r => r.GetById(1)).Returns(scheme);

            // Act
            var result = _schemeService.GetSchemeById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.SchemeId);
            _mockSchemeRepository.Verify(r => r.GetById(1), Times.Once);
        }

        [Fact]
        public void GetActiveSchemes_ReturnsOnlyActiveSchemes()
        {
            // Arrange
            var schemes = new List<EvilScheme>
            {
                new EvilScheme { SchemeId = 1, Name = "Active 1", Status = "Active" },
                new EvilScheme { SchemeId = 2, Name = "Completed", Status = "Completed" },
                new EvilScheme { SchemeId = 3, Name = "Active 2", Status = "Active" },
                new EvilScheme { SchemeId = 4, Name = "Failed", Status = "Failed" }
            };
            _mockSchemeRepository.Setup(r => r.GetAll()).Returns(schemes);

            // Act
            var result = _schemeService.GetActiveSchemes();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, s => Assert.Equal("Active", s.Status));
        }

        [Fact]
        public void CalculateAverageSuccess_WithMultipleSchemes_ReturnsAverage()
        {
            // Arrange
            var scheme1 = new EvilScheme
            {
                SchemeId = 1,
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 50000,
                TargetCompletionDate = DateTime.Now.AddDays(30)
            };
            var scheme2 = new EvilScheme
            {
                SchemeId = 2,
                RequiredSpecialty = "Technician",
                Budget = 200000,
                CurrentSpending = 100000,
                TargetCompletionDate = DateTime.Now.AddDays(60)
            };

            var schemes = new List<EvilScheme> { scheme1, scheme2 };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(new List<Minion>());
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(new List<Equipment>());

            // Act
            var result = _schemeService.CalculateAverageSuccess(schemes);

            // Assert
            Assert.True(result >= 0 && result <= 100);
        }

        [Fact]
        public void CalculateAverageSuccess_WithEmptyList_ReturnsZero()
        {
            // Act
            var result = _schemeService.CalculateAverageSuccess(new List<EvilScheme>());

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateAverageSuccess_WithNullList_ReturnsZero()
        {
            // Act
            var result = _schemeService.CalculateAverageSuccess(null);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateSuccessLikelihood_WithMatchingSpecialists_AddsBonus()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 50000,
                TargetCompletionDate = DateTime.Now.AddDays(30)
            };

            var minions = new List<Minion>
            {
                new Minion { CurrentSchemeId = 1, Specialty = "Henchman" },
                new Minion { CurrentSchemeId = 1, Specialty = "Henchman" }
            };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(new List<Equipment>());

            // Act
            var result = _schemeService.CalculateSuccessLikelihood(scheme);

            // Assert - Base 50 + 20 (2 matching specialists * 10)
            Assert.True(result >= 70);
        }

        [Fact]
        public void CalculateSuccessLikelihood_WithOperationalEquipment_AddsBonus()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 50000,
                TargetCompletionDate = DateTime.Now.AddDays(30)
            };

            var equipment = new List<Equipment>
            {
                new Equipment { AssignedToSchemeId = 1, Condition = 80 },
                new Equipment { AssignedToSchemeId = 1, Condition = 70 }
            };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(new List<Minion>());
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

            // Act
            var result = _schemeService.CalculateSuccessLikelihood(scheme);

            // Assert - Base 50 - 15 (resource penalty) + 10 (2 equipment * 5)
            Assert.True(result >= 35);
        }

        [Fact]
        public void CalculateSuccessLikelihood_WhenOverBudget_AppliesPenalty()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 150000, // Over budget
                TargetCompletionDate = DateTime.Now.AddDays(30)
            };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(new List<Minion>());
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(new List<Equipment>());

            // Act
            var result = _schemeService.CalculateSuccessLikelihood(scheme);

            // Assert - Should have budget penalty
            Assert.True(result < 50); // Less than base due to penalties
        }

        [Fact]
        public void CalculateSuccessLikelihood_WhenPastDeadline_AppliesPenalty()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 50000,
                TargetCompletionDate = DateTime.Now.AddDays(-10) // Past deadline
            };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(new List<Minion>());
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(new List<Equipment>());

            // Act
            var result = _schemeService.CalculateSuccessLikelihood(scheme);

            // Assert - Should have timeline penalty
            Assert.True(result < 50); // Less than base due to penalties
        }

        [Fact]
        public void CalculateSuccessLikelihood_ClampsToMaximum100()
        {
            // Arrange - Set up a super successful scheme
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                RequiredSpecialty = "Henchman",
                Budget = 1000000,
                CurrentSpending = 50000,
                TargetCompletionDate = DateTime.Now.AddDays(90)
            };

            // Many matching specialists
            var minions = Enumerable.Range(1, 10)
                .Select(i => new Minion { CurrentSchemeId = 1, Specialty = "Henchman" })
                .ToList();

            // Lots of equipment
            var equipment = Enumerable.Range(1, 10)
                .Select(i => new Equipment { AssignedToSchemeId = 1, Condition = 100 })
                .ToList();

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipment);

            // Act
            var result = _schemeService.CalculateSuccessLikelihood(scheme);

            // Assert
            Assert.Equal(100, result); // Should clamp to 100
        }

        [Fact]
        public void CalculateSuccessLikelihood_ClampsToMinimum0()
        {
            // Arrange - Terrible scheme
            var scheme = new EvilScheme
            {
                SchemeId = 1,
                RequiredSpecialty = "Henchman",
                Budget = 1000,
                CurrentSpending = 100000, // Way over budget
                TargetCompletionDate = DateTime.Now.AddDays(-100) // Way past deadline
            };

            _mockMinionRepository.Setup(r => r.GetAll()).Returns(new List<Minion>());
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(new List<Equipment>());

            // Act
            var result = _schemeService.CalculateSuccessLikelihood(scheme);

            // Assert
            Assert.Equal(0, result); // Should clamp to 0
        }
    }
}
