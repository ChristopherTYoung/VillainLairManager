using VillainLairManager.Models;

namespace VillainLairManager.Repositories
{
    /// <summary>
    /// Repository interface for Equipment-specific operations
    /// </summary>
    public interface IEquipmentRepository : IRepository<Equipment>
    {
        /// <summary>
        /// Get count of equipment assigned to a specific scheme
        /// </summary>
        int GetSchemeAssignedEquipmentCount(int schemeId);
    }
}
