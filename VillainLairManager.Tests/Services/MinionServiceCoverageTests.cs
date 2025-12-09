using Moq;
using System;
using System.Collections.Generic;
using VillainLairManager.Models;
using VillainLairManager.Repositories;
using VillainLairManager.Services;
using Xunit;

namespace VillainLairManager.Tests.Services
{
    /// <summary>
    /// Additional coverage tests for MinionService CRUD and validation operations
    /// </summary>
    public class MinionServiceCoverageTests
    {
        private readonly Mock<IMinionRepository> _mockMinionRepository;
        private readonly MinionService _minionService;

        public MinionServiceCoverageTests()
        {
            _mockMinionRepository = new Mock<IMinionRepository>();
            _minionService = new MinionService(_mockMinionRepository.Object);
        }

        #region Validation Tests

        [Fact]
        public void ValidateMinion_WithValidData_ReturnsTrue()
        {
            // Act
            var result = _minionService.ValidateMinion("Bob", "Henchman", 5, 5000, 50);

            // Assert
            Assert.True(result.isValid);
            Assert.Empty(result.errorMessage);
        }

        [Fact]
        public void ValidateMinion_WithEmptyName_ReturnsFalse()
        {
            // Act
            var result = _minionService.ValidateMinion("", "Henchman", 5, 5000, 50);

            // Assert
            Assert.False(result.isValid);
            Assert.Contains("Name is required", result.errorMessage);
        }

        [Fact]
        public void ValidateMinion_WithInvalidSpecialty_ReturnsFalse()
        {
            // Act
            var result = _minionService.ValidateMinion("Bob", "InvalidSpecialty", 5, 5000, 50);

            // Assert
            Assert.False(result.isValid);
            Assert.Contains("Invalid specialty", result.errorMessage);
        }

        [Fact]
        public void ValidateMinion_WithSkillLevelTooLow_ReturnsFalse()
        {
            // Act
            var result = _minionService.ValidateMinion("Bob", "Henchman", 0, 5000, 50);

            // Assert
            Assert.False(result.isValid);
            Assert.Contains("Skill level must be between", result.errorMessage);
        }

        [Fact]
        public void ValidateMinion_WithSkillLevelTooHigh_ReturnsFalse()
        {
            // Act
            var result = _minionService.ValidateMinion("Bob", "Henchman", 11, 5000, 50);

            // Assert
            Assert.False(result.isValid);
            Assert.Contains("Skill level must be between", result.errorMessage);
        }

        [Fact]
        public void ValidateMinion_WithNegativeSalary_ReturnsFalse()
        {
            // Act
            var result = _minionService.ValidateMinion("Bob", "Henchman", 5, -1000, 50);

            // Assert
            Assert.False(result.isValid);
            Assert.Contains("Salary cannot be negative", result.errorMessage);
        }

        [Fact]
        public void ValidateMinion_WithLoyaltyTooLow_ReturnsFalse()
        {
            // Act
            var result = _minionService.ValidateMinion("Bob", "Henchman", 5, 5000, -1);

            // Assert
            Assert.False(result.isValid);
            Assert.Contains("Loyalty must be between", result.errorMessage);
        }

        [Fact]
        public void ValidateMinion_WithLoyaltyTooHigh_ReturnsFalse()
        {
            // Act
            var result = _minionService.ValidateMinion("Bob", "Henchman", 5, 5000, 101);

            // Assert
            Assert.False(result.isValid);
            Assert.Contains("Loyalty must be between", result.errorMessage);
        }

        #endregion

        #region Create Tests

        [Fact]
        public void CreateMinion_WithValidData_ReturnsSuccess()
        {
            // Arrange
            _mockMinionRepository.Setup(r => r.Insert(It.IsAny<Minion>()));

            // Act
            var result = _minionService.CreateMinion("Bob", "Henchman", 5, 5000, 60, 1, null, null);

            // Assert
            Assert.True(result.success);
            Assert.Equal("Minion added successfully!", result.message);
            Assert.NotNull(result.minion);
            Assert.Equal("Bob", result.minion.Name);
            Assert.Equal("Grumpy", result.minion.MoodStatus); // Loyalty 60 is between 40-70 = Grumpy
        }

