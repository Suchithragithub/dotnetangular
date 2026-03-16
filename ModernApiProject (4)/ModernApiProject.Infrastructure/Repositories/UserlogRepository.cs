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
    /// Repository implementation for Userlog entity operations.
    /// Provides asynchronous CRUD operations using Entity Framework Core.
    /// </summary>
    public class UserlogRepository : IUserlogRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the UserlogRepository class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public UserlogRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves a user log entry by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user log entry.</param>
        /// <returns>The user log entity if found; otherwise, null.</returns>
        public async Task<Userlog?> GetByIdAsync(int id)
        {
            return await _context.Userlogs.FindAsync(id);
        }

        /// <summary>
        /// Retrieves all user log entries from the database.
        /// </summary>
        /// <returns>A collection of all user log entities.</returns>
        public async Task<IEnumerable<Userlog>> GetAllAsync()
        {
            return await _context.Userlogs.ToListAsync();
        }

        /// <summary>
        /// Adds a new user log entry to the database.
        /// </summary>
        /// <param name="userlog">The user log entity to add.</param>
        /// <returns>The added user log entity with generated identifier.</returns>
        public async Task<Userlog> AddAsync(Userlog userlog)
        {
            if (userlog == null)
            {
                throw new ArgumentNullException(nameof(userlog));
            }

            await _context.Userlogs.AddAsync(userlog);
            await _context.SaveChangesAsync();
            return userlog;
        }

        /// <summary>
        /// Updates an existing user log entry in the database.
        /// </summary>
        /// <param name="userlog">The user log entity with updated values.</param>
        /// <returns>The updated user log entity.</returns>
        public async Task<Userlog> UpdateAsync(Userlog userlog)
        {
            if (userlog == null)
            {
                throw new ArgumentNullException(nameof(userlog));
            }

            _context.Userlogs.Update(userlog);
            await _context.SaveChangesAsync();
            return userlog;
        }

        /// <summary>
        /// Deletes a user log entry from the database by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user log entry to delete.</param>
        /// <returns>True if the user log entry was deleted successfully; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var userlog = await _context.Userlogs.FindAsync(id);
            if (userlog == null)
            {
                return false;
            }

            _context.Userlogs.Remove(userlog);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
