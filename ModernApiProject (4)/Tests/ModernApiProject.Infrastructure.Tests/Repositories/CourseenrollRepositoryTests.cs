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
    public class CourseenrollRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsCorrectCourseenroll()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var courseenroll = new Courseenroll
            {
                StudentRegno = "STU001",
                Pincode = "1234",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 1,
                EnrollDate = DateTime.UtcNow
            };

            context.Courseenrolls.Add(courseenroll);
            await context.SaveChangesAsync();

            var repository = new CourseenrollRepository(context);

            // Act
            var result = await repository.GetByIdAsync(courseenroll.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(courseenroll.Id, result.Id);
            Assert.Equal("STU001", result.StudentRegno);
            Assert.Equal("1234", result.Pincode);
            Assert.Equal(1, result.Session);
            Assert.Equal(1, result.Department);
            Assert.Equal(1, result.Level);
            Assert.Equal(1, result.Semester);
            Assert.Equal(1, result.Course);

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

            var repository = new CourseenrollRepository(context);

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleCourseenrolls_ReturnsAllCourseenrolls()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var courseenroll1 = new Courseenroll
            {
                StudentRegno = "STU001",
                Pincode = "1234",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 1,
                EnrollDate = DateTime.UtcNow
            };

            var courseenroll2 = new Courseenroll
            {
                StudentRegno = "STU002",
                Pincode = "5678",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 2,
                EnrollDate = DateTime.UtcNow
            };

            var courseenroll3 = new Courseenroll
            {
                StudentRegno = "STU003",
                Pincode = "9012",
                Session = 2,
                Department = 2,
                Level = 2,
                Semester = 2,
                Course = 3,
                EnrollDate = DateTime.UtcNow
            };

            context.Courseenrolls.AddRange(courseenroll1, courseenroll2, courseenroll3);
            await context.SaveChangesAsync();

            var repository = new CourseenrollRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var courseenrollList = result.ToList();
            Assert.Equal(3, courseenrollList.Count);
            Assert.Contains(courseenrollList, c => c.StudentRegno == "STU001");
            Assert.Contains(courseenrollList, c => c.StudentRegno == "STU002");
            Assert.Contains(courseenrollList, c => c.StudentRegno == "STU003");

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithNoCourseenrolls_ReturnsEmptyCollection()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseenrollRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithValidCourseenroll_AddsCourseenrollSuccessfully()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseenrollRepository(context);

            var courseenroll = new Courseenroll
            {
                StudentRegno = "STU001",
                Pincode = "1234",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 1,
                EnrollDate = DateTime.UtcNow
            };

            // Act
            var result = await repository.AddAsync(courseenroll);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("STU001", result.StudentRegno);
            Assert.Equal("1234", result.Pincode);

            var savedCourseenroll = await context.Courseenrolls.FindAsync(result.Id);
            Assert.NotNull(savedCourseenroll);
            Assert.Equal("STU001", savedCourseenroll.StudentRegno);

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithNullCourseenroll_ThrowsArgumentNullException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseenrollRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null));

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithValidCourseenroll_UpdatesCourseenrollSuccessfully()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var courseenroll = new Courseenroll
            {
                StudentRegno = "STU001",
                Pincode = "1234",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 1,
                EnrollDate = DateTime.UtcNow
            };

            context.Courseenrolls.Add(courseenroll);
            await context.SaveChangesAsync();

            var repository = new CourseenrollRepository(context);

            // Act
            courseenroll.Pincode = "9999";
            courseenroll.Session = 2;
            var result = await repository.UpdateAsync(courseenroll);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("9999", result.Pincode);
            Assert.Equal(2, result.Session);

            var updatedCourseenroll = await context.Courseenrolls.FindAsync(courseenroll.Id);
            Assert.NotNull(updatedCourseenroll);
            Assert.Equal("9999", updatedCourseenroll.Pincode);
            Assert.Equal(2, updatedCourseenroll.Session);

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithNullCourseenroll_ThrowsArgumentNullException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseenrollRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateAsync(null));

            connection.Close();
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_DeletesCourseenrollSuccessfully()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var courseenroll = new Courseenroll
            {
                StudentRegno = "STU001",
                Pincode = "1234",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 1,
                EnrollDate = DateTime.UtcNow
            };

            context.Courseenrolls.Add(courseenroll);
            await context.SaveChangesAsync();

            var repository = new CourseenrollRepository(context);

            // Act
            var result = await repository.DeleteAsync(courseenroll.Id);

            // Assert
            Assert.True(result);

            var deletedCourseenroll = await context.Courseenrolls.FindAsync(courseenroll.Id);
            Assert.Null(deletedCourseenroll);

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

            var repository = new CourseenrollRepository(context);

            // Act
            var result = await repository.DeleteAsync(999);

            // Assert
            Assert.False(result);

            connection.Close();
        }

        [Fact]
        public async Task GetByStudentRegnoAsync_WithValidRegno_ReturnsStudentEnrollments()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var courseenroll1 = new Courseenroll
            {
                StudentRegno = "STU001",
                Pincode = "1234",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 1,
                EnrollDate = DateTime.UtcNow
            };

            var courseenroll2 = new Courseenroll
            {
                StudentRegno = "STU001",
                Pincode = "1234",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 2,
                EnrollDate = DateTime.UtcNow
            };

            var courseenroll3 = new Courseenroll
            {
                StudentRegno = "STU002",
                Pincode = "5678",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 3,
                EnrollDate = DateTime.UtcNow
            };

            context.Courseenrolls.AddRange(courseenroll1, courseenroll2, courseenroll3);
            await context.SaveChangesAsync();

            var repository = new CourseenrollRepository(context);

            // Act
            var result = await repository.GetByStudentRegnoAsync("STU001");

            // Assert
            Assert.NotNull(result);
            var enrollmentList = result.ToList();
            Assert.Equal(2, enrollmentList.Count);
            Assert.All(enrollmentList, e => Assert.Equal("STU001", e.StudentRegno));

            connection.Close();
        }

        [Fact]
        public async Task GetByStudentRegnoAsync_WithNullRegno_ThrowsArgumentException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseenrollRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => repository.GetByStudentRegnoAsync(null));

            connection.Close();
        }

        [Fact]
        public async Task GetByStudentRegnoAsync_WithEmptyRegno_ThrowsArgumentException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseenrollRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => repository.GetByStudentRegnoAsync(""));

            connection.Close();
        }

        [Fact]
        public async Task GetByStudentRegnoAsync_WithNonExistentRegno_ReturnsEmptyCollection()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseenrollRepository(context);

            // Act
            var result = await repository.GetByStudentRegnoAsync("NONEXISTENT");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            connection.Close();
        }

        [Fact]
        public async Task GetByCourseIdAsync_WithValidCourseId_ReturnsCourseEnrollments()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var courseenroll1 = new Courseenroll
            {
                StudentRegno = "STU001",
                Pincode = "1234",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 1,
                EnrollDate = DateTime.UtcNow
            };

            var courseenroll2 = new Courseenroll
            {
                StudentRegno = "STU002",
                Pincode = "5678",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 1,
                EnrollDate = DateTime.UtcNow
            };

            var courseenroll3 = new Courseenroll
            {
                StudentRegno = "STU003",
                Pincode = "9012",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 2,
                EnrollDate = DateTime.UtcNow
            };

            context.Courseenrolls.AddRange(courseenroll1, courseenroll2, courseenroll3);
            await context.SaveChangesAsync();

            var repository = new CourseenrollRepository(context);

            // Act
            var result = await repository.GetByCourseIdAsync(1);

            // Assert
            Assert.NotNull(result);
            var enrollmentList = result.ToList();
            Assert.Equal(2, enrollmentList.Count);
            Assert.All(enrollmentList, e => Assert.Equal(1, e.Course));

            connection.Close();
        }

        [Fact]
        public async Task GetByCourseIdAsync_WithNonExistentCourseId_ReturnsEmptyCollection()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseenrollRepository(context);

            // Act
            var result = await repository.GetByCourseIdAsync(999);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            connection.Close();
        }

        [Fact]
        public async Task GetByAcademicContextAsync_WithValidContext_ReturnsMatchingEnrollments()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var courseenroll1 = new Courseenroll
            {
                StudentRegno = "STU001",
                Pincode = "1234",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 1,
                EnrollDate = DateTime.UtcNow
            };

            var courseenroll2 = new Courseenroll
            {
                StudentRegno = "STU002",
                Pincode = "5678",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 2,
                EnrollDate = DateTime.UtcNow
            };

            var courseenroll3 = new Courseenroll
            {
                StudentRegno = "STU003",
                Pincode = "9012",
                Session = 2,
                Department = 2,
                Level = 2,
                Semester = 2,
                Course = 3,
                EnrollDate = DateTime.UtcNow
            };

            context.Courseenrolls.AddRange(courseenroll1, courseenroll2, courseenroll3);
            await context.SaveChangesAsync();

            var repository = new CourseenrollRepository(context);

            // Act
            var result = await repository.GetByAcademicContextAsync(1, 1, 1, 1);

            // Assert
            Assert.NotNull(result);
            var enrollmentList = result.ToList();
            Assert.Equal(2, enrollmentList.Count);
            Assert.All(enrollmentList, e =>
            {
                Assert.Equal(1, e.Session);
                Assert.Equal(1, e.Department);
                Assert.Equal(1, e.Level);
                Assert.Equal(1, e.Semester);
            });

            connection.Close();
        }

        [Fact]
        public async Task GetByAcademicContextAsync_WithNonMatchingContext_ReturnsEmptyCollection()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var courseenroll = new Courseenroll
            {
                StudentRegno = "STU001",
                Pincode = "1234",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 1,
                EnrollDate = DateTime.UtcNow
            };

            context.Courseenrolls.Add(courseenroll);
            await context.SaveChangesAsync();

            var repository = new CourseenrollRepository(context);

            // Act
            var result = await repository.GetByAcademicContextAsync(2, 2, 2, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            connection.Close();
        }

        [Fact]
        public async Task IsStudentEnrolledAsync_WithEnrolledStudent_ReturnsTrue()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var courseenroll = new Courseenroll
            {
                StudentRegno = "STU001",
                Pincode = "1234",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 1,
                EnrollDate = DateTime.UtcNow
            };

            context.Courseenrolls.Add(courseenroll);
            await context.SaveChangesAsync();

            var repository = new CourseenrollRepository(context);

            // Act
            var result = await repository.IsStudentEnrolledAsync("STU001", 1);

            // Assert
            Assert.True(result);

            connection.Close();
        }

        [Fact]
        public async Task IsStudentEnrolledAsync_WithNotEnrolledStudent_ReturnsFalse()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseenrollRepository(context);

            // Act
            var result = await repository.IsStudentEnrolledAsync("STU001", 1);

            // Assert
            Assert.False(result);

            connection.Close();
        }

        [Fact]
        public async Task IsStudentEnrolledAsync_WithNullRegno_ThrowsArgumentException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseenrollRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => repository.IsStudentEnrolledAsync(null, 1));

            connection.Close();
        }

        [Fact]
        public async Task IsStudentEnrolledAsync_WithEmptyRegno_ThrowsArgumentException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseenrollRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => repository.IsStudentEnrolledAsync("", 1));

            connection.Close();
        }

        [Fact]
        public async Task GetEnrollmentCountByCourseAsync_WithEnrollments_ReturnsCorrectCount()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var courseenroll1 = new Courseenroll
            {
                StudentRegno = "STU001",
                Pincode = "1234",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 1,
                EnrollDate = DateTime.UtcNow
            };

            var courseenroll2 = new Courseenroll
            {
                StudentRegno = "STU002",
                Pincode = "5678",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 1,
                EnrollDate = DateTime.UtcNow
            };

            var courseenroll3 = new Courseenroll
            {
                StudentRegno = "STU003",
                Pincode = "9012",
                Session = 1,
                Department = 1,
                Level = 1,
                Semester = 1,
                Course = 2,
                EnrollDate = DateTime.UtcNow
            };

            context.Courseenrolls.AddRange(courseenroll1, courseenroll2, courseenroll3);
            await context.SaveChangesAsync();

            var repository = new CourseenrollRepository(context);

            // Act
            var result = await repository.GetEnrollmentCountByCourseAsync(1);

            // Assert
            Assert.Equal(2, result);

            connection.Close();
        }

        [Fact]
        public async Task GetEnrollmentCountByCourseAsync_WithNoEnrollments_ReturnsZero()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseenrollRepository(context);

            // Act
            var result = await repository.GetEnrollmentCountByCourseAsync(1);

            // Assert
            Assert.Equal(0, result);

            connection.Close();
        }

        [Fact]
        public void Constructor_WithNullContext_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new CourseenrollRepository(null));
        }
    }
}
