using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModernApiProject.Domain.Entities;
using ModernApiProject.Domain.Repositories;
using ModernApiProject.Infrastructure.Data;

namespace ModernApiProject.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Course Enrollment entity operations.
    /// Provides asynchronous CRUD operations and specialized queries for course enrollment records.
    /// </summary>
    public class CourseenrollRepository : ICourseenrollRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the CourseenrollRepository class.
        /// </summary>
        /// <param name="context">The database context for data access.</param>
        public CourseenrollRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves a course enrollment by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the course enrollment.</param>
        /// <returns>The course enrollment entity if found; otherwise, null.</returns>
        public async Task<Courseenroll?> GetByIdAsync(int id)
        {
            return await _context.Courseenrolls
                .FindAsync(id);
        }

        /// <summary>
        /// Retrieves all course enrollments from the repository.
        /// </summary>
        /// <returns>A collection of all course enrollment entities.</returns>
        public async Task<IEnumerable<Courseenroll>> GetAllAsync()
        {
            return await _context.Courseenrolls
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new course enrollment to the repository.
        /// </summary>
        /// <param name="courseenroll">The course enrollment entity to add.</param>
        /// <returns>The added course enrollment entity with any generated values (e.g., ID).</returns>
        public async Task<Courseenroll> AddAsync(Courseenroll courseenroll)
        {
            if (courseenroll == null)
            {
                throw new ArgumentNullException(nameof(courseenroll));
            }

            await _context.Courseenrolls.AddAsync(courseenroll);
            await _context.SaveChangesAsync();
            return courseenroll;
        }

        /// <summary>
        /// Updates an existing course enrollment in the repository.
        /// </summary>
        /// <param name="courseenroll">The course enrollment entity with updated values.</param>
        /// <returns>The updated course enrollment entity.</returns>
        public async Task<Courseenroll> UpdateAsync(Courseenroll courseenroll)
        {
            if (courseenroll == null)
            {
                throw new ArgumentNullException(nameof(courseenroll));
            }

            _context.Courseenrolls.Update(courseenroll);
            await _context.SaveChangesAsync();
            return courseenroll;
        }

        /// <summary>
        /// Deletes a course enrollment from the repository by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the course enrollment to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var courseenroll = await _context.Courseenrolls.FindAsync(id);
            if (courseenroll == null)
            {
                return false;
            }

            _context.Courseenrolls.Remove(courseenroll);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves all course enrollments for a specific student.
        /// </summary>
        /// <param name="studentRegno">The student registration number.</param>
        /// <returns>A collection of course enrollment entities for the specified student.</returns>
        public async Task<IEnumerable<Courseenroll>> GetByStudentRegnoAsync(string studentRegno)
        {
            if (string.IsNullOrWhiteSpace(studentRegno))
            {
                throw new ArgumentException("Student registration number cannot be null or empty.", nameof(studentRegno));
            }

            return await _context.Courseenrolls
                .Where(ce => ce.StudentRegno == studentRegno)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all course enrollments for a specific course.
        /// </summary>
        /// <param name="courseId">The course identifier.</param>
        /// <returns>A collection of course enrollment entities for the specified course.</returns>
        public async Task<IEnumerable<Courseenroll>> GetByCourseIdAsync(int courseId)
        {
            return await _context.Courseenrolls
                .Where(ce => ce.Course == courseId)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all course enrollments for a specific session, department, level, and semester combination.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="departmentId">The department identifier.</param>
        /// <param name="levelId">The level identifier.</param>
        /// <param name="semesterId">The semester identifier.</param>
        /// <returns>A collection of course enrollment entities matching the criteria.</returns>
        public async Task<IEnumerable<Courseenroll>> GetByAcademicContextAsync(int sessionId, int departmentId, int levelId, int semesterId)
        {
            return await _context.Courseenrolls
                .Where(ce => ce.Session == sessionId &&
                            ce.Department == departmentId &&
                            ce.Level == levelId &&
                            ce.Semester == semesterId)
                .ToListAsync();
        }

        /// <summary>
        /// Checks if a student is already enrolled in a specific course.
        /// </summary>
        /// <param name="studentRegno">The student registration number.</param>
        /// <param name="courseId">The course identifier.</param>
        /// <returns>True if the student is already enrolled; otherwise, false.</returns>
        public async Task<bool> IsStudentEnrolledAsync(string studentRegno, int courseId)
        {
            if (string.IsNullOrWhiteSpace(studentRegno))
            {
                throw new ArgumentException("Student registration number cannot be null or empty.", nameof(studentRegno));
            }

            return await _context.Courseenrolls
                .AnyAsync(ce => ce.StudentRegno == studentRegno && ce.Course == courseId);
        }

        /// <summary>
        /// Gets the count of enrollments for a specific course.
        /// </summary>
        /// <param name="courseId">The course identifier.</param>
        /// <returns>The number of students enrolled in the course.</returns>
        public async Task<int> GetEnrollmentCountByCourseAsync(int courseId)
        {
            return await _context.Courseenrolls
                .CountAsync(ce => ce.Course == courseId);
        }
    }
}