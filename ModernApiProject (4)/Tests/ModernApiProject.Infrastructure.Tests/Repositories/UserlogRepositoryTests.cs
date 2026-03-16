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
    public class UserlogRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsUserlog()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var userlog = new Userlog
            {
                StudentRegno = "REG001",
                Userip = new byte[] { 192, 168, 1, 1 },
                LoginTime = DateTime.UtcNow,
                Logout = "N",
                Status = 1
            };

            await context.Userlogs.AddAsync(userlog);
            await context.SaveChangesAsync();

            var repository = new UserlogRepository(context);

            // Act
            var result = await repository.GetByIdAsync(userlog.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userlog.Id, result.Id);
            Assert.Equal("REG001", result.StudentRegno);
            Assert.Equal(new byte[] { 192, 168, 1, 1 }, result.Userip);
            Assert.Equal("N", result.Logout);
            Assert.Equal(1, result.Status);

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

            var repository = new UserlogRepository(context);

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithNoUserlogs_ReturnsEmptyCollection()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new UserlogRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleUserlogs_ReturnsAllUserlogs()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var userlog1 = new Userlog
            {
                StudentRegno = "REG001",
                Userip = new byte[] { 192, 168, 1, 1 },
                LoginTime = DateTime.UtcNow,
                Logout = "N",
                Status = 1
            };

            var userlog2 = new Userlog
            {
                StudentRegno = "REG002",
                Userip = new byte[] { 192, 168, 1, 2 },
                LoginTime = DateTime.UtcNow.AddHours(-1),
                Logout = "Y",
                Status = 0
            };

            var userlog3 = new Userlog
            {
                StudentRegno = "REG003",
                Userip = new byte[] { 192, 168, 1, 3 },
                LoginTime = DateTime.UtcNow.AddHours(-2),
                Logout = "N",
                Status = 1
            };

            await context.Userlogs.AddRangeAsync(userlog1, userlog2, userlog3);
            await context.SaveChangesAsync();

            var repository = new UserlogRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var userlogList = result.ToList();
            Assert.Equal(3, userlogList.Count);
            Assert.Contains(userlogList, u => u.StudentRegno == "REG001");
            Assert.Contains(userlogList, u => u.StudentRegno == "REG002");
            Assert.Contains(userlogList, u => u.StudentRegno == "REG003");

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithValidUserlog_AddsUserlogToDatabase()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new UserlogRepository(context);

            var userlog = new Userlog
            {
                StudentRegno = "REG001",
                Userip = new byte[] { 192, 168, 1, 1 },
                LoginTime = DateTime.UtcNow,
                Logout = "N",
                Status = 1
            };

            // Act
            var result = await repository.AddAsync(userlog);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("REG001", result.StudentRegno);

            var savedUserlog = await context.Userlogs.FindAsync(result.Id);
            Assert.NotNull(savedUserlog);
            Assert.Equal("REG001", savedUserlog.StudentRegno);
            Assert.Equal(new byte[] { 192, 168, 1, 1 }, savedUserlog.Userip);
            Assert.Equal("N", savedUserlog.Logout);
            Assert.Equal(1, savedUserlog.Status);

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithNullUserlog_ThrowsArgumentNullException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new UserlogRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null));

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithValidUserlog_UpdatesUserlogInDatabase()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var userlog = new Userlog
            {
                StudentRegno = "REG001",
                Userip = new byte[] { 192, 168, 1, 1 },
                LoginTime = DateTime.UtcNow,
                Logout = "N",
                Status = 1
            };

            await context.Userlogs.AddAsync(userlog);
            await context.SaveChangesAsync();

            var repository = new UserlogRepository(context);

            // Act
            userlog.StudentRegno = "REG001_UPDATED";
            userlog.Logout = "Y";
            userlog.Status = 0;
            userlog.Userip = new byte[] { 192, 168, 1, 100 };

            var result = await repository.UpdateAsync(userlog);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("REG001_UPDATED", result.StudentRegno);
            Assert.Equal("Y", result.Logout);
            Assert.Equal(0, result.Status);

            var updatedUserlog = await context.Userlogs.FindAsync(userlog.Id);
            Assert.NotNull(updatedUserlog);
            Assert.Equal("REG001_UPDATED", updatedUserlog.StudentRegno);
            Assert.Equal("Y", updatedUserlog.Logout);
            Assert.Equal(0, updatedUserlog.Status);
            Assert.Equal(new byte[] { 192, 168, 1, 100 }, updatedUserlog.Userip);

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithNullUserlog_ThrowsArgumentNullException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new UserlogRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateAsync(null));

            connection.Close();
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_DeletesUserlogAndReturnsTrue()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var userlog = new Userlog
            {
                StudentRegno = "REG001",
                Userip = new byte[] { 192, 168, 1, 1 },
                LoginTime = DateTime.UtcNow,
                Logout = "N",
                Status = 1
            };

            await context.Userlogs.AddAsync(userlog);
            await context.SaveChangesAsync();

            var repository = new UserlogRepository(context);

            // Act
            var result = await repository.DeleteAsync(userlog.Id);

            // Assert
            Assert.True(result);

            var deletedUserlog = await context.Userlogs.FindAsync(userlog.Id);
            Assert.Null(deletedUserlog);

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

            var repository = new UserlogRepository(context);

            // Act
            var result = await repository.DeleteAsync(999);

            // Assert
            Assert.False(result);

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithMultipleUserlogs_AddsAllSuccessfully()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new UserlogRepository(context);

            var userlog1 = new Userlog
            {
                StudentRegno = "REG001",
                Userip = new byte[] { 192, 168, 1, 1 },
                LoginTime = DateTime.UtcNow,
                Logout = "N",
                Status = 1
            };

            var userlog2 = new Userlog
            {
                StudentRegno = "REG002",
                Userip = new byte[] { 192, 168, 1, 2 },
                LoginTime = DateTime.UtcNow.AddHours(-1),
                Logout = "Y",
                Status = 0
            };

            // Act
            var result1 = await repository.AddAsync(userlog1);
            var result2 = await repository.AddAsync(userlog2);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.True(result1.Id > 0);
            Assert.True(result2.Id > 0);
            Assert.NotEqual(result1.Id, result2.Id);

            var allUserlogs = await context.Userlogs.ToListAsync();
            Assert.Equal(2, allUserlogs.Count);

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

            var userlog = new Userlog
            {
                StudentRegno = "REG001",
                Userip = new byte[] { 192, 168, 1, 1 },
                LoginTime = DateTime.UtcNow,
                Logout = "N",
                Status = 1
            };

            await context.Userlogs.AddAsync(userlog);
            await context.SaveChangesAsync();

            var repository = new UserlogRepository(context);

            // Act
            await repository.DeleteAsync(userlog.Id);
            var result = await repository.GetByIdAsync(userlog.Id);

            // Assert
            Assert.Null(result);

            connection.Close();
        }

        [Fact]
        public async Task Constructor_WithNullContext_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UserlogRepository(null));
        }

        [Fact]
        public async Task AddAsync_WithByteArrayUserip_StoresCorrectly()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new UserlogRepository(context);

            var ipAddress = new byte[] { 10, 0, 0, 1 };
            var userlog = new Userlog
            {
                StudentRegno = "REG001",
                Userip = ipAddress,
                LoginTime = DateTime.UtcNow,
                Logout = "N",
                Status = 1
            };

            // Act
            var result = await repository.AddAsync(userlog);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ipAddress, result.Userip);

            var savedUserlog = await context.Userlogs.FindAsync(result.Id);
            Assert.NotNull(savedUserlog);
            Assert.Equal(ipAddress, savedUserlog.Userip);

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithChangedLoginTime_UpdatesCorrectly()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var originalLoginTime = DateTime.UtcNow.AddHours(-5);
            var userlog = new Userlog
            {
                StudentRegno = "REG001",
                Userip = new byte[] { 192, 168, 1, 1 },
                LoginTime = originalLoginTime,
                Logout = "N",
                Status = 1
            };

            await context.Userlogs.AddAsync(userlog);
            await context.SaveChangesAsync();

            var repository = new UserlogRepository(context);

            // Act
            var newLoginTime = DateTime.UtcNow;
            userlog.LoginTime = newLoginTime;
            var result = await repository.UpdateAsync(userlog);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newLoginTime, result.LoginTime);

            var updatedUserlog = await context.Userlogs.FindAsync(userlog.Id);
            Assert.NotNull(updatedUserlog);
            Assert.Equal(newLoginTime, updatedUserlog.LoginTime);

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

            var userlog1 = new Userlog
            {
                StudentRegno = "REG001",
                Userip = new byte[] { 192, 168, 1, 1 },
                LoginTime = DateTime.UtcNow,
                Logout = "N",
                Status = 1
            };

            var userlog2 = new Userlog
            {
                StudentRegno = "REG002",
                Userip = new byte[] { 192, 168, 1, 2 },
                LoginTime = DateTime.UtcNow,
                Logout = "N",
                Status = 1
            };

            await context.Userlogs.AddRangeAsync(userlog1, userlog2);
            await context.SaveChangesAsync();

            var repository = new UserlogRepository(context);

            // Act
            await repository.DeleteAsync(userlog1.Id);
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var userlogList = result.ToList();
            Assert.Single(userlogList);
            Assert.Equal("REG002", userlogList[0].StudentRegno);

            connection.Close();
        }
    }
}
