using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ModernApiProject.Application.Models;
using ModernApiProject.Application.Services;
using ModernApiProject.Domain.Entities;
using ModernApiProject.Infrastructure.Data;

namespace ModernApiProject.Application.Tests.Services
{
    public class StandaloneServiceTests
    {
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly Mock<ILogger<StandaloneService>> _mockLogger;
        private readonly StandaloneService _service;

        public StandaloneServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .Options;
            _mockContext = new Mock<ApplicationDbContext>(options);
            _mockLogger = new Mock<ILogger<StandaloneService>>();
            _service = new StandaloneService(_mockContext.Object, _mockLogger.Object);
        }

        #region ValidateStudentPasswordAsync Tests

        [Fact]
        public async Task ValidateStudentPasswordAsync_WithValidCredentials_ReturnsTrue()
        {
            // Arrange
            var studentRegno = "STU001";
            var password = "password123";
            var students = new List<Student>
            {
                new Student { StudentRegno = studentRegno, Password = password }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(students);
            _mockContext.Setup(c => c.Students).Returns(mockSet.Object);

            // Act
            var result = await _service.ValidateStudentPasswordAsync(studentRegno, password);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateStudentPasswordAsync_WithInvalidPassword_ReturnsFalse()
        {
            // Arrange
            var studentRegno = "STU001";
            var correctPassword = "password123";
            var wrongPassword = "wrongpassword";
            var students = new List<Student>
            {
                new Student { StudentRegno = studentRegno, Password = correctPassword }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(students);
            _mockContext.Setup(c => c.Students).Returns(mockSet.Object);

            // Act
            var result = await _service.ValidateStudentPasswordAsync(studentRegno, wrongPassword);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateStudentPasswordAsync_WithNonExistentStudent_ReturnsFalse()
        {
            // Arrange
            var studentRegno = "NONEXISTENT";
            var password = "password123";
            var students = new List<Student>().AsQueryable();

            var mockSet = CreateMockDbSet(students);
            _mockContext.Setup(c => c.Students).Returns(mockSet.Object);

            // Act
            var result = await _service.ValidateStudentPasswordAsync(studentRegno, password);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null, "password")]
        [InlineData("", "password")]
        [InlineData("   ", "password")]
        public async Task ValidateStudentPasswordAsync_WithInvalidStudentRegno_ThrowsArgumentException(string invalidRegno, string password)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.ValidateStudentPasswordAsync(invalidRegno, password));
        }

        [Theory]
        [InlineData("STU001", null)]
        [InlineData("STU001", "")]
        [InlineData("STU001", "   ")]
        public async Task ValidateStudentPasswordAsync_WithInvalidPassword_ThrowsArgumentException(string studentRegno, string invalidPassword)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.ValidateStudentPasswordAsync(studentRegno, invalidPassword));
        }

        #endregion

        #region UpdateStudentPasswordAsync Tests

        [Fact]
        public async Task UpdateStudentPasswordAsync_WithValidStudent_ReturnsTrue()
        {
            // Arrange
            var studentRegno = "STU001";
            var newPassword = "newpassword123";
            var student = new Student 
            { 
                StudentRegno = studentRegno, 
                Password = "oldpassword",
                UpdationDate = null
            };
            var students = new List<Student> { student }.AsQueryable();

            var mockSet = CreateMockDbSet(students);
            _mockContext.Setup(c => c.Students).Returns(mockSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _service.UpdateStudentPasswordAsync(studentRegno, newPassword);

            // Assert
            Assert.True(result);
            Assert.Equal(newPassword, student.Password);
            Assert.NotNull(student.UpdationDate);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateStudentPasswordAsync_WithNonExistentStudent_ReturnsFalse()
        {
            // Arrange
            var studentRegno = "NONEXISTENT";
            var newPassword = "newpassword123";
            var students = new List<Student>().AsQueryable();

            var mockSet = CreateMockDbSet(students);
            _mockContext.Setup(c => c.Students).Returns(mockSet.Object);

            // Act
            var result = await _service.UpdateStudentPasswordAsync(studentRegno, newPassword);

            // Assert
            Assert.False(result);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData(null, "password")]
        [InlineData("", "password")]
        [InlineData("   ", "password")]
        public async Task UpdateStudentPasswordAsync_WithInvalidStudentRegno_ThrowsArgumentException(string invalidRegno, string password)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.UpdateStudentPasswordAsync(invalidRegno, password));
        }

        [Theory]
        [InlineData("STU001", null)]
        [InlineData("STU001", "")]
        [InlineData("STU001", "   ")]
        public async Task UpdateStudentPasswordAsync_WithInvalidPassword_ThrowsArgumentException(string studentRegno, string invalidPassword)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.UpdateStudentPasswordAsync(studentRegno, invalidPassword));
        }

        #endregion

        #region IsStudentEnrolledInCourseAsync Tests

        [Fact]
        public async Task IsStudentEnrolledInCourseAsync_WhenEnrolled_ReturnsTrue()
        {
            // Arrange
            var studentRegno = "STU001";
            var courseId = 1;
            var enrollments = new List<Courseenroll>
            {
                new Courseenroll { StudentRegno = studentRegno, Course = courseId }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(enrollments);
            _mockContext.Setup(c => c.Courseenrolls).Returns(mockSet.Object);

            // Act
            var result = await _service.IsStudentEnrolledInCourseAsync(studentRegno, courseId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsStudentEnrolledInCourseAsync_WhenNotEnrolled_ReturnsFalse()
        {
            // Arrange
            var studentRegno = "STU001";
            var courseId = 1;
            var enrollments = new List<Courseenroll>().AsQueryable();

            var mockSet = CreateMockDbSet(enrollments);
            _mockContext.Setup(c => c.Courseenrolls).Returns(mockSet.Object);

            // Act
            var result = await _service.IsStudentEnrolledInCourseAsync(studentRegno, courseId);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task IsStudentEnrolledInCourseAsync_WithInvalidStudentRegno_ThrowsArgumentException(string invalidRegno)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.IsStudentEnrolledInCourseAsync(invalidRegno, 1));
        }

        #endregion

        #region GetCourseEnrollmentCountAsync Tests

        [Fact]
        public async Task GetCourseEnrollmentCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            var courseId = 1;
            var enrollments = new List<Courseenroll>
            {
                new Courseenroll { Course = courseId },
                new Courseenroll { Course = courseId },
                new Courseenroll { Course = 2 }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(enrollments);
            _mockContext.Setup(c => c.Courseenrolls).Returns(mockSet.Object);

            // Act
            var result = await _service.GetCourseEnrollmentCountAsync(courseId);

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task GetCourseEnrollmentCountAsync_WithNoEnrollments_ReturnsZero()
        {
            // Arrange
            var courseId = 1;
            var enrollments = new List<Courseenroll>().AsQueryable();

            var mockSet = CreateMockDbSet(enrollments);
            _mockContext.Setup(c => c.Courseenrolls).Returns(mockSet.Object);

            // Act
            var result = await _service.GetCourseEnrollmentCountAsync(courseId);

            // Assert
            Assert.Equal(0, result);
        }

        #endregion

        #region GetCourseAvailableSeatsAsync Tests

        [Fact]
        public async Task GetCourseAvailableSeatsAsync_ReturnsCorrectAvailableSeats()
        {
            // Arrange
            var courseId = 1;
            var totalSeats = 30;
            var courses = new List<Course>
            {
                new Course { Id = courseId, NoofSeats = totalSeats }
            }.AsQueryable();
            var enrollments = new List<Courseenroll>
            {
                new Courseenroll { Course = courseId },
                new Courseenroll { Course = courseId }
            }.AsQueryable();

            var mockCourseSet = CreateMockDbSet(courses);
            var mockEnrollmentSet = CreateMockDbSet(enrollments);
            _mockContext.Setup(c => c.Courses).Returns(mockCourseSet.Object);
            _mockContext.Setup(c => c.Courseenrolls).Returns(mockEnrollmentSet.Object);

            // Act
            var result = await _service.GetCourseAvailableSeatsAsync(courseId);

            // Assert
            Assert.Equal(28, result);
        }

        [Fact]
        public async Task GetCourseAvailableSeatsAsync_WhenFull_ReturnsZero()
        {
            // Arrange
            var courseId = 1;
            var totalSeats = 2;
            var courses = new List<Course>
            {
                new Course { Id = courseId, NoofSeats = totalSeats }
            }.AsQueryable();
            var enrollments = new List<Courseenroll>
            {
                new Courseenroll { Course = courseId },
                new Courseenroll { Course = courseId }
            }.AsQueryable();

            var mockCourseSet = CreateMockDbSet(courses);
            var mockEnrollmentSet = CreateMockDbSet(enrollments);
            _mockContext.Setup(c => c.Courses).Returns(mockCourseSet.Object);
            _mockContext.Setup(c => c.Courseenrolls).Returns(mockEnrollmentSet.Object);

            // Act
            var result = await _service.GetCourseAvailableSeatsAsync(courseId);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetCourseAvailableSeatsAsync_WhenCourseNotFound_ReturnsNegativeOne()
        {
            // Arrange
            var courseId = 999;
            var courses = new List<Course>().AsQueryable();

            var mockCourseSet = CreateMockDbSet(courses);
            _mockContext.Setup(c => c.Courses).Returns(mockCourseSet.Object);

            // Act
            var result = await _service.GetCourseAvailableSeatsAsync(courseId);

            // Assert
            Assert.Equal(-1, result);
        }

        #endregion

        #region GetEnrollmentHistoryByStudentAsync Tests

        [Fact]
        public async Task GetEnrollmentHistoryByStudentAsync_ReturnsEnrollments()
        {
            // Arrange
            var studentRegno = "STU001";
            var enrollments = new List<Courseenroll>
            {
                new Courseenroll 
                { 
                    Id = 1, 
                    StudentRegno = studentRegno, 
                    Course = 1, 
                    Session = 1, 
                    Department = 1, 
                    Level = 1, 
                    Semester = 1,
                    Pincode = "1234",
                    EnrollDate = DateTime.UtcNow
                }
            }.AsQueryable();
            var courses = new List<Course> { new Course { Id = 1 } }.AsQueryable();
            var sessions = new List<Session> { new Session { Id = 1 } }.AsQueryable();
            var departments = new List<Department> { new Department { Id = 1 } }.AsQueryable();
            var levels = new List<Level> { new Level { Id = 1 } }.AsQueryable();
            var semesters = new List<Semester> { new Semester { Id = 1 } }.AsQueryable();

            _mockContext.Setup(c => c.Courseenrolls).Returns(CreateMockDbSet(enrollments).Object);
            _mockContext.Setup(c => c.Courses).Returns(CreateMockDbSet(courses).Object);
            _mockContext.Setup(c => c.Sessions).Returns(CreateMockDbSet(sessions).Object);
            _mockContext.Setup(c => c.Departments).Returns(CreateMockDbSet(departments).Object);
            _mockContext.Setup(c => c.Levels).Returns(CreateMockDbSet(levels).Object);
            _mockContext.Setup(c => c.Semesters).Returns(CreateMockDbSet(semesters).Object);

            // Act
            var result = await _service.GetEnrollmentHistoryByStudentAsync(studentRegno);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var enrollment = result.First();
            Assert.Equal(studentRegno, enrollment.StudentRegno);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetEnrollmentHistoryByStudentAsync_WithInvalidStudentRegno_ThrowsArgumentException(string invalidRegno)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.GetEnrollmentHistoryByStudentAsync(invalidRegno));
        }

        #endregion

        #region CreateEnrollmentAsync Tests

        [Fact]
        public async Task CreateEnrollmentAsync_WithValidData_CreatesEnrollment()
        {
            // Arrange
            var studentRegno = "STU001";
            var pincode = "1234";
            var courseId = 1;
            var students = new List<Student>
            {
                new Student { StudentRegno = studentRegno, Pincode = pincode }
            }.AsQueryable();
            var courses = new List<Course>
            {
                new Course { Id = courseId, NoofSeats = 30 }
            }.AsQueryable();
            var enrollments = new List<Courseenroll>().AsQueryable();

            _mockContext.Setup(c => c.Students).Returns(CreateMockDbSet(students).Object);
            _mockContext.Setup(c => c.Courses).Returns(CreateMockDbSet(courses).Object);
            _mockContext.Setup(c => c.Courseenrolls).Returns(CreateMockDbSet(enrollments).Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _service.CreateEnrollmentAsync(studentRegno, pincode, 1, 1, 1, courseId, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(studentRegno, result.StudentRegno);
            Assert.Equal(pincode, result.Pincode);
        }

        [Fact]
        public async Task CreateEnrollmentAsync_WhenAlreadyEnrolled_ThrowsInvalidOperationException()
        {
            // Arrange
            var studentRegno = "STU001";
            var pincode = "1234";
            var courseId = 1;
            var enrollments = new List<Courseenroll>
            {
                new Courseenroll { StudentRegno = studentRegno, Course = courseId }
            }.AsQueryable();

            _mockContext.Setup(c => c.Courseenrolls).Returns(CreateMockDbSet(enrollments).Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CreateEnrollmentAsync(studentRegno, pincode, 1, 1, 1, courseId, 1));
        }

        [Fact]
        public async Task CreateEnrollmentAsync_WithInvalidPincode_ThrowsInvalidOperationException()
        {
            // Arrange
            var studentRegno = "STU001";
            var correctPincode = "1234";
            var wrongPincode = "9999";
            var courseId = 1;
            var students = new List<Student>
            {
                new Student { StudentRegno = studentRegno, Pincode = correctPincode }
            }.AsQueryable();
            var enrollments = new List<Courseenroll>().AsQueryable();

            _mockContext.Setup(c => c.Students).Returns(CreateMockDbSet(students).Object);
            _mockContext.Setup(c => c.Courseenrolls).Returns(CreateMockDbSet(enrollments).Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CreateEnrollmentAsync(studentRegno, wrongPincode, 1, 1, 1, courseId, 1));
        }

        [Fact]
        public async Task CreateEnrollmentAsync_WhenCourseNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var studentRegno = "STU001";
            var pincode = "1234";
            var courseId = 999;
            var students = new List<Student>
            {
                new Student { StudentRegno = studentRegno, Pincode = pincode }
            }.AsQueryable();
            var enrollments = new List<Courseenroll>().AsQueryable();
            var courses = new List<Course>().AsQueryable();

            _mockContext.Setup(c => c.Students).Returns(CreateMockDbSet(students).Object);
            _mockContext.Setup(c => c.Courseenrolls).Returns(CreateMockDbSet(enrollments).Object);
            _mockContext.Setup(c => c.Courses).Returns(CreateMockDbSet(courses).Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CreateEnrollmentAsync(studentRegno, pincode, 1, 1, 1, courseId, 1));
        }

        [Fact]
        public async Task CreateEnrollmentAsync_WhenCourseFull_ThrowsInvalidOperationException()
        {
            // Arrange
            var studentRegno = "STU001";
            var pincode = "1234";
            var courseId = 1;
            var students = new List<Student>
            {
                new Student { StudentRegno = studentRegno, Pincode = pincode }
            }.AsQueryable();
            var courses = new List<Course>
            {
                new Course { Id = courseId, NoofSeats = 1 }
            }.AsQueryable();
            var enrollments = new List<Courseenroll>
            {
                new Courseenroll { Course = courseId, StudentRegno = "OTHER" }
            }.AsQueryable();

            _mockContext.Setup(c => c.Students).Returns(CreateMockDbSet(students).Object);
            _mockContext.Setup(c => c.Courses).Returns(CreateMockDbSet(courses).Object);
            _mockContext.Setup(c => c.Courseenrolls).Returns(CreateMockDbSet(enrollments).Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CreateEnrollmentAsync(studentRegno, pincode, 1, 1, 1, courseId, 1));
        }

        [Theory]
        [InlineData(null, "1234")]
        [InlineData("", "1234")]
        [InlineData("   ", "1234")]
        public async Task CreateEnrollmentAsync_WithInvalidStudentRegno_ThrowsArgumentException(string invalidRegno, string pincode)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateEnrollmentAsync(invalidRegno, pincode, 1, 1, 1, 1, 1));
        }

        [Theory]
        [InlineData("STU001", null)]
        [InlineData("STU001", "")]
        [InlineData("STU001", "   ")]
        public async Task CreateEnrollmentAsync_WithInvalidPincode_ThrowsArgumentException(string studentRegno, string invalidPincode)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateEnrollmentAsync(studentRegno, invalidPincode, 1, 1, 1, 1, 1));
        }

        #endregion

        #region GetStudentByRegnoAsync Tests

        [Fact]
        public async Task GetStudentByRegnoAsync_WithValidRegno_ReturnsStudent()
        {
            // Arrange
            var studentRegno = "STU001";
            var students = new List<Student>
            {
                new Student 
                { 
                    StudentRegno = studentRegno,
                    StudentName = "John Doe",
                    Password = "password",
                    Pincode = "1234",
                    Cgpa = 3.5m
                }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(students);
            _mockContext.Setup(c => c.Students).Returns(mockSet.Object);

            // Act
            var result = await _service.GetStudentByRegnoAsync(studentRegno);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(studentRegno, result.StudentRegno);
            Assert.Equal("John Doe", result.StudentName);
        }

        [Fact]
        public async Task GetStudentByRegnoAsync_WithNonExistentRegno_ReturnsNull()
        {
            // Arrange
            var studentRegno = "NONEXISTENT";
            var students = new List<Student>().AsQueryable();

            var mockSet = CreateMockDbSet(students);
            _mockContext.Setup(c => c.Students).Returns(mockSet.Object);

            // Act
            var result = await _service.GetStudentByRegnoAsync(studentRegno);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetStudentByRegnoAsync_WithInvalidRegno_ThrowsArgumentException(string invalidRegno)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.GetStudentByRegnoAsync(invalidRegno));
        }

        #endregion

        #region GetAllSessionsAsync Tests

        [Fact]
        public async Task GetAllSessionsAsync_ReturnsAllSessions()
        {
            // Arrange
            var sessions = new List<Session>
            {
                new Session { Id = 1, SessionName = "2023-2024" },
                new Session { Id = 2, SessionName = "2024-2025" }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(sessions);
            _mockContext.Setup(c => c.Sessions).Returns(mockSet.Object);

            // Act
            var result = await _service.GetAllSessionsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        #endregion

        #region GetAllDepartmentsAsync Tests

        [Fact]
        public async Task GetAllDepartmentsAsync_ReturnsAllDepartments()
        {
            // Arrange
            var departments = new List<Department>
            {
                new Department { Id = 1, DepartmentName = "Computer Science" },
                new Department { Id = 2, DepartmentName = "Mathematics" }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(departments);
            _mockContext.Setup(c => c.Departments).Returns(mockSet.Object);

            // Act
            var result = await _service.GetAllDepartmentsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        #endregion

        #region GetAllLevelsForEnrollmentAsync Tests

        [Fact]
        public async Task GetAllLevelsForEnrollmentAsync_ReturnsAllLevels()
        {
            // Arrange
            var levels = new List<Level>
            {
                new Level { Id = 1, LevelName = "100 Level" },
                new Level { Id = 2, LevelName = "200 Level" }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(levels);
            _mockContext.Setup(c => c.Levels).Returns(mockSet.Object);

            // Act
            var result = await _service.GetAllLevelsForEnrollmentAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        #endregion

        #region GetAllSemestersForEnrollmentAsync Tests

        [Fact]
        public async Task GetAllSemestersForEnrollmentAsync_ReturnsAllSemesters()
        {
            // Arrange
            var semesters = new List<Semester>
            {
                new Semester { Id = 1, SemesterName = "First Semester" },
                new Semester { Id = 2, SemesterName = "Second Semester" }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(semesters);
            _mockContext.Setup(c => c.Semesters).Returns(mockSet.Object);

            // Act
            var result = await _service.GetAllSemestersForEnrollmentAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        #endregion

        #region GetAllCoursesForEnrollmentAsync Tests

        [Fact]
        public async Task GetAllCoursesForEnrollmentAsync_ReturnsAllCourses()
        {
            // Arrange
            var courses = new List<Course>
            {
                new Course { Id = 1, CourseName = "Data Structures", CourseCode = "CS101" },
                new Course { Id = 2, CourseName = "Algorithms", CourseCode = "CS102" }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(courses);
            _mockContext.Setup(c => c.Courses).Returns(mockSet.Object);

            // Act
            var result = await _service.GetAllCoursesForEnrollmentAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        #endregion

        #region AuthenticateStudentAsync Tests

        [Fact]
        public async Task AuthenticateStudentAsync_WithValidCredentials_ReturnsStudent()
        {
            // Arrange
            var regno = "STU001";
            var password = "password123";
            var students = new List<Student>
            {
                new Student 
                { 
                    StudentRegno = regno, 
                    Password = password,
                    StudentName = "John Doe"
                }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(students);
            _mockContext.Setup(c => c.Students).Returns(mockSet.Object);

            // Act
            var result = await _service.AuthenticateStudentAsync(regno, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(regno, result.StudentRegno);
        }

        [Fact]
        public async Task AuthenticateStudentAsync_WithInvalidCredentials_ReturnsNull()
        {
            // Arrange
            var regno = "STU001";
            var password = "wrongpassword";
            var students = new List<Student>
            {
                new Student { StudentRegno = regno, Password = "correctpassword" }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(students);
            _mockContext.Setup(c => c.Students).Returns(mockSet.Object);

            // Act
            var result = await _service.AuthenticateStudentAsync(regno, password);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(null, "password")]
        [InlineData("", "password")]
        [InlineData("   ", "password")]
        public async Task AuthenticateStudentAsync_WithInvalidRegno_ReturnsNull(string invalidRegno, string password)
        {
            // Act
            var result = await _service.AuthenticateStudentAsync(invalidRegno, password);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("STU001", null)]
        [InlineData("STU001", "")]
        [InlineData("STU001", "   ")]
        public async Task AuthenticateStudentAsync_WithInvalidPassword_ReturnsNull(string regno, string invalidPassword)
        {
            // Act
            var result = await _service.AuthenticateStudentAsync(regno, invalidPassword);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region CreateUserLoginLogAsync Tests

        [Fact]
        public async Task CreateUserLoginLogAsync_WithValidData_CreatesLog()
        {
            // Arrange
            var studentRegno = "STU001";
            var userIp = new byte[] { 192, 168, 1, 1 };
            var status = 1;
            var userlogs = new List<Userlog>().AsQueryable();

            var mockSet = CreateMockDbSet(userlogs);
            _mockContext.Setup(c => c.Userlogs).Returns(mockSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _service.CreateUserLoginLogAsync(studentRegno, userIp, status);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(studentRegno, result.StudentRegno);
            Assert.Equal(userIp, result.Userip);
            Assert.Equal(status, result.Status);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateUserLoginLogAsync_WithInvalidStudentRegno_ThrowsArgumentException(string invalidRegno)
        {
            // Arrange
            var userIp = new byte[] { 192, 168, 1, 1 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateUserLoginLogAsync(invalidRegno, userIp, 1));
        }

        [Fact]
        public async Task CreateUserLoginLogAsync_WithNullUserIp_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateUserLoginLogAsync("STU001", null, 1));
        }

        [Fact]
        public async Task CreateUserLoginLogAsync_WithEmptyUserIp_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateUserLoginLogAsync("STU001", new byte[0], 1));
        }

        #endregion

        #region UpdateUserlogLogoutAsync Tests

        [Fact]
        public async Task UpdateUserlogLogoutAsync_WithValidData_UpdatesLogout()
        {
            // Arrange
            var studentRegno = "STU001";
            var logoutDate = DateTime.UtcNow;
            var userlog = new Userlog 
            { 
                StudentRegno = studentRegno, 
                LoginTime = DateTime.UtcNow.AddHours(-1),
                Logout = null
            };
            var userlogs = new List<Userlog> { userlog }.AsQueryable();

            var mockSet = CreateMockDbSet(userlogs);
            _mockContext.Setup(c => c.Userlogs).Returns(mockSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _service.UpdateUserlogLogoutAsync(studentRegno, logoutDate);

            // Assert
            Assert.True(result);
            Assert.NotNull(userlog.Logout);
        }

        [Fact]
        public async Task UpdateUserlogLogoutAsync_WithNoActiveLog_ReturnsFalse()
        {
            // Arrange
            var studentRegno = "STU001";
            var logoutDate = DateTime.UtcNow;
            var userlogs = new List<Userlog>().AsQueryable();

            var mockSet = CreateMockDbSet(userlogs);
            _mockContext.Setup(c => c.Userlogs).Returns(mockSet.Object);

            // Act
            var result = await _service.UpdateUserlogLogoutAsync(studentRegno, logoutDate);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateUserlogLogoutAsync_WithInvalidStudentRegno_ReturnsFalse(string invalidRegno)
        {
            // Act
            var result = await _service.UpdateUserlogLogoutAsync(invalidRegno, DateTime.UtcNow);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region UpdateStudentProfileAsync Tests

        [Fact]
        public async Task UpdateStudentProfileAsync_WithValidData_UpdatesProfile()
        {
            // Arrange
            var studentRegno = "STU001";
            var studentName = "John Updated";
            var studentPhoto = "photo.jpg";
            var cgpa = 3.8m;
            var student = new Student 
            { 
                StudentRegno = studentRegno,
                StudentName = "John Doe",
                StudentPhoto = "old.jpg",
                Cgpa = 3.5m
            };
            var students = new List<Student> { student }.AsQueryable();

            var mockSet = CreateMockDbSet(students);
            _mockContext.Setup(c => c.Students).Returns(mockSet.Object);
            _mockContext.Setup(c => c.Students.FindAsync(studentRegno)).ReturnsAsync(student);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _service.UpdateStudentProfileAsync(studentRegno, studentName, studentPhoto, cgpa);

            // Assert
            Assert.True(result);
            Assert.Equal(studentName, student.StudentName);
            Assert.Equal(studentPhoto, student.StudentPhoto);
            Assert.Equal(cgpa, student.Cgpa);
            Assert.NotNull(student.UpdationDate);
        }

        [Fact]
        public async Task UpdateStudentProfileAsync_WithNonExistentStudent_ReturnsFalse()
        {
            // Arrange
            var studentRegno = "NONEXISTENT";
            _mockContext.Setup(c => c.Students.FindAsync(studentRegno)).ReturnsAsync((Student)null);

            // Act
            var result = await _service.UpdateStudentProfileAsync(studentRegno, "Name", "photo.jpg", 3.5m);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateStudentProfileAsync_WithInvalidStudentRegno_ReturnsFalse(string invalidRegno)
        {
            // Act
            var result = await _service.UpdateStudentProfileAsync(invalidRegno, "Name", "photo.jpg", 3.5m);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetStudentByPincodeAsync Tests

        [Fact]
        public async Task GetStudentByPincodeAsync_WithValidPincode_ReturnsStudent()
        {
            // Arrange
            var pincode = "1234";
            var students = new List<Student>
            {
                new Student 
                { 
                    StudentRegno = "STU001",
                    Pincode = pincode,
                    StudentName = "John Doe"
                }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(students);
            _mockContext.Setup(c => c.Students).Returns(mockSet.Object);

            // Act
            var result = await _service.GetStudentByPincodeAsync(pincode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(pincode, result.Pincode);
        }

        [Fact]
        public async Task GetStudentByPincodeAsync_WithNonExistentPincode_ReturnsNull()
        {
            // Arrange
            var pincode = "9999";
            var students = new List<Student>().AsQueryable();

            var mockSet = CreateMockDbSet(students);
            _mockContext.Setup(c => c.Students).Returns(mockSet.Object);

            // Act
            var result = await _service.GetStudentByPincodeAsync(pincode);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetStudentByPincodeAsync_WithInvalidPincode_ReturnsNull(string invalidPincode)
        {
            // Act
            var result = await _service.GetStudentByPincodeAsync(invalidPincode);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetPrintableCourseEnrollmentDetailsAsync Tests

        [Fact]
        public async Task GetPrintableCourseEnrollmentDetailsAsync_WithValidData_ReturnsDetails()
        {
            // Arrange
            var studentRegno = "STU001";
            var enrollments = new List<Courseenroll>
            {
                new Courseenroll 
                { 
                    StudentRegno = studentRegno,
                    Course = 1,
                    Session = 1,
                    Department = 1,
                    Level = 1,
                    Semester = 1,
                    EnrollDate = DateTime.UtcNow
                }
            }.AsQueryable();
            var courses = new List<Course>
            {
                new Course { Id = 1, CourseName = "Data Structures", CourseCode = "CS101", CourseUnit = "3" }
            }.AsQueryable();
            var sessions = new List<Session>
            {
                new Session { Id = 1, SessionName = "2023-2024" }
            }.AsQueryable();
            var departments = new List<Department>
            {
                new Department { Id = 1, DepartmentName = "Computer Science" }
            }.AsQueryable();
            var levels = new List<Level>
            {
                new Level { Id = 1, LevelName = "100 Level" }
            }.AsQueryable();
            var students = new List<Student>
            {
                new Student 
                { 
                    StudentRegno = studentRegno,
                    StudentName = "John Doe",
                    StudentPhoto = "photo.jpg",
                    Cgpa = 3.5m,
                    Creationdate = DateTime.UtcNow
                }
            }.AsQueryable();
            var semesters = new List<Semester>
            {
                new Semester { Id = 1, SemesterName = "First Semester" }
            }.AsQueryable();

            _mockContext.Setup(c => c.Courseenrolls).Returns(CreateMockDbSet(enrollments).Object);
            _mockContext.Setup(c => c.Courses).Returns(CreateMockDbSet(courses).Object);
            _mockContext.Setup(c => c.Sessions).Returns(CreateMockDbSet(sessions).Object);
            _mockContext.Setup(c => c.Departments).Returns(CreateMockDbSet(departments).Object);
            _mockContext.Setup(c => c.Levels).Returns(CreateMockDbSet(levels).Object);
            _mockContext.Setup(c => c.Students).Returns(CreateMockDbSet(students).Object);
            _mockContext.Setup(c => c.Semesters).Returns(CreateMockDbSet(semesters).Object);

            // Act
            var result = await _service.GetPrintableCourseEnrollmentDetailsAsync(studentRegno);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Data Structures", result.CourseName);
            Assert.Equal("John Doe", result.StudentName);
        }

        [Fact]
        public async Task GetPrintableCourseEnrollmentDetailsAsync_WithNoEnrollment_ReturnsNull()
        {
            // Arrange
            var studentRegno = "STU001";
            var enrollments = new List<Courseenroll>().AsQueryable();

            _mockContext.Setup(c => c.Courseenrolls).Returns(CreateMockDbSet(enrollments).Object);
            _mockContext.Setup(c => c.Courses).Returns(CreateMockDbSet(new List<Course>().AsQueryable()).Object);
            _mockContext.Setup(c => c.Sessions).Returns(CreateMockDbSet(new List<Session>().AsQueryable()).Object);
            _mockContext.Setup(c => c.Departments).Returns(CreateMockDbSet(new List<Department>().AsQueryable()).Object);
            _mockContext.Setup(c => c.Levels).Returns(CreateMockDbSet(new List<Level>().AsQueryable()).Object);
            _mockContext.Setup(c => c.Students).Returns(CreateMockDbSet(new List<Student>().AsQueryable()).Object);
            _mockContext.Setup(c => c.Semesters).Returns(CreateMockDbSet(new List<Semester>().AsQueryable()).Object);

            // Act
            var result = await _service.GetPrintableCourseEnrollmentDetailsAsync(studentRegno);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetPrintableCourseEnrollmentDetailsAsync_WithInvalidStudentRegno_ReturnsNull(string invalidRegno)
        {
            // Act
            var result = await _service.GetPrintableCourseEnrollmentDetailsAsync(invalidRegno);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region Helper Methods

        private Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
            mockSet.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(data.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }

        #endregion
    }

    
}
