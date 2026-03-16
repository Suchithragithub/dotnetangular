using System.Collections.Generic;
using System.Threading.Tasks;
using ModernApiProject.Domain.Entities;

namespace ModernApiProject.Domain.Repositories
{
    /// <summary>
    /// Repository interface for Course entity operations.
    /// Provides asynchronous methods for CRUD operations on courses.
    /// </summary>
    public interface ICourseRepository
    {
        /// <summary>
        /// Retrieves a course by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the course.</param>
        /// <returns>The course entity if found; otherwise, null.</returns>
        Task<Course?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all courses from the repository.
        /// </summary>
        /// <returns>A collection of all course entities.</returns>
        Task<IEnumerable<Course>> GetAllAsync();

        /// <summary>
        /// Adds a new course to the repository.
        /// </summary>
        /// <param name="course">The course entity to add.</param>
        /// <returns>The added course entity with generated identifier.</returns>
        Task<Course> AddAsync(Course course);

        /// <summary>
        /// Updates an existing course in the repository.
        /// </summary>
        /// <param name="course">The course entity with updated values.</param>
        /// <returns>The updated course entity.</returns>
        Task<Course> UpdateAsync(Course course);

        /// <summary>
        /// Deletes a course from the repository by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the course to delete.</param>
        /// <returns>True if the course was deleted successfully; otherwise, false.</returns>
        Task<bool> DeleteAsync(int id);
    }
}
