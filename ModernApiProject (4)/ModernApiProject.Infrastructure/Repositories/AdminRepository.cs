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
    /// Repository implementation for Admin entity operations.
    /// Provides asynchronous CRUD operations using Entity Framework Core.
    /// </summary>
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the AdminRepository class.
        /// </summary>
        /// <param name="context">The database context for admin operations.</param>
        public AdminRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves an admin by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the admin.</param>
        /// <returns>The admin entity if found; otherwise, null.</returns>
        public async Task<Admin?> GetByIdAsync(int id)
        {
            return await _context.Admins.FindAsync(id);
        }

        /// <summary>
        /// Retrieves all admin users from the system.
        /// </summary>
        /// <returns>A collection of all admin entities.</returns>
        public async Task<IEnumerable<Admin>> GetAllAsync()
        {
            return await _context.Admins.ToListAsync();
        }

        /// <summary>
        /// Adds a new admin user to the system.
        /// </summary>
        /// <param name="admin">The admin entity to add.</param>
        /// <returns>The added admin entity with generated identifier.</returns>
        public async Task<Admin> AddAsync(Admin admin)
        {
            if (admin == null)
            {
                throw new ArgumentNullException(nameof(admin));
            }

            await _context.Admins.AddAsync(admin);
            await _context.SaveChangesAsync();
            return admin;
        }

        /// <summary>
        /// Updates an existing admin user's information.
        /// </summary>
        /// <param name="admin">The admin entity with updated information.</param>
        /// <returns>The updated admin entity.</returns>
        public async Task<Admin> UpdateAsync(Admin admin)
        {
            if (admin == null)
            {
                throw new ArgumentNullException(nameof(admin));
            }

            _context.Admins.Update(admin);
            await _context.SaveChangesAsync();
            return admin;
        }

        /// <summary>
        /// Deletes an admin user from the system.
        /// </summary>
        /// <param name="id">The unique identifier of the admin to delete.</param>
        /// <returns>True if deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
            {
                return false;
            }

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}