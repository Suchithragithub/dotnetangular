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
    /// Repository implementation for Session entity operations.
    /// Provides asynchronous CRUD operations using Entity Framework Core.
    /// </summary>
    public class SessionRepository : ISessionRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the SessionRepository class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public SessionRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves a session by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the session.</param>
        /// <returns>The session entity if found; otherwise, null.</returns>
        public async Task<Session?> GetByIdAsync(int id)
        {
            return await _context.Sessions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Retrieves all sessions from the database.
        /// </summary>
        /// <returns>A collection of all session entities.</returns>
        public async Task<IEnumerable<Session>> GetAllAsync()
        {
            return await _context.Sessions
                .AsNoTracking()
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new session to the database.
        /// </summary>
        /// <param name="session">The session entity to add.</param>
        /// <returns>The added session entity with generated identifier.</returns>
        public async Task<Session> AddAsync(Session session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            // Set creation date if not already set
            if (session.CreationDate == default)
            {
                session.CreationDate = DateTime.UtcNow;
            }

            await _context.Sessions.AddAsync(session);
            await _context.SaveChangesAsync();

            return session;
        }

        /// <summary>
        /// Updates an existing session in the database.
        /// </summary>
        /// <param name="session">The session entity with updated values.</param>
        /// <returns>The updated session entity.</returns>
        public async Task<Session> UpdateAsync(Session session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            var existingSession = await _context.Sessions
                .FirstOrDefaultAsync(s => s.Id == session.Id);

            if (existingSession == null)
            {
                throw new InvalidOperationException($"Session with ID {session.Id} not found.");
            }

            // Update properties
            existingSession.SessionName = session.SessionName;

            _context.Sessions.Update(existingSession);
            await _context.SaveChangesAsync();

            return existingSession;
        }

        /// <summary>
        /// Deletes a session from the database by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the session to delete.</param>
        /// <returns>True if the session was deleted successfully; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null)
            {
                return false;
            }

            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
