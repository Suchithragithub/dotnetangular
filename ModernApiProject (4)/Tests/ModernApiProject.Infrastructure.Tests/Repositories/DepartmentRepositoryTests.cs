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
    public class DepartmentRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsDepartment()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var department = new Department
            {
                DepartmentName = "Computer Science",
                CreationDate = DateTime.UtcNow
            };

            context.Departments.Add(department);
            await context.SaveChangesAsync();

            var repository = new DepartmentRepository(context);

            // Act
            var result = await repository.GetByIdAsync(department.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(department.Id, result.Id);
            Assert.Equal("Computer Science", result.DepartmentName);
            Assert.Equal(department.CreationDate, result.CreationDate);

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

            var repository = new DepartmentRepository(context);

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithNoDepartments_ReturnsEmptyCollection()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new DepartmentRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleDepartments_ReturnsAllDepartmentsOrderedByName()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var departments = new List<Department>
            {
                new Department { DepartmentName = "Mathematics", CreationDate = DateTime.UtcNow },
                new Department { DepartmentName = "Computer Science", CreationDate = DateTime.UtcNow },
                new Department { DepartmentName = "Physics", CreationDate = DateTime.UtcNow },
                new Department { DepartmentName = "Biology", CreationDate = DateTime.UtcNow }
            };

            context.Departments.AddRange(departments);
            await context.SaveChangesAsync();

            var repository = new DepartmentRepository(context);

            // Act
            var result = await repository.GetAllAsync();
            var resultList = result.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, resultList.Count);
            Assert.Equal("Biology", resultList[0].DepartmentName);
            Assert.Equal("Computer Science", resultList[1].DepartmentName);
            Assert.Equal("Mathematics", resultList[2].DepartmentName);
            Assert.Equal("Physics", resultList[3].DepartmentName);

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithValidDepartment_AddsDepartmentAndReturnsWithId()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new DepartmentRepository(context);
            var department = new Department
            {
                DepartmentName = "Engineering",
                CreationDate = DateTime.UtcNow
            };

            // Act
            var result = await repository.AddAsync(department);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Engineering", result.DepartmentName);

            var savedDepartment = await context.Departments.FindAsync(result.Id);
            Assert.NotNull(savedDepartment);
            Assert.Equal("Engineering", savedDepartment.DepartmentName);

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithDepartmentWithoutCreationDate_SetsCreationDateToUtcNow()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new DepartmentRepository(context);
            var beforeAdd = DateTime.UtcNow;
            var department = new Department
            {
                DepartmentName = "Chemistry"
            };

            // Act
            var result = await repository.AddAsync(department);
            var afterAdd = DateTime.UtcNow;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.CreationDate >= beforeAdd);
            Assert.True(result.CreationDate <= afterAdd);

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithNullDepartment_ThrowsArgumentNullException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new DepartmentRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null));

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithValidDepartment_UpdatesDepartmentName()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var originalCreationDate = DateTime.UtcNow.AddDays(-10);
            var department = new Department
            {
                DepartmentName = "Old Name",
                CreationDate = originalCreationDate
            };

            context.Departments.Add(department);
            await context.SaveChangesAsync();

            var repository = new DepartmentRepository(context);
            department.DepartmentName = "Updated Name";

            // Act
            var result = await repository.UpdateAsync(department);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.DepartmentName);
            Assert.Equal(originalCreationDate, result.CreationDate);

            var updatedDepartment = await context.Departments.FindAsync(department.Id);
            Assert.NotNull(updatedDepartment);
            Assert.Equal("Updated Name", updatedDepartment.DepartmentName);
            Assert.Equal(originalCreationDate, updatedDepartment.CreationDate);

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentDepartment_ThrowsInvalidOperationException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new DepartmentRepository(context);
            var department = new Department
            {
                Id = 999,
                DepartmentName = "Non-existent",
                CreationDate = DateTime.UtcNow
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpdateAsync(department));
            Assert.Contains("Department with ID 999 not found", exception.Message);

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithNullDepartment_ThrowsArgumentNullException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new DepartmentRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateAsync(null));

            connection.Close();
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_DeletesDepartmentAndReturnsTrue()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var department = new Department
            {
                DepartmentName = "To Be Deleted",
                CreationDate = DateTime.UtcNow
            };

            context.Departments.Add(department);
            await context.SaveChangesAsync();

            var repository = new DepartmentRepository(context);

            // Act
            var result = await repository.DeleteAsync(department.Id);

            // Assert
            Assert.True(result);

            var deletedDepartment = await context.Departments.FindAsync(department.Id);
            Assert.Null(deletedDepartment);

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

            var repository = new DepartmentRepository(context);

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
            Assert.Throws<ArgumentNullException>(() => new DepartmentRepository(null));
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

            var department = new Department
            {
                DepartmentName = "Test Department",
                CreationDate = DateTime.UtcNow
            };

            context.Departments.Add(department);
            await context.SaveChangesAsync();

            var repository = new DepartmentRepository(context);

            // Act
            var result = await repository.GetByIdAsync(department.Id);

            // Assert
            Assert.NotNull(result);
            var trackedEntity = context.ChangeTracker.Entries<Department>()
                .FirstOrDefault(e => e.Entity.Id == department.Id);
            Assert.Null(trackedEntity);

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

            var departments = new List<Department>
            {
                new Department { DepartmentName = "Department 1", CreationDate = DateTime.UtcNow },
                new Department { DepartmentName = "Department 2", CreationDate = DateTime.UtcNow }
            };

            context.Departments.AddRange(departments);
            await context.SaveChangesAsync();

            var repository = new DepartmentRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            var trackedEntities = context.ChangeTracker.Entries<Department>().ToList();
            Assert.Empty(trackedEntities);

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithExistingCreationDate_PreservesOriginalCreationDate()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new DepartmentRepository(context);
            var specificDate = new DateTime(2023, 1, 15, 10, 30, 0, DateTimeKind.Utc);
            var department = new Department
            {
                DepartmentName = "History",
                CreationDate = specificDate
            };

            // Act
            var result = await repository.AddAsync(department);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(specificDate, result.CreationDate);

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_DoesNotModifyCreationDate()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var originalDate = new DateTime(2022, 5, 10, 8, 0, 0, DateTimeKind.Utc);
            var department = new Department
            {
                DepartmentName = "Original",
                CreationDate = originalDate
            };

            context.Departments.Add(department);
            await context.SaveChangesAsync();

            var repository = new DepartmentRepository(context);
            department.DepartmentName = "Modified";
            department.CreationDate = DateTime.UtcNow;

            // Act
            var result = await repository.UpdateAsync(department);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(originalDate, result.CreationDate);

            connection.Close();
        }
    }
}