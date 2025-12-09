using System;
using System.Data.SQLite;
using VillainLairManager.Utils;

namespace VillainLairManager
{
    /// <summary>
    /// Database helper for initialization only - CRUD operations moved to repositories
    /// </summary>
    public static class DatabaseHelper
    {
        private static SQLiteConnection _connection = null;
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            if (_isInitialized)
                return;

            string dbPath = ConfigManager.Instance.DatabasePath;
            _connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            _connection.Open();
            _isInitialized = true;
        }

        public static SQLiteConnection GetConnection()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("DatabaseHelper must be initialized first.");
            return _connection;
        }

        public static void CreateSchemaIfNotExists()
        {
            // Minions table
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Minions (
                    MinionId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    SkillLevel INTEGER NOT NULL CHECK(SkillLevel >= 1 AND SkillLevel <= 10),
                    Specialty TEXT NOT NULL,
                    LoyaltyScore INTEGER NOT NULL CHECK(LoyaltyScore >= 0 AND LoyaltyScore <= 100),
                    SalaryDemand REAL NOT NULL CHECK(SalaryDemand >= 0),
                    CurrentBaseId INTEGER,
                    CurrentSchemeId INTEGER,
                    MoodStatus TEXT NOT NULL,
                    LastMoodUpdate TEXT NOT NULL,
                    FOREIGN KEY (CurrentBaseId) REFERENCES SecretBases(BaseId) ON DELETE SET NULL,
                    FOREIGN KEY (CurrentSchemeId) REFERENCES EvilSchemes(SchemeId) ON DELETE SET NULL
                );
            ");

            // Evil Schemes table
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS EvilSchemes (
                    SchemeId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    Budget REAL NOT NULL CHECK(Budget >= 0),
                    CurrentSpending REAL DEFAULT 0 CHECK(CurrentSpending >= 0),
                    RequiredSkillLevel INTEGER NOT NULL CHECK(RequiredSkillLevel >= 1 AND RequiredSkillLevel <= 10),
                    RequiredSpecialty TEXT NOT NULL,
                    Status TEXT NOT NULL,
                    StartDate TEXT,
                    TargetCompletionDate TEXT NOT NULL,
                    DiabolicalRating INTEGER NOT NULL CHECK(DiabolicalRating >= 1 AND DiabolicalRating <= 10),
                    SuccessLikelihood INTEGER NOT NULL CHECK(SuccessLikelihood >= 0 AND SuccessLikelihood <= 100)
                );
            ");

            // Secret Bases table
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS SecretBases (
                    BaseId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Location TEXT NOT NULL,
                    Capacity INTEGER NOT NULL CHECK(Capacity > 0),
                    SecurityLevel INTEGER NOT NULL CHECK(SecurityLevel >= 1 AND SecurityLevel <= 10),
                    MonthlyMaintenanceCost REAL NOT NULL CHECK(MonthlyMaintenanceCost >= 0),
                    HasDoomsdayDevice INTEGER NOT NULL CHECK(HasDoomsdayDevice IN (0, 1)),
                    IsDiscovered INTEGER NOT NULL CHECK(IsDiscovered IN (0, 1)),
                    LastInspectionDate TEXT
                );
            ");

            // Equipment table
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Equipment (
                    EquipmentId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Category TEXT NOT NULL,
                    Condition INTEGER NOT NULL CHECK(Condition >= 0 AND Condition <= 100),
                    PurchasePrice REAL NOT NULL CHECK(PurchasePrice >= 0),
                    MaintenanceCost REAL NOT NULL CHECK(MaintenanceCost >= 0),
                    AssignedToSchemeId INTEGER,
                    StoredAtBaseId INTEGER,
                    RequiresSpecialist INTEGER NOT NULL CHECK(RequiresSpecialist IN (0, 1)),
                    LastMaintenanceDate TEXT,
                    FOREIGN KEY (AssignedToSchemeId) REFERENCES EvilSchemes(SchemeId) ON DELETE SET NULL,
                    FOREIGN KEY (StoredAtBaseId) REFERENCES SecretBases(BaseId) ON DELETE SET NULL
                );
            ");
        }

        public static void SeedInitialData()
        {
            // Check if data already exists
            var minionCount = ExecuteScalar<long>("SELECT COUNT(*) FROM Minions");
            if (minionCount > 0)
                return; // Data already exists

            // Seed Secret Bases first (no dependencies)
            ExecuteNonQuery(@"
                INSERT INTO SecretBases (Name, Location, Capacity, SecurityLevel, MonthlyMaintenanceCost, HasDoomsdayDevice, IsDiscovered, LastInspectionDate)
                VALUES
                    ('Volcano Fortress', 'Pacific Island', 50, 9, 50000, 1, 0, '2025-11-01'),
                    ('Arctic Hideout', 'North Pole', 30, 7, 30000, 0, 0, '2025-10-15'),
                    ('Underwater Lair', 'Mariana Trench', 40, 10, 45000, 1, 0, '2025-11-20'),
                    ('Desert Bunker', 'Sahara Desert', 25, 6, 20000, 0, 0, NULL);
            ");

            // Seed Evil Schemes
            ExecuteNonQuery(@"
                INSERT INTO EvilSchemes (Name, Description, Budget, CurrentSpending, RequiredSkillLevel, RequiredSpecialty, Status, StartDate, TargetCompletionDate, DiabolicalRating, SuccessLikelihood)
                VALUES
                    ('Steal the Moon', 'Use shrink ray to steal the moon and hold world hostage', 1000000, 15000, 8, 'Engineering', 'Planning', NULL, '2026-06-01', 10, 50),
                    ('Freeze Entire City', 'Deploy freeze ray to freeze major city and demand ransom', 500000, 45000, 6, 'Engineering', 'Active', '2025-11-01', '2025-12-31', 8, 60),
                    ('Replace World Leaders', 'Use disguises to infiltrate governments worldwide', 750000, 0, 9, 'Disguise', 'Planning', NULL, '2026-03-01', 9, 45),
                    ('Hack Global Banks', 'Steal millions from international banking systems', 250000, 12000, 8, 'Hacking', 'Active', '2025-10-15', '2025-12-15', 7, 55),
                    ('Build Robot Army', 'Create army of combat robots for world domination', 2000000, 0, 7, 'Engineering', 'Planning', NULL, '2027-01-01', 10, 40);
            ");

            // Seed Minions
            ExecuteNonQuery(@"
                INSERT INTO Minions (Name, SkillLevel, Specialty, LoyaltyScore, SalaryDemand, CurrentBaseId, CurrentSchemeId, MoodStatus, LastMoodUpdate)
                VALUES
                    ('Igor', 3, 'Combat', 85, 3000, 1, NULL, 'Happy', '2025-12-01'),
                    ('Helga', 8, 'Hacking', 45, 8000, 1, 4, 'Grumpy', '2025-12-01'),
                    ('Boris', 6, 'Explosives', 92, 5500, 2, 2, 'Happy', '2025-12-01'),
                    ('Natasha', 9, 'Disguise', 25, 9500, 1, NULL, 'Plotting Betrayal', '2025-12-01'),
                    ('Klaus', 7, 'Engineering', 78, 6500, 1, 2, 'Happy', '2025-12-01'),
                    ('Olga', 5, 'Combat', 55, 4500, 2, NULL, 'Grumpy', '2025-12-01'),
                    ('Vladimir', 9, 'Hacking', 88, 9000, 3, 4, 'Happy', '2025-12-01'),
                    ('Svetlana', 4, 'Disguise', 62, 4000, 3, NULL, 'Grumpy', '2025-12-01'),
                    ('Dimitri', 8, 'Engineering', 90, 7500, 1, NULL, 'Happy', '2025-12-01'),
                    ('Anastasia', 6, 'Piloting', 70, 5800, 2, NULL, 'Happy', '2025-12-01');
            ");

            // Seed Equipment
            ExecuteNonQuery(@"
                INSERT INTO Equipment (Name, Category, Condition, PurchasePrice, MaintenanceCost, AssignedToSchemeId, StoredAtBaseId, RequiresSpecialist, LastMaintenanceDate)
                VALUES
                    ('Freeze Ray', 'Weapon', 85, 100000, 15000, 2, 1, 1, '2025-11-01'),
                    ('Drill Tank', 'Vehicle', 72, 250000, 30000, NULL, 1, 0, '2025-10-20'),
                    ('Shrink Ray', 'Doomsday Device', 95, 500000, 150000, NULL, 1, 1, '2025-11-15'),
                    ('Invisibility Cloak', 'Gadget', 60, 50000, 5000, NULL, 2, 0, '2025-09-30'),
                    ('Hacking Suite', 'Gadget', 90, 75000, 10000, 4, 3, 1, '2025-11-10'),
                    ('Combat Mech', 'Vehicle', 45, 300000, 40000, NULL, 3, 1, '2025-08-15'),
                    ('EMP Generator', 'Weapon', 78, 150000, 20000, 4, 3, 1, '2025-10-25'),
                    ('Jetpack', 'Vehicle', 55, 80000, 12000, NULL, 2, 0, '2025-10-10');
            ");
        }

        private static void ExecuteNonQuery(string sql)
        {
            var command = new SQLiteCommand(sql, _connection);
            command.ExecuteNonQuery();
        }

        private static T ExecuteScalar<T>(string sql)
        {
            var command = new SQLiteCommand(sql, _connection);
            var result = command.ExecuteScalar();
            if (result == null || result == DBNull.Value)
                return default(T);
            return (T)Convert.ChangeType(result, typeof(T));
        }

        // Legacy methods for Models that still reference DatabaseHelper (anti-pattern to be fixed separately)
        public static int GetBaseOccupancy(int baseId)
        {
            var command = new SQLiteCommand("SELECT COUNT(*) FROM Minions WHERE CurrentBaseId = @id", _connection);
            command.Parameters.AddWithValue("@id", baseId);
            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public static void UpdateMinion(Models.Minion minion)
        {
            var sql = @"UPDATE Minions SET Name = @name, SkillLevel = @skill, Specialty = @specialty,
                       LoyaltyScore = @loyalty, SalaryDemand = @salary, CurrentBaseId = @baseId,
                       CurrentSchemeId = @schemeId, MoodStatus = @mood, LastMoodUpdate = @lastUpdate
                       WHERE MinionId = @id";
            var command = new SQLiteCommand(sql, _connection);
            command.Parameters.AddWithValue("@id", minion.MinionId);
            command.Parameters.AddWithValue("@name", minion.Name);
            command.Parameters.AddWithValue("@skill", minion.SkillLevel);
            command.Parameters.AddWithValue("@specialty", minion.Specialty);
            command.Parameters.AddWithValue("@loyalty", minion.LoyaltyScore);
            command.Parameters.AddWithValue("@salary", minion.SalaryDemand);
            command.Parameters.AddWithValue("@baseId", minion.CurrentBaseId.HasValue ? (object)minion.CurrentBaseId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@schemeId", minion.CurrentSchemeId.HasValue ? (object)minion.CurrentSchemeId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@mood", minion.MoodStatus);
            command.Parameters.AddWithValue("@lastUpdate", minion.LastMoodUpdate.ToString("yyyy-MM-dd"));
            command.ExecuteNonQuery();
        }

        public static Models.EvilScheme GetSchemeById(int schemeId)
        {
            var command = new SQLiteCommand("SELECT * FROM EvilSchemes WHERE SchemeId = @id", _connection);
            command.Parameters.AddWithValue("@id", schemeId);
            var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new Models.EvilScheme
                {
                    SchemeId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    Budget = reader.GetDecimal(3),
                    CurrentSpending = reader.GetDecimal(4),
                    RequiredSkillLevel = reader.GetInt32(5),
                    RequiredSpecialty = reader.GetString(6),
                    Status = reader.GetString(7),
                    StartDate = reader.IsDBNull(8) ? null : (DateTime?)DateTime.Parse(reader.GetString(8)),
                    TargetCompletionDate = DateTime.Parse(reader.GetString(9)),
                    DiabolicalRating = reader.GetInt32(10),
                    SuccessLikelihood = reader.GetInt32(11)
                };
            }

            return null;
        }

        public static void UpdateEquipment(Models.Equipment equipment)
        {
            var sql = @"UPDATE Equipment SET Name = @name, Category = @category, Condition = @condition,
                       PurchasePrice = @price, MaintenanceCost = @maintenance, AssignedToSchemeId = @schemeId,
                       StoredAtBaseId = @baseId, RequiresSpecialist = @specialist, LastMaintenanceDate = @lastMaint
                       WHERE EquipmentId = @id";
            var command = new SQLiteCommand(sql, _connection);
            command.Parameters.AddWithValue("@id", equipment.EquipmentId);
            command.Parameters.AddWithValue("@name", equipment.Name);
            command.Parameters.AddWithValue("@category", equipment.Category);
            command.Parameters.AddWithValue("@condition", equipment.Condition);
            command.Parameters.AddWithValue("@price", equipment.PurchasePrice);
            command.Parameters.AddWithValue("@maintenance", equipment.MaintenanceCost);
            command.Parameters.AddWithValue("@schemeId", equipment.AssignedToSchemeId.HasValue ? (object)equipment.AssignedToSchemeId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@baseId", equipment.StoredAtBaseId.HasValue ? (object)equipment.StoredAtBaseId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@specialist", equipment.RequiresSpecialist ? 1 : 0);
            command.Parameters.AddWithValue("@lastMaint", equipment.LastMaintenanceDate.HasValue ? (object)equipment.LastMaintenanceDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
            command.ExecuteNonQuery();
        }

        public static System.Collections.Generic.List<Models.Minion> GetAllMinions()
        {
            var minions = new System.Collections.Generic.List<Models.Minion>();
            var command = new SQLiteCommand("SELECT * FROM Minions", _connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                minions.Add(new Models.Minion
                {
                    MinionId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    SkillLevel = reader.GetInt32(2),
                    Specialty = reader.GetString(3),
                    LoyaltyScore = reader.GetInt32(4),
                    SalaryDemand = reader.GetDecimal(5),
                    CurrentBaseId = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                    CurrentSchemeId = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7),
                    MoodStatus = reader.GetString(8),
                    LastMoodUpdate = DateTime.Parse(reader.GetString(9))
                });
            }

            return minions;
        }

        public static System.Collections.Generic.List<Models.Equipment> GetAllEquipment()
        {
            var equipment = new System.Collections.Generic.List<Models.Equipment>();
            var command = new SQLiteCommand("SELECT * FROM Equipment", _connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                equipment.Add(new Models.Equipment
                {
                    EquipmentId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Category = reader.GetString(2),
                    Condition = reader.GetInt32(3),
                    PurchasePrice = reader.GetDecimal(4),
                    MaintenanceCost = reader.GetDecimal(5),
                    AssignedToSchemeId = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                    StoredAtBaseId = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7),
                    RequiresSpecialist = reader.GetInt32(8) == 1,
                    LastMaintenanceDate = reader.IsDBNull(9) ? null : (DateTime?)DateTime.Parse(reader.GetString(9))
                });
            }

            return equipment;
        }
    }
}
