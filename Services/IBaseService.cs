using System.Collections.Generic;
using VillainLairManager.Models;

namespace VillainLairManager.Services
{
    /// <summary>
    /// Service interface for secret base-related business logic
    /// </summary>
    public interface IBaseService
    {
        /// <summary>
        /// Gets the current occupancy of a base
        /// </summary>
        int GetCurrentOccupancy(int baseId);

        /// <summary>
        /// Gets the available capacity of a base
        /// </summary>
        int GetAvailableCapacity(SecretBase baseObj);

        /// <summary>
        /// Checks if a base can accommodate another minion
        /// </summary>
        bool CanAccommodateMinion(SecretBase baseObj);

        /// <summary>
        /// Gets all bases
        /// </summary>
        List<SecretBase> GetAllBases();

        /// <summary>
        /// Gets a base by ID
        /// </summary>
        SecretBase GetBaseById(int baseId);
    }
}
