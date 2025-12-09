using System;
using System.Collections.Generic;
using VillainLairManager.Models;
using VillainLairManager.Repositories;
using VillainLairManager.Utils;

namespace VillainLairManager.Services
{
    /// <summary>
    /// Service class containing all minion-related business logic
    /// Extracted from UI event handlers and model classes
    /// </summary>
    public class MinionService : IMinionService
    {
        private readonly IMinionRepository _minionRepository;
        private readonly ConfigManager _config;

        public MinionService(IMinionRepository minionRepository)
        {
            _minionRepository = minionRepository ?? throw new ArgumentNullException(nameof(minionRepository));
            _config = ConfigManager.Instance;
        }

        /// <summary>
        /// Validates all minion data according to business rules
        /// </summary>
        public (bool isValid, string errorMessage) ValidateMinion(string name, string specialty, int skillLevel, decimal salary, int loyalty)
        {
            // Validate name
            if (string.IsNullOrWhiteSpace(name))
            {
                return (false, "Name is required!");
            }

            // Validate specialty
            if (string.IsNullOrEmpty(specialty) || !ValidationHelper.IsValidSpecialty(specialty))
            {
                return (false, $"Invalid specialty! Must be one of: {string.Join(", ", _config.ValidSpecialties)}");
            }

            // Validate skill level
            if (!ValidationHelper.IsValidSkillLevel(skillLevel))
            {
                return (false, $"Skill level must be between {_config.SkillLevelRange.Min} and {_config.SkillLevelRange.Max}!");
            }

            // Validate salary
            if (salary < 0)
            {
                return (false, "Salary cannot be negative!");
            }

            // Validate loyalty
            if (!ValidationHelper.IsValidLoyalty(loyalty))
            {
                return (false, $"Loyalty must be between {_config.LoyaltyScoreRange.Min} and {_config.LoyaltyScoreRange.Max}!");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Calculates mood status based on loyalty score using business rules
        /// </summary>
        public string CalculateMood(int loyaltyScore)
        {
            if (loyaltyScore > _config.HighLoyaltyThreshold)
                return _config.MoodHappy;
            else if (loyaltyScore < _config.LowLoyaltyThreshold)
                return _config.MoodBetrayal;
            else
                return _config.MoodGrumpy;
        }

        /// <summary>
        /// Creates a new minion with full validation and business logic
        /// </summary>
        public (bool success, string message, Minion minion) CreateMinion(string name, string specialty, int skillLevel, decimal salary, int loyalty, int? baseId, int? schemeId, string mood)
        {
            // Validate input
            var validation = ValidateMinion(name, specialty, skillLevel, salary, loyalty);
            if (!validation.isValid)
            {
                return (false, validation.errorMessage, null);
            }

            // Calculate mood if not provided
            if (string.IsNullOrEmpty(mood))
            {
                mood = CalculateMood(loyalty);
            }

            // Create minion object
            var minion = new Minion
            {
                Name = name,
                SkillLevel = skillLevel,
                Specialty = specialty,
                LoyaltyScore = loyalty,
                SalaryDemand = salary,
                CurrentBaseId = baseId,
                CurrentSchemeId = schemeId,
                MoodStatus = mood,
                LastMoodUpdate = DateTime.Now
            };

            try
            {
                _minionRepository.Insert(minion);
                return (true, "Minion added successfully!", minion);
            }
            catch (Exception ex)
            {
                return (false, $"Error adding minion: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Updates an existing minion with validation
        /// </summary>
        public (bool success, string message) UpdateMinion(int minionId, string name, string specialty, int skillLevel, decimal salary, int loyalty, int? baseId, int? schemeId, string mood)
        {
            // Validate input
            var validation = ValidateMinion(name, specialty, skillLevel, salary, loyalty);
            if (!validation.isValid)
            {
                return (false, validation.errorMessage);
            }

            // Calculate mood if not provided
            if (string.IsNullOrEmpty(mood))
            {
                mood = CalculateMood(loyalty);
            }

            // Create updated minion object
            var minion = new Minion
            {
                MinionId = minionId,
                Name = name,
                SkillLevel = skillLevel,
                Specialty = specialty,
                LoyaltyScore = loyalty,
                SalaryDemand = salary,
                CurrentBaseId = baseId,
                CurrentSchemeId = schemeId,
                MoodStatus = mood,
                LastMoodUpdate = DateTime.Now
            };

            try
            {
                _minionRepository.Update(minion);
                return (true, "Minion updated successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating minion: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a minion
        /// </summary>
        public (bool success, string message) DeleteMinion(int minionId)
        {
            try
            {
                _minionRepository.Delete(minionId);
                return (true, "Minion deleted successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting minion: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates minion loyalty based on actual salary paid
        /// Business logic for loyalty growth/decay
        /// </summary>
        public void UpdateLoyalty(Minion minion, decimal actualSalaryPaid)
        {
            if (minion == null)
                throw new ArgumentNullException(nameof(minion));

            // Apply loyalty changes based on payment
            if (actualSalaryPaid >= minion.SalaryDemand)
            {
                minion.LoyaltyScore += _config.LoyaltyGrowthRate;
            }
            else
            {
                minion.LoyaltyScore -= _config.LoyaltyDecayRate;
            }

            // Clamp to valid range
            if (minion.LoyaltyScore > _config.LoyaltyScoreRange.Max)
                minion.LoyaltyScore = _config.LoyaltyScoreRange.Max;
            if (minion.LoyaltyScore < _config.LoyaltyScoreRange.Min)
                minion.LoyaltyScore = _config.LoyaltyScoreRange.Min;

            // Update mood based on new loyalty
            minion.MoodStatus = CalculateMood(minion.LoyaltyScore);
            minion.LastMoodUpdate = DateTime.Now;

            // Persist changes
            _minionRepository.Update(minion);
        }

        /// <summary>
        /// Gets all minions from repository
        /// </summary>
        public List<Minion> GetAllMinions()
        {
            return _minionRepository.GetAll();
        }

        /// <summary>
        /// Gets a specific minion by ID
        /// </summary>
        public Minion GetMinionById(int minionId)
        {
            return _minionRepository.GetById(minionId);
        }
    }
}
