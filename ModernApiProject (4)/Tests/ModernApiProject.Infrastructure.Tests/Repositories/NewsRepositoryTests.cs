using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ModernApiProject.Domain.Entities;
using ModernApiProject.Infrastructure.Data;
using ModernApiProject.Infrastructure.Repositories;
using Xunit;


namespace ModernApiProject.Infrastructure.Tests.Repositories
{
    public class NewsRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsNewsItem()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var news = new News
            {
                Newstitle = "Test News Title",
                NewsDescription = "Test News Description",
                PostingDate = DateTime.UtcNow
            };

            context.News.Add(news);
            await context.SaveChangesAsync();

            var repository = new NewsRepository(context);

            // Act
            var result = await repository.GetByIdAsync(news.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(news.Id, result.Id);
            Assert.Equal("Test News Title", result.Newstitle);
            Assert.Equal("Test News Description", result.NewsDescription);

            connection.Close();
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new NewsRepository(context);

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithNoNews_ReturnsEmptyCollection()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new NewsRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleNews_ReturnsAllOrderedByPostingDateDescending()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var news1 = new News
            {
                Newstitle = "First News",
                NewsDescription = "First Description",
                PostingDate = DateTime.UtcNow.AddDays(-2)
            };

            var news2 = new News
            {
                Newstitle = "Second News",
                NewsDescription = "Second Description",
                PostingDate = DateTime.UtcNow.AddDays(-1)
            };

            var news3 = new News
            {
                Newstitle = "Third News",
                NewsDescription = "Third Description",
                PostingDate = DateTime.UtcNow
            };

            context.News.AddRange(news1, news2, news3);
            await context.SaveChangesAsync();

            var repository = new NewsRepository(context);

            // Act
            var result = await repository.GetAllAsync();
            var resultList = result.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.Equal("Third News", resultList[0].Newstitle);
            Assert.Equal("Second News", resultList[1].Newstitle);
            Assert.Equal("First News", resultList[2].Newstitle);

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithValidNews_AddsNewsAndReturnsWithId()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new NewsRepository(context);

            var news = new News
            {
                Newstitle = "New News Title",
                NewsDescription = "New News Description",
                PostingDate = DateTime.UtcNow
            };

            // Act
            var result = await repository.AddAsync(news);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("New News Title", result.Newstitle);
            Assert.Equal("New News Description", result.NewsDescription);

            var savedNews = await context.News.FindAsync(result.Id);
            Assert.NotNull(savedNews);
            Assert.Equal("New News Title", savedNews.Newstitle);

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithNullNews_ThrowsArgumentNullException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new NewsRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null));

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithValidNews_UpdatesNewsAndReturns()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var news = new News
            {
                Newstitle = "Original Title",
                NewsDescription = "Original Description",
                PostingDate = DateTime.UtcNow
            };

            context.News.Add(news);
            await context.SaveChangesAsync();

            var repository = new NewsRepository(context);

            var updatedNews = new News
            {
                Id = news.Id,
                Newstitle = "Updated Title",
                NewsDescription = "Updated Description",
                PostingDate = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var result = await repository.UpdateAsync(updatedNews);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(news.Id, result.Id);
            Assert.Equal("Updated Title", result.Newstitle);
            Assert.Equal("Updated Description", result.NewsDescription);

            var savedNews = await context.News.FindAsync(news.Id);
            Assert.NotNull(savedNews);
            Assert.Equal("Updated Title", savedNews.Newstitle);
            Assert.Equal("Updated Description", savedNews.NewsDescription);

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithNullNews_ThrowsArgumentNullException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new NewsRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateAsync(null));

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentNews_ThrowsInvalidOperationException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new NewsRepository(context);

            var nonExistentNews = new News
            {
                Id = 999,
                Newstitle = "Non-existent Title",
                NewsDescription = "Non-existent Description",
                PostingDate = DateTime.UtcNow
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpdateAsync(nonExistentNews));
            Assert.Contains("News item with ID 999 not found", exception.Message);

            connection.Close();
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_DeletesNewsAndReturnsTrue()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var news = new News
            {
                Newstitle = "News to Delete",
                NewsDescription = "Description to Delete",
                PostingDate = DateTime.UtcNow
            };

            context.News.Add(news);
            await context.SaveChangesAsync();

            var repository = new NewsRepository(context);

            // Act
            var result = await repository.DeleteAsync(news.Id);

            // Assert
            Assert.True(result);

            var deletedNews = await context.News.FindAsync(news.Id);
            Assert.Null(deletedNews);

            connection.Close();
        }

        [Fact]
        public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new NewsRepository(context);

            // Act
            var result = await repository.DeleteAsync(999);

            // Assert
            Assert.False(result);

            connection.Close();
        }

        [Fact]
        public void Constructor_WithNullContext_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new NewsRepository(null));
        }
    }
}