        [Fact]
        public void CreateMinion_WithInvalidData_ReturnsFailure()
        {
            // Act
            var result = _minionService.CreateMinion("", "Henchman", 5, 5000, 60, null, null, null);

            // Assert
            Assert.False(result.success);
            Assert.Contains("Name is required", result.message);
            Assert.Null(result.minion);
        }

        [Fact]
        public void CreateMinion_WhenRepositoryThrows_ReturnsFailure()
        {
            // Arrange
            _mockMinionRepository.Setup(r => r.Insert(It.IsAny<Minion>()))
                .Throws(new Exception("Database error"));

            // Act
            var result = _minionService.CreateMinion("Bob", "Henchman", 5, 5000, 60, null, null, null);

            // Assert
            Assert.False(result.success);
            Assert.Contains("Error adding minion", result.message);
            Assert.Null(result.minion);
        }

        [Fact]
        public void CreateMinion_WithProvidedMood_UsesMoodProvided()
        {
            // Arrange
            _mockMinionRepository.Setup(r => r.Insert(It.IsAny<Minion>()));

            // Act
            var result = _minionService.CreateMinion("Bob", "Henchman", 5, 5000, 60, null, null, "Custom Mood");

            // Assert
            Assert.True(result.success);
            Assert.Equal("Custom Mood", result.minion.MoodStatus);
        }

        #endregion

        #region Update Tests

        [Fact]
        public void UpdateMinion_WithValidData_ReturnsSuccess()
        {
            // Arrange
            _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()));

            // Act
            var result = _minionService.UpdateMinion(1, "Bob Updated", "Henchman", 6, 6000, 70, 1, 1, null);

