using System.Collections.Generic;
using System.Threading.Tasks;
using ModernApiProject.Domain.Entities;

namespace ModernApiProject.Domain.Repositories
{
    /// <summary>
    /// Repository interface for managing Course Enrollment entities.
    /// Provides asynchronous CRUD operations for course enrollment records.
    /// </summary>
    public interface ICourseenrollRepository
    {
        /// <summary>
        /// Retrieves a course enrollment by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the course enrollment.</param>
        /// <returns>The course enrollment entity if found; otherwise, null.</returns>
        Task<Courseenroll?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all course enrollments from the repository.
        /// </summary>
        /// <returns>A collection of all course enrollment entities.</returns>
        Task<IEnumerable<Courseenroll>> GetAllAsync();

        /// <summary>
        /// Adds a new course enrollment to the repository.
        /// </summary>
        /// <param name="courseenroll">The course enrollment entity to add.</param>
        /// <returns>The added course enrollment entity with any generated values (e.g., ID).</returns>
        Task<Courseenroll> AddAsync(Courseenroll courseenroll);

        /// <summary>
        /// Updates an existing course enrollment in the repository.
        /// </summary>
        /// <param name="courseenroll">The course enrollment entity with updated values.</param>
        /// <returns>The updated course enrollment entity.</returns>
        Task<Courseenroll> UpdateAsync(Courseenroll courseenroll);

        /// <summary>
        /// Deletes a course enrollment from the repository by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the course enrollment to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Retrieves all course enrollments for a specific student.
        /// </summary>
        /// <param name="studentRegno">The student registration number.</param>
        /// <returns>A collection of course enrollment entities for the specified student.</returns>
        Task<IEnumerable<Courseenroll>> GetByStudentRegnoAsync(string studentRegno);

        /// <summary>
        /// Retrieves all course enrollments for a specific course.
        /// </summary>
        /// <param name="courseId">The course identifier.</param>
        /// <returns>A collection of course enrollment entities for the specified course.</returns>
        Task<IEnumerable<Courseenroll>> GetByCourseIdAsync(int courseId);

        /// <summary>
        /// Retrieves all course enrollments for a specific session, department, level, and semester combination.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="departmentId">The department identifier.</param>
        /// <param name="levelId">The level identifier.</param>
        /// <param name="semesterId">The semester identifier.</param>
        /// <returns>A collection of course enrollment entities matching the criteria.</returns>
        Task<IEnumerable<Courseenroll>> GetByAcademicContextAsync(int sessionId, int departmentId, int levelId, int semesterId);

        /// <summary>
        /// Checks if a student is already enrolled in a specific course.
        /// </summary>
        /// <param name="studentRegno">The student registration number.</param>
        /// <param name="courseId">The course identifier.</param>
        /// <returns>True if the student is already enrolled; otherwise, false.</returns>
        Task<bool> IsStudentEnrolledAsync(string studentRegno, int courseId);

        /// <summary>
        /// Gets the count of enrollments for a specific course.
        /// </summary>
        /// <param name="courseId">The course identifier.</param>
        /// <returns>The number of students enrolled in the course.</returns>
        Task<int> GetEnrollmentCountByCourseAsync(int courseId);
    }
}
