using System;
using System.Collections.Generic;
using System.Data.SQLite;
using VillainLairManager.Models;

namespace VillainLairManager.Repositories
{
    /// <summary>
    /// Concrete repository implementation for Minion entities
    /// </summary>
    public class MinionRepository : IMinionRepository
    {
        private readonly SQLiteConnection _connection;

        public MinionRepository(SQLiteConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public List<Minion> GetAll()
        {
            var minions = new List<Minion>();
            var command = new SQLiteCommand("SELECT * FROM Minions", _connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                minions.Add(MapFromReader(reader));
            }

            return minions;
        }

        public Minion GetById(int id)
        {
            var command = new SQLiteCommand("SELECT * FROM Minions WHERE MinionId = @id", _connection);
            command.Parameters.AddWithValue("@id", id);
            var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return MapFromReader(reader);
            }

            return null;
        }

        public void Insert(Minion minion)
        {
            var sql = @"INSERT INTO Minions (Name, SkillLevel, Specialty, LoyaltyScore, SalaryDemand, CurrentBaseId, CurrentSchemeId, MoodStatus, LastMoodUpdate)
                       VALUES (@name, @skill, @specialty, @loyalty, @salary, @baseId, @schemeId, @mood, @lastUpdate)";
            var command = new SQLiteCommand(sql, _connection);
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

        public void Update(Minion minion)
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

        public void Delete(int id)
        {
            var command = new SQLiteCommand("DELETE FROM Minions WHERE MinionId = @id", _connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        public int GetSchemeAssignedMinionsCount(int schemeId)
        {
            var command = new SQLiteCommand("SELECT COUNT(*) FROM Minions WHERE CurrentSchemeId = @id", _connection);
            command.Parameters.AddWithValue("@id", schemeId);
            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public int GetBaseOccupancy(int baseId)
        {
            var command = new SQLiteCommand("SELECT COUNT(*) FROM Minions WHERE CurrentBaseId = @id", _connection);
            command.Parameters.AddWithValue("@id", baseId);
            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        private Minion MapFromReader(SQLiteDataReader reader)
        {
            return new Minion
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
            };
        }
    }
}
