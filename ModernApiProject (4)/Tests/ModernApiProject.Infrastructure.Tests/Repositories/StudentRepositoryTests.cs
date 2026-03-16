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
    public class StudentRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsStudent()
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

                var student = new Student
                {
                    StudentRegno = "STU001",
                    StudentPhoto = "photo1.jpg",
                    Password = "password123",
                    StudentName = "John Doe",
                    Pincode = "12345",
                    Session = "2023-2024",
                    Department = "Computer Science",
                    Semester = "Fall",
                    Cgpa = 3.5m,
                    Creationdate = DateTime.UtcNow,
                    UpdationDate = null
                };

                context.Students.Add(student);
                await context.SaveChangesAsync();

                var repository = new StudentRepository(context);

                // Act
                var result = await repository.GetByIdAsync("STU001");

                // Assert
                Assert.NotNull(result);
                Assert.Equal("STU001", result.StudentRegno);
                Assert.Equal("John Doe", result.StudentName);
                Assert.Equal("Computer Science", result.Department);
                Assert.Equal(3.5m, result.Cgpa);
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

                var repository = new StudentRepository(context);

                // Act
                var result = await repository.GetByIdAsync("NONEXISTENT");

                // Assert
                Assert.Null(result);
            }

            connection.Close();
        }

        [Fact]
        public async Task GetByIdAsync_WithNullId_ThrowsArgumentException()
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

                var repository = new StudentRepository(context);

                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(() => repository.GetByIdAsync(null));
            }

            connection.Close();
        }

        [Fact]
        public async Task GetByIdAsync_WithEmptyId_ThrowsArgumentException()
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

                var repository = new StudentRepository(context);

                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(() => repository.GetByIdAsync(""));
            }

            connection.Close();
        }

        [Fact]
        public async Task GetByIdAsync_WithWhitespaceId_ThrowsArgumentException()
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

                var repository = new StudentRepository(context);

                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(() => repository.GetByIdAsync("   "));
            }

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithNoStudents_ReturnsEmptyCollection()
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

                var repository = new StudentRepository(context);

                // Act
                var result = await repository.GetAllAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result);
            }

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleStudents_ReturnsAllStudents()
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

                var students = new List<Student>
                {
                    new Student
                    {
                        StudentRegno = "STU001",
                        StudentPhoto = "photo1.jpg",
                        Password = "password123",
                        StudentName = "John Doe",
                        Pincode = "12345",
                        Session = "2023-2024",
                        Department = "Computer Science",
                        Semester = "Fall",
                        Cgpa = 3.5m,
                        Creationdate = DateTime.UtcNow,
                        UpdationDate = null
                    },
                    new Student
                    {
                        StudentRegno = "STU002",
                        StudentPhoto = "photo2.jpg",
                        Password = "password456",
                        StudentName = "Jane Smith",
                        Pincode = "67890",
                        Session = "2023-2024",
                        Department = "Mathematics",
                        Semester = "Spring",
                        Cgpa = 3.8m,
                        Creationdate = DateTime.UtcNow,
                        UpdationDate = null
                    },
                    new Student
                    {
                        StudentRegno = "STU003",
                        StudentPhoto = "photo3.jpg",
                        Password = "password789",
                        StudentName = "Bob Johnson",
                        Pincode = "11111",
                        Session = "2023-2024",
                        Department = "Physics",
                        Semester = "Fall",
                        Cgpa = 3.2m,
                        Creationdate = DateTime.UtcNow,
                        UpdationDate = null
                    }
                };

                context.Students.AddRange(students);
                await context.SaveChangesAsync();

                var repository = new StudentRepository(context);

                // Act
                var result = await repository.GetAllAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Equal(3, result.Count());
                Assert.Contains(result, s => s.StudentRegno == "STU001");
                Assert.Contains(result, s => s.StudentRegno == "STU002");
                Assert.Contains(result, s => s.StudentRegno == "STU003");
            }

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithValidStudent_AddsStudentToDatabase()
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

                var repository = new StudentRepository(context);

                var student = new Student
                {
                    StudentRegno = "STU001",
                    StudentPhoto = "photo1.jpg",
                    Password = "password123",
                    StudentName = "John Doe",
                    Pincode = "12345",
                    Session = "2023-2024",
                    Department = "Computer Science",
                    Semester = "Fall",
                    Cgpa = 3.5m,
                    Creationdate = DateTime.UtcNow,
                    UpdationDate = null
                };

                // Act
                var result = await repository.AddAsync(student);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("STU001", result.StudentRegno);

                var savedStudent = await context.Students.FindAsync("STU001");
                Assert.NotNull(savedStudent);
                Assert.Equal("John Doe", savedStudent.StudentName);
                Assert.Equal("Computer Science", savedStudent.Department);
            }

            connection.Close();
        }

        [Fact]
        public async Task AddAsync_WithNullStudent_ThrowsArgumentNullException()
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

                var repository = new StudentRepository(context);

                // Act & Assert
                await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null));
            }

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithValidStudent_UpdatesStudentInDatabase()
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

                var student = new Student
                {
                    StudentRegno = "STU001",
                    StudentPhoto = "photo1.jpg",
                    Password = "password123",
                    StudentName = "John Doe",
                    Pincode = "12345",
                    Session = "2023-2024",
                    Department = "Computer Science",
                    Semester = "Fall",
                    Cgpa = 3.5m,
                    Creationdate = DateTime.UtcNow,
                    UpdationDate = null
                };

                context.Students.Add(student);
                await context.SaveChangesAsync();

                context.Entry(student).State = EntityState.Detached;

                var repository = new StudentRepository(context);

                var updatedStudent = new Student
                {
                    StudentRegno = "STU001",
                    StudentPhoto = "photo1_updated.jpg",
                    Password = "newpassword123",
                    StudentName = "John Doe Updated",
                    Pincode = "54321",
                    Session = "2024-2025",
                    Department = "Software Engineering",
                    Semester = "Spring",
                    Cgpa = 3.9m,
                    Creationdate = student.Creationdate,
                    UpdationDate = DateTime.UtcNow.ToString()
                };

                // Act
                var result = await repository.UpdateAsync(updatedStudent);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("STU001", result.StudentRegno);

                var savedStudent = await context.Students.FindAsync("STU001");
                Assert.NotNull(savedStudent);
                Assert.Equal("John Doe Updated", savedStudent.StudentName);
                Assert.Equal("Software Engineering", savedStudent.Department);
                Assert.Equal(3.9m, savedStudent.Cgpa);
                Assert.Equal("54321", savedStudent.Pincode);
            }

            connection.Close();
        }

        [Fact]
        public async Task UpdateAsync_WithNullStudent_ThrowsArgumentNullException()
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

                var repository = new StudentRepository(context);

                // Act & Assert
                await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateAsync(null));
            }

            connection.Close();
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_DeletesStudentAndReturnsTrue()
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

                var student = new Student
                {
                    StudentRegno = "STU001",
                    StudentPhoto = "photo1.jpg",
                    Password = "password123",
                    StudentName = "John Doe",
                    Pincode = "12345",
                    Session = "2023-2024",
                    Department = "Computer Science",
                    Semester = "Fall",
                    Cgpa = 3.5m,
                    Creationdate = DateTime.UtcNow,
                    UpdationDate = null
                };

                context.Students.Add(student);
                await context.SaveChangesAsync();

                var repository = new StudentRepository(context);

                // Act
                var result = await repository.DeleteAsync("STU001");

                // Assert
                Assert.True(result);

                var deletedStudent = await context.Students.FindAsync("STU001");
                Assert.Null(deletedStudent);
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

                var repository = new StudentRepository(context);

                // Act
                var result = await repository.DeleteAsync("NONEXISTENT");

                // Assert
                Assert.False(result);
            }

            connection.Close();
        }

        [Fact]
        public async Task DeleteAsync_WithNullId_ThrowsArgumentException()
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

                var repository = new StudentRepository(context);

                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(() => repository.DeleteAsync(null));
            }

            connection.Close();
        }

        [Fact]
        public async Task DeleteAsync_WithEmptyId_ThrowsArgumentException()
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

                var repository = new StudentRepository(context);

                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(() => repository.DeleteAsync(""));
            }

            connection.Close();
        }

        [Fact]
        public async Task DeleteAsync_WithWhitespaceId_ThrowsArgumentException()
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

                var repository = new StudentRepository(context);

                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(() => repository.DeleteAsync("   "));
            }

            connection.Close();
        }

        [Fact]
        public void Constructor_WithNullContext_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new StudentRepository(null));
        }

        [Fact]
        public async Task AddAsync_WithCompleteStudentData_PersistsAllProperties()
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

                var repository = new StudentRepository(context);

                var creationDate = DateTime.UtcNow;
                var updationDate = DateTime.UtcNow.AddDays(1).ToString();

                var student = new Student
                {
                    StudentRegno = "STU999",
                    StudentPhoto = "detailed_photo.jpg",
                    Password = "securePassword123",
                    StudentName = "Alice Wonder",
                    Pincode = "99999",
                    Session = "2024-2025",
                    Department = "Electrical Engineering",
                    Semester = "Winter",
                    Cgpa = 4.0m,
                    Creationdate = creationDate,
                    UpdationDate = updationDate
                };

                // Act
                await repository.AddAsync(student);

                // Assert
                var savedStudent = await context.Students.FindAsync("STU999");
                Assert.NotNull(savedStudent);
                Assert.Equal("STU999", savedStudent.StudentRegno);
                Assert.Equal("detailed_photo.jpg", savedStudent.StudentPhoto);
                Assert.Equal("securePassword123", savedStudent.Password);
                Assert.Equal("Alice Wonder", savedStudent.StudentName);
                Assert.Equal("99999", savedStudent.Pincode);
                Assert.Equal("2024-2025", savedStudent.Session);
                Assert.Equal("Electrical Engineering", savedStudent.Department);
                Assert.Equal("Winter", savedStudent.Semester);
                Assert.Equal(4.0m, savedStudent.Cgpa);
                Assert.Equal(creationDate, savedStudent.Creationdate);
                Assert.Equal(updationDate, savedStudent.UpdationDate);
            }

            connection.Close();
        }

        [Fact]
        public async Task GetAllAsync_WithSingleStudent_ReturnsCollectionWithOneStudent()
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

                var student = new Student
                {
                    StudentRegno = "STU001",
                    StudentPhoto = "photo1.jpg",
                    Password = "password123",
                    StudentName = "John Doe",
                    Pincode = "12345",
                    Session = "2023-2024",
                    Department = "Computer Science",
                    Semester = "Fall",
                    Cgpa = 3.5m,
                    Creationdate = DateTime.UtcNow,
                    UpdationDate = null
                };

                context.Students.Add(student);
                await context.SaveChangesAsync();

                var repository = new StudentRepository(context);

                // Act
                var result = await repository.GetAllAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Single(result);
                Assert.Equal("STU001", result.First().StudentRegno);
            }

            connection.Close();
        }
    }
}