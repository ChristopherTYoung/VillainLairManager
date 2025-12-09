using System;
using System.Data.SQLite;
using System.IO;
using VillainLairManager.Models;
using VillainLairManager.Repositories;
using Xunit;

namespace VillainLairManager.Tests.Integration
{
    /// <summary>
    /// Integration tests for EquipmentRepository with actual SQLite database
    /// Tests verify database operations with a real database instance
    /// </summary>
    public class EquipmentRepositoryIntegrationTests : IDisposable
    {
        private readonly string _testDbPath;
        private readonly SQLiteConnection _connection;
        private readonly EquipmentRepository _repository;

        public EquipmentRepositoryIntegrationTests()
        {
            // Create a unique test database for each test run
            _testDbPath = $"test_equipment_{Guid.NewGuid()}.db";
            _connection = new SQLiteConnection($"Data Source={_testDbPath};Version=3;Pooling=False;");
            _connection.Open();

            // Create the Equipment table
            CreateEquipmentTable();

            _repository = new EquipmentRepository(_connection);
        }

        private void CreateEquipmentTable()
        {
            string createTableSql = @"
                CREATE TABLE IF NOT EXISTS Equipment (
                    EquipmentId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Category TEXT NOT NULL,
                    Condition INTEGER NOT NULL,
                    PurchasePrice REAL NOT NULL,
                    MaintenanceCost REAL NOT NULL,
                    AssignedToSchemeId INTEGER,
                    StoredAtBaseId INTEGER,
                    RequiresSpecialist INTEGER NOT NULL,
                    LastMaintenanceDate TEXT
                )";

            using var command = new SQLiteCommand(createTableSql, _connection);
            command.ExecuteNonQuery();
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
            SQLiteConnection.ClearAllPools();
            
            // Force garbage collection to release any lingering handles
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Clean up test database file with retry logic
            if (File.Exists(_testDbPath))
            {
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        File.Delete(_testDbPath);
                        break;
                    }
                    catch (IOException) when (i < 4)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
        }

        [Fact]
        public void Insert_And_GetById_EquipmentSuccessfully()
        {
            // Arrange
            var equipment = new Equipment
            {
                Name = "Death Ray",
                Category = "Doomsday Device",
                Condition = 100,
                PurchasePrice = 1000000,
                MaintenanceCost = 50000,
                AssignedToSchemeId = null,
                StoredAtBaseId = 1,
                RequiresSpecialist = true,
                LastMaintenanceDate = DateTime.Now
            };

            // Act
            _repository.Insert(equipment);

            // Get the inserted equipment's ID
            var allEquipment = _repository.GetAll();
            Assert.Single(allEquipment);
            var insertedId = allEquipment[0].EquipmentId;

            var retrieved = _repository.GetById(insertedId);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal("Death Ray", retrieved.Name);
            Assert.Equal("Doomsday Device", retrieved.Category);
            Assert.Equal(100, retrieved.Condition);
            Assert.Equal(1000000, retrieved.PurchasePrice);
            Assert.Equal(50000, retrieved.MaintenanceCost);
            Assert.Null(retrieved.AssignedToSchemeId);
            Assert.Equal(1, retrieved.StoredAtBaseId);
            Assert.True(retrieved.RequiresSpecialist);
            Assert.NotNull(retrieved.LastMaintenanceDate);
        }

        [Fact]
        public void Update_Equipment_PersistsChanges()
        {
            // Arrange
            var equipment = new Equipment
            {
                Name = "Laser Gun",
                Category = "Weapons",
                Condition = 80,
                PurchasePrice = 50000,
                MaintenanceCost = 2000,
                RequiresSpecialist = false,
                LastMaintenanceDate = DateTime.Now
            };

            _repository.Insert(equipment);
            var allEquipment = _repository.GetAll();
            var insertedEquipment = allEquipment[0];

            // Act - Update the equipment
            insertedEquipment.Condition = 60;
            insertedEquipment.AssignedToSchemeId = 5;
            insertedEquipment.MaintenanceCost = 3000;
            _repository.Update(insertedEquipment);

            // Retrieve again
            var updated = _repository.GetById(insertedEquipment.EquipmentId);

            // Assert
            Assert.NotNull(updated);
            Assert.Equal(60, updated.Condition);
            Assert.Equal(5, updated.AssignedToSchemeId);
            Assert.Equal(3000, updated.MaintenanceCost);
        }

        [Fact]
        public void Delete_Equipment_RemovesFromDatabase()
        {
            // Arrange
            var equipment = new Equipment
            {
                Name = "Obsolete Gadget",
                Category = "Gadget",
                Condition = 20,
                PurchasePrice = 10000,
                MaintenanceCost = 500,
                RequiresSpecialist = false,
                LastMaintenanceDate = DateTime.Now
            };

            _repository.Insert(equipment);
            var allEquipment = _repository.GetAll();
            var insertedId = allEquipment[0].EquipmentId;

            // Act
            _repository.Delete(insertedId);

            // Assert
            var retrieved = _repository.GetById(insertedId);
            Assert.Null(retrieved);

            var remainingEquipment = _repository.GetAll();
            Assert.Empty(remainingEquipment);
        }

