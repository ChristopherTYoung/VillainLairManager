using System;
using System.Data.SQLite;
using System.IO;
using VillainLairManager.Models;
using VillainLairManager.Repositories;
using Xunit;

namespace VillainLairManager.Tests.Integration
{
    /// <summary>
    /// Integration tests for EvilSchemeRepository with actual SQLite database
    /// Tests verify database operations with a real database instance
    /// </summary>
    public class EvilSchemeRepositoryIntegrationTests : IDisposable
    {
        private readonly string _testDbPath;
        private readonly SQLiteConnection _connection;
        private readonly EvilSchemeRepository _repository;

        public EvilSchemeRepositoryIntegrationTests()
        {
            // Create a unique test database for each test run
            _testDbPath = $"test_schemes_{Guid.NewGuid()}.db";
            _connection = new SQLiteConnection($"Data Source={_testDbPath};Version=3;Pooling=False;");
            _connection.Open();

            // Create the EvilSchemes table
            CreateEvilSchemesTable();

            _repository = new EvilSchemeRepository(_connection);
        }

        private void CreateEvilSchemesTable()
        {
            string createTableSql = @"
                CREATE TABLE IF NOT EXISTS EvilSchemes (
                    SchemeId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    Budget REAL NOT NULL,
                    CurrentSpending REAL NOT NULL,
                    RequiredSkillLevel INTEGER NOT NULL,
                    RequiredSpecialty TEXT NOT NULL,
                    Status TEXT NOT NULL,
                    StartDate TEXT,
                    TargetCompletionDate TEXT NOT NULL,
                    DiabolicalRating INTEGER NOT NULL,
                    SuccessLikelihood INTEGER NOT NULL
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
        public void Insert_And_GetById_SchemeSuccessfully()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                Name = "World Domination",
                Description = "Take over the world",
                RequiredSkillLevel = 8,
                RequiredSpecialty = "Henchman",
                Budget = 1000000,
                CurrentSpending = 250000,
                Status = "Active",
                SuccessLikelihood = 75,
                TargetCompletionDate = DateTime.Now.AddMonths(6),
                DiabolicalRating = 9
            };

            // Act
            _repository.Insert(scheme);

            // Get the inserted scheme's ID
            var allSchemes = _repository.GetAll();
            Assert.Single(allSchemes);
            var insertedId = allSchemes[0].SchemeId;

            var retrieved = _repository.GetById(insertedId);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal("World Domination", retrieved.Name);
            Assert.Equal("Take over the world", retrieved.Description);
            Assert.Equal("Henchman", retrieved.RequiredSpecialty);
            Assert.Equal(1000000, retrieved.Budget);
            Assert.Equal(250000, retrieved.CurrentSpending);
            Assert.Equal("Active", retrieved.Status);
            Assert.Equal(75, retrieved.SuccessLikelihood);
            Assert.Equal(9, retrieved.DiabolicalRating);
        }

        [Fact]
        public void Update_Scheme_PersistsChanges()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                Name = "Original Plan",
                Description = "Original description",
                RequiredSkillLevel = 6,
                RequiredSpecialty = "Technician",
                Budget = 500000,
                CurrentSpending = 100000,
                Status = "Planning",
                SuccessLikelihood = 50,
                TargetCompletionDate = DateTime.Now.AddMonths(3),
                DiabolicalRating = 7
            };

            _repository.Insert(scheme);
            var allSchemes = _repository.GetAll();
            var insertedScheme = allSchemes[0];

            // Act - Update the scheme
            insertedScheme.Name = "Updated Plan";
            insertedScheme.Status = "Active";
            insertedScheme.CurrentSpending = 200000;
            insertedScheme.SuccessLikelihood = 65;
            _repository.Update(insertedScheme);

            // Retrieve again
            var updated = _repository.GetById(insertedScheme.SchemeId);

