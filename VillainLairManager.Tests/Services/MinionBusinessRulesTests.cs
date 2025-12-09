using Moq;
using VillainLairManager.Models;
using VillainLairManager.Repositories;
using VillainLairManager.Services;
using Xunit;

namespace VillainLairManager.Tests.Services
{
    /// <summary>
    /// Comprehensive tests for minion business rules:
    /// - Loyalty decay
    /// - Mood determination
    /// - Skill matching
    /// </summary>
    public class MinionBusinessRulesTests
    {
        private readonly Mock<IMinionRepository> _mockMinionRepository;
        private readonly MinionService _minionService;

        public MinionBusinessRulesTests()
        {
            _mockMinionRepository = new Mock<IMinionRepository>();
            _minionService = new MinionService(_mockMinionRepository.Object);
        }

        #region Mood Determination Tests

        [Fact]
        public void CalculateMood_WithZeroLoyalty_ReturnsPlottingBetrayal()
        {
            // Arrange - minimum loyalty
            int loyalty = 0;

            // Act
            string mood = _minionService.CalculateMood(loyalty);

            // Assert
            Assert.Equal("Plotting Betrayal", mood);
        }

        [Fact]
        public void CalculateMood_WithLowLoyalty_ReturnsPlottingBetrayal()
        {
            // Arrange - below threshold (40)
            int loyalty = 39;

            // Act
            string mood = _minionService.CalculateMood(loyalty);

            // Assert
            Assert.Equal("Plotting Betrayal", mood);
        }

        [Fact]
        public void CalculateMood_AtLowLoyaltyThreshold_ReturnsGrumpy()
        {
            // Arrange - at threshold (40)
            int loyalty = 40;

            // Act
            string mood = _minionService.CalculateMood(loyalty);

            // Assert
            Assert.Equal("Grumpy", mood);
        }

        [Fact]
        public void CalculateMood_WithMediumLoyalty_ReturnsGrumpy()
        {
            // Arrange - between thresholds (40-70)
            int loyalty = 55;

            // Act
            string mood = _minionService.CalculateMood(loyalty);

            // Assert
            Assert.Equal("Grumpy", mood);
        }

        [Fact]
        public void CalculateMood_AtHighLoyaltyThreshold_ReturnsGrumpy()
        {
            // Arrange - at threshold (70)
            int loyalty = 70;

            // Act
            string mood = _minionService.CalculateMood(loyalty);

            // Assert
            Assert.Equal("Grumpy", mood);
        }

        [Fact]
        public void CalculateMood_AboveHighLoyaltyThreshold_ReturnsHappy()
        {
            // Arrange - above threshold (> 70)
            int loyalty = 71;

            // Act
            string mood = _minionService.CalculateMood(loyalty);

            // Assert
            Assert.Equal("Happy", mood);
        }

        [Fact]
        public void CalculateMood_WithMaxLoyalty_ReturnsHappy()
        {
            // Arrange - maximum loyalty
            int loyalty = 100;

            // Act
            string mood = _minionService.CalculateMood(loyalty);

            // Assert
            Assert.Equal("Happy", mood);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(20)]
        [InlineData(39)]
        public void CalculateMood_BelowLowThreshold_AlwaysReturnsPlottingBetrayal(int loyalty)
        {
            // Act
            string mood = _minionService.CalculateMood(loyalty);

            // Assert
            Assert.Equal("Plotting Betrayal", mood);
        }

        [Theory]
        [InlineData(40)]
        [InlineData(55)]
        [InlineData(70)]
        public void CalculateMood_BetweenThresholds_AlwaysReturnsGrumpy(int loyalty)
        {
            // Act
            string mood = _minionService.CalculateMood(loyalty);

            // Assert
            Assert.Equal("Grumpy", mood);
        }

        [Theory]
        [InlineData(71)]
        [InlineData(85)]
        [InlineData(100)]
        public void CalculateMood_AboveHighThreshold_AlwaysReturnsHappy(int loyalty)
        {
            // Act
            string mood = _minionService.CalculateMood(loyalty);

            // Assert
            Assert.Equal("Happy", mood);
        }

        #endregion

        #region Loyalty Decay Tests

        [Fact]
        public void UpdateLoyalty_WithPositiveChange_IncreasesLoyalty()
        {
            // Arrange
            var minion = new Minion
            {
                MinionId = 1,
                Name = "Test Minion",
                LoyaltyScore = 50
            };
            _mockMinionRepository.Setup(r => r.GetById(1)).Returns(minion);

            // Act
            _minionService.UpdateLoyalty(minion, 60000); // Paid more than demanded

            // Assert
            Assert.True(minion.LoyaltyScore > 50);
            _mockMinionRepository.Verify(r => r.Update(minion), Times.Once);
        }

        [Fact]
        public void UpdateLoyalty_WithNegativeChange_DecreasesLoyalty()
        {
            // Arrange
            var minion = new Minion
            {
                MinionId = 1,
                Name = "Test Minion",
                LoyaltyScore = 50,
                SalaryDemand = 50000
            };
            _mockMinionRepository.Setup(r => r.GetById(1)).Returns(minion);

            // Act
            _minionService.UpdateLoyalty(minion, 30000); // Paid less than demanded

            // Assert
            Assert.True(minion.LoyaltyScore < 50);
            _mockMinionRepository.Verify(r => r.Update(minion), Times.Once);
        }

