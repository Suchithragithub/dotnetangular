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
    public class SessionRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsSession()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var session = new Session
            {
                SessionName = "2023/2024",
                CreationDate = DateTime.UtcNow
            };

            context.Sessions.Add(session);
            await context.SaveChangesAsync();

            var repository = new SessionRepository(context);

            // Act
            var result = await repository.GetByIdAsync(session.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(session.Id, result.Id);
            Assert.Equal(session.SessionName, result.SessionName);
            Assert.Equal(session.CreationDate, result.CreationDate, TimeSpan.FromSeconds(1));

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

            var repository = new SessionRepository(context);

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithNoSessions_ReturnsEmptyCollection()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new SessionRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleSessions_ReturnsAllSessionsOrderedById()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var session1 = new Session
            {
                SessionName = "2021/2022",
                CreationDate = DateTime.UtcNow.AddDays(-2)
            };

            var session2 = new Session
            {
                SessionName = "2022/2023",
                CreationDate = DateTime.UtcNow.AddDays(-1)
            };

            var session3 = new Session
            {
                SessionName = "2023/2024",
                CreationDate = DateTime.UtcNow
            };

            context.Sessions.AddRange(session1, session2, session3);
            await context.SaveChangesAsync();

            var repository = new SessionRepository(context);

            // Act
            var result = await repository.GetAllAsync();
            var resultList = result.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);
            Assert.Equal(session1.Id, resultList[0].Id);
            Assert.Equal(session2.Id, resultList[1].Id);
            Assert.Equal(session3.Id, resultList[2].Id);
            Assert.True(resultList[0].Id < resultList[1].Id);
            Assert.True(resultList[1].Id < resultList[2].Id);

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithValidSession_AddsSessionAndReturnsWithId()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new SessionRepository(context);

            var session = new Session
            {
                SessionName = "2023/2024",
                CreationDate = DateTime.UtcNow
            };

            // Act
            var result = await repository.AddAsync(session);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal(session.SessionName, result.SessionName);
            Assert.Equal(session.CreationDate, result.CreationDate, TimeSpan.FromSeconds(1));

            var savedSession = await context.Sessions.FindAsync(result.Id);
            Assert.NotNull(savedSession);
            Assert.Equal(session.SessionName, savedSession.SessionName);

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithDefaultCreationDate_SetsCreationDateToUtcNow()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new SessionRepository(context);

            var session = new Session
            {
                SessionName = "2023/2024"
                // CreationDate not set (default value)
            };

            var beforeAdd = DateTime.UtcNow;

            // Act
            var result = await repository.AddAsync(session);
            var afterAdd = DateTime.UtcNow;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.CreationDate >= beforeAdd);
            Assert.True(result.CreationDate <= afterAdd);

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithNullSession_ThrowsArgumentNullException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new SessionRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null));

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithValidSession_UpdatesSessionAndReturns()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var session = new Session
            {
                SessionName = "2023/2024",
                CreationDate = DateTime.UtcNow
            };

            context.Sessions.Add(session);
            await context.SaveChangesAsync();

            var repository = new SessionRepository(context);

            var updatedSession = new Session
            {
                Id = session.Id,
                SessionName = "2024/2025",
                CreationDate = session.CreationDate
            };

            // Act
            var result = await repository.UpdateAsync(updatedSession);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(session.Id, result.Id);
            Assert.Equal("2024/2025", result.SessionName);

            var savedSession = await context.Sessions.FindAsync(session.Id);
            Assert.NotNull(savedSession);
            Assert.Equal("2024/2025", savedSession.SessionName);

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithNullSession_ThrowsArgumentNullException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new SessionRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateAsync(null));

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentSession_ThrowsInvalidOperationException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new SessionRepository(context);

            var nonExistentSession = new Session
            {
                Id = 999,
                SessionName = "2024/2025",
                CreationDate = DateTime.UtcNow
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => repository.UpdateAsync(nonExistentSession));
            Assert.Contains("Session with ID 999 not found", exception.Message);

            connection.Close();
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_DeletesSessionAndReturnsTrue()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var session = new Session
            {
                SessionName = "2023/2024",
                CreationDate = DateTime.UtcNow
            };

            context.Sessions.Add(session);
            await context.SaveChangesAsync();

            var repository = new SessionRepository(context);

            // Act
            var result = await repository.DeleteAsync(session.Id);

            // Assert
            Assert.True(result);

            var deletedSession = await context.Sessions.FindAsync(session.Id);
            Assert.Null(deletedSession);

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

            var repository = new SessionRepository(context);

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
            Assert.Throws<ArgumentNullException>(() => new SessionRepository(null));
        }

        [Fact]
        public async Task GetByIdAsync_UsesAsNoTracking_DoesNotTrackEntity()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var session = new Session
            {
                SessionName = "2023/2024",
                CreationDate = DateTime.UtcNow
            };

            context.Sessions.Add(session);
            await context.SaveChangesAsync();

            var repository = new SessionRepository(context);

            // Act
            var result = await repository.GetByIdAsync(session.Id);

            // Assert
            Assert.NotNull(result);
            var trackedEntities = context.ChangeTracker.Entries<Session>().ToList();
            Assert.DoesNotContain(trackedEntities, e => e.Entity.Id == result.Id && e.State != EntityState.Detached);

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_UsesAsNoTracking_DoesNotTrackEntities()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var session1 = new Session
            {
                SessionName = "2021/2022",
                CreationDate = DateTime.UtcNow
            };

            var session2 = new Session
            {
                SessionName = "2022/2023",
                CreationDate = DateTime.UtcNow
            };

            context.Sessions.AddRange(session1, session2);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();

            var repository = new SessionRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            var trackedEntities = context.ChangeTracker.Entries<Session>()
                .Where(e => e.State != EntityState.Detached)
                .ToList();
            Assert.Empty(trackedEntities);

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithExplicitCreationDate_PreservesProvidedDate()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new SessionRepository(context);

            var explicitDate = new DateTime(2023, 1, 15, 10, 30, 0, DateTimeKind.Utc);
            var session = new Session
            {
                SessionName = "2023/2024",
                CreationDate = explicitDate
            };

            // Act
            var result = await repository.AddAsync(session);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(explicitDate, result.CreationDate);

            connection.Close();
        }
    }
}
