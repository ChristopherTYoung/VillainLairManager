using System;
using System.Collections.Generic;
using System.Data.SQLite;
using VillainLairManager.Models;

namespace VillainLairManager.Repositories
{
    /// <summary>
    /// Concrete repository implementation for SecretBase entities
    /// </summary>
    public class SecretBaseRepository : ISecretBaseRepository
    {
        private readonly SQLiteConnection _connection;

        public SecretBaseRepository(SQLiteConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public List<SecretBase> GetAll()
        {
            var bases = new List<SecretBase>();
            var command = new SQLiteCommand("SELECT * FROM SecretBases", _connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                bases.Add(MapFromReader(reader));
            }

            return bases;
        }

        public SecretBase GetById(int id)
        {
            var command = new SQLiteCommand("SELECT * FROM SecretBases WHERE BaseId = @id", _connection);
            command.Parameters.AddWithValue("@id", id);
            var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return MapFromReader(reader);
            }

            return null;
        }

        public void Insert(SecretBase baseObj)
        {
            var sql = @"INSERT INTO SecretBases (Name, Location, Capacity, SecurityLevel, MonthlyMaintenanceCost, HasDoomsdayDevice, IsDiscovered, LastInspectionDate)
                       VALUES (@name, @location, @capacity, @security, @cost, @doomsday, @discovered, @inspection)";
            var command = new SQLiteCommand(sql, _connection);
            command.Parameters.AddWithValue("@name", baseObj.Name);
            command.Parameters.AddWithValue("@location", baseObj.Location);
            command.Parameters.AddWithValue("@capacity", baseObj.Capacity);
            command.Parameters.AddWithValue("@security", baseObj.SecurityLevel);
            command.Parameters.AddWithValue("@cost", baseObj.MonthlyMaintenanceCost);
            command.Parameters.AddWithValue("@doomsday", baseObj.HasDoomsdayDevice ? 1 : 0);
            command.Parameters.AddWithValue("@discovered", baseObj.IsDiscovered ? 1 : 0);
            command.Parameters.AddWithValue("@inspection", baseObj.LastInspectionDate.HasValue ? (object)baseObj.LastInspectionDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
            command.ExecuteNonQuery();
        }

        public void Update(SecretBase baseObj)
        {
            var sql = @"UPDATE SecretBases SET Name = @name, Location = @location, Capacity = @capacity,
                       SecurityLevel = @security, MonthlyMaintenanceCost = @cost, HasDoomsdayDevice = @doomsday,
                       IsDiscovered = @discovered, LastInspectionDate = @inspection
                       WHERE BaseId = @id";
            var command = new SQLiteCommand(sql, _connection);
            command.Parameters.AddWithValue("@id", baseObj.BaseId);
            command.Parameters.AddWithValue("@name", baseObj.Name);
            command.Parameters.AddWithValue("@location", baseObj.Location);
            command.Parameters.AddWithValue("@capacity", baseObj.Capacity);
            command.Parameters.AddWithValue("@security", baseObj.SecurityLevel);
            command.Parameters.AddWithValue("@cost", baseObj.MonthlyMaintenanceCost);
            command.Parameters.AddWithValue("@doomsday", baseObj.HasDoomsdayDevice ? 1 : 0);
            command.Parameters.AddWithValue("@discovered", baseObj.IsDiscovered ? 1 : 0);
            command.Parameters.AddWithValue("@inspection", baseObj.LastInspectionDate.HasValue ? (object)baseObj.LastInspectionDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            var command = new SQLiteCommand("DELETE FROM SecretBases WHERE BaseId = @id", _connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        private SecretBase MapFromReader(SQLiteDataReader reader)
        {
            return new SecretBase
            {
                BaseId = reader.GetInt32(0),
                Name = reader.GetString(1),
                Location = reader.GetString(2),
                Capacity = reader.GetInt32(3),
                SecurityLevel = reader.GetInt32(4),
                MonthlyMaintenanceCost = reader.GetDecimal(5),
                HasDoomsdayDevice = reader.GetInt32(6) == 1,
                IsDiscovered = reader.GetInt32(7) == 1,
                LastInspectionDate = reader.IsDBNull(8) ? null : (DateTime?)DateTime.Parse(reader.GetString(8))
            };
        }
    }
}