using System.Collections.Generic;

namespace VillainLairManager.Repositories
{
    /// <summary>
    /// Generic repository interface for basic CRUD operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Get all entities
        /// </summary>
        List<T> GetAll();

        /// <summary>
        /// Get entity by ID
        /// </summary>
        T GetById(int id);

        /// <summary>
        /// Insert a new entity
        /// </summary>
        void Insert(T entity);

        /// <summary>
        /// Update an existing entity
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// Delete an entity by ID
        /// </summary>
        void Delete(int id);
    }
}
