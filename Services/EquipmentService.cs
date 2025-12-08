using System;
using System.Collections.Generic;
using VillainLairManager.Models;
using VillainLairManager.Repositories;
using VillainLairManager.Utils;

namespace VillainLairManager.Services
{
    /// <summary>
    /// Service class containing all equipment-related business logic
    /// Extracted from UI forms and Equipment model
    /// </summary>
    public class EquipmentService : IEquipmentService
    {
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly IEvilSchemeRepository _schemeRepository;
        private readonly ConfigManager _config;

        public EquipmentService(
            IEquipmentRepository equipmentRepository,
            IEvilSchemeRepository schemeRepository)
        {
            _equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
            _schemeRepository = schemeRepository ?? throw new ArgumentNullException(nameof(schemeRepository));
            _config = ConfigManager.Instance;
        }

        /// <summary>
        /// Performs maintenance on equipment, restores condition, and calculates cost
        /// Business logic extracted from Equipment.PerformMaintenance()
        /// </summary>
        public (bool success, string message, decimal cost) PerformMaintenance(Equipment equipment)
        {
            if (equipment == null)
                throw new ArgumentNullException(nameof(equipment));

            try
            {
                // Calculate maintenance cost based on equipment category
                decimal cost;
                if (equipment.Category == "Doomsday Device")
                {
                    cost = equipment.PurchasePrice * _config.DoomsdayMaintenanceCostPercentage;
                }
                else
                {
                    cost = equipment.PurchasePrice * _config.MaintenanceCostPercentage;
                }

                // Restore condition to default
                equipment.Condition = _config.DefaultCondition;
                equipment.LastMaintenanceDate = DateTime.Now;

                // Persist changes
                _equipmentRepository.Update(equipment);

                return (true, $"Maintenance completed. Cost: ${cost:N2}", cost);
            }
            catch (Exception ex)
            {
                return (false, $"Error performing maintenance: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Degrades equipment condition based on usage in active schemes
        /// Business logic extracted from Equipment.DegradeCondition()
        /// </summary>
        public void DegradeCondition(Equipment equipment)
        {
            if (equipment == null)
                throw new ArgumentNullException(nameof(equipment));

            // Only degrade if assigned to a scheme
            if (!equipment.AssignedToSchemeId.HasValue)
                return;

            // Check if scheme is active
            var scheme = _schemeRepository.GetById(equipment.AssignedToSchemeId.Value);
            if (scheme == null || scheme.Status != _config.StatusActive)
                return;

            // Calculate degradation based on time since last maintenance
            int monthsSinceMaintenance = 1; // Simplified calculation
            if (equipment.LastMaintenanceDate.HasValue)
            {
                var timeSpan = DateTime.Now - equipment.LastMaintenanceDate.Value;
                monthsSinceMaintenance = Math.Max(1, (int)(timeSpan.TotalDays / 30));
            }

            int degradation = monthsSinceMaintenance * _config.ConditionDegradationRate;
            equipment.Condition -= degradation;

            // Ensure condition doesn't go below 0
            if (equipment.Condition < 0)
                equipment.Condition = 0;

            _equipmentRepository.Update(equipment);
        }

        /// <summary>
        /// Validates equipment data before creation or update
        /// </summary>
        public (bool isValid, string errorMessage) ValidateEquipment(string name, string category, decimal purchasePrice, decimal maintenanceCost)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return (false, "Equipment name is required!");
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                return (false, "Equipment category is required!");
            }

            if (purchasePrice < 0)
            {
                return (false, "Purchase price cannot be negative!");
            }

            if (maintenanceCost < 0)
            {
                return (false, "Maintenance cost cannot be negative!");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Assigns equipment to a scheme
        /// </summary>
        public (bool success, string message) AssignToScheme(Equipment equipment, int schemeId)
        {
            if (equipment == null)
                throw new ArgumentNullException(nameof(equipment));

            try
            {
                // Verify scheme exists
                var scheme = _schemeRepository.GetById(schemeId);
                if (scheme == null)
                {
                    return (false, "Scheme not found!");
                }

                equipment.AssignedToSchemeId = schemeId;
                _equipmentRepository.Update(equipment);

                return (true, $"Equipment assigned to scheme '{scheme.Name}'");
            }
            catch (Exception ex)
            {
                return (false, $"Error assigning equipment: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if equipment is operational (above minimum condition threshold)
        /// </summary>
        public bool IsOperational(Equipment equipment)
        {
            if (equipment == null)
                throw new ArgumentNullException(nameof(equipment));

            return equipment.Condition >= _config.MinEquipmentCondition;
        }

        /// <summary>
        /// Checks if equipment is broken (below broken condition threshold)
        /// </summary>
        public bool IsBroken(Equipment equipment)
        {
            if (equipment == null)
                throw new ArgumentNullException(nameof(equipment));

            return equipment.Condition < _config.BrokenEquipmentCondition;
        }

        /// <summary>
        /// Gets all equipment from repository
        /// </summary>
        public List<Equipment> GetAllEquipment()
        {
            return _equipmentRepository.GetAll();
        }

        /// <summary>
        /// Gets a specific equipment by ID
        /// </summary>
        public Equipment GetEquipmentById(int equipmentId)
        {
            return _equipmentRepository.GetById(equipmentId);
        }

        /// <summary>
        /// Creates new equipment with validation
        /// </summary>
        public (bool success, string message, Equipment equipment) CreateEquipment(string name, string category, decimal purchasePrice, decimal maintenanceCost, bool requiresSpecialist, int? storedAtBaseId)
        {
            // Validate input
            var validation = ValidateEquipment(name, category, purchasePrice, maintenanceCost);
            if (!validation.isValid)
            {
                return (false, validation.errorMessage, null);
            }

            var equipment = new Equipment
            {
                Name = name,
                Category = category,
                Condition = _config.DefaultCondition,
                PurchasePrice = purchasePrice,
                MaintenanceCost = maintenanceCost,
                RequiresSpecialist = requiresSpecialist,
                StoredAtBaseId = storedAtBaseId,
                AssignedToSchemeId = null,
                LastMaintenanceDate = DateTime.Now
            };

            try
            {
                _equipmentRepository.Insert(equipment);
                return (true, "Equipment created successfully!", equipment);
            }
            catch (Exception ex)
            {
                return (false, $"Error creating equipment: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Updates equipment with validation
        /// </summary>
        public (bool success, string message) UpdateEquipment(Equipment equipment)
        {
            if (equipment == null)
                throw new ArgumentNullException(nameof(equipment));

            // Validate
            var validation = ValidateEquipment(equipment.Name, equipment.Category, equipment.PurchasePrice, equipment.MaintenanceCost);
            if (!validation.isValid)
            {
                return (false, validation.errorMessage);
            }

            try
            {
                _equipmentRepository.Update(equipment);
                return (true, "Equipment updated successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating equipment: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes equipment
        /// </summary>
        public (bool success, string message) DeleteEquipment(int equipmentId)
        {
            try
            {
                _equipmentRepository.Delete(equipmentId);
                return (true, "Equipment deleted successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting equipment: {ex.Message}");
            }
        }
    }
}
