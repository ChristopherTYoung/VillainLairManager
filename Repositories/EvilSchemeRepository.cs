using System;
using System.Collections.Generic;
using System.Data.SQLite;
using VillainLairManager.Models;

namespace VillainLairManager.Repositories
{
    /// <summary>
    /// Concrete repository implementation for EvilScheme entities
    /// </summary>
    public class EvilSchemeRepository : IEvilSchemeRepository
    {
        private readonly SQLiteConnection _connection;

        public EvilSchemeRepository(SQLiteConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public List<EvilScheme> GetAll()
        {
            var schemes = new List<EvilScheme>();
            var command = new SQLiteCommand("SELECT * FROM EvilSchemes", _connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                schemes.Add(MapFromReader(reader));
            }

            return schemes;
        }

        public EvilScheme GetById(int id)
        {
            var command = new SQLiteCommand("SELECT * FROM EvilSchemes WHERE SchemeId = @id", _connection);
            command.Parameters.AddWithValue("@id", id);
            var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return MapFromReader(reader);
            }

            return null;
        }

        public void Insert(EvilScheme scheme)
        {
            var sql = @"INSERT INTO EvilSchemes (Name, Description, Budget, CurrentSpending, RequiredSkillLevel, RequiredSpecialty, Status, StartDate, TargetCompletionDate, DiabolicalRating, SuccessLikelihood)
                       VALUES (@name, @desc, @budget, @spending, @skill, @specialty, @status, @start, @target, @rating, @success)";
            var command = new SQLiteCommand(sql, _connection);
            command.Parameters.AddWithValue("@name", scheme.Name);
            command.Parameters.AddWithValue("@desc", scheme.Description);
            command.Parameters.AddWithValue("@budget", scheme.Budget);
            command.Parameters.AddWithValue("@spending", scheme.CurrentSpending);
            command.Parameters.AddWithValue("@skill", scheme.RequiredSkillLevel);
            command.Parameters.AddWithValue("@specialty", scheme.RequiredSpecialty);
            command.Parameters.AddWithValue("@status", scheme.Status);
            command.Parameters.AddWithValue("@start", scheme.StartDate.HasValue ? (object)scheme.StartDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
            command.Parameters.AddWithValue("@target", scheme.TargetCompletionDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@rating", scheme.DiabolicalRating);
            command.Parameters.AddWithValue("@success", scheme.SuccessLikelihood);
            command.ExecuteNonQuery();
        }

        public void Update(EvilScheme scheme)
        {
            var sql = @"UPDATE EvilSchemes SET Name = @name, Description = @desc, Budget = @budget,
                       CurrentSpending = @spending, RequiredSkillLevel = @skill, RequiredSpecialty = @specialty,
                       Status = @status, StartDate = @start, TargetCompletionDate = @target,
                       DiabolicalRating = @rating, SuccessLikelihood = @success
                       WHERE SchemeId = @id";
            var command = new SQLiteCommand(sql, _connection);
            command.Parameters.AddWithValue("@id", scheme.SchemeId);
            command.Parameters.AddWithValue("@name", scheme.Name);
            command.Parameters.AddWithValue("@desc", scheme.Description);
            command.Parameters.AddWithValue("@budget", scheme.Budget);
            command.Parameters.AddWithValue("@spending", scheme.CurrentSpending);
            command.Parameters.AddWithValue("@skill", scheme.RequiredSkillLevel);
            command.Parameters.AddWithValue("@specialty", scheme.RequiredSpecialty);
            command.Parameters.AddWithValue("@status", scheme.Status);
            command.Parameters.AddWithValue("@start", scheme.StartDate.HasValue ? (object)scheme.StartDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
            command.Parameters.AddWithValue("@target", scheme.TargetCompletionDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@rating", scheme.DiabolicalRating);
            command.Parameters.AddWithValue("@success", scheme.SuccessLikelihood);
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            var command = new SQLiteCommand("DELETE FROM EvilSchemes WHERE SchemeId = @id", _connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        private EvilScheme MapFromReader(SQLiteDataReader reader)
        {
            return new EvilScheme
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
    }
}