        [Fact]
        public void UpdateLoyalty_CannotExceedMaximum()
        {
            // Arrange
            var minion = new Minion
            {
                MinionId = 1,
                Name = "Test Minion",
                LoyaltyScore = 95,
                SalaryDemand = 10000
            };
            _mockMinionRepository.Setup(r => r.GetById(1)).Returns(minion);

            // Act
            _minionService.UpdateLoyalty(minion, 100000); // Huge overpayment

            // Assert
            Assert.True(minion.LoyaltyScore <= 100, "Loyalty should not exceed 100");
        }

        [Fact]
        public void UpdateLoyalty_CannotGoBelowMinimum()
        {
            // Arrange
            var minion = new Minion
            {
                MinionId = 1,
                Name = "Test Minion",
                LoyaltyScore = 5,
                SalaryDemand = 50000
            };
            _mockMinionRepository.Setup(r => r.GetById(1)).Returns(minion);

            // Act
            _minionService.UpdateLoyalty(minion, 0); // No payment

            // Assert
            Assert.True(minion.LoyaltyScore >= 0, "Loyalty should not go below 0");
        }

        #endregion

        #region Skill Matching Tests

        [Fact]
        public void ValidateMinion_WithSpecialistSkillLevel_AcceptsHighSkill()
        {
            // Arrange - skill level 8+ is specialist
            string name = "Master Evil";
            string specialty = "Henchman";
            int skillLevel = 8;
            decimal salary = 100000;
            int loyalty = 80;

            // Act
            var (isValid, errorMessage) = _minionService.ValidateMinion(name, specialty, skillLevel, salary, loyalty);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateMinion_WithMaxSkillLevel_IsValid()
        {
            // Arrange
            string name = "Elite Specialist";
            string specialty = "Henchman";
            int skillLevel = 10; // Maximum
            decimal salary = 150000;
            int loyalty = 90;

            // Act
            var (isValid, errorMessage) = _minionService.ValidateMinion(name, specialty, skillLevel, salary, loyalty);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateMinion_WithMinSkillLevel_IsValid()
        {
            // Arrange
            string name = "Intern Minion";
            string specialty = "Henchman";
            int skillLevel = 1; // Minimum
            decimal salary = 5000;
            int loyalty = 40;

            // Act
            var (isValid, errorMessage) = _minionService.ValidateMinion(name, specialty, skillLevel, salary, loyalty);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateMinion_WithSkillLevelTooHigh_ReturnsInvalid()
        {
            // Arrange
            string name = "Over Skilled";
            string specialty = "Henchman";
            int skillLevel = 11; // Above maximum
            decimal salary = 50000;
            int loyalty = 50;

            // Act
            var (isValid, errorMessage) = _minionService.ValidateMinion(name, specialty, skillLevel, salary, loyalty);

            // Assert
            Assert.False(isValid);
            Assert.Contains("Skill level must be between", errorMessage);
        }

        [Fact]
        public void ValidateMinion_WithSkillLevelTooLow_ReturnsInvalid()
        {
            // Arrange
            string name = "Under Skilled";
            string specialty = "Henchman";
            int skillLevel = 0; // Below minimum
            decimal salary = 50000;
            int loyalty = 50;

            // Act
            var (isValid, errorMessage) = _minionService.ValidateMinion(name, specialty, skillLevel, salary, loyalty);

            // Assert
            Assert.False(isValid);
            Assert.Contains("Skill level must be between", errorMessage);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(7)]
        public void ValidateMinion_WithRegularSkillLevels_IsValid(int skillLevel)
        {
            // Arrange
            string name = "Regular Minion";
            string specialty = "Henchman";
            decimal salary = 50000;
            int loyalty = 50;

            // Act
            var (isValid, errorMessage) = _minionService.ValidateMinion(name, specialty, skillLevel, salary, loyalty);

            // Assert
            Assert.True(isValid, $"Skill level {skillLevel} should be valid");
        }

        [Theory]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        public void ValidateMinion_WithSpecialistSkillLevels_IsValid(int skillLevel)
        {
            // Arrange
            string name = "Specialist Minion";
            string specialty = "Henchman";
            decimal salary = 100000;
            int loyalty = 80;

            // Act
            var (isValid, errorMessage) = _minionService.ValidateMinion(name, specialty, skillLevel, salary, loyalty);

            // Assert
            Assert.True(isValid, $"Specialist skill level {skillLevel} should be valid");
        }

        #endregion

        #region Salary and Loyalty Interaction Tests

        [Fact]
        public void ValidateMinion_WithZeroSalary_IsValid()
        {
            // Arrange - unpaid intern
            string name = "Intern";
            string specialty = "Henchman";
            int skillLevel = 1;
            decimal salary = 0;
            int loyalty = 20; // Probably low

            // Act
            var (isValid, errorMessage) = _minionService.ValidateMinion(name, specialty, skillLevel, salary, loyalty);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateMinion_WithNegativeSalary_ReturnsInvalid()
        {
            // Arrange
            string name = "Debt Minion";
            string specialty = "Henchman";
            int skillLevel = 5;
            decimal salary = -1000;
            int loyalty = 50;

            // Act
            var (isValid, errorMessage) = _minionService.ValidateMinion(name, specialty, skillLevel, salary, loyalty);

            // Assert
            Assert.False(isValid);
            Assert.Contains("Salary cannot be negative", errorMessage);
        }

        #endregion
    }
}
