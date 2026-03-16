using System.Collections.Generic;
using System.Threading.Tasks;
using ModernApiProject.Domain.Entities;

namespace ModernApiProject.Domain.Repositories
{
    /// <summary>
    /// Repository interface for Student entity operations.
    /// Provides asynchronous methods for CRUD operations on student records.
    /// </summary>
    public interface IStudentRepository
    {
        /// <summary>
        /// Retrieves a student by their registration number.
        /// </summary>
        /// <param name="studentRegno">The unique student registration number.</param>
        /// <returns>The student entity if found; otherwise, null.</returns>
        Task<Student?> GetByIdAsync(string studentRegno);

        /// <summary>
        /// Retrieves all students from the database.
        /// </summary>
        /// <returns>A collection of all student entities.</returns>
        Task<IEnumerable<Student>> GetAllAsync();

        /// <summary>
        /// Adds a new student to the database.
        /// </summary>
        /// <param name="student">The student entity to add.</param>
        /// <returns>The added student entity with any generated values populated.</returns>
        Task<Student> AddAsync(Student student);

        /// <summary>
        /// Updates an existing student in the database.
        /// </summary>
        /// <param name="student">The student entity with updated values.</param>
        /// <returns>The updated student entity.</returns>
        Task<Student> UpdateAsync(Student student);

        /// <summary>
        /// Deletes a student from the database by their registration number.
        /// </summary>
        /// <param name="studentRegno">The unique student registration number.</param>
        /// <returns>True if the student was deleted; otherwise, false.</returns>
        Task<bool> DeleteAsync(string studentRegno);
    }
}
