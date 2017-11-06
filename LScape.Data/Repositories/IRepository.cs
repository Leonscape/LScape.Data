using System.Collections.Generic;

namespace LScape.Data.Repositories
{
    /// <summary>
    /// Basic repository interface for single key object
    /// </summary>
    /// <typeparam name="T">The type of object his repository is for</typeparam>
    /// <typeparam name="TKey">The type of the key for this repository</typeparam>
    public interface IRepository<T, TKey> where T : class, new() where TKey : struct
    {
        /// <summary>
        /// Finds a specific entity in the repository
        /// </summary>
        /// <param name="key">The key value of the entity to find</param>
        T Find(TKey key);

        /// <summary>
        /// Counts how many entities are in the repository
        /// </summary>
        int Count();

        /// <summary>
        /// Returns all the entities in the repository
        /// </summary>
        IEnumerable<T> All();

        /// <summary>
        /// Saves the entity to the repository
        /// </summary>
        /// <param name="entity">The entity to save</param>
        /// <returns>The state of the entity in the repository after the save</returns>
        T Save(T entity);

        /// <summary>
        /// Deletes an entity from the repository
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        void Delete(T entity);

        /// <summary>
        /// Deletes an entity from the repository
        /// </summary>
        /// <param name="key">The key value of the entity to delete</param>
        void Delete(TKey key);
    }
}
