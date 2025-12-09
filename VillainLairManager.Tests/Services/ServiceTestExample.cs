using Moq;
using VillainLairManager.Models;
using VillainLairManager.Repositories;
using VillainLairManager.Services;
using Xunit;

namespace VillainLairManager.Tests.Services
{
    /// <summary>
    /// Example test class demonstrating xUnit with Moq
    /// This shows the test project is properly configured
    /// </summary>
    public class ServiceTestExample
    {
        [Fact]
        public void MinionService_CanBeInstantiated()
        {
            // Arrange
            var mockRepo = new Mock<IMinionRepository>();
            
            // Act
            var service = new MinionService(mockRepo.Object);
            
            // Assert
            Assert.NotNull(service);
        }

        [Theory]
        [InlineData(1, 2, 3)]
        [InlineData(5, 5, 10)]
        [InlineData(0, 100, 100)]
        public void Math_Addition_Works(int a, int b, int expected)
        {
            // Act
            int result = a + b;
            
            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Mock_Repository_CanSetupAndVerify()
        {
            // Arrange
            var mockRepo = new Mock<IMinionRepository>();
            var testMinions = new List<Minion>
            {
                new Minion { MinionId = 1, Name = "Test Minion" }
            };
            mockRepo.Setup(r => r.GetAll()).Returns(testMinions);
            
            var service = new MinionService(mockRepo.Object);
            
            // Act
            var result = service.GetAllMinions();
            
            // Assert
            Assert.Equal(testMinions, result);
            mockRepo.Verify(r => r.GetAll(), Times.Once);
        }
    }
}
