using System.Collections.Generic;
using VillainLairManager.Models;

namespace VillainLairManager.Services
{
    /// <summary>
    /// Service interface for equipment-related business logic
    /// </summary>
    public interface IEquipmentService
    {
        /// <summary>
        /// Performs maintenance on equipment and calculates cost
        /// </summary>
        (bool success, string message, decimal cost) PerformMaintenance(Equipment equipment);

        /// <summary>
        /// Degrades equipment condition based on usage
        /// </summary>
        void DegradeCondition(Equipment equipment);

        /// <summary>
        /// Validates equipment data
        /// </summary>
        (bool isValid, string errorMessage) ValidateEquipment(string name, string category, decimal purchasePrice, decimal maintenanceCost);

        /// <summary>
        /// Assigns equipment to a scheme
        /// </summary>
        (bool success, string message) AssignToScheme(Equipment equipment, int schemeId);

        /// <summary>
        /// Checks if equipment is operational
        /// </summary>
        bool IsOperational(Equipment equipment);

        /// <summary>
        /// Checks if equipment is broken
        /// </summary>
        bool IsBroken(Equipment equipment);

        /// <summary>
        /// Gets all equipment
        /// </summary>
        List<Equipment> GetAllEquipment();

        /// <summary>
        /// Gets equipment by ID
        /// </summary>
        Equipment GetEquipmentById(int equipmentId);

        /// <summary>
        /// Creates new equipment
        /// </summary>
        (bool success, string message, Equipment equipment) CreateEquipment(string name, string category, decimal purchasePrice, decimal maintenanceCost, bool requiresSpecialist, int? storedAtBaseId);

        /// <summary>
        /// Updates equipment
        /// </summary>
        (bool success, string message) UpdateEquipment(Equipment equipment);

        /// <summary>
        /// Deletes equipment
        /// </summary>
        (bool success, string message) DeleteEquipment(int equipmentId);
    }
}
