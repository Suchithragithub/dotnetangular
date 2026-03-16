using System.Collections.Generic;
using System.Threading.Tasks;
using ModernApiProject.Domain.Entities;

namespace ModernApiProject.Domain.Repositories
{
    /// <summary>
    /// Repository interface for Admin entity operations.
    /// Provides asynchronous methods for CRUD operations on administrative users.
    /// </summary>
    public interface IAdminRepository
    {
        /// <summary>
        /// Retrieves an admin by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the admin.</param>
        /// <returns>The admin entity if found; otherwise, null.</returns>
        Task<Admin?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all admin users from the system.
        /// </summary>
        /// <returns>A collection of all admin entities.</returns>
        Task<IEnumerable<Admin>> GetAllAsync();

        /// <summary>
        /// Adds a new admin user to the system.
        /// </summary>
        /// <param name="admin">The admin entity to add.</param>
        /// <returns>The added admin entity with generated identifier.</returns>
        Task<Admin> AddAsync(Admin admin);

        /// <summary>
        /// Updates an existing admin user's information.
        /// </summary>
        /// <param name="admin">The admin entity with updated information.</param>
        /// <returns>The updated admin entity.</returns>
        Task<Admin> UpdateAsync(Admin admin);

        /// <summary>
        /// Deletes an admin user from the system.
        /// </summary>
        /// <param name="id">The unique identifier of the admin to delete.</param>
        /// <returns>True if deletion was successful; otherwise, false.</returns>
        Task<bool> DeleteAsync(int id);
    }
}