            // Assert
            Assert.NotNull(updated);
            Assert.Equal("Updated Plan", updated.Name);
            Assert.Equal("Active", updated.Status);
            Assert.Equal(200000, updated.CurrentSpending);
            Assert.Equal(65, updated.SuccessLikelihood);
        }

        [Fact]
        public void Delete_Scheme_RemovesFromDatabase()
        {
            // Arrange
            var scheme = new EvilScheme
            {
                Name = "Failed Plan",
                Description = "This won't work",
                RequiredSkillLevel = 3,
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 50000,
                Status = "Failed",
                SuccessLikelihood = 10,
                TargetCompletionDate = DateTime.Now.AddDays(-10),
                DiabolicalRating = 3
            };

            _repository.Insert(scheme);
            var allSchemes = _repository.GetAll();
            var insertedId = allSchemes[0].SchemeId;

            // Act
            _repository.Delete(insertedId);

            // Assert
            var retrieved = _repository.GetById(insertedId);
            Assert.Null(retrieved);

            var remainingSchemes = _repository.GetAll();
            Assert.Empty(remainingSchemes);
        }

        [Fact]
        public void GetAll_ReturnsAllSchemes()
        {
            // Arrange
            var scheme1 = new EvilScheme
            {
                Name = "Scheme One",
                Description = "First scheme",
                RequiredSkillLevel = 6,
                RequiredSpecialty = "Henchman",
                Budget = 200000,
                CurrentSpending = 50000,
                Status = "Planning",
                SuccessLikelihood = 60,
                TargetCompletionDate = DateTime.Now.AddMonths(2),
                DiabolicalRating = 7
            };

            var scheme2 = new EvilScheme
            {
                Name = "Scheme Two",
                Description = "Second scheme",
                RequiredSkillLevel = 8,
                RequiredSpecialty = "Scientist",
                Budget = 500000,
                CurrentSpending = 100000,
                Status = "Active",
                SuccessLikelihood = 80,
                TargetCompletionDate = DateTime.Now.AddMonths(4),
                DiabolicalRating = 9
            };

            // Act
            _repository.Insert(scheme1);
            _repository.Insert(scheme2);

            var allSchemes = _repository.GetAll();

            // Assert
            Assert.Equal(2, allSchemes.Count);
            Assert.Contains(allSchemes, s => s.Name == "Scheme One");
            Assert.Contains(allSchemes, s => s.Name == "Scheme Two");
        }

        [Fact]
        public void InsertAndRetrieve_WithLongDescription_HandlesCorrectly()
        {
            // Arrange
            var longDescription = new string('A', 1000); // 1000 character description
            var scheme = new EvilScheme
            {
                Name = "Complex Scheme",
                Description = longDescription,
                RequiredSkillLevel = 9,
                RequiredSpecialty = "Engineer",
                Budget = 2000000,
                CurrentSpending = 500000,
                Status = "Active",
                SuccessLikelihood = 70,
                TargetCompletionDate = DateTime.Now.AddYears(1),
                DiabolicalRating = 10
            };

            // Act
            _repository.Insert(scheme);
            var retrieved = _repository.GetAll()[0];

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal("Complex Scheme", retrieved.Name);
            Assert.Equal(longDescription, retrieved.Description);
            Assert.Equal(1000, retrieved.Description.Length);
        }

        [Fact]
        public void Update_ModifiesOnlySpecifiedScheme()
        {
            // Arrange
            var scheme1 = new EvilScheme
            {
                Name = "Scheme A",
                Description = "First",
                RequiredSkillLevel = 5,
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 10000,
                Status = "Planning",
                SuccessLikelihood = 50,
                TargetCompletionDate = DateTime.Now.AddMonths(1),
                DiabolicalRating = 5
            };

            var scheme2 = new EvilScheme
            {
                Name = "Scheme B",
                Description = "Second",
                RequiredSkillLevel = 7,
                RequiredSpecialty = "Scientist",
                Budget = 200000,
                CurrentSpending = 20000,
                Status = "Planning",
                SuccessLikelihood = 50,
                TargetCompletionDate = DateTime.Now.AddMonths(2),
                DiabolicalRating = 6
            };

            _repository.Insert(scheme1);
            _repository.Insert(scheme2);

            var allSchemes = _repository.GetAll();
            var firstScheme = allSchemes.Find(s => s.Name == "Scheme A");

            // Act - Update only the first scheme
            firstScheme!.Status = "Active";
            firstScheme.SuccessLikelihood = 75;
            _repository.Update(firstScheme);

            // Retrieve both schemes
            var updatedScheme1 = _repository.GetById(firstScheme.SchemeId);
            var unchangedScheme2 = _repository.GetAll().Find(s => s.Name == "Scheme B");

            // Assert
            Assert.Equal("Active", updatedScheme1!.Status);
            Assert.Equal(75, updatedScheme1.SuccessLikelihood);
            Assert.Equal("Planning", unchangedScheme2!.Status);
            Assert.Equal(50, unchangedScheme2.SuccessLikelihood);
        }

        [Fact]
        public void InsertAndRetrieve_PreservesDateAccurately()
        {
            // Arrange
            var targetDate = new DateTime(2025, 12, 31, 23, 59, 59);
            var scheme = new EvilScheme
            {
                Name = "New Year Scheme",
                Description = "End of year plan",
                RequiredSkillLevel = 7,
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 0,
                Status = "Planning",
                SuccessLikelihood = 50,
                TargetCompletionDate = targetDate,
                DiabolicalRating = 8
            };

            // Act
            _repository.Insert(scheme);
            var retrieved = _repository.GetAll()[0];

            // Assert - Compare dates (SQLite stores as text, so we compare date components)
            Assert.Equal(targetDate.Year, retrieved.TargetCompletionDate.Year);
            Assert.Equal(targetDate.Month, retrieved.TargetCompletionDate.Month);
            Assert.Equal(targetDate.Day, retrieved.TargetCompletionDate.Day);
        }

        [Fact]
        public void InsertMultipleSchemes_WithDifferentStatuses()
        {
            // Arrange & Act
            _repository.Insert(new EvilScheme
            {
                Name = "Planning Scheme",
                Description = "A scheme in planning phase",
                RequiredSkillLevel = 4,
                RequiredSpecialty = "Henchman",
                Budget = 100000,
                CurrentSpending = 0,
                Status = "Planning",
                SuccessLikelihood = 40,
                TargetCompletionDate = DateTime.Now.AddMonths(1),
                DiabolicalRating = 5
            });

            _repository.Insert(new EvilScheme
            {
                Name = "Active Scheme",
                Description = "A scheme currently active",
                RequiredSkillLevel = 6,
                RequiredSpecialty = "Technician",
                Budget = 200000,
                CurrentSpending = 50000,
                Status = "Active",
                SuccessLikelihood = 70,
                TargetCompletionDate = DateTime.Now.AddMonths(2),
                DiabolicalRating = 7
            });

            _repository.Insert(new EvilScheme
            {
                Name = "Completed Scheme",
                Description = "A scheme that was completed",
                RequiredSkillLevel = 10,
                RequiredSpecialty = "Scientist",
                Budget = 300000,
                CurrentSpending = 280000,
                Status = "Completed",
                SuccessLikelihood = 100,
                TargetCompletionDate = DateTime.Now.AddDays(-30),
                DiabolicalRating = 10
            });

            // Act
            var allSchemes = _repository.GetAll();

            // Assert
            Assert.Equal(3, allSchemes.Count);
            Assert.Contains(allSchemes, s => s.Status == "Planning");
            Assert.Contains(allSchemes, s => s.Status == "Active");
            Assert.Contains(allSchemes, s => s.Status == "Completed");
        }
    }
}
