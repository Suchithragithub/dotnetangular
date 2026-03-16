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
    /// Repository implementation for News entity operations.
    /// Provides asynchronous CRUD operations using Entity Framework Core.
    /// </summary>
    public class NewsRepository : INewsRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the NewsRepository class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public NewsRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves a news item by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the news item.</param>
        /// <returns>The news item if found; otherwise, null.</returns>
        public async Task<News?> GetByIdAsync(int id)
        {
            return await _context.News.FindAsync(id);
        }

        /// <summary>
        /// Retrieves all news items from the repository.
        /// </summary>
        /// <returns>A collection of all news items.</returns>
        public async Task<IEnumerable<News>> GetAllAsync()
        {
            return await _context.News
                .OrderByDescending(n => n.PostingDate)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new news item to the repository.
        /// </summary>
        /// <param name="news">The news item to add.</param>
        /// <returns>The added news item with generated identifier.</returns>
        public async Task<News> AddAsync(News news)
        {
            if (news == null)
            {
                throw new ArgumentNullException(nameof(news));
            }

            await _context.News.AddAsync(news);
            await _context.SaveChangesAsync();
            return news;
        }

        /// <summary>
        /// Updates an existing news item in the repository.
        /// </summary>
        /// <param name="news">The news item with updated information.</param>
        /// <returns>The updated news item.</returns>
        public async Task<News> UpdateAsync(News news)
        {
            if (news == null)
            {
                throw new ArgumentNullException(nameof(news));
            }

            var existingNews = await _context.News.FindAsync(news.Id);
            if (existingNews == null)
            {
                throw new InvalidOperationException($"News item with ID {news.Id} not found.");
            }

            _context.Entry(existingNews).CurrentValues.SetValues(news);
            await _context.SaveChangesAsync();
            return existingNews;
        }

        /// <summary>
        /// Deletes a news item from the repository by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the news item to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return false;
            }

            _context.News.Remove(news);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
