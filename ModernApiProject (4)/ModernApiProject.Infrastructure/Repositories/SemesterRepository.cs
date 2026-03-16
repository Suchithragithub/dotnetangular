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
    /// Repository implementation for Semester entity operations.
    /// Provides concrete implementation of CRUD operations using Entity Framework Core.
    /// </summary>
    public class SemesterRepository : ISemesterRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the SemesterRepository class.
        /// </summary>
        /// <param name="context">The database context for data access.</param>
        public SemesterRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves a semester by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the semester.</param>
        /// <returns>The semester entity if found; otherwise, null.</returns>
        public async Task<Semester?> GetByIdAsync(int id)
        {
            return await _context.Semesters
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Retrieves all semesters from the repository.
        /// </summary>
        /// <returns>A collection of all semester entities.</returns>
        public async Task<IEnumerable<Semester>> GetAllAsync()
        {
            return await _context.Semesters
                .AsNoTracking()
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new semester to the repository.
        /// </summary>
        /// <param name="semester">The semester entity to add.</param>
        /// <returns>The added semester entity with generated identifier.</returns>
        public async Task<Semester> AddAsync(Semester semester)
        {
            if (semester == null)
            {
                throw new ArgumentNullException(nameof(semester));
            }

            // Set creation date if not already set
            if (semester.CreationDate == default)
            {
                semester.CreationDate = DateTime.UtcNow;
            }

            await _context.Semesters.AddAsync(semester);
            await _context.SaveChangesAsync();

            return semester;
        }

        /// <summary>
        /// Updates an existing semester in the repository.
        /// </summary>
        /// <param name="semester">The semester entity with updated values.</param>
        /// <returns>The updated semester entity.</returns>
        public async Task<Semester> UpdateAsync(Semester semester)
        {
            if (semester == null)
            {
                throw new ArgumentNullException(nameof(semester));
            }

            var existingSemester = await _context.Semesters
                .FirstOrDefaultAsync(s => s.Id == semester.Id);

            if (existingSemester == null)
            {
                throw new InvalidOperationException($"Semester with ID {semester.Id} not found.");
            }

            // Update properties
            existingSemester.SemesterName = semester.SemesterName;
            existingSemester.UpdationDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            _context.Semesters.Update(existingSemester);
            await _context.SaveChangesAsync();

            return existingSemester;
        }

        /// <summary>
        /// Deletes a semester from the repository by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the semester to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var semester = await _context.Semesters
                .FirstOrDefaultAsync(s => s.Id == id);

            if (semester == null)
            {
                return false;
            }

            _context.Semesters.Remove(semester);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
