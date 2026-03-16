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
    /// Repository implementation for Level entity operations.
    /// Provides asynchronous CRUD operations using Entity Framework Core.
    /// </summary>
    public class LevelRepository : ILevelRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the LevelRepository class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public LevelRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves a level by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the level.</param>
        /// <returns>The level entity if found; otherwise, null.</returns>
        public async Task<Level?> GetByIdAsync(int id)
        {
            return await _context.Levels.FindAsync(id);
        }

        /// <summary>
        /// Retrieves all levels from the repository.
        /// </summary>
        /// <returns>A collection of all level entities.</returns>
        public async Task<IEnumerable<Level>> GetAllAsync()
        {
            return await _context.Levels.ToListAsync();
        }

        /// <summary>
        /// Adds a new level to the repository.
        /// </summary>
        /// <param name="level">The level entity to add.</param>
        /// <returns>The added level entity with generated identifier.</returns>
        public async Task<Level> AddAsync(Level level)
        {
            if (level == null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            await _context.Levels.AddAsync(level);
            await _context.SaveChangesAsync();
            return level;
        }

        /// <summary>
        /// Updates an existing level in the repository.
        /// </summary>
        /// <param name="level">The level entity with updated values.</param>
        /// <returns>The updated level entity.</returns>
        public async Task<Level> UpdateAsync(Level level)
        {
            if (level == null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            var existingLevel = await _context.Levels.FindAsync(level.Id);
            if (existingLevel == null)
            {
                throw new InvalidOperationException($"Level with ID {level.Id} not found.");
            }

            _context.Entry(existingLevel).CurrentValues.SetValues(level);
            await _context.SaveChangesAsync();
            return existingLevel;
        }

        /// <summary>
        /// Deletes a level from the repository by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the level to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var level = await _context.Levels.FindAsync(id);
            if (level == null)
            {
                return false;
            }

            _context.Levels.Remove(level);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
