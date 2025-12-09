using Moq;
using VillainLairManager.Models;
using VillainLairManager.Repositories;
using VillainLairManager.Services;
using Xunit;

namespace VillainLairManager.Tests.Services
{
    public class MinionServiceTests
    {
        private readonly Mock<IMinionRepository> _mockMinionRepository;
        private readonly MinionService _minionService;

        public MinionServiceTests()
        {
            _mockMinionRepository = new Mock<IMinionRepository>();
            _minionService = new MinionService(_mockMinionRepository.Object);
        }

        [Fact]
        public void ValidateMinion_WithValidData_ReturnsTrue()
        {
            // Arrange
            string name = "Evil Bob";
            string specialty = "Henchman";  // Must be a valid specialty from config
            int skillLevel = 5;
            decimal salary = 50000;
            int loyalty = 75;

            // Act
            var (isValid, errorMessage) = _minionService.ValidateMinion(name, specialty, skillLevel, salary, loyalty);

            // Assert
            Assert.True(isValid, $"Validation failed: {errorMessage}");
            Assert.Empty(errorMessage);
        }

        [Theory]
        [InlineData("", "Henchman", 5, 50000, 75)]
        [InlineData("Evil Bob", "", 5, 50000, 75)]
        [InlineData("Evil Bob", "Henchman", -1, 50000, 75)]
        [InlineData("Evil Bob", "Henchman", 11, 50000, 75)]
        [InlineData("Evil Bob", "Henchman", 5, -1, 75)]
        [InlineData("Evil Bob", "Henchman", 5, 50000, -1)]
        [InlineData("Evil Bob", "Henchman", 5, 50000, 101)]
        public void ValidateMinion_WithInvalidData_ReturnsFalse(
            string name, string specialty, int skillLevel, decimal salary, int loyalty)
        {
            // Act
            var (isValid, errorMessage) = _minionService.ValidateMinion(name, specialty, skillLevel, salary, loyalty);

            // Assert
            Assert.False(isValid);
            Assert.NotEmpty(errorMessage);
        }

        [Theory]
        [InlineData(0, "Plotting Betrayal")]
        [InlineData(25, "Plotting Betrayal")]
        [InlineData(50, "Grumpy")]
        [InlineData(75, "Happy")]
        [InlineData(100, "Happy")]
        public void CalculateMood_WithVariousLoyalty_ReturnsCorrectMood(int loyalty, string expectedMood)
        {
            // Act
            string mood = _minionService.CalculateMood(loyalty);

            // Assert
            Assert.Equal(expectedMood, mood);
        }

        [Fact]
        public void CreateMinion_WithValidData_ReturnsSuccess()
        {
            // Arrange
            string name = "Evil Bob";
            string specialty = "Henchman";  // Valid specialty
            int skillLevel = 5;
            decimal salary = 50000;
            int loyalty = 75;
            string mood = "Happy";  // Changed to valid mood

            // Act
            var (success, message, minion) = _minionService.CreateMinion(
                name, specialty, skillLevel, salary, loyalty, null, null, mood);

            // Assert
            Assert.True(success, $"Creation failed: {message}");
            Assert.NotNull(minion);
            Assert.Equal(name, minion.Name);
            Assert.Equal(specialty, minion.Specialty);
            _mockMinionRepository.Verify(r => r.Insert(It.IsAny<Minion>()), Times.Once);
        }

        [Fact]
        public void GetAllMinions_ReturnsRepositoryData()
        {
            // Arrange
            var expectedMinions = new List<Minion>
            {
                new Minion { MinionId = 1, Name = "Evil Bob" },
                new Minion { MinionId = 2, Name = "Wicked Jane" }
            };
            _mockMinionRepository.Setup(r => r.GetAll()).Returns(expectedMinions);

            // Act
            var result = _minionService.GetAllMinions();

            // Assert
            Assert.Equal(expectedMinions, result);
            _mockMinionRepository.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public void UpdateLoyalty_DecreasesLoyaltyWhenUnderpaid()
        {
            // Arrange
            var minion = new Minion
            {
                MinionId = 1,
                Name = "Evil Bob",
                LoyaltyScore = 50,
                SalaryDemand = 50000
            };

            // Act - Pay less than demanded
            _minionService.UpdateLoyalty(minion, 40000);

            // Assert
            Assert.True(minion.LoyaltyScore < 50); // Loyalty should decrease
            _mockMinionRepository.Verify(r => r.Update(minion), Times.Once);
        }

        [Fact]
        public void UpdateLoyalty_IncreasesLoyaltyWhenOverpaid()
        {
            // Arrange
            var minion = new Minion
            {
                MinionId = 1,
                Name = "Evil Bob",
                LoyaltyScore = 50,
                SalaryDemand = 50000
            };

            // Act - Pay more than demanded
            _minionService.UpdateLoyalty(minion, 60000);

            // Assert
            Assert.True(minion.LoyaltyScore > 50); // Loyalty should increase
            _mockMinionRepository.Verify(r => r.Update(minion), Times.Once);
        }

        [Fact]
        public void UpdateLoyalty_CapsAt100()
        {
            // Arrange
            var minion = new Minion
            {
                MinionId = 1,
                Name = "Evil Bob",
                LoyaltyScore = 95,
                SalaryDemand = 50000
            };

            // Act - Overpay significantly
            _minionService.UpdateLoyalty(minion, 100000);

            // Assert
            Assert.True(minion.LoyaltyScore <= 100);
        }

        [Fact]
        public void DeleteMinion_CallsRepositoryDelete()
        {
            // Arrange
            int minionId = 1;
            _mockMinionRepository.Setup(r => r.GetById(minionId)).Returns(new Minion { MinionId = minionId });

            // Act
            var (success, message) = _minionService.DeleteMinion(minionId);

            // Assert
            Assert.True(success);
            _mockMinionRepository.Verify(r => r.Delete(minionId), Times.Once);
        }
    }
}