            // Assert
            Assert.True(result.success);
            Assert.Equal("Minion updated successfully!", result.message);
            _mockMinionRepository.Verify(r => r.Update(It.Is<Minion>(m => 
                m.Name == "Bob Updated" && 
                m.SkillLevel == 6 && 
                m.SalaryDemand == 6000)), Times.Once);
        }

        [Fact]
        public void UpdateMinion_WithInvalidData_ReturnsFailure()
        {
            // Act
            var result = _minionService.UpdateMinion(1, "", "Henchman", 5, 5000, 60, null, null, null);

            // Assert
            Assert.False(result.success);
            Assert.Contains("Name is required", result.message);
        }

        [Fact]
        public void UpdateMinion_WhenRepositoryThrows_ReturnsFailure()
        {
            // Arrange
            _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()))
                .Throws(new Exception("Database error"));

            // Act
            var result = _minionService.UpdateMinion(1, "Bob", "Henchman", 5, 5000, 60, null, null, null);

            // Assert
            Assert.False(result.success);
            Assert.Contains("Error updating minion", result.message);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public void DeleteMinion_WithValidId_ReturnsSuccess()
        {
            // Arrange
            _mockMinionRepository.Setup(r => r.Delete(1));

            // Act
            var result = _minionService.DeleteMinion(1);

            // Assert
            Assert.True(result.success);
            Assert.Equal("Minion deleted successfully!", result.message);
            _mockMinionRepository.Verify(r => r.Delete(1), Times.Once);
        }

        [Fact]
        public void DeleteMinion_WhenRepositoryThrows_ReturnsFailure()
        {
            // Arrange
            _mockMinionRepository.Setup(r => r.Delete(1))
                .Throws(new Exception("Database error"));

            // Act
            var result = _minionService.DeleteMinion(1);

            // Assert
            Assert.False(result.success);
            Assert.Contains("Error deleting minion", result.message);
        }

        #endregion

        #region Loyalty Update Tests

        [Fact]
        public void UpdateLoyalty_WhenPaidFully_IncreasesLoyalty()
        {
            // Arrange
            var minion = new Minion
            {
                MinionId = 1,
                Name = "Bob",
                LoyaltyScore = 50,
                SalaryDemand = 5000
            };
            _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()));

            // Act
            _minionService.UpdateLoyalty(minion, 5000);

            // Assert
            Assert.True(minion.LoyaltyScore > 50); // Should increase
            _mockMinionRepository.Verify(r => r.Update(minion), Times.Once);
        }

        [Fact]
        public void UpdateLoyalty_WhenUnderpaid_DecreasesLoyalty()
        {
            // Arrange
            var minion = new Minion
            {
                MinionId = 1,
                Name = "Bob",
                LoyaltyScore = 50,
                SalaryDemand = 5000
            };
            _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()));

            // Act
            _minionService.UpdateLoyalty(minion, 3000);

            // Assert
            Assert.True(minion.LoyaltyScore < 50); // Should decrease
            _mockMinionRepository.Verify(r => r.Update(minion), Times.Once);
        }

        [Fact]
        public void UpdateLoyalty_ClampsToMaximum()
        {
            // Arrange
            var minion = new Minion
            {
                MinionId = 1,
                Name = "Bob",
                LoyaltyScore = 95,
                SalaryDemand = 5000
            };
            _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()));

            // Act
            _minionService.UpdateLoyalty(minion, 10000); // Overpaid

            // Assert
            Assert.True(minion.LoyaltyScore <= 100); // Should not exceed 100
            _mockMinionRepository.Verify(r => r.Update(minion), Times.Once);
        }

        [Fact]
        public void UpdateLoyalty_ClampsToMinimum()
        {
            // Arrange
            var minion = new Minion
            {
                MinionId = 1,
                Name = "Bob",
                LoyaltyScore = 5,
                SalaryDemand = 5000
            };
            _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()));

            // Act
            _minionService.UpdateLoyalty(minion, 0); // Not paid

            // Assert
            Assert.True(minion.LoyaltyScore >= 0); // Should not go below 0
            _mockMinionRepository.Verify(r => r.Update(minion), Times.Once);
        }

        [Fact]
        public void UpdateLoyalty_WithNullMinion_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _minionService.UpdateLoyalty(null, 5000));
        }

        [Fact]
        public void UpdateLoyalty_UpdatesMoodBasedOnNewLoyalty()
        {
            // Arrange
            var minion = new Minion
            {
                MinionId = 1,
                Name = "Bob",
                LoyaltyScore = 38, // Just below low threshold
                SalaryDemand = 5000,
                MoodStatus = "Plotting Betrayal"
            };
            _mockMinionRepository.Setup(r => r.Update(It.IsAny<Minion>()));

            // Act - Pay well to increase loyalty (adds 3)
            _minionService.UpdateLoyalty(minion, 10000);

            // Assert - Mood should change from Betrayal to Grumpy
            Assert.NotEqual("Plotting Betrayal", minion.MoodStatus);
            Assert.Equal("Grumpy", minion.MoodStatus);
        }

        #endregion

        #region Repository Access Tests

        [Fact]
        public void GetAllMinions_CallsRepository()
        {
            // Arrange
            var minions = new List<Minion>
            {
                new Minion { MinionId = 1, Name = "Bob" },
                new Minion { MinionId = 2, Name = "Alice" }
            };
            _mockMinionRepository.Setup(r => r.GetAll()).Returns(minions);

            // Act
            var result = _minionService.GetAllMinions();

            // Assert
            Assert.Equal(2, result.Count);
            _mockMinionRepository.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public void GetMinionById_CallsRepository()
        {
            // Arrange
            var minion = new Minion { MinionId = 1, Name = "Bob" };
            _mockMinionRepository.Setup(r => r.GetById(1)).Returns(minion);

            // Act
            var result = _minionService.GetMinionById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.MinionId);
            _mockMinionRepository.Verify(r => r.GetById(1), Times.Once);
        }

        #endregion
    }
}
