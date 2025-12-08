using System.Collections.Generic;
using VillainLairManager.Models;

namespace VillainLairManager.Services
{
    /// <summary>
    /// Service interface for minion-related business logic
    /// </summary>
    public interface IMinionService
    {
        /// <summary>
        /// Validates minion data before creation or update
        /// </summary>
        (bool isValid, string errorMessage) ValidateMinion(string name, string specialty, int skillLevel, decimal salary, int loyalty);

        /// <summary>
        /// Calculates the appropriate mood based on loyalty score
        /// </summary>
        string CalculateMood(int loyaltyScore);

        /// <summary>
        /// Creates a new minion with validation and mood calculation
        /// </summary>
        (bool success, string message, Minion minion) CreateMinion(string name, string specialty, int skillLevel, decimal salary, int loyalty, int? baseId, int? schemeId, string mood);

        /// <summary>
        /// Updates an existing minion with validation
        /// </summary>
        (bool success, string message) UpdateMinion(int minionId, string name, string specialty, int skillLevel, decimal salary, int loyalty, int? baseId, int? schemeId, string mood);

        /// <summary>
        /// Deletes a minion
        /// </summary>
        (bool success, string message) DeleteMinion(int minionId);

        /// <summary>
        /// Updates minion loyalty based on payment
        /// </summary>
        void UpdateLoyalty(Minion minion, decimal actualSalaryPaid);

        /// <summary>
        /// Gets all minions
        /// </summary>
        List<Minion> GetAllMinions();

        /// <summary>
        /// Gets a minion by ID
        /// </summary>
        Minion GetMinionById(int minionId);
    }
}
