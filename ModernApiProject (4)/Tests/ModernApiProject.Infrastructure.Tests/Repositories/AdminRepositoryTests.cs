using System;
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
    public class AdminRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsAdmin()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Database.EnsureCreated();

                var admin = new Admin
                {
                    Username = "admin1",
                    Password = "password123",
                    CreationDate = DateTime.UtcNow,
                    UpdationDate = DateTime.UtcNow
                };

                context.Admins.Add(admin);
                await context.SaveChangesAsync();

                var repository = new AdminRepository(context);

                // Act
                var result = await repository.GetByIdAsync(admin.Id);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(admin.Id, result.Id);
                Assert.Equal("admin1", result.Username);
                Assert.Equal("password123", result.Password);
            }

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

            using (var context = new ApplicationDbContext(options))
            {
                context.Database.EnsureCreated();

                var repository = new AdminRepository(context);

                // Act
                var result = await repository.GetByIdAsync(999);

                // Assert
                Assert.Null(result);
            }

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleAdmins_ReturnsAllAdmins()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Database.EnsureCreated();

                var admin1 = new Admin
                {
                    Username = "admin1",
                    Password = "password1",
                    CreationDate = DateTime.UtcNow,
                    UpdationDate = DateTime.UtcNow
                };

                var admin2 = new Admin
                {
                    Username = "admin2",
                    Password = "password2",
                    CreationDate = DateTime.UtcNow,
                    UpdationDate = DateTime.UtcNow
                };

                var admin3 = new Admin
                {
                    Username = "admin3",
                    Password = "password3",
                    CreationDate = DateTime.UtcNow,
                    UpdationDate = DateTime.UtcNow
                };

                context.Admins.AddRange(admin1, admin2, admin3);
                await context.SaveChangesAsync();

                var repository = new AdminRepository(context);

                // Act
                var result = await repository.GetAllAsync();

                // Assert
                Assert.NotNull(result);
                var adminList = result.ToList();
                Assert.Equal(3, adminList.Count);
                Assert.Contains(adminList, a => a.Username == "admin1");
                Assert.Contains(adminList, a => a.Username == "admin2");
                Assert.Contains(adminList, a => a.Username == "admin3");
            }

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithNoAdmins_ReturnsEmptyCollection()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Database.EnsureCreated();

                var repository = new AdminRepository(context);

                // Act
                var result = await repository.GetAllAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result);
            }

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithValidAdmin_AddsAdminAndReturnsWithId()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Database.EnsureCreated();

                var repository = new AdminRepository(context);

                var newAdmin = new Admin
                {
                    Username = "newadmin",
                    Password = "newpassword",
                    CreationDate = DateTime.UtcNow,
                    UpdationDate = DateTime.UtcNow
                };

                // Act
                var result = await repository.AddAsync(newAdmin);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Id > 0);
                Assert.Equal("newadmin", result.Username);
                Assert.Equal("newpassword", result.Password);

                var savedAdmin = await context.Admins.FindAsync(result.Id);
                Assert.NotNull(savedAdmin);
                Assert.Equal("newadmin", savedAdmin.Username);
            }

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithNullAdmin_ThrowsArgumentNullException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Database.EnsureCreated();

                var repository = new AdminRepository(context);

                // Act & Assert
                await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null));
            }

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithValidAdmin_UpdatesAdminSuccessfully()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Database.EnsureCreated();

                var admin = new Admin
                {
                    Username = "originaladmin",
                    Password = "originalpassword",
                    CreationDate = DateTime.UtcNow,
                    UpdationDate = DateTime.UtcNow
                };

                context.Admins.Add(admin);
                await context.SaveChangesAsync();

                var repository = new AdminRepository(context);

                // Modify the admin
                admin.Username = "updatedadmin";
                admin.Password = "updatedpassword";
                admin.UpdationDate = DateTime.UtcNow.AddDays(1);

                // Act
                var result = await repository.UpdateAsync(admin);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("updatedadmin", result.Username);
                Assert.Equal("updatedpassword", result.Password);

                var updatedAdmin = await context.Admins.FindAsync(admin.Id);
                Assert.NotNull(updatedAdmin);
                Assert.Equal("updatedadmin", updatedAdmin.Username);
                Assert.Equal("updatedpassword", updatedAdmin.Password);
            }

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithNullAdmin_ThrowsArgumentNullException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Database.EnsureCreated();

                var repository = new AdminRepository(context);

                // Act & Assert
                await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateAsync(null));
            }

            connection.Close();
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_DeletesAdminAndReturnsTrue()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Database.EnsureCreated();

                var admin = new Admin
                {
                    Username = "admintodelete",
                    Password = "password",
                    CreationDate = DateTime.UtcNow,
                    UpdationDate = DateTime.UtcNow
                };

                context.Admins.Add(admin);
                await context.SaveChangesAsync();

                var repository = new AdminRepository(context);

                // Act
                var result = await repository.DeleteAsync(admin.Id);

                // Assert
                Assert.True(result);

                var deletedAdmin = await context.Admins.FindAsync(admin.Id);
                Assert.Null(deletedAdmin);
            }

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

            using (var context = new ApplicationDbContext(options))
            {
                context.Database.EnsureCreated();

                var repository = new AdminRepository(context);

                // Act
                var result = await repository.DeleteAsync(999);

                // Assert
                Assert.False(result);
            }

            connection.Close();
        }

        [Fact]
        public void Constructor_WithNullContext_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AdminRepository(null));
        }

        [Fact]
        public async Task AddAsync_WithMultipleAdmins_AddsAllSuccessfully()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Database.EnsureCreated();

                var repository = new AdminRepository(context);

                var admin1 = new Admin
                {
                    Username = "admin1",
                    Password = "password1",
                    CreationDate = DateTime.UtcNow,
                    UpdationDate = DateTime.UtcNow
                };

                var admin2 = new Admin
                {
                    Username = "admin2",
                    Password = "password2",
                    CreationDate = DateTime.UtcNow,
                    UpdationDate = DateTime.UtcNow
                };

                // Act
                var result1 = await repository.AddAsync(admin1);
                var result2 = await repository.AddAsync(admin2);

                // Assert
                Assert.NotNull(result1);
                Assert.NotNull(result2);
                Assert.True(result1.Id > 0);
                Assert.True(result2.Id > 0);
                Assert.NotEqual(result1.Id, result2.Id);

                var allAdmins = await context.Admins.ToListAsync();
                Assert.Equal(2, allAdmins.Count);
            }

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentAdmin_ThrowsDbUpdateConcurrencyException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Database.EnsureCreated();

                var repository = new AdminRepository(context);

                var nonExistentAdmin = new Admin
                {
                    Id = 999,
                    Username = "nonexistent",
                    Password = "password",
                    CreationDate = DateTime.UtcNow,
                    UpdationDate = DateTime.UtcNow
                };

                // Act & Assert
                await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => repository.UpdateAsync(nonExistentAdmin));
            }

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

            using (var context = new ApplicationDbContext(options))
            {
                context.Database.EnsureCreated();

                var admin = new Admin
                {
                    Username = "admintodelete",
                    Password = "password",
                    CreationDate = DateTime.UtcNow,
                    UpdationDate = DateTime.UtcNow
                };

                context.Admins.Add(admin);
                await context.SaveChangesAsync();

                var repository = new AdminRepository(context);
                var adminId = admin.Id;

                // Act
                await repository.DeleteAsync(adminId);
                var result = await repository.GetByIdAsync(adminId);

                // Assert
                Assert.Null(result);
            }

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

            using (var context = new ApplicationDbContext(options))
            {
                context.Database.EnsureCreated();

                var admin1 = new Admin
                {
                    Username = "admin1",
                    Password = "password1",
                    CreationDate = DateTime.UtcNow,
                    UpdationDate = DateTime.UtcNow
                };

                var admin2 = new Admin
                {
                    Username = "admin2",
                    Password = "password2",
                    CreationDate = DateTime.UtcNow,
                    UpdationDate = DateTime.UtcNow
                };

                var admin3 = new Admin
                {
                    Username = "admin3",
                    Password = "password3",
                    CreationDate = DateTime.UtcNow,
                    UpdationDate = DateTime.UtcNow
                };

                context.Admins.AddRange(admin1, admin2, admin3);
                await context.SaveChangesAsync();

                var repository = new AdminRepository(context);

                // Act
                await repository.DeleteAsync(admin2.Id);
                var result = await repository.GetAllAsync();

                // Assert
                Assert.NotNull(result);
                var adminList = result.ToList();
                Assert.Equal(2, adminList.Count);
                Assert.Contains(adminList, a => a.Username == "admin1");
                Assert.Contains(adminList, a => a.Username == "admin3");
                Assert.DoesNotContain(adminList, a => a.Username == "admin2");
            }

            connection.Close();
        }
    }
}