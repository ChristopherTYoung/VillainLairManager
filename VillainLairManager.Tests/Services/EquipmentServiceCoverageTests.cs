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
    /// Additional coverage tests for EquipmentService CRUD and validation operations
    /// </summary>
    public class EquipmentServiceCoverageTests
    {
        private readonly Mock<IEquipmentRepository> _mockEquipmentRepository;
        private readonly Mock<IEvilSchemeRepository> _mockSchemeRepository;
        private readonly EquipmentService _equipmentService;

        public EquipmentServiceCoverageTests()
        {
            _mockEquipmentRepository = new Mock<IEquipmentRepository>();
            _mockSchemeRepository = new Mock<IEvilSchemeRepository>();
            _equipmentService = new EquipmentService(
                _mockEquipmentRepository.Object,
                _mockSchemeRepository.Object);
        }

        #region Validation Tests

        [Fact]
        public void ValidateEquipment_WithValidData_ReturnsTrue()
        {
            // Act
            var result = _equipmentService.ValidateEquipment("Death Ray", "Doomsday Device", 1000000, 50000);

            // Assert
            Assert.True(result.isValid);
            Assert.Empty(result.errorMessage);
        }

        [Fact]
        public void ValidateEquipment_WithEmptyName_ReturnsFalse()
        {
            // Act
            var result = _equipmentService.ValidateEquipment("", "Weapons", 1000, 100);

            // Assert
            Assert.False(result.isValid);
            Assert.Contains("name is required", result.errorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ValidateEquipment_WithEmptyCategory_ReturnsFalse()
        {
            // Act
            var result = _equipmentService.ValidateEquipment("Test Equipment", "", 1000, 100);

            // Assert
            Assert.False(result.isValid);
            Assert.Contains("category is required", result.errorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ValidateEquipment_WithNegativePrice_ReturnsFalse()
        {
            // Act
            var result = _equipmentService.ValidateEquipment("Test Equipment", "Weapons", -1000, 100);

            // Assert
            Assert.False(result.isValid);
            Assert.Contains("price cannot be negative", result.errorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ValidateEquipment_WithNegativeMaintenanceCost_ReturnsFalse()
        {
            // Act
            var result = _equipmentService.ValidateEquipment("Test Equipment", "Weapons", 1000, -100);

            // Assert
            Assert.False(result.isValid);
            Assert.Contains("maintenance cost cannot be negative", result.errorMessage, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region CRUD Operation Tests

        [Fact]
        public void CreateEquipment_WithValidData_ReturnsSuccess()
        {
            // Arrange
            _mockEquipmentRepository.Setup(r => r.Insert(It.IsAny<Equipment>()));

            // Act
            var result = _equipmentService.CreateEquipment("Laser Gun", "Weapons", 50000, 2000, false, 1);

            // Assert
            Assert.True(result.success);
            Assert.Equal("Equipment created successfully!", result.message);
            Assert.NotNull(result.equipment);
            Assert.Equal("Laser Gun", result.equipment.Name);
            Assert.Equal("Weapons", result.equipment.Category);
            Assert.Equal(100, result.equipment.Condition); // Default condition
        }

        [Fact]
        public void CreateEquipment_WithInvalidData_ReturnsFailure()
        {
            // Act
            var result = _equipmentService.CreateEquipment("", "Weapons", 50000, 2000, false, null);

            // Assert
            Assert.False(result.success);
            Assert.Contains("name is required", result.message, StringComparison.OrdinalIgnoreCase);
            Assert.Null(result.equipment);
        }

        [Fact]
        public void CreateEquipment_WhenRepositoryThrows_ReturnsFailure()
        {
            // Arrange
            _mockEquipmentRepository.Setup(r => r.Insert(It.IsAny<Equipment>()))
                .Throws(new Exception("Database error"));

            // Act
            var result = _equipmentService.CreateEquipment("Test", "Weapons", 1000, 100, false, null);

            // Assert
            Assert.False(result.success);
            Assert.Contains("Error creating equipment", result.message);
        }

        [Fact]
        public void UpdateEquipment_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var equipment = new Equipment
            {
                EquipmentId = 1,
                Name = "Updated Laser",
                Category = "Weapons",
                PurchasePrice = 60000,
                MaintenanceCost = 3000
            };
            _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

            // Act
            var result = _equipmentService.UpdateEquipment(equipment);

            // Assert
            Assert.True(result.success);
            Assert.Equal("Equipment updated successfully!", result.message);
            _mockEquipmentRepository.Verify(r => r.Update(equipment), Times.Once);
        }

        [Fact]
        public void UpdateEquipment_WithNullEquipment_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _equipmentService.UpdateEquipment(null));
        }

        [Fact]
        public void UpdateEquipment_WithInvalidData_ReturnsFailure()
        {
            // Arrange
            var equipment = new Equipment
            {
                EquipmentId = 1,
                Name = "",
                Category = "Weapons",
                PurchasePrice = 60000,
                MaintenanceCost = 3000
            };

            // Act
            var result = _equipmentService.UpdateEquipment(equipment);

            // Assert
            Assert.False(result.success);
            Assert.Contains("name is required", result.message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void DeleteEquipment_WithValidId_ReturnsSuccess()
        {
            // Arrange
            _mockEquipmentRepository.Setup(r => r.Delete(1));

            // Act
            var result = _equipmentService.DeleteEquipment(1);

            // Assert
            Assert.True(result.success);
            Assert.Equal("Equipment deleted successfully!", result.message);
            _mockEquipmentRepository.Verify(r => r.Delete(1), Times.Once);
        }

        [Fact]
        public void DeleteEquipment_WhenRepositoryThrows_ReturnsFailure()
        {
            // Arrange
            _mockEquipmentRepository.Setup(r => r.Delete(1))
                .Throws(new Exception("Database error"));

            // Act
            var result = _equipmentService.DeleteEquipment(1);

            // Assert
            Assert.False(result.success);
            Assert.Contains("Error deleting equipment", result.message);
        }

        #endregion

        #region Status Check Tests

        [Fact]
        public void IsOperational_WithHighCondition_ReturnsTrue()
        {
            // Arrange
            var equipment = new Equipment { Condition = 75 };

            // Act
            var result = _equipmentService.IsOperational(equipment);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsOperational_WithLowCondition_ReturnsFalse()
        {
            // Arrange
            var equipment = new Equipment { Condition = 40 };

            // Act
            var result = _equipmentService.IsOperational(equipment);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsOperational_WithNullEquipment_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _equipmentService.IsOperational(null));
        }

        [Fact]
        public void IsBroken_WithVeryLowCondition_ReturnsTrue()
        {
            // Arrange
            var equipment = new Equipment { Condition = 15 };

            // Act
            var result = _equipmentService.IsBroken(equipment);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsBroken_WithHighCondition_ReturnsFalse()
        {
            // Arrange
            var equipment = new Equipment { Condition = 70 };

            // Act
            var result = _equipmentService.IsBroken(equipment);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsBroken_WithNullEquipment_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _equipmentService.IsBroken(null));
        }

        #endregion

        #region Assignment Tests

        [Fact]
        public void AssignToScheme_WithValidScheme_ReturnsSuccess()
        {
            // Arrange
            var equipment = new Equipment { EquipmentId = 1, Name = "Test Equipment" };
            var scheme = new EvilScheme { SchemeId = 1, Name = "World Domination" };
            
            _mockSchemeRepository.Setup(r => r.GetById(1)).Returns(scheme);
            _mockEquipmentRepository.Setup(r => r.Update(It.IsAny<Equipment>()));

            // Act
            var result = _equipmentService.AssignToScheme(equipment, 1);

            // Assert
            Assert.True(result.success);
            Assert.Contains("World Domination", result.message);
            Assert.Equal(1, equipment.AssignedToSchemeId);
        }

        [Fact]
        public void AssignToScheme_WithNonExistentScheme_ReturnsFailure()
        {
            // Arrange
            var equipment = new Equipment { EquipmentId = 1 };
            _mockSchemeRepository.Setup(r => r.GetById(999)).Returns((EvilScheme?)null);

            // Act
            var result = _equipmentService.AssignToScheme(equipment, 999);

            // Assert
            Assert.False(result.success);
            Assert.Equal("Scheme not found!", result.message);
        }

        [Fact]
        public void AssignToScheme_WithNullEquipment_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _equipmentService.AssignToScheme(null, 1));
        }

        #endregion

        #region Repository Access Tests

        [Fact]
        public void GetAllEquipment_CallsRepository()
        {
            // Arrange
            var equipmentList = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, Name = "Equipment 1" },
                new Equipment { EquipmentId = 2, Name = "Equipment 2" }
            };
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipmentList);

            // Act
            var result = _equipmentService.GetAllEquipment();

            // Assert
            Assert.Equal(2, result.Count);
            _mockEquipmentRepository.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public void GetOperationalEquipment_ReturnsOnlyOperational()
        {
            // Arrange
            var equipmentList = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, Name = "Good Equipment", Condition = 75 },
                new Equipment { EquipmentId = 2, Name = "Bad Equipment", Condition = 30 },
                new Equipment { EquipmentId = 3, Name = "Decent Equipment", Condition = 55 }
            };
            _mockEquipmentRepository.Setup(r => r.GetAll()).Returns(equipmentList);

            // Act
            var result = _equipmentService.GetOperationalEquipment();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, e => Assert.True(e.Condition >= 50));
        }

        [Fact]
        public void GetEquipmentById_CallsRepository()
        {
            // Arrange
            var equipment = new Equipment { EquipmentId = 1, Name = "Test Equipment" };
            _mockEquipmentRepository.Setup(r => r.GetById(1)).Returns(equipment);

            // Act
            var result = _equipmentService.GetEquipmentById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.EquipmentId);
            _mockEquipmentRepository.Verify(r => r.GetById(1), Times.Once);
        }

        #endregion
    }
}
