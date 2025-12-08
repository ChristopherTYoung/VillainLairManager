using VillainLairManager.Models;

namespace VillainLairManager.Repositories
{
    /// <summary>
    /// Repository interface for Minion-specific operations
    /// </summary>
    public interface IMinionRepository : IRepository<Minion>
    {
        /// <summary>
        /// Get minions assigned to a specific scheme
        /// </summary>
        int GetSchemeAssignedMinionsCount(int schemeId);

        /// <summary>
        /// Get minions at a specific base
        /// </summary>
        int GetBaseOccupancy(int baseId);
    }
}
