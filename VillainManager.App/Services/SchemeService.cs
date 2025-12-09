using System;
using System.Collections.Generic;
using System.Linq;
using VillainLairManager.Models;
using VillainLairManager.Repositories;
using VillainLairManager.Utils;

namespace VillainLairManager.Services
{
    /// <summary>
    /// Service class containing all evil scheme-related business logic
    /// Extracted from UI forms and model classes
    /// </summary>
    public class SchemeService : ISchemeService
    {
        private readonly IEvilSchemeRepository _schemeRepository;
        private readonly IMinionRepository _minionRepository;
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly ConfigManager _config;

        public SchemeService(
            IEvilSchemeRepository schemeRepository,
            IMinionRepository minionRepository,
            IEquipmentRepository equipmentRepository)
        {
            _schemeRepository = schemeRepository ?? throw new ArgumentNullException(nameof(schemeRepository));
            _minionRepository = minionRepository ?? throw new ArgumentNullException(nameof(minionRepository));
            _equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
            _config = ConfigManager.Instance;
        }

        /// <summary>
        /// Calculates success likelihood based on assigned minions, equipment, budget, and timeline
        /// Centralized business logic previously duplicated in EvilScheme model and MainForm
        /// </summary>
        public int CalculateSuccessLikelihood(EvilScheme scheme)
        {
            if (scheme == null)
                throw new ArgumentNullException(nameof(scheme));

            int baseSuccess = 50;

            // Get assigned minions
            var assignedMinions = _minionRepository.GetAll()
                .Where(m => m.CurrentSchemeId == scheme.SchemeId)
                .ToList();

            int matchingMinions = assignedMinions.Count(m => m.Specialty == scheme.RequiredSpecialty);
            int totalMinions = assignedMinions.Count;

            // Minion bonus: each matching specialist adds 10%
            int minionBonus = matchingMinions * 10;

            // Get assigned equipment
            var assignedEquipment = _equipmentRepository.GetAll()
                .Where(e => e.AssignedToSchemeId == scheme.SchemeId && 
                           e.Condition >= _config.MinEquipmentCondition)
                .ToList();

            // Equipment bonus: each working equipment adds 5%
            int equipmentBonus = assignedEquipment.Count * 5;

            // Calculate penalties
            int budgetPenalty = scheme.CurrentSpending > scheme.Budget ? -20 : 0;
            int resourcePenalty = (totalMinions >= 2 && matchingMinions >= 1) ? 0 : -15;
            int timelinePenalty = DateTime.Now > scheme.TargetCompletionDate ? -25 : 0;

            // Calculate final success
            int success = baseSuccess + minionBonus + equipmentBonus + budgetPenalty + resourcePenalty + timelinePenalty;

            // Clamp to 0-100 range
            if (success < 0) success = 0;
            if (success > 100) success = 100;

            return success;
        }

        /// <summary>
        /// Updates and persists the success likelihood for a scheme
        /// </summary>
        public void UpdateSuccessLikelihood(EvilScheme scheme)
        {
            if (scheme == null)
                throw new ArgumentNullException(nameof(scheme));

            scheme.SuccessLikelihood = CalculateSuccessLikelihood(scheme);
            _schemeRepository.Update(scheme);
        }

        /// <summary>
        /// Checks if a scheme has exceeded its budget
        /// </summary>
        public bool IsOverBudget(EvilScheme scheme)
        {
            if (scheme == null)
                throw new ArgumentNullException(nameof(scheme));

            return scheme.CurrentSpending > scheme.Budget;
        }

        /// <summary>
        /// Gets all schemes from repository
        /// </summary>
        public List<EvilScheme> GetAllSchemes()
        {
            return _schemeRepository.GetAll();
        }

        /// <summary>
        /// Gets a specific scheme by ID
        /// </summary>
        public EvilScheme GetSchemeById(int schemeId)
        {
            return _schemeRepository.GetById(schemeId);
        }

        /// <summary>
        /// Gets all schemes with active status
        /// </summary>
        public List<EvilScheme> GetActiveSchemes()
        {
            return _schemeRepository.GetAll()
                .Where(s => s.Status == _config.StatusActive)
                .ToList();
        }

        /// <summary>
        /// Calculates average success likelihood across multiple schemes
        /// Used for dashboard statistics
        /// </summary>
        public double CalculateAverageSuccess(List<EvilScheme> schemes)
        {
            if (schemes == null || !schemes.Any())
                return 0;

            return schemes.Average(s => CalculateSuccessLikelihood(s));
        }
    }
}
