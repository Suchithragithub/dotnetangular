using System.Collections.Generic;
using System.Threading.Tasks;
using ModernApiProject.Domain.Entities;

namespace ModernApiProject.Domain.Repositories
{
    /// <summary>
    /// Repository interface for Department entity operations.
    /// Provides asynchronous methods for CRUD operations on departments.
    /// </summary>
    public interface IDepartmentRepository
    {
        /// <summary>
        /// Retrieves a department by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the department.</param>
        /// <returns>The department entity if found; otherwise, null.</returns>
        Task<Department?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all departments from the system.
        /// </summary>
        /// <returns>A collection of all department entities.</returns>
        Task<IEnumerable<Department>> GetAllAsync();

        /// <summary>
        /// Adds a new department to the system.
        /// </summary>
        /// <param name="department">The department entity to add.</param>
        /// <returns>The added department entity with generated identifier.</returns>
        Task<Department> AddAsync(Department department);

        /// <summary>
        /// Updates an existing department in the system.
        /// </summary>
        /// <param name="department">The department entity with updated values.</param>
        /// <returns>The updated department entity.</returns>
        Task<Department> UpdateAsync(Department department);

        /// <summary>
        /// Deletes a department from the system by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the department to delete.</param>
        /// <returns>True if the department was deleted successfully; otherwise, false.</returns>
        Task<bool> DeleteAsync(int id);
    }
}
