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
    /// Repository implementation for Department entity operations.
    /// Provides asynchronous CRUD operations using Entity Framework Core.
    /// </summary>
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the DepartmentRepository class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public DepartmentRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves a department by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the department.</param>
        /// <returns>The department entity if found; otherwise, null.</returns>
        public async Task<Department?> GetByIdAsync(int id)
        {
            return await _context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        /// <summary>
        /// Retrieves all departments from the system.
        /// </summary>
        /// <returns>A collection of all department entities.</returns>
        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await _context.Departments
                .AsNoTracking()
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new department to the system.
        /// </summary>
        /// <param name="department">The department entity to add.</param>
        /// <returns>The added department entity with generated identifier.</returns>
        public async Task<Department> AddAsync(Department department)
        {
            if (department == null)
            {
                throw new ArgumentNullException(nameof(department));
            }

            // Set creation date if not already set
            if (department.CreationDate == default)
            {
                department.CreationDate = DateTime.UtcNow;
            }

            await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();

            return department;
        }

        /// <summary>
        /// Updates an existing department in the system.
        /// </summary>
        /// <param name="department">The department entity with updated values.</param>
        /// <returns>The updated department entity.</returns>
        public async Task<Department> UpdateAsync(Department department)
        {
            if (department == null)
            {
                throw new ArgumentNullException(nameof(department));
            }

            var existingDepartment = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == department.Id);

            if (existingDepartment == null)
            {
                throw new InvalidOperationException($"Department with ID {department.Id} not found.");
            }

            // Update properties
            existingDepartment.DepartmentName = department.DepartmentName;
            // Note: CreationDate should not be updated

            _context.Departments.Update(existingDepartment);
            await _context.SaveChangesAsync();

            return existingDepartment;
        }

        /// <summary>
        /// Deletes a department from the system by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the department to delete.</param>
        /// <returns>True if the department was deleted successfully; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return false;
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
