using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernApiProject.Application.Interfaces;
using ModernApiProject.Application.Models;
using ModernApiProject.WebApi.Controllers;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace ModernApiProject.WebApi.Tests.Controllers
{
    public class StandaloneControllerTests
    {
        private readonly Mock<IStandaloneService> _mockStandaloneService;
        private readonly Mock<ILogger<StandaloneController>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly StandaloneController _controller;

        public StandaloneControllerTests()
        {
            _mockStandaloneService = new Mock<IStandaloneService>();
            _mockLogger = new Mock<ILogger<StandaloneController>>();
            _mockConfig = new Mock<IConfiguration>(); // <-- ADD THIS
            // Setup mock config to return a fake secret key for tests
            _mockConfig.Setup(c => c["Jwt:SecretKey"]).Returns("this-is-a-test-secret-key-that-is-long-enough");

            // _controller = new StandaloneController(_mockStandaloneService.Object, _mockLogger.Object);
            // PASS IT TO THE CONTROLLER
            _controller = new StandaloneController(_mockStandaloneService.Object, _mockLogger.Object, _mockConfig.Object);
            
            // Setup HttpContext for IP address
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        #region ValidateStudentPassword Tests

        [Fact]
        public async Task ValidateStudentPassword_WithValidCredentials_ReturnsOkWithTrue()
        {
            // Arrange
            var studentRegno = "STU001";
            var password = "password123";
            _mockStandaloneService.Setup(s => s.ValidateStudentPasswordAsync(studentRegno, password))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.ValidateStudentPassword(studentRegno, password);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var isValidProperty = value.GetType().GetProperty("isValid");
            Assert.NotNull(isValidProperty);
            Assert.True((bool)isValidProperty.GetValue(value));
        }

        [Fact]
        public async Task ValidateStudentPassword_WithInvalidCredentials_ReturnsOkWithFalse()
        {
            // Arrange
            var studentRegno = "STU001";
            var password = "wrongpassword";
            _mockStandaloneService.Setup(s => s.ValidateStudentPasswordAsync(studentRegno, password))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.ValidateStudentPassword(studentRegno, password);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var isValidProperty = value.GetType().GetProperty("isValid");
            Assert.NotNull(isValidProperty);
            Assert.False((bool)isValidProperty.GetValue(value));
        }

        [Theory]
        [InlineData(null, "password")]
        [InlineData("", "password")]
        [InlineData("   ", "password")][InlineData("STU001", null)]
        [InlineData("STU001", "")][InlineData("STU001", "   ")]
        public async Task ValidateStudentPassword_WithInvalidInput_ReturnsBadRequest(string studentRegno, string password)
        {
            // Act
            var result = await _controller.ValidateStudentPassword(studentRegno, password);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Student registration number and password are required.", badRequestResult.Value);
        }

        [Fact]
        public async Task ValidateStudentPassword_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var studentRegno = "STU001";
            var password = "password123";
            _mockStandaloneService.Setup(s => s.ValidateStudentPasswordAsync(studentRegno, password))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.ValidateStudentPassword(studentRegno, password);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while validating the password.", statusCodeResult.Value);
        }

        #endregion

        #region UpdateStudentPassword Tests[Fact]
        public async Task UpdateStudentPassword_WithValidData_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            var studentRegno = "STU001";
            var newPassword = "newpassword123";
            _mockStandaloneService.Setup(s => s.UpdateStudentPasswordAsync(studentRegno, newPassword))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateStudentPassword(studentRegno, newPassword);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal("Password updated successfully.", messageProperty.GetValue(value));
        }

        [Fact]
        public async Task UpdateStudentPassword_WhenStudentNotFound_ReturnsNotFound()
        {
            // Arrange
            var studentRegno = "STU999";
            var newPassword = "newpassword123";
            _mockStandaloneService.Setup(s => s.UpdateStudentPasswordAsync(studentRegno, newPassword))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateStudentPassword(studentRegno, newPassword);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Student not found or password update failed.", notFoundResult.Value);
        }

        [Theory]
        [InlineData(null, "password")]
        [InlineData("", "password")]
        [InlineData("   ", "password")][InlineData("STU001", null)]
        [InlineData("STU001", "")]
        [InlineData("STU001", "   ")]
        public async Task UpdateStudentPassword_WithInvalidInput_ReturnsBadRequest(string studentRegno, string newPassword)
        {
            // Act
            var result = await _controller.UpdateStudentPassword(studentRegno, newPassword);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Student registration number and new password are required.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateStudentPassword_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var studentRegno = "STU001";
            var newPassword = "newpassword123";
            _mockStandaloneService.Setup(s => s.UpdateStudentPasswordAsync(studentRegno, newPassword))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdateStudentPassword(studentRegno, newPassword);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while updating the password.", statusCodeResult.Value);
        }

        #endregion

        #region CheckStudentEnrollment Tests

        [Fact]
        public async Task CheckStudentEnrollment_WhenEnrolled_ReturnsOkWithTrue()
        {
            // Arrange
            var studentRegno = "STU001";
            var courseId = 1;
            _mockStandaloneService.Setup(s => s.IsStudentEnrolledInCourseAsync(studentRegno, courseId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.CheckStudentEnrollment(studentRegno, courseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var isEnrolledProperty = value.GetType().GetProperty("isEnrolled");
            Assert.NotNull(isEnrolledProperty);
            Assert.True((bool)isEnrolledProperty.GetValue(value));
        }

        [Fact]
        public async Task CheckStudentEnrollment_WhenNotEnrolled_ReturnsOkWithFalse()
        {
            // Arrange
            var studentRegno = "STU001";
            var courseId = 1;
            _mockStandaloneService.Setup(s => s.IsStudentEnrolledInCourseAsync(studentRegno, courseId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.CheckStudentEnrollment(studentRegno, courseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var isEnrolledProperty = value.GetType().GetProperty("isEnrolled");
            Assert.NotNull(isEnrolledProperty);
            Assert.False((bool)isEnrolledProperty.GetValue(value));
        }

        [Theory]
        [InlineData(null, 1)]
        [InlineData("", 1)]
        [InlineData("   ", 1)][InlineData("STU001", 0)]
        [InlineData("STU001", -1)]
        public async Task CheckStudentEnrollment_WithInvalidInput_ReturnsBadRequest(string studentRegno, int courseId)
        {
            // Act
            var result = await _controller.CheckStudentEnrollment(studentRegno, courseId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Valid student registration number and course ID are required.", badRequestResult.Value);
        }[Fact]
        public async Task CheckStudentEnrollment_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var studentRegno = "STU001";
            var courseId = 1;
            _mockStandaloneService.Setup(s => s.IsStudentEnrolledInCourseAsync(studentRegno, courseId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CheckStudentEnrollment(studentRegno, courseId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while checking enrollment.", statusCodeResult.Value);
        }

        #endregion

        #region GetCourseEnrollmentCount Tests

        [Fact]
        public async Task GetCourseEnrollmentCount_WithValidCourseId_ReturnsOkWithCount()
        {
            // Arrange
            var courseId = 1;
            var expectedCount = 25;
            _mockStandaloneService.Setup(s => s.GetCourseEnrollmentCountAsync(courseId))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _controller.GetCourseEnrollmentCount(courseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var courseIdProperty = value.GetType().GetProperty("courseId");
            var enrollmentCountProperty = value.GetType().GetProperty("enrollmentCount");
            Assert.NotNull(courseIdProperty);
            Assert.NotNull(enrollmentCountProperty);
            Assert.Equal(courseId, courseIdProperty.GetValue(value));
            Assert.Equal(expectedCount, enrollmentCountProperty.GetValue(value));
        }

        [Theory]
        [InlineData(0)][InlineData(-1)]
        public async Task GetCourseEnrollmentCount_WithInvalidCourseId_ReturnsBadRequest(int courseId)
        {
            // Act
            var result = await _controller.GetCourseEnrollmentCount(courseId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Valid course ID is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetCourseEnrollmentCount_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var courseId = 1;
            _mockStandaloneService.Setup(s => s.GetCourseEnrollmentCountAsync(courseId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetCourseEnrollmentCount(courseId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while retrieving enrollment count.", statusCodeResult.Value);
        }

        #endregion

        #region GetCourseAvailableSeats Tests

        [Fact]
        public async Task GetCourseAvailableSeats_WithValidCourseId_ReturnsOkWithSeats()
        {
            // Arrange
            var courseId = 1;
            var expectedSeats = 15;
            _mockStandaloneService.Setup(s => s.GetCourseAvailableSeatsAsync(courseId))
                .ReturnsAsync(expectedSeats);

            // Act
            var result = await _controller.GetCourseAvailableSeats(courseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var courseIdProperty = value.GetType().GetProperty("courseId");
            var availableSeatsProperty = value.GetType().GetProperty("availableSeats");
            Assert.NotNull(courseIdProperty);
            Assert.NotNull(availableSeatsProperty);
            Assert.Equal(courseId, courseIdProperty.GetValue(value));
            Assert.Equal(expectedSeats, availableSeatsProperty.GetValue(value));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetCourseAvailableSeats_WithInvalidCourseId_ReturnsBadRequest(int courseId)
        {
            // Act
            var result = await _controller.GetCourseAvailableSeats(courseId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Valid course ID is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetCourseAvailableSeats_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var courseId = 1;
            _mockStandaloneService.Setup(s => s.GetCourseAvailableSeatsAsync(courseId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetCourseAvailableSeats(courseId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while retrieving available seats.", statusCodeResult.Value);
        }

        #endregion

        #region GetEnrollmentHistory Tests

        [Fact]
        public async Task GetEnrollmentHistory_WithValidRegno_ReturnsOkWithHistory()
        {
            // Arrange
            var studentRegno = "STU001";
            var expectedHistory = new List<CourseenrollModel> { new CourseenrollModel { Course = 1 } };
            _mockStandaloneService.Setup(s => s.GetEnrollmentHistoryByStudentAsync(studentRegno))
                .ReturnsAsync(expectedHistory);

            // Act
            var result = await _controller.GetEnrollmentHistory(studentRegno);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedHistory, okResult.Value);
        }[Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetEnrollmentHistory_WithInvalidRegno_ReturnsBadRequest(string studentRegno)
        {
            // Act
            var result = await _controller.GetEnrollmentHistory(studentRegno);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Student registration number is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetEnrollmentHistory_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var studentRegno = "STU001";
            _mockStandaloneService.Setup(s => s.GetEnrollmentHistoryByStudentAsync(studentRegno))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetEnrollmentHistory(studentRegno);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while retrieving enrollment history.", statusCodeResult.Value);
        }

        #endregion

        #region CreateEnrollment Tests

        [Fact]
        public async Task CreateEnrollment_WithValidData_ReturnsOkWithEnrollment()
        {
            // Arrange
            var studentRegno = "STU001";
            var pincode = "1234";
            var sessionId = 1;
            var departmentId = 1;
            var levelId = 1;
            var courseId = 1;
            var semesterId = 1;
            var expectedEnrollment = new CourseenrollModel { Id = 1, StudentRegno = studentRegno };
            _mockStandaloneService.Setup(s => s.CreateEnrollmentAsync(studentRegno, pincode, sessionId, departmentId, levelId, courseId, semesterId))
                .ReturnsAsync(expectedEnrollment);

            // Act
            var result = await _controller.CreateEnrollment(studentRegno, pincode, sessionId, departmentId, levelId, courseId, semesterId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedEnrollment, okResult.Value);
        }

        [Fact]
        public async Task CreateEnrollment_WhenEnrollmentFails_ReturnsBadRequest()
        {
            // Arrange
            var studentRegno = "STU001";
            var pincode = "1234";
            var sessionId = 1;
            var departmentId = 1;
            var levelId = 1;
            var courseId = 1;
            var semesterId = 1;
            _mockStandaloneService.Setup(s => s.CreateEnrollmentAsync(studentRegno, pincode, sessionId, departmentId, levelId, courseId, semesterId))
                .ReturnsAsync((CourseenrollModel)null);

            // Act
            var result = await _controller.CreateEnrollment(studentRegno, pincode, sessionId, departmentId, levelId, courseId, semesterId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Enrollment failed. Please check the provided information.", badRequestResult.Value);
        }

        [Theory][InlineData(null, "1234", 1, 1, 1, 1, 1)][InlineData("", "1234", 1, 1, 1, 1, 1)][InlineData("   ", "1234", 1, 1, 1, 1, 1)][InlineData("STU001", null, 1, 1, 1, 1, 1)][InlineData("STU001", "", 1, 1, 1, 1, 1)][InlineData("STU001", "   ", 1, 1, 1, 1, 1)][InlineData("STU001", "1234", 0, 1, 1, 1, 1)][InlineData("STU001", "1234", 1, 0, 1, 1, 1)][InlineData("STU001", "1234", 1, 1, 0, 1, 1)][InlineData("STU001", "1234", 1, 1, 1, 0, 1)][InlineData("STU001", "1234", 1, 1, 1, 1, 0)]
        public async Task CreateEnrollment_WithInvalidInput_ReturnsBadRequest(string studentRegno, string pincode, int sessionId, int departmentId, int levelId, int courseId, int semesterId)
        {
            // Act
            var result = await _controller.CreateEnrollment(studentRegno, pincode, sessionId, departmentId, levelId, courseId, semesterId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("All enrollment parameters are required and must be valid.", badRequestResult.Value);
        }[Fact]
        public async Task CreateEnrollment_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var studentRegno = "STU001";
            var pincode = "1234";
            var sessionId = 1;
            var departmentId = 1;
            var levelId = 1;
            var courseId = 1;
            var semesterId = 1;
            _mockStandaloneService.Setup(s => s.CreateEnrollmentAsync(studentRegno, pincode, sessionId, departmentId, levelId, courseId, semesterId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateEnrollment(studentRegno, pincode, sessionId, departmentId, levelId, courseId, semesterId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while creating the enrollment.", statusCodeResult.Value);
        }

        #endregion

        #region GetStudentByRegno Tests

        [Fact]
        public async Task GetStudentByRegno_WithValidRegno_ReturnsOkWithStudent()
        {
            // Arrange
            var studentRegno = "STU001";
            var expectedStudent = new StudentModel { StudentRegno = studentRegno, StudentName = "John Doe" };
            _mockStandaloneService.Setup(s => s.GetStudentByRegnoAsync(studentRegno))
                .ReturnsAsync(expectedStudent);

            // Act
            var result = await _controller.GetStudentByRegno(studentRegno);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedStudent, okResult.Value);
        }

        [Fact]
        public async Task GetStudentByRegno_WhenStudentNotFound_ReturnsNotFound()
        {
            // Arrange
            var studentRegno = "STU999";
            _mockStandaloneService.Setup(s => s.GetStudentByRegnoAsync(studentRegno))
                .ReturnsAsync((StudentModel)null);

            // Act
            var result = await _controller.GetStudentByRegno(studentRegno);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Student not found.", notFoundResult.Value);
        }

        [Theory][InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetStudentByRegno_WithInvalidRegno_ReturnsBadRequest(string studentRegno)
        {
            // Act
            var result = await _controller.GetStudentByRegno(studentRegno);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Student registration number is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetStudentByRegno_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var studentRegno = "STU001";
            _mockStandaloneService.Setup(s => s.GetStudentByRegnoAsync(studentRegno))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetStudentByRegno(studentRegno);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while retrieving student information.", statusCodeResult.Value);
        }

        #endregion

        #region GetAllSessions Tests

        [Fact]
        public async Task GetAllSessions_ReturnsOkWithSessions()
        {
            // Arrange
            var expectedSessions = new List<SessionModel> { new SessionModel { Id = 1, SessionName = "2023/2024" } };
            _mockStandaloneService.Setup(s => s.GetAllSessionsAsync())
                .ReturnsAsync(expectedSessions);

            // Act
            var result = await _controller.GetAllSessions();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedSessions, okResult.Value);
        }

        [Fact]
        public async Task GetAllSessions_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockStandaloneService.Setup(s => s.GetAllSessionsAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAllSessions();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while retrieving sessions.", statusCodeResult.Value);
        }

        #endregion

        #region GetAllDepartments Tests

        [Fact]
        public async Task GetAllDepartments_ReturnsOkWithDepartments()
        {
            // Arrange
            var expectedDepartments = new List<DepartmentModel> { new DepartmentModel { Id = 1, DepartmentName = "Computer Science" } };
            _mockStandaloneService.Setup(s => s.GetAllDepartmentsAsync())
                .ReturnsAsync(expectedDepartments);

            // Act
            var result = await _controller.GetAllDepartments();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedDepartments, okResult.Value);
        }

        [Fact]
        public async Task GetAllDepartments_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockStandaloneService.Setup(s => s.GetAllDepartmentsAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAllDepartments();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while retrieving departments.", statusCodeResult.Value);
        }

        #endregion

        #region GetAllLevels Tests[Fact]
        public async Task GetAllLevels_ReturnsOkWithLevels()
        {
            // Arrange
            var expectedLevels = new List<LevelModel> { new LevelModel { Id = 1, LevelName = "100 Level" } };
            _mockStandaloneService.Setup(s => s.GetAllLevelsForEnrollmentAsync())
                .ReturnsAsync(expectedLevels);

            // Act
            var result = await _controller.GetAllLevels();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedLevels, okResult.Value);
        }

        [Fact]
        public async Task GetAllLevels_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockStandaloneService.Setup(s => s.GetAllLevelsForEnrollmentAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAllLevels();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while retrieving levels.", statusCodeResult.Value);
        }

        #endregion

        #region GetAllSemesters Tests

        [Fact]
        public async Task GetAllSemesters_ReturnsOkWithSemesters()
        {
            // Arrange
            var expectedSemesters = new List<SemesterModel> { new SemesterModel { Id = 1, SemesterName = "First Semester" } };
            _mockStandaloneService.Setup(s => s.GetAllSemestersForEnrollmentAsync())
                .ReturnsAsync(expectedSemesters);

            // Act
            var result = await _controller.GetAllSemesters();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedSemesters, okResult.Value);
        }[Fact]
        public async Task GetAllSemesters_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockStandaloneService.Setup(s => s.GetAllSemestersForEnrollmentAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAllSemesters();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while retrieving semesters.", statusCodeResult.Value);
        }

        #endregion

        #region GetAllCourses Tests

        [Fact]
        public async Task GetAllCourses_ReturnsOkWithCourses()
        {
            // Arrange
            var expectedCourses = new List<CourseModel> { new CourseModel { Id = 1, CourseName = "Mathematics" } };
            _mockStandaloneService.Setup(s => s.GetAllCoursesForEnrollmentAsync())
                .ReturnsAsync(expectedCourses);

            // Act
            var result = await _controller.GetAllCourses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedCourses, okResult.Value);
        }

        [Fact]
        public async Task GetAllCourses_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockStandaloneService.Setup(s => s.GetAllCoursesForEnrollmentAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAllCourses();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while retrieving courses.", statusCodeResult.Value);
        }

        #endregion

        #region AuthenticateStudent Tests

        [Fact]
        public async Task AuthenticateStudent_WithValidCredentials_ReturnsOkWithStudent()
        {
            // Arrange
            var regno = "STU001";
            var password = "password123";
            var expectedStudent = new StudentModel { StudentRegno = regno, StudentName = "John Doe" };
            _mockStandaloneService.Setup(s => s.AuthenticateStudentAsync(regno, password))
                .ReturnsAsync(expectedStudent);

            // Act
            var result = await _controller.AuthenticateStudent(regno, password);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedStudent, okResult.Value);
        }

        [Fact]
        public async Task AuthenticateStudent_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var regno = "STU001";
            var password = "wrongpassword";
            _mockStandaloneService.Setup(s => s.AuthenticateStudentAsync(regno, password))
                .ReturnsAsync((StudentModel)null);

            // Act
            var result = await _controller.AuthenticateStudent(regno, password);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid credentials.", unauthorizedResult.Value);
        }

        [Theory][InlineData(null, "password")]
        [InlineData("", "password")][InlineData("   ", "password")]
        [InlineData("STU001", null)]
        [InlineData("STU001", "")]
        [InlineData("STU001", "   ")]
        public async Task AuthenticateStudent_WithInvalidInput_ReturnsBadRequest(string regno, string password)
        {
            // Act
            var result = await _controller.AuthenticateStudent(regno, password);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Registration number and password are required.", badRequestResult.Value);
        }

        [Fact]
        public async Task AuthenticateStudent_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var regno = "STU001";
            var password = "password123";
            _mockStandaloneService.Setup(s => s.AuthenticateStudentAsync(regno, password))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.AuthenticateStudent(regno, password);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred during authentication.", statusCodeResult.Value);
        }

        #endregion

        #region CreateUserLoginLog Tests

        [Fact]
        public async Task CreateUserLoginLog_WithValidData_ReturnsOkWithLog()
        {
            // Arrange
            var studentRegno = "STU001";
            var status = 1;
            var expectedLog = new UserlogModel { Id = 1, StudentRegno = studentRegno };
            _mockStandaloneService.Setup(s => s.CreateUserLoginLogAsync(studentRegno, It.IsAny<byte[]>(), status))
                .ReturnsAsync(expectedLog);

            // Act
            var result = await _controller.CreateUserLoginLog(studentRegno, status);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedLog, okResult.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")][InlineData("   ")]
        public async Task CreateUserLoginLog_WithInvalidRegno_ReturnsBadRequest(string studentRegno)
        {
            // Act
            var result = await _controller.CreateUserLoginLog(studentRegno, 1);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Student registration number is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateUserLoginLog_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var studentRegno = "STU001";
            var status = 1;
            _mockStandaloneService.Setup(s => s.CreateUserLoginLogAsync(studentRegno, It.IsAny<byte[]>(), status))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateUserLoginLog(studentRegno, status);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while creating the login log.", statusCodeResult.Value);
        }

        #endregion

        #region UpdateUserlogLogout Tests

        [Fact]
        public async Task UpdateUserlogLogout_WithValidRegno_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            var studentRegno = "STU001";
            _mockStandaloneService.Setup(s => s.UpdateUserlogLogoutAsync(studentRegno, It.IsAny<DateTime>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateUserlogLogout(studentRegno);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal("Logout recorded successfully.", messageProperty.GetValue(value));
        }

        [Fact]
        public async Task UpdateUserlogLogout_WhenLogNotFound_ReturnsNotFound()
        {
            // Arrange
            var studentRegno = "STU999";
            _mockStandaloneService.Setup(s => s.UpdateUserlogLogoutAsync(studentRegno, It.IsAny<DateTime>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateUserlogLogout(studentRegno);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User log not found or logout update failed.", notFoundResult.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")][InlineData("   ")]
        public async Task UpdateUserlogLogout_WithInvalidRegno_ReturnsBadRequest(string studentRegno)
        {
            // Act
            var result = await _controller.UpdateUserlogLogout(studentRegno);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Student registration number is required.", badRequestResult.Value);
        }[Fact]
        public async Task UpdateUserlogLogout_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var studentRegno = "STU001";
            _mockStandaloneService.Setup(s => s.UpdateUserlogLogoutAsync(studentRegno, It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdateUserlogLogout(studentRegno);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while recording logout.", statusCodeResult.Value);
        }

        #endregion

        #region UpdateStudentProfile Tests

        [Fact]
        public async Task UpdateStudentProfile_WithValidData_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            var studentRegno = "STU001";
            var studentName = "John Doe";
            var studentPhoto = "photo.jpg";
            var cgpa = 3.5m;
            _mockStandaloneService.Setup(s => s.UpdateStudentProfileAsync(studentRegno, studentName, studentPhoto, cgpa))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateStudentProfile(studentRegno, studentName, studentPhoto, cgpa);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal("Profile updated successfully.", messageProperty.GetValue(value));
        }

        [Fact]
        public async Task UpdateStudentProfile_WhenStudentNotFound_ReturnsNotFound()
        {
            // Arrange
            var studentRegno = "STU999";
            var studentName = "John Doe";
            var studentPhoto = "photo.jpg";
            var cgpa = 3.5m;
            _mockStandaloneService.Setup(s => s.UpdateStudentProfileAsync(studentRegno, studentName, studentPhoto, cgpa))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateStudentProfile(studentRegno, studentName, studentPhoto, cgpa);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Student not found or profile update failed.", notFoundResult.Value);
        }

        [Theory][InlineData(null, "John Doe")]
        [InlineData("", "John Doe")][InlineData("   ", "John Doe")]
        [InlineData("STU001", null)][InlineData("STU001", "")]
        [InlineData("STU001", "   ")]
        public async Task UpdateStudentProfile_WithInvalidInput_ReturnsBadRequest(string studentRegno, string studentName)
        {
            // Act
            var result = await _controller.UpdateStudentProfile(studentRegno, studentName, "photo.jpg", 3.5m);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Student registration number and name are required.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateStudentProfile_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var studentRegno = "STU001";
            var studentName = "John Doe";
            var studentPhoto = "photo.jpg";
            var cgpa = 3.5m;
            _mockStandaloneService.Setup(s => s.UpdateStudentProfileAsync(studentRegno, studentName, studentPhoto, cgpa))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdateStudentProfile(studentRegno, studentName, studentPhoto, cgpa);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while updating the profile.", statusCodeResult.Value);
        }

        #endregion

        #region GetStudentByPincode Tests

        [Fact]
        public async Task GetStudentByPincode_WithValidPincode_ReturnsOkWithStudent()
        {
            // Arrange
            var pincode = "1234";
            var expectedStudent = new StudentModel { StudentRegno = "STU001", StudentName = "John Doe" };
            _mockStandaloneService.Setup(s => s.GetStudentByPincodeAsync(pincode))
                .ReturnsAsync(expectedStudent);

            // Act
            var result = await _controller.GetStudentByPincode(pincode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedStudent, okResult.Value);
        }

        [Fact]
        public async Task GetStudentByPincode_WhenStudentNotFound_ReturnsNotFound()
        {
            // Arrange
            var pincode = "9999";
            _mockStandaloneService.Setup(s => s.GetStudentByPincodeAsync(pincode))
                .ReturnsAsync((StudentModel)null);

            // Act
            var result = await _controller.GetStudentByPincode(pincode);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Student not found with the provided pincode.", notFoundResult.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetStudentByPincode_WithInvalidPincode_ReturnsBadRequest(string pincode)
        {
            // Act
            var result = await _controller.GetStudentByPincode(pincode);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Pincode is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetStudentByPincode_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var pincode = "1234";
            _mockStandaloneService.Setup(s => s.GetStudentByPincodeAsync(pincode))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetStudentByPincode(pincode);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while verifying the pincode.", statusCodeResult.Value);
        }

        #endregion

        #region GetPrintableCourseEnrollmentDetails Tests

        [Fact]
        public async Task GetPrintableCourseEnrollmentDetails_WithValidRegno_ReturnsOkWithDetails()
        {
            // Arrange
            var studentRegno = "STU001";
            var expectedDetails = new CourseEnrollmentDetailModel { StudentName = "John Doe", CourseName = "Math" };
            _mockStandaloneService.Setup(s => s.GetPrintableCourseEnrollmentDetailsAsync(studentRegno))
                .ReturnsAsync(expectedDetails);

            // Act
            var result = await _controller.GetPrintableCourseEnrollmentDetails(studentRegno);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedDetails, okResult.Value);
        }

        [Fact]
        public async Task GetPrintableCourseEnrollmentDetails_WhenDetailsNotFound_ReturnsNotFound()
        {
            // Arrange
            var studentRegno = "STU999";
            _mockStandaloneService.Setup(s => s.GetPrintableCourseEnrollmentDetailsAsync(studentRegno))
                .ReturnsAsync((CourseEnrollmentDetailModel)null);

            // Act
            var result = await _controller.GetPrintableCourseEnrollmentDetails(studentRegno);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Enrollment details not found for the student.", notFoundResult.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetPrintableCourseEnrollmentDetails_WithInvalidRegno_ReturnsBadRequest(string studentRegno)
        {
            // Act
            var result = await _controller.GetPrintableCourseEnrollmentDetails(studentRegno);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Student registration number is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPrintableCourseEnrollmentDetails_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var studentRegno = "STU001";
            _mockStandaloneService.Setup(s => s.GetPrintableCourseEnrollmentDetailsAsync(studentRegno))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetPrintableCourseEnrollmentDetails(studentRegno);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An error occurred while retrieving enrollment details.", statusCodeResult.Value);
        }

        #endregion
    }
}