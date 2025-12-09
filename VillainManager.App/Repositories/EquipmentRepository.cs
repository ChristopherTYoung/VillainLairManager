using System;
using System.Collections.Generic;
using System.Data.SQLite;
using VillainLairManager.Models;

namespace VillainLairManager.Repositories
{
    /// <summary>
    /// Concrete repository implementation for Equipment entities
    /// </summary>
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly SQLiteConnection _connection;

        public EquipmentRepository(SQLiteConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public List<Equipment> GetAll()
        {
            var equipment = new List<Equipment>();
            var command = new SQLiteCommand("SELECT * FROM Equipment", _connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                equipment.Add(MapFromReader(reader));
            }

            return equipment;
        }

        public Equipment GetById(int id)
        {
            var command = new SQLiteCommand("SELECT * FROM Equipment WHERE EquipmentId = @id", _connection);
            command.Parameters.AddWithValue("@id", id);
            var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return MapFromReader(reader);
            }

            return null;
        }

        public void Insert(Equipment equipment)
        {
            var sql = @"INSERT INTO Equipment (Name, Category, Condition, PurchasePrice, MaintenanceCost, AssignedToSchemeId, StoredAtBaseId, RequiresSpecialist, LastMaintenanceDate)
                       VALUES (@name, @category, @condition, @price, @maintenance, @schemeId, @baseId, @specialist, @lastMaint)";
            var command = new SQLiteCommand(sql, _connection);
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

        public void Update(Equipment equipment)
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

        public void Delete(int id)
        {
            var command = new SQLiteCommand("DELETE FROM Equipment WHERE EquipmentId = @id", _connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        public int GetSchemeAssignedEquipmentCount(int schemeId)
        {
            var command = new SQLiteCommand("SELECT COUNT(*) FROM Equipment WHERE AssignedToSchemeId = @id", _connection);
            command.Parameters.AddWithValue("@id", schemeId);
            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        private Equipment MapFromReader(SQLiteDataReader reader)
        {
            return new Equipment
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
            };
        }
    }
}
