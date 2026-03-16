using System.Collections.Generic;
using System.Threading.Tasks;
using ModernApiProject.Domain.Entities;

namespace ModernApiProject.Domain.Repositories
{
    /// <summary>
    /// Repository interface for Level entity operations.
    /// Provides asynchronous methods for CRUD operations on academic levels.
    /// </summary>
    public interface ILevelRepository
    {
        /// <summary>
        /// Retrieves a level by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the level.</param>
        /// <returns>The level entity if found; otherwise, null.</returns>
        Task<Level?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all levels from the repository.
        /// </summary>
        /// <returns>A collection of all level entities.</returns>
        Task<IEnumerable<Level>> GetAllAsync();

        /// <summary>
        /// Adds a new level to the repository.
        /// </summary>
        /// <param name="level">The level entity to add.</param>
        /// <returns>The added level entity with generated identifier.</returns>
        Task<Level> AddAsync(Level level);

        /// <summary>
        /// Updates an existing level in the repository.
        /// </summary>
        /// <param name="level">The level entity with updated values.</param>
        /// <returns>The updated level entity.</returns>
        Task<Level> UpdateAsync(Level level);

        /// <summary>
        /// Deletes a level from the repository by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the level to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        Task<bool> DeleteAsync(int id);
    }
}
