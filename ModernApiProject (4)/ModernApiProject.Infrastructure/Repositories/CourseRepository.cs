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
    /// Repository implementation for Course entity operations.
    /// Provides asynchronous CRUD operations using Entity Framework Core.
    /// </summary>
    public class CourseRepository : ICourseRepository
    {
        private readonly ApplicationDbContext _context;

        public CourseRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public async Task<Course?> GetByIdAsync(int id)
        {
            return await _context.Courses.FindAsync(id);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await _context.Courses.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Course> AddAsync(Course course)
        {
            if (course == null)
                throw new ArgumentNullException(nameof(course));

            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
            return course;
        }

        /// <inheritdoc />
        public async Task<Course> UpdateAsync(Course course)
        {
            if (course == null)
                throw new ArgumentNullException(nameof(course));

            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
            return course;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return false;

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}