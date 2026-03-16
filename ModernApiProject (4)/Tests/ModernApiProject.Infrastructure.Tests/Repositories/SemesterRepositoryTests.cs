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
    public class SemesterRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsSemester()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();

                    var semester = new Semester
                    {
                        SemesterName = "Fall 2024",
                        CreationDate = DateTime.UtcNow,
                        UpdationDate = null
                    };

                    context.Semesters.Add(semester);
                    await context.SaveChangesAsync();

                    var repository = new SemesterRepository(context);

                    // Act
                    var result = await repository.GetByIdAsync(semester.Id);

                    // Assert
                    Assert.NotNull(result);
                    Assert.Equal(semester.Id, result.Id);
                    Assert.Equal("Fall 2024", result.SemesterName);
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();

                    var repository = new SemesterRepository(context);

                    // Act
                    var result = await repository.GetByIdAsync(999);

                    // Assert
                    Assert.Null(result);
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        [Fact]
        public async Task GetAllAsync_WithNoSemesters_ReturnsEmptyCollection()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();

                    var repository = new SemesterRepository(context);

                    // Act
                    var result = await repository.GetAllAsync();

                    // Assert
                    Assert.NotNull(result);
                    Assert.Empty(result);
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleSemesters_ReturnsAllSemestersOrderedById()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();

                    var semester1 = new Semester
                    {
                        SemesterName = "Fall 2024",
                        CreationDate = DateTime.UtcNow,
                        UpdationDate = null
                    };

                    var semester2 = new Semester
                    {
                        SemesterName = "Spring 2025",
                        CreationDate = DateTime.UtcNow,
                        UpdationDate = null
                    };

                    var semester3 = new Semester
                    {
                        SemesterName = "Summer 2025",
                        CreationDate = DateTime.UtcNow,
                        UpdationDate = null
                    };

                    context.Semesters.AddRange(semester1, semester2, semester3);
                    await context.SaveChangesAsync();

                    var repository = new SemesterRepository(context);

                    // Act
                    var result = await repository.GetAllAsync();

                    // Assert
                    Assert.NotNull(result);
                    var semesterList = result.ToList();
                    Assert.Equal(3, semesterList.Count);
                    Assert.Equal("Fall 2024", semesterList[0].SemesterName);
                    Assert.Equal("Spring 2025", semesterList[1].SemesterName);
                    Assert.Equal("Summer 2025", semesterList[2].SemesterName);
                    // Verify ordering by Id
                    Assert.True(semesterList[0].Id < semesterList[1].Id);
                    Assert.True(semesterList[1].Id < semesterList[2].Id);
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        [Fact]
        public async Task AddAsync_WithValidSemester_AddsSemesterAndReturnsWithGeneratedId()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();

                    var repository = new SemesterRepository(context);

                    var semester = new Semester
                    {
                        SemesterName = "Fall 2024",
                        CreationDate = DateTime.UtcNow,
                        UpdationDate = null
                    };

                    // Act
                    var result = await repository.AddAsync(semester);

                    // Assert
                    Assert.NotNull(result);
                    Assert.True(result.Id > 0);
                    Assert.Equal("Fall 2024", result.SemesterName);

                    // Verify it was actually saved
                    var savedSemester = await context.Semesters.FindAsync(result.Id);
                    Assert.NotNull(savedSemester);
                    Assert.Equal("Fall 2024", savedSemester.SemesterName);
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        [Fact]
        public async Task AddAsync_WithDefaultCreationDate_SetsCreationDateToUtcNow()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();

                    var repository = new SemesterRepository(context);

                    var beforeAdd = DateTime.UtcNow;

                    var semester = new Semester
                    {
                        SemesterName = "Fall 2024",
                        UpdationDate = null
                    };

                    // Act
                    var result = await repository.AddAsync(semester);
                    var afterAdd = DateTime.UtcNow;

                    // Assert
                    Assert.NotNull(result);
                    Assert.True(result.CreationDate >= beforeAdd);
                    Assert.True(result.CreationDate <= afterAdd);
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        [Fact]
        public async Task AddAsync_WithNullSemester_ThrowsArgumentNullException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();

                    var repository = new SemesterRepository(context);

                    // Act & Assert
                    await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null));
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        [Fact]
        public async Task UpdateAsync_WithValidSemester_UpdatesSemesterAndReturnsUpdatedEntity()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();

                    var semester = new Semester
                    {
                        SemesterName = "Fall 2024",
                        CreationDate = DateTime.UtcNow,
                        UpdationDate = null
                    };

                    context.Semesters.Add(semester);
                    await context.SaveChangesAsync();

                    var repository = new SemesterRepository(context);

                    // Detach to simulate a new context scenario
                    context.Entry(semester).State = EntityState.Detached;

                    var updatedSemester = new Semester
                    {
                        Id = semester.Id,
                        SemesterName = "Spring 2025",
                        CreationDate = semester.CreationDate,
                        UpdationDate = null
                    };

                    var beforeUpdate = DateTime.UtcNow;

                    // Act
                    var result = await repository.UpdateAsync(updatedSemester);
                    var afterUpdate = DateTime.UtcNow;

                    // Assert
                    Assert.NotNull(result);
                    Assert.Equal(semester.Id, result.Id);
                    Assert.Equal("Spring 2025", result.SemesterName);
                    Assert.NotNull(result.UpdationDate);

                    // Verify the update was persisted
                    var verifyContext = new ApplicationDbContext(options);
                    var savedSemester = await verifyContext.Semesters.FindAsync(semester.Id);
                    Assert.NotNull(savedSemester);
                    Assert.Equal("Spring 2025", savedSemester.SemesterName);
                    Assert.NotNull(savedSemester.UpdationDate);
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        [Fact]
        public async Task UpdateAsync_WithNullSemester_ThrowsArgumentNullException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();

                    var repository = new SemesterRepository(context);

                    // Act & Assert
                    await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateAsync(null));
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentSemester_ThrowsInvalidOperationException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();

                    var repository = new SemesterRepository(context);

                    var nonExistentSemester = new Semester
                    {
                        Id = 999,
                        SemesterName = "Non-existent Semester",
                        CreationDate = DateTime.UtcNow,
                        UpdationDate = null
                    };

                    // Act & Assert
                    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                        () => repository.UpdateAsync(nonExistentSemester));
                    Assert.Contains("Semester with ID 999 not found", exception.Message);
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_DeletesSemesterAndReturnsTrue()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();

                    var semester = new Semester
                    {
                        SemesterName = "Fall 2024",
                        CreationDate = DateTime.UtcNow,
                        UpdationDate = null
                    };

                    context.Semesters.Add(semester);
                    await context.SaveChangesAsync();

                    var repository = new SemesterRepository(context);

                    // Act
                    var result = await repository.DeleteAsync(semester.Id);

                    // Assert
                    Assert.True(result);

                    // Verify deletion
                    var deletedSemester = await context.Semesters.FindAsync(semester.Id);
                    Assert.Null(deletedSemester);
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        [Fact]
        public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();

                    var repository = new SemesterRepository(context);

                    // Act
                    var result = await repository.DeleteAsync(999);

                    // Assert
                    Assert.False(result);
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        [Fact]
        public void Constructor_WithNullContext_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SemesterRepository(null));
        }

        [Fact]
        public async Task GetByIdAsync_UsesAsNoTracking_DoesNotTrackEntity()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();

                    var semester = new Semester
                    {
                        SemesterName = "Fall 2024",
                        CreationDate = DateTime.UtcNow,
                        UpdationDate = null
                    };

                    context.Semesters.Add(semester);
                    await context.SaveChangesAsync();

                    var repository = new SemesterRepository(context);

                    // Act
                    var result = await repository.GetByIdAsync(semester.Id);

                    // Assert
                    Assert.NotNull(result);
                    var trackedEntity = context.ChangeTracker.Entries<Semester>()
                        .FirstOrDefault(e => e.Entity.Id == result.Id);
                    Assert.Null(trackedEntity);
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        [Fact]
        public async Task GetAllAsync_UsesAsNoTracking_DoesNotTrackEntities()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();

                    var semester1 = new Semester
                    {
                        SemesterName = "Fall 2024",
                        CreationDate = DateTime.UtcNow,
                        UpdationDate = null
                    };

                    var semester2 = new Semester
                    {
                        SemesterName = "Spring 2025",
                        CreationDate = DateTime.UtcNow,
                        UpdationDate = null
                    };

                    context.Semesters.AddRange(semester1, semester2);
                    await context.SaveChangesAsync();

                    var repository = new SemesterRepository(context);

                    // Act
                    var result = await repository.GetAllAsync();

                    // Assert
                    Assert.NotNull(result);
                    Assert.Equal(2, result.Count());
                    var trackedEntities = context.ChangeTracker.Entries<Semester>().ToList();
                    Assert.Empty(trackedEntities);
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        [Fact]
        public async Task AddAsync_WithExistingCreationDate_PreservesOriginalCreationDate()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();

                    var repository = new SemesterRepository(context);

                    var specificDate = new DateTime(2023, 1, 15, 10, 30, 0, DateTimeKind.Utc);
                    var semester = new Semester
                    {
                        SemesterName = "Fall 2024",
                        CreationDate = specificDate,
                        UpdationDate = null
                    };

                    // Act
                    var result = await repository.AddAsync(semester);

                    // Assert
                    Assert.NotNull(result);
                    Assert.Equal(specificDate, result.CreationDate);
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        [Fact]
        public async Task UpdateAsync_SetsUpdationDateToCurrentUtcTime()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();

                    var semester = new Semester
                    {
                        SemesterName = "Fall 2024",
                        CreationDate = DateTime.UtcNow,
                        UpdationDate = null
                    };

                    context.Semesters.Add(semester);
                    await context.SaveChangesAsync();

                    var repository = new SemesterRepository(context);

                    context.Entry(semester).State = EntityState.Detached;

                    var updatedSemester = new Semester
                    {
                        Id = semester.Id,
                        SemesterName = "Spring 2025",
                        CreationDate = semester.CreationDate,
                        UpdationDate = null
                    };

                    var beforeUpdate = DateTime.UtcNow;

                    // Act
                    var result = await repository.UpdateAsync(updatedSemester);
                    var afterUpdate = DateTime.UtcNow;

                    // Assert
                    Assert.NotNull(result);
                    Assert.NotNull(result.UpdationDate);
                    
                    // Parse the UpdationDate string and verify it's within the expected range
                    var updationDate = DateTime.Parse(result.UpdationDate);
                    Assert.True(updationDate >= beforeUpdate.AddSeconds(-1));
                    Assert.True(updationDate <= afterUpdate.AddSeconds(1));
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }
}
