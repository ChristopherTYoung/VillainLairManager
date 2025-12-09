using System;
using System.Data.SQLite;
using System.IO;
using VillainLairManager.Models;
using VillainLairManager.Repositories;
using Xunit;

namespace VillainLairManager.Tests.Integration
{
    /// <summary>
    /// Integration tests for MinionRepository with actual SQLite database
    /// Tests verify database operations with a real database instance
    /// </summary>
    public class MinionRepositoryIntegrationTests : IDisposable
    {
        private readonly string _testDbPath;
        private readonly SQLiteConnection _connection;
        private readonly MinionRepository _repository;

        public MinionRepositoryIntegrationTests()
        {
            // Create a unique test database for each test run
            _testDbPath = $"test_minions_{Guid.NewGuid()}.db";
            _connection = new SQLiteConnection($"Data Source={_testDbPath};Version=3;Pooling=False;");
            _connection.Open();

            // Create the Minions table
            CreateMinionsTable();

            _repository = new MinionRepository(_connection);
        }

        private void CreateMinionsTable()
        {
            string createTableSql = @"
                CREATE TABLE IF NOT EXISTS Minions (
                    MinionId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    SkillLevel INTEGER NOT NULL,
                    Specialty TEXT NOT NULL,
                    LoyaltyScore INTEGER NOT NULL,
                    SalaryDemand REAL NOT NULL,
                    CurrentBaseId INTEGER,
                    CurrentSchemeId INTEGER,
                    MoodStatus TEXT NOT NULL,
                    LastMoodUpdate TEXT NOT NULL
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
        public void Insert_And_GetById_MinionsSuccessfully()
        {
            // Arrange
            var minion = new Minion
            {
                Name = "Test Minion",
                SkillLevel = 7,
                Specialty = "Henchman",
                LoyaltyScore = 65,
                SalaryDemand = 6000,
                CurrentBaseId = 1,
                CurrentSchemeId = null,
                MoodStatus = "Happy",
                LastMoodUpdate = DateTime.Now
            };

            // Act
            _repository.Insert(minion);

            // Get the inserted minion's ID
            var allMinions = _repository.GetAll();
            Assert.Single(allMinions);
            var insertedId = allMinions[0].MinionId;

            var retrieved = _repository.GetById(insertedId);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal("Test Minion", retrieved.Name);
            Assert.Equal(7, retrieved.SkillLevel);
            Assert.Equal("Henchman", retrieved.Specialty);
            Assert.Equal(65, retrieved.LoyaltyScore);
            Assert.Equal(6000, retrieved.SalaryDemand);
            Assert.Equal(1, retrieved.CurrentBaseId);
            Assert.Null(retrieved.CurrentSchemeId);
            Assert.Equal("Happy", retrieved.MoodStatus);
        }

        [Fact]
        public void Update_Minion_PersistsChanges()
        {
            // Arrange
            var minion = new Minion
            {
                Name = "Original Name",
                SkillLevel = 5,
                Specialty = "Technician",
                LoyaltyScore = 50,
                SalaryDemand = 5000,
                MoodStatus = "Grumpy",
                LastMoodUpdate = DateTime.Now
            };

            _repository.Insert(minion);
            var allMinions = _repository.GetAll();
            var insertedMinion = allMinions[0];

            // Act - Update the minion
            insertedMinion.Name = "Updated Name";
            insertedMinion.SkillLevel = 8;
            insertedMinion.LoyaltyScore = 75;
            insertedMinion.MoodStatus = "Happy";
            _repository.Update(insertedMinion);

            // Retrieve again
            var updated = _repository.GetById(insertedMinion.MinionId);

            // Assert
            Assert.NotNull(updated);
            Assert.Equal("Updated Name", updated.Name);
            Assert.Equal(8, updated.SkillLevel);
            Assert.Equal(75, updated.LoyaltyScore);
            Assert.Equal("Happy", updated.MoodStatus);
        }

        [Fact]
        public void Delete_Minion_RemovesFromDatabase()
        {
            // Arrange
            var minion = new Minion
            {
                Name = "Doomed Minion",
                SkillLevel = 3,
                Specialty = "Henchman",
                LoyaltyScore = 30,
                SalaryDemand = 3000,
                MoodStatus = "Plotting Betrayal",
                LastMoodUpdate = DateTime.Now
            };

            _repository.Insert(minion);
            var allMinions = _repository.GetAll();
            var insertedId = allMinions[0].MinionId;

            // Act
            _repository.Delete(insertedId);

            // Assert
            var retrieved = _repository.GetById(insertedId);
            Assert.Null(retrieved);

            var remainingMinions = _repository.GetAll();
            Assert.Empty(remainingMinions);
        }

        [Fact]
        public void GetAll_ReturnsAllMinions()
        {
            // Arrange
            var minion1 = new Minion
            {
                Name = "Minion One",
                SkillLevel = 5,
                Specialty = "Henchman",
                LoyaltyScore = 50,
                SalaryDemand = 5000,
                MoodStatus = "Grumpy",
                LastMoodUpdate = DateTime.Now
            };

            var minion2 = new Minion
            {
                Name = "Minion Two",
                SkillLevel = 7,
                Specialty = "Scientist",
                LoyaltyScore = 70,
                SalaryDemand = 8000,
                MoodStatus = "Happy",
                LastMoodUpdate = DateTime.Now
            };

            // Act
            _repository.Insert(minion1);
            _repository.Insert(minion2);

            var allMinions = _repository.GetAll();

            // Assert
            Assert.Equal(2, allMinions.Count);
            Assert.Contains(allMinions, m => m.Name == "Minion One");
            Assert.Contains(allMinions, m => m.Name == "Minion Two");
        }

        [Fact]
        public void GetSchemeAssignedMinionsCount_CountsCorrectly()
        {
            // Arrange
            var minion1 = new Minion
            {
                Name = "Assigned Minion 1",
                SkillLevel = 5,
                Specialty = "Henchman",
                LoyaltyScore = 50,
                SalaryDemand = 5000,
                CurrentSchemeId = 42,
                MoodStatus = "Grumpy",
                LastMoodUpdate = DateTime.Now
            };

            var minion2 = new Minion
            {
                Name = "Assigned Minion 2",
                SkillLevel = 6,
                Specialty = "Technician",
                LoyaltyScore = 60,
                SalaryDemand = 6000,
                CurrentSchemeId = 42,
                MoodStatus = "Grumpy",
                LastMoodUpdate = DateTime.Now
            };

            var minion3 = new Minion
            {
                Name = "Unassigned Minion",
                SkillLevel = 4,
                Specialty = "Henchman",
                LoyaltyScore = 40,
                SalaryDemand = 4000,
                CurrentSchemeId = null,
                MoodStatus = "Plotting Betrayal",
                LastMoodUpdate = DateTime.Now
            };

            _repository.Insert(minion1);
            _repository.Insert(minion2);
            _repository.Insert(minion3);

            // Act
            int count = _repository.GetSchemeAssignedMinionsCount(42);

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public void GetBaseOccupancy_CountsMinionsAtBase()
        {
            // Arrange
            var minion1 = new Minion
            {
                Name = "Base Minion 1",
                SkillLevel = 5,
                Specialty = "Henchman",
                LoyaltyScore = 50,
                SalaryDemand = 5000,
                CurrentBaseId = 10,
                MoodStatus = "Grumpy",
                LastMoodUpdate = DateTime.Now
            };

            var minion2 = new Minion
            {
                Name = "Base Minion 2",
                SkillLevel = 6,
                Specialty = "Scientist",
                LoyaltyScore = 60,
                SalaryDemand = 6000,
                CurrentBaseId = 10,
                MoodStatus = "Happy",
                LastMoodUpdate = DateTime.Now
            };

            var minion3 = new Minion
            {
                Name = "Other Base Minion",
                SkillLevel = 4,
                Specialty = "Henchman",
                LoyaltyScore = 40,
                SalaryDemand = 4000,
                CurrentBaseId = 20,
                MoodStatus = "Grumpy",
                LastMoodUpdate = DateTime.Now
            };

            _repository.Insert(minion1);
            _repository.Insert(minion2);
            _repository.Insert(minion3);

            // Act
            int count = _repository.GetBaseOccupancy(10);

            // Assert
            Assert.Equal(2, count);
        }
    }
}
