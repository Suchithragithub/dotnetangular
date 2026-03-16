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
    public class CourseRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsCourse()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var course = new Course
            {
                CourseCode = "CS101",
                CourseName = "Introduction to Computer Science",
                CourseUnit = "3",
                NoofSeats = 30,
                CreationDate = DateTime.UtcNow,
                UpdationDate = DateTime.UtcNow.ToString()
            };

            context.Courses.Add(course);
            await context.SaveChangesAsync();

            var repository = new CourseRepository(context);

            // Act
            var result = await repository.GetByIdAsync(course.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(course.Id, result.Id);
            Assert.Equal("CS101", result.CourseCode);
            Assert.Equal("Introduction to Computer Science", result.CourseName);
            Assert.Equal("3", result.CourseUnit);
            Assert.Equal(30, result.NoofSeats);

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

            var repository = new CourseRepository(context);

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleCourses_ReturnsAllCourses()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var course1 = new Course
            {
                CourseCode = "CS101",
                CourseName = "Introduction to Computer Science",
                CourseUnit = "3",
                NoofSeats = 30,
                CreationDate = DateTime.UtcNow,
                UpdationDate = DateTime.UtcNow.ToString()
            };

            var course2 = new Course
            {
                CourseCode = "CS102",
                CourseName = "Data Structures",
                CourseUnit = "4",
                NoofSeats = 25,
                CreationDate = DateTime.UtcNow,
                UpdationDate = DateTime.UtcNow.ToString()
            };

            var course3 = new Course
            {
                CourseCode = "CS103",
                CourseName = "Algorithms",
                CourseUnit = "3",
                NoofSeats = 20,
                CreationDate = DateTime.UtcNow,
                UpdationDate = DateTime.UtcNow.ToString()
            };

            context.Courses.AddRange(course1, course2, course3);
            await context.SaveChangesAsync();

            var repository = new CourseRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var courseList = result.ToList();
            Assert.Equal(3, courseList.Count);
            Assert.Contains(courseList, c => c.CourseCode == "CS101");
            Assert.Contains(courseList, c => c.CourseCode == "CS102");
            Assert.Contains(courseList, c => c.CourseCode == "CS103");

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithNoCourses_ReturnsEmptyCollection()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithValidCourse_AddsCourseAndReturnsWithId()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseRepository(context);

            var course = new Course
            {
                CourseCode = "CS201",
                CourseName = "Advanced Programming",
                CourseUnit = "4",
                NoofSeats = 35,
                CreationDate = DateTime.UtcNow,
                UpdationDate = DateTime.UtcNow.ToString()
            };

            // Act
            var result = await repository.AddAsync(course);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("CS201", result.CourseCode);
            Assert.Equal("Advanced Programming", result.CourseName);
            Assert.Equal("4", result.CourseUnit);
            Assert.Equal(35, result.NoofSeats);

            var savedCourse = await context.Courses.FindAsync(result.Id);
            Assert.NotNull(savedCourse);
            Assert.Equal("CS201", savedCourse.CourseCode);

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithNullCourse_ThrowsArgumentNullException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null));

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithValidCourse_UpdatesCourseAndReturns()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var course = new Course
            {
                CourseCode = "CS301",
                CourseName = "Database Systems",
                CourseUnit = "3",
                NoofSeats = 40,
                CreationDate = DateTime.UtcNow,
                UpdationDate = DateTime.UtcNow.ToString()
            };

            context.Courses.Add(course);
            await context.SaveChangesAsync();

            var repository = new CourseRepository(context);

            // Act
            course.CourseName = "Advanced Database Systems";
            course.NoofSeats = 50;
            course.UpdationDate = DateTime.UtcNow.AddDays(1).ToString();

            var result = await repository.UpdateAsync(course);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Advanced Database Systems", result.CourseName);
            Assert.Equal(50, result.NoofSeats);

            var updatedCourse = await context.Courses.FindAsync(course.Id);
            Assert.NotNull(updatedCourse);
            Assert.Equal("Advanced Database Systems", updatedCourse.CourseName);
            Assert.Equal(50, updatedCourse.NoofSeats);

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithNullCourse_ThrowsArgumentNullException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateAsync(null));

            connection.Close();
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_DeletesCourseAndReturnsTrue()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var course = new Course
            {
                CourseCode = "CS401",
                CourseName = "Software Engineering",
                CourseUnit = "4",
                NoofSeats = 30,
                CreationDate = DateTime.UtcNow,
                UpdationDate = DateTime.UtcNow.ToString()
            };

            context.Courses.Add(course);
            await context.SaveChangesAsync();

            var repository = new CourseRepository(context);

            // Act
            var result = await repository.DeleteAsync(course.Id);

            // Assert
            Assert.True(result);

            var deletedCourse = await context.Courses.FindAsync(course.Id);
            Assert.Null(deletedCourse);

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

            var repository = new CourseRepository(context);

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
            Assert.Throws<ArgumentNullException>(() => new CourseRepository(null));
        }

        [Fact]
        public async Task AddAsync_MultipleCourses_AddsAllSuccessfully()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseRepository(context);

            var course1 = new Course
            {
                CourseCode = "CS501",
                CourseName = "Machine Learning",
                CourseUnit = "3",
                NoofSeats = 25,
                CreationDate = DateTime.UtcNow,
                UpdationDate = DateTime.UtcNow.ToString()
            };

            var course2 = new Course
            {
                CourseCode = "CS502",
                CourseName = "Artificial Intelligence",
                CourseUnit = "4",
                NoofSeats = 20,
                CreationDate = DateTime.UtcNow,
                UpdationDate = DateTime.UtcNow.ToString()
            };

            // Act
            var result1 = await repository.AddAsync(course1);
            var result2 = await repository.AddAsync(course2);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.True(result1.Id > 0);
            Assert.True(result2.Id > 0);
            Assert.NotEqual(result1.Id, result2.Id);

            var allCourses = await repository.GetAllAsync();
            Assert.Equal(2, allCourses.Count());

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_NonExistentCourse_ThrowsDbUpdateConcurrencyException()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseRepository(context);

            var course = new Course
            {
                Id = 999,
                CourseCode = "CS999",
                CourseName = "Non-existent Course",
                CourseUnit = "3",
                NoofSeats = 10,
                CreationDate = DateTime.UtcNow,
                UpdationDate = DateTime.UtcNow.ToString()
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => repository.UpdateAsync(course));

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

            var course = new Course
            {
                CourseCode = "CS601",
                CourseName = "Cloud Computing",
                CourseUnit = "3",
                NoofSeats = 30,
                CreationDate = DateTime.UtcNow,
                UpdationDate = DateTime.UtcNow.ToString()
            };

            context.Courses.Add(course);
            await context.SaveChangesAsync();

            var repository = new CourseRepository(context);

            // Act
            await repository.DeleteAsync(course.Id);
            var result = await repository.GetByIdAsync(course.Id);

            // Assert
            Assert.Null(result);

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_CourseWithAllProperties_SavesAllProperties()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            var repository = new CourseRepository(context);

            var creationDate = DateTime.UtcNow;
            var updationDate = DateTime.UtcNow.AddHours(1).ToString();

            var course = new Course
            {
                CourseCode = "CS701",
                CourseName = "Cybersecurity",
                CourseUnit = "4",
                NoofSeats = 15,
                CreationDate = creationDate,
                UpdationDate = updationDate
            };

            // Act
            var result = await repository.AddAsync(course);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CS701", result.CourseCode);
            Assert.Equal("Cybersecurity", result.CourseName);
            Assert.Equal("4", result.CourseUnit);
            Assert.Equal(15, result.NoofSeats);
            Assert.Equal(creationDate, result.CreationDate);
            Assert.Equal(updationDate, result.UpdationDate);

            connection.Close();
        }
    }
}