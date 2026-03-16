using System.Collections.Generic;
using System.Threading.Tasks;
using ModernApiProject.Domain.Entities;

namespace ModernApiProject.Domain.Repositories
{
    /// <summary>
    /// Repository interface for Semester entity operations.
    /// Provides asynchronous methods for CRUD operations on semester data.
    /// </summary>
    public interface ISemesterRepository
    {
        /// <summary>
        /// Retrieves a semester by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the semester.</param>
        /// <returns>The semester entity if found; otherwise, null.</returns>
        Task<Semester?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all semesters from the repository.
        /// </summary>
        /// <returns>A collection of all semester entities.</returns>
        Task<IEnumerable<Semester>> GetAllAsync();

        /// <summary>
        /// Adds a new semester to the repository.
        /// </summary>
        /// <param name="semester">The semester entity to add.</param>
        /// <returns>The added semester entity with generated identifier.</returns>
        Task<Semester> AddAsync(Semester semester);

        /// <summary>
        /// Updates an existing semester in the repository.
        /// </summary>
        /// <param name="semester">The semester entity with updated values.</param>
        /// <returns>The updated semester entity.</returns>
        Task<Semester> UpdateAsync(Semester semester);

        /// <summary>
        /// Deletes a semester from the repository by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the semester to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        Task<bool> DeleteAsync(int id);
    }
}
