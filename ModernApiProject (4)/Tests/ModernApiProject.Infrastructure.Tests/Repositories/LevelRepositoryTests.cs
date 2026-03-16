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
    public class LevelRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsLevel()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var level = new Level
            {
                LevelName = "100 Level",
                CreationDate = DateTime.UtcNow
            };

            context.Levels.Add(level);
            await context.SaveChangesAsync();

            var repository = new LevelRepository(context);

            // Act
            var result = await repository.GetByIdAsync(level.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(level.Id, result.Id);
            Assert.Equal("100 Level", result.LevelName);
            Assert.Equal(level.CreationDate, result.CreationDate);

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

            var repository = new LevelRepository(context);

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithNoLevels_ReturnsEmptyCollection()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new LevelRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleLevels_ReturnsAllLevels()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var level1 = new Level
            {
                LevelName = "100 Level",
                CreationDate = DateTime.UtcNow
            };

            var level2 = new Level
            {
                LevelName = "200 Level",
                CreationDate = DateTime.UtcNow.AddDays(-1)
            };

            var level3 = new Level
            {
                LevelName = "300 Level",
                CreationDate = DateTime.UtcNow.AddDays(-2)
            };

            context.Levels.AddRange(level1, level2, level3);
            await context.SaveChangesAsync();

            var repository = new LevelRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var levelsList = result.ToList();
            Assert.Equal(3, levelsList.Count);
            Assert.Contains(levelsList, l => l.LevelName == "100 Level");
            Assert.Contains(levelsList, l => l.LevelName == "200 Level");
            Assert.Contains(levelsList, l => l.LevelName == "300 Level");

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithValidLevel_AddsLevelAndReturnsWithId()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new LevelRepository(context);

            var level = new Level
            {
                LevelName = "400 Level",
                CreationDate = DateTime.UtcNow
            };

            // Act
            var result = await repository.AddAsync(level);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("400 Level", result.LevelName);

            var savedLevel = await context.Levels.FindAsync(result.Id);
            Assert.NotNull(savedLevel);
            Assert.Equal("400 Level", savedLevel.LevelName);

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithNullLevel_ThrowsArgumentNullException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new LevelRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null));

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithValidLevel_UpdatesLevelSuccessfully()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var level = new Level
            {
                LevelName = "100 Level",
                CreationDate = DateTime.UtcNow
            };

            context.Levels.Add(level);
            await context.SaveChangesAsync();

            var repository = new LevelRepository(context);

            var updatedLevel = new Level
            {
                Id = level.Id,
                LevelName = "100 Level Updated",
                CreationDate = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var result = await repository.UpdateAsync(updatedLevel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(level.Id, result.Id);
            Assert.Equal("100 Level Updated", result.LevelName);

            var savedLevel = await context.Levels.FindAsync(level.Id);
            Assert.NotNull(savedLevel);
            Assert.Equal("100 Level Updated", savedLevel.LevelName);

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithNullLevel_ThrowsArgumentNullException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new LevelRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateAsync(null));

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentLevel_ThrowsInvalidOperationException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new LevelRepository(context);

            var nonExistentLevel = new Level
            {
                Id = 999,
                LevelName = "Non-existent Level",
                CreationDate = DateTime.UtcNow
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpdateAsync(nonExistentLevel));
            Assert.Contains("Level with ID 999 not found", exception.Message);

            connection.Close();
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_DeletesLevelAndReturnsTrue()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var level = new Level
            {
                LevelName = "100 Level",
                CreationDate = DateTime.UtcNow
            };

            context.Levels.Add(level);
            await context.SaveChangesAsync();

            var repository = new LevelRepository(context);

            // Act
            var result = await repository.DeleteAsync(level.Id);

            // Assert
            Assert.True(result);

            var deletedLevel = await context.Levels.FindAsync(level.Id);
            Assert.Null(deletedLevel);

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

            var repository = new LevelRepository(context);

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
            Assert.Throws<ArgumentNullException>(() => new LevelRepository(null));
        }

        [Fact]
        public async Task AddAsync_MultipleLevels_AddsAllSuccessfully()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new LevelRepository(context);

            var level1 = new Level
            {
                LevelName = "100 Level",
                CreationDate = DateTime.UtcNow
            };

            var level2 = new Level
            {
                LevelName = "200 Level",
                CreationDate = DateTime.UtcNow
            };

            // Act
            var result1 = await repository.AddAsync(level1);
            var result2 = await repository.AddAsync(level2);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.True(result1.Id > 0);
            Assert.True(result2.Id > 0);
            Assert.NotEqual(result1.Id, result2.Id);

            var allLevels = await repository.GetAllAsync();
            Assert.Equal(2, allLevels.Count());

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_PartialUpdate_UpdatesOnlySpecifiedFields()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var originalDate = DateTime.UtcNow.AddDays(-10);
            var level = new Level
            {
                LevelName = "100 Level",
                CreationDate = originalDate
            };

            context.Levels.Add(level);
            await context.SaveChangesAsync();

            var repository = new LevelRepository(context);

            var updatedLevel = new Level
            {
                Id = level.Id,
                LevelName = "100 Level Modified",
                CreationDate = originalDate
            };

            // Act
            var result = await repository.UpdateAsync(updatedLevel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("100 Level Modified", result.LevelName);
            Assert.Equal(originalDate, result.CreationDate);

            connection.Close();
        }

        [Fact]
        public async Task GetByIdAsync_AfterDelete_ReturnsNull()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var level = new Level
            {
                LevelName = "100 Level",
                CreationDate = DateTime.UtcNow
            };

            context.Levels.Add(level);
            await context.SaveChangesAsync();

            var repository = new LevelRepository(context);

            // Act
            await repository.DeleteAsync(level.Id);
            var result = await repository.GetByIdAsync(level.Id);

            // Assert
            Assert.Null(result);

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_AfterAddingAndDeleting_ReturnsCorrectCount()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var level1 = new Level { LevelName = "100 Level", CreationDate = DateTime.UtcNow };
            var level2 = new Level { LevelName = "200 Level", CreationDate = DateTime.UtcNow };
            var level3 = new Level { LevelName = "300 Level", CreationDate = DateTime.UtcNow };

            context.Levels.AddRange(level1, level2, level3);
            await context.SaveChangesAsync();

            var repository = new LevelRepository(context);

            // Act
            await repository.DeleteAsync(level2.Id);
            var result = await repository.GetAllAsync();

            // Assert
            var levelsList = result.ToList();
            Assert.Equal(2, levelsList.Count);
            Assert.Contains(levelsList, l => l.LevelName == "100 Level");
            Assert.Contains(levelsList, l => l.LevelName == "300 Level");
            Assert.DoesNotContain(levelsList, l => l.LevelName == "200 Level");

            connection.Close();
        }
    }
}