using System;
using System.Collections.Generic;
using System.Linq;
using VillainLairManager.Models;
using VillainLairManager.Repositories;

namespace VillainLairManager.Services
{
    /// <summary>
    /// Service class containing all secret base-related business logic
    /// Extracted from SecretBase model
    /// </summary>
    public class BaseService : IBaseService
    {
        private readonly ISecretBaseRepository _baseRepository;
        private readonly IMinionRepository _minionRepository;

        public BaseService(
            ISecretBaseRepository baseRepository,
            IMinionRepository minionRepository)
        {
            _baseRepository = baseRepository ?? throw new ArgumentNullException(nameof(baseRepository));
            _minionRepository = minionRepository ?? throw new ArgumentNullException(nameof(minionRepository));
        }

        /// <summary>
        /// Calculates current occupancy by counting assigned minions
        /// Business logic extracted from SecretBase.GetCurrentOccupancy()
        /// </summary>
        public int GetCurrentOccupancy(int baseId)
        {
            var minions = _minionRepository.GetAll();
            return minions.Count(m => m.CurrentBaseId == baseId);
        }

        /// <summary>
        /// Calculates available capacity in a base
        /// Business logic extracted from SecretBase.GetAvailableCapacity()
        /// </summary>
        public int GetAvailableCapacity(SecretBase baseObj)
        {
            if (baseObj == null)
                throw new ArgumentNullException(nameof(baseObj));

            int currentOccupancy = GetCurrentOccupancy(baseObj.BaseId);
            return baseObj.Capacity - currentOccupancy;
        }

        /// <summary>
        /// Checks if a base has available capacity for another minion
        /// Business logic extracted from SecretBase.CanAccommodateMinion()
        /// </summary>
        public bool CanAccommodateMinion(SecretBase baseObj)
        {
            if (baseObj == null)
                throw new ArgumentNullException(nameof(baseObj));

            return GetCurrentOccupancy(baseObj.BaseId) < baseObj.Capacity;
        }

        /// <summary>
        /// Gets all bases from repository
        /// </summary>
        public List<SecretBase> GetAllBases()
        {
            return _baseRepository.GetAll();
        }

        /// <summary>
        /// Gets a specific base by ID
        /// </summary>
        public SecretBase GetBaseById(int baseId)
        {
            return _baseRepository.GetById(baseId);
        }
    }
}