        [Fact]
        public void GetAll_ReturnsAllEquipment()
        {
            // Arrange
            var equipment1 = new Equipment
            {
                Name = "Item One",
                Category = "Weapon",
                Condition = 90,
                PurchasePrice = 20000,
                MaintenanceCost = 1000,
                RequiresSpecialist = false,
                LastMaintenanceDate = DateTime.Now
            };

            var equipment2 = new Equipment
            {
                Name = "Item Two",
                Category = "Vehicle",
                Condition = 75,
                PurchasePrice = 100000,
                MaintenanceCost = 5000,
                RequiresSpecialist = true,
                LastMaintenanceDate = DateTime.Now
            };

            // Act
            _repository.Insert(equipment1);
            _repository.Insert(equipment2);

            var allEquipment = _repository.GetAll();

            // Assert
            Assert.Equal(2, allEquipment.Count);
            Assert.Contains(allEquipment, e => e.Name == "Item One");
            Assert.Contains(allEquipment, e => e.Name == "Item Two");
        }

        [Fact]
        public void GetSchemeAssignedEquipmentCount_CountsCorrectly()
        {
            // Arrange
            var equipment1 = new Equipment
            {
                Name = "Assigned Equipment 1",
                Category = "Weapon",
                Condition = 100,
                PurchasePrice = 50000,
                MaintenanceCost = 2000,
                AssignedToSchemeId = 42,
                RequiresSpecialist = false,
                LastMaintenanceDate = DateTime.Now
            };

            var equipment2 = new Equipment
            {
                Name = "Assigned Equipment 2",
                Category = "Gadget",
                Condition = 80,
                PurchasePrice = 30000,
                MaintenanceCost = 1500,
                AssignedToSchemeId = 42,
                RequiresSpecialist = true,
                LastMaintenanceDate = DateTime.Now
            };

            var equipment3 = new Equipment
            {
                Name = "Unassigned Equipment",
                Category = "Vehicle",
                Condition = 90,
                PurchasePrice = 100000,
                MaintenanceCost = 5000,
                AssignedToSchemeId = null,
                RequiresSpecialist = false,
                LastMaintenanceDate = DateTime.Now
            };

            _repository.Insert(equipment1);
            _repository.Insert(equipment2);
            _repository.Insert(equipment3);

            // Act
            int count = _repository.GetSchemeAssignedEquipmentCount(42);

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public void InsertAndRetrieve_WithNullValues_HandlesCorrectly()
        {
            // Arrange
            var equipment = new Equipment
            {
                Name = "Simple Equipment",
                Category = "Gadget",
                Condition = 100,
                PurchasePrice = 5000,
                MaintenanceCost = 250,
                AssignedToSchemeId = null,
                StoredAtBaseId = null,
                RequiresSpecialist = false,
                LastMaintenanceDate = null
            };

            // Act
            _repository.Insert(equipment);
            var allEquipment = _repository.GetAll();
            var retrieved = allEquipment[0];

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal("Simple Equipment", retrieved.Name);
            Assert.Null(retrieved.AssignedToSchemeId);
            Assert.Null(retrieved.StoredAtBaseId);
            Assert.Null(retrieved.LastMaintenanceDate);
        }

        [Fact]
        public void Update_ChangesAssignmentAndStorage()
        {
            // Arrange
            var equipment = new Equipment
            {
                Name = "Flexible Equipment",
                Category = "Weapon",
                Condition = 100,
                PurchasePrice = 40000,
                MaintenanceCost = 2000,
                AssignedToSchemeId = null,
                StoredAtBaseId = null,
                RequiresSpecialist = false,
                LastMaintenanceDate = DateTime.Now
            };

            _repository.Insert(equipment);
            var insertedEquipment = _repository.GetAll()[0];

            // Act - Assign to scheme and base
            insertedEquipment.AssignedToSchemeId = 10;
            insertedEquipment.StoredAtBaseId = 5;
            _repository.Update(insertedEquipment);

            var afterFirstUpdate = _repository.GetById(insertedEquipment.EquipmentId);

            // Act - Unassign from scheme
            afterFirstUpdate!.AssignedToSchemeId = null;
            _repository.Update(afterFirstUpdate);

            var afterSecondUpdate = _repository.GetById(insertedEquipment.EquipmentId);

            // Assert
            Assert.Equal(10, insertedEquipment.AssignedToSchemeId);
            Assert.Equal(5, afterFirstUpdate.StoredAtBaseId);
            Assert.Null(afterSecondUpdate!.AssignedToSchemeId);
            Assert.Equal(5, afterSecondUpdate.StoredAtBaseId);
        }
    }
}
