using System.Collections.Generic;
using System.Threading.Tasks;
using ModernApiProject.Domain.Entities;

namespace ModernApiProject.Domain.Repositories
{
    /// <summary>
    /// Repository interface for News entity operations.
    /// Provides asynchronous methods for CRUD operations on news announcements.
    /// </summary>
    public interface INewsRepository
    {
        /// <summary>
        /// Retrieves a news item by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the news item.</param>
        /// <returns>The news item if found; otherwise, null.</returns>
        Task<News?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all news items from the repository.
        /// </summary>
        /// <returns>A collection of all news items.</returns>
        Task<IEnumerable<News>> GetAllAsync();

        /// <summary>
        /// Adds a new news item to the repository.
        /// </summary>
        /// <param name="news">The news item to add.</param>
        /// <returns>The added news item with generated identifier.</returns>
        Task<News> AddAsync(News news);

        /// <summary>
        /// Updates an existing news item in the repository.
        /// </summary>
        /// <param name="news">The news item with updated information.</param>
        /// <returns>The updated news item.</returns>
        Task<News> UpdateAsync(News news);

        /// <summary>
        /// Deletes a news item from the repository by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the news item to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        Task<bool> DeleteAsync(int id);
    }
}
