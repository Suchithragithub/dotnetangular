using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ModernApiProject.Application.Models;
using ModernApiProject.Application.Services;
using ModernApiProject.Domain.Entities;
using ModernApiProject.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Query;

namespace ModernApiProject.Application.Tests.Services
{
    public class AdminServiceTests
    {
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly Mock<ILogger<AdminService>> _mockLogger;
        private readonly AdminService _adminService;

        public AdminServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            _mockContext = new Mock<ApplicationDbContext>(options);
            _mockLogger = new Mock<ILogger<AdminService>>();
            
            // Fix: Mock the new repository dependencies
            var mockLevelRepo = new Mock<ModernApiProject.Domain.Repositories.ILevelRepository>();
            var mockStudentRepo = new Mock<ModernApiProject.Domain.Repositories.IStudentRepository>();

            _adminService = new AdminService(
                _mockContext.Object, 
                _mockLogger.Object, 
                mockLevelRepo.Object, 
                mockStudentRepo.Object
            );
        }

        #region ValidateAdminPasswordAsync Tests

        [Fact]
        public async Task ValidateAdminPasswordAsync_WithValidPassword_ReturnsTrue()
        {
            // Arrange
            var password = "validPassword123";
            var admin = new Admin { Id = 1, Password = password };
            var adminList = new List<Admin> { admin }.AsQueryable();

            var mockSet = CreateMockDbSet(adminList);
            _mockContext.Setup(c => c.Admins).Returns(mockSet.Object);

            // Act
            var result = await _adminService.ValidateAdminPasswordAsync(password);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateAdminPasswordAsync_WithInvalidPassword_ReturnsFalse()
        {
            // Arrange
            var password = "invalidPassword";
            var adminList = new List<Admin>().AsQueryable();

            var mockSet = CreateMockDbSet(adminList);
            _mockContext.Setup(c => c.Admins).Returns(mockSet.Object);

            // Act
            var result = await _adminService.ValidateAdminPasswordAsync(password);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ValidateAdminPasswordAsync_WithNullOrEmptyPassword_ThrowsArgumentException(string password)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _adminService.ValidateAdminPasswordAsync(password));
        }

        #endregion

        #region UpdateAdminPasswordAsync Tests

        [Fact]
        public async Task UpdateAdminPasswordAsync_WithValidOldPassword_UpdatesPasswordAndReturnsTrue()
        {
            // Arrange
            var oldPassword = "oldPass123";
            var newPassword = "newPass456";
            var admin = new Admin { Id = 1, Password = oldPassword, UpdationDate = DateTime.UtcNow.AddDays(-1) };
            var adminList = new List<Admin> { admin }.AsQueryable();

            var mockSet = CreateMockDbSet(adminList);
            _mockContext.Setup(c => c.Admins).Returns(mockSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _adminService.UpdateAdminPasswordAsync(oldPassword, newPassword);

            // Assert
            Assert.True(result);
            Assert.Equal(newPassword, admin.Password);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateAdminPasswordAsync_WithInvalidOldPassword_ReturnsFalse()
        {
            // Arrange
            var oldPassword = "wrongPassword";
            var newPassword = "newPass456";
            var adminList = new List<Admin>().AsQueryable();

            var mockSet = CreateMockDbSet(adminList);
            _mockContext.Setup(c => c.Admins).Returns(mockSet.Object);

            // Act
            var result = await _adminService.UpdateAdminPasswordAsync(oldPassword, newPassword);

            // Assert
            Assert.False(result);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
        }

        [Theory]
        [InlineData(null, "newPass")]
        [InlineData("", "newPass")]
        [InlineData("   ", "newPass")]
        public async Task UpdateAdminPasswordAsync_WithNullOrEmptyOldPassword_ThrowsArgumentException(string oldPassword, string newPassword)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _adminService.UpdateAdminPasswordAsync(oldPassword, newPassword));
        }

        [Theory]
        [InlineData("oldPass", null)]
        [InlineData("oldPass", "")]
        [InlineData("oldPass", "   ")]
        public async Task UpdateAdminPasswordAsync_WithNullOrEmptyNewPassword_ThrowsArgumentException(string oldPassword, string newPassword)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _adminService.UpdateAdminPasswordAsync(oldPassword, newPassword));
        }

        #endregion

        #region CheckStudentRegnoAvailabilityAsync Tests

        [Fact]
        public async Task CheckStudentRegnoAvailabilityAsync_WithExistingRegno_ReturnsTrue()
        {
            // Arrange
            var regno = "STU001";
            var student = new Student { StudentRegno = regno };
            var studentList = new List<Student> { student }.AsQueryable();

            var mockSet = CreateMockDbSet(studentList);
            _mockContext.Setup(c => c.Students).Returns(mockSet.Object);

            // Act
            var result = await _adminService.CheckStudentRegnoAvailabilityAsync(regno);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CheckStudentRegnoAvailabilityAsync_WithNonExistingRegno_ReturnsFalse()
        {
            // Arrange
            var regno = "STU999";
            var studentList = new List<Student>().AsQueryable();

            var mockSet = CreateMockDbSet(studentList);
            _mockContext.Setup(c => c.Students).Returns(mockSet.Object);

            // Act
            var result = await _adminService.CheckStudentRegnoAvailabilityAsync(regno);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CheckStudentRegnoAvailabilityAsync_WithNullOrEmptyRegno_ThrowsArgumentException(string regno)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _adminService.CheckStudentRegnoAvailabilityAsync(regno));
        }

        #endregion

        #region CreateCourseAsync Tests

        [Fact]
        public async Task CreateCourseAsync_WithValidData_CreatesCourseAndReturnsModel()
        {
            // Arrange
            var courseCode = "CS101";
            var courseName = "Introduction to Computer Science";
            var courseUnit = "3";
            var seatLimit = 30;

            _mockContext.Setup(c => c.Courses.AddAsync(It.IsAny<Course>(), default))
                .Callback<Course, System.Threading.CancellationToken>((course, token) => course.Id = 1);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _adminService.CreateCourseAsync(courseCode, courseName, courseUnit, seatLimit);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(courseCode, result.CourseCode);
            Assert.Equal(courseName, result.CourseName);
            Assert.Equal(courseUnit, result.CourseUnit);
            Assert.Equal(seatLimit, result.NoofSeats);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Theory]
        [InlineData(null, "Course Name", "3", 30)]
        [InlineData("", "Course Name", "3", 30)]
        [InlineData("   ", "Course Name", "3", 30)]
        public async Task CreateCourseAsync_WithNullOrEmptyCourseCode_ThrowsArgumentException(string courseCode, string courseName, string courseUnit, int seatLimit)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _adminService.CreateCourseAsync(courseCode, courseName, courseUnit, seatLimit));
        }

        [Theory]
        [InlineData("CS101", null, "3", 30)]
        [InlineData("CS101", "", "3", 30)]
        [InlineData("CS101", "   ", "3", 30)]
        public async Task CreateCourseAsync_WithNullOrEmptyCourseName_ThrowsArgumentException(string courseCode, string courseName, string courseUnit, int seatLimit)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _adminService.CreateCourseAsync(courseCode, courseName, courseUnit, seatLimit));
        }

        [Theory]
        [InlineData("CS101", "Course Name", null, 30)]
        [InlineData("CS101", "Course Name", "", 30)]
        [InlineData("CS101", "Course Name", "   ", 30)]
        public async Task CreateCourseAsync_WithNullOrEmptyCourseUnit_ThrowsArgumentException(string courseCode, string courseName, string courseUnit, int seatLimit)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _adminService.CreateCourseAsync(courseCode, courseName, courseUnit, seatLimit));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public async Task CreateCourseAsync_WithInvalidSeatLimit_ThrowsArgumentException(int seatLimit)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _adminService.CreateCourseAsync("CS101", "Course Name", "3", seatLimit));
        }

        #endregion

        #region DeleteCourseWithDependencyCheckAsync Tests

        [Fact]
        public async Task DeleteCourseWithDependencyCheckAsync_WithNoDependencies_DeletesCourseAndReturnsTrue()
        {
            // Arrange
            var courseId = 1;
            var course = new Course { Id = courseId };
            var enrollmentList = new List<Courseenroll>().AsQueryable();

            _mockContext.Setup(c => c.Courses.FindAsync(courseId)).ReturnsAsync(course);
            var mockEnrollSet = CreateMockDbSet(enrollmentList);
            _mockContext.Setup(c => c.Courseenrolls).Returns(mockEnrollSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _adminService.DeleteCourseWithDependencyCheckAsync(courseId);

            // Assert
            Assert.True(result);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task DeleteCourseWithDependencyCheckAsync_WithDependencies_ReturnsFalse()
        {
            // Arrange
            var courseId = 1;
            var course = new Course { Id = courseId };
            var enrollment = new Courseenroll { Id = 1, Course = courseId };
            var enrollmentList = new List<Courseenroll> { enrollment }.AsQueryable();

            _mockContext.Setup(c => c.Courses.FindAsync(courseId)).ReturnsAsync(course);
            var mockEnrollSet = CreateMockDbSet(enrollmentList);
            _mockContext.Setup(c => c.Courseenrolls).Returns(mockEnrollSet.Object);

            // Act
            var result = await _adminService.DeleteCourseWithDependencyCheckAsync(courseId);

            // Assert
            Assert.False(result);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
        }

        [Fact]
        public async Task DeleteCourseWithDependencyCheckAsync_WithNonExistingCourse_ReturnsFalse()
        {
            // Arrange
            var courseId = 999;
            _mockContext.Setup(c => c.Courses.FindAsync(courseId)).ReturnsAsync((Course)null);

            // Act
            var result = await _adminService.DeleteCourseWithDependencyCheckAsync(courseId);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteCourseWithDependencyCheckAsync_WithInvalidCourseId_ThrowsArgumentException(int courseId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _adminService.DeleteCourseWithDependencyCheckAsync(courseId));
        }

        #endregion

        #region GetAllCoursesAsync Tests

        [Fact]
        public async Task GetAllCoursesAsync_ReturnsAllCoursesOrderedByName()
        {
            // Arrange
            var courses = new List<Course>
            {
                new Course { Id = 1, CourseCode = "CS101", CourseName = "Computer Science", CourseUnit = "3", NoofSeats = 30 },
                new Course { Id = 2, CourseCode = "MATH101", CourseName = "Advanced Mathematics", CourseUnit = "4", NoofSeats = 25 }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(courses);
            _mockContext.Setup(c => c.Courses).Returns(mockSet.Object);

            // Act
            var result = await _adminService.GetAllCoursesAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.Equal("Advanced Mathematics", resultList[0].CourseName);
            Assert.Equal("Computer Science", resultList[1].CourseName);
        }

        [Fact]
        public async Task GetAllCoursesAsync_WithNoCourses_ReturnsEmptyCollection()
        {
            // Arrange
            var courses = new List<Course>().AsQueryable();
            var mockSet = CreateMockDbSet(courses);
            _mockContext.Setup(c => c.Courses).Returns(mockSet.Object);

            // Act
            var result = await _adminService.GetAllCoursesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region CreateDepartmentAsync Tests

        [Fact]
        public async Task CreateDepartmentAsync_WithValidName_CreatesDepartmentAndReturnsModel()
        {
            // Arrange
            var departmentName = "Computer Science";

            _mockContext.Setup(c => c.Departments.AddAsync(It.IsAny<Department>(), default))
                .Callback<Department, System.Threading.CancellationToken>((dept, token) => dept.Id = 1);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _adminService.CreateDepartmentAsync(departmentName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(departmentName, result.DepartmentName);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateDepartmentAsync_WithNullOrEmptyName_ThrowsArgumentException(string departmentName)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _adminService.CreateDepartmentAsync(departmentName));
        }

        #endregion

        #region DeleteDepartmentAsync Tests

        [Fact]
        public async Task DeleteDepartmentAsync_WithNoDependencies_DeletesDepartmentAndReturnsTrue()
        {
            // Arrange
            var departmentId = 1;
            var department = new Department { Id = departmentId };
            var enrollmentList = new List<Courseenroll>().AsQueryable();
            var studentList = new List<Student>().AsQueryable();

            _mockContext.Setup(c => c.Departments.FindAsync(departmentId)).ReturnsAsync(department);
            var mockEnrollSet = CreateMockDbSet(enrollmentList);
            _mockContext.Setup(c => c.Courseenrolls).Returns(mockEnrollSet.Object);
            var mockStudentSet = CreateMockDbSet(studentList);
            _mockContext.Setup(c => c.Students).Returns(mockStudentSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _adminService.DeleteDepartmentAsync(departmentId);

            // Assert
            Assert.True(result);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task DeleteDepartmentAsync_WithDependentEnrollments_ReturnsFalse()
        {
            // Arrange
            var departmentId = 1;
            var department = new Department { Id = departmentId };
            var enrollment = new Courseenroll { Id = 1, Department = departmentId };
            var enrollmentList = new List<Courseenroll> { enrollment }.AsQueryable();

            _mockContext.Setup(c => c.Departments.FindAsync(departmentId)).ReturnsAsync(department);
            var mockEnrollSet = CreateMockDbSet(enrollmentList);
            _mockContext.Setup(c => c.Courseenrolls).Returns(mockEnrollSet.Object);

            // Act
            var result = await _adminService.DeleteDepartmentAsync(departmentId);

            // Assert
            Assert.False(result);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
        }

        [Fact]
        public async Task DeleteDepartmentAsync_WithDependentStudents_ReturnsFalse()
        {
            // Arrange
            var departmentId = 1;
            var department = new Department { Id = departmentId };
            var student = new Student { StudentRegno = "STU001", Department = "1" };
            var enrollmentList = new List<Courseenroll>().AsQueryable();
            var studentList = new List<Student> { student }.AsQueryable();

            _mockContext.Setup(c => c.Departments.FindAsync(departmentId)).ReturnsAsync(department);
            var mockEnrollSet = CreateMockDbSet(enrollmentList);
            _mockContext.Setup(c => c.Courseenrolls).Returns(mockEnrollSet.Object);
            var mockStudentSet = CreateMockDbSet(studentList);
            _mockContext.Setup(c => c.Students).Returns(mockStudentSet.Object);

            // Act
            var result = await _adminService.DeleteDepartmentAsync(departmentId);

            // Assert
            Assert.False(result);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
        }

        [Fact]
        public async Task DeleteDepartmentAsync_WithNonExistingDepartment_ReturnsFalse()
        {
            // Arrange
            var departmentId = 999;
            _mockContext.Setup(c => c.Departments.FindAsync(departmentId)).ReturnsAsync((Department)null);

            // Act
            var result = await _adminService.DeleteDepartmentAsync(departmentId);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetAllDepartmentsAsync Tests

        [Fact]
        public async Task GetAllDepartmentsAsync_ReturnsAllDepartmentsOrderedByName()
        {
            // Arrange
            var departments = new List<Department>
            {
                new Department { Id = 1, DepartmentName = "Computer Science" },
                new Department { Id = 2, DepartmentName = "Biology" }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(departments);
            _mockContext.Setup(c => c.Departments).Returns(mockSet.Object);

            // Act
            var result = await _adminService.GetAllDepartmentsAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.Equal("Biology", resultList[0].DepartmentName);
            Assert.Equal("Computer Science", resultList[1].DepartmentName);
        }

        #endregion

        #region UpdateCourseAsync Tests

        [Fact]
        public async Task UpdateCourseAsync_WithValidData_UpdatesCourseAndReturnsModel()
        {
            // Arrange
            var courseId = 1;
            var course = new Course { Id = courseId, CourseCode = "OLD", CourseName = "Old Name", CourseUnit = "2", NoofSeats = 20 };
            var newCode = "NEW";
            var newName = "New Name";
            var newUnit = "3";
            var newSeats = 30;

            _mockContext.Setup(c => c.Courses.FindAsync(courseId)).ReturnsAsync(course);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _adminService.UpdateCourseAsync(courseId, newCode, newName, newUnit, newSeats);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newCode, result.CourseCode);
            Assert.Equal(newName, result.CourseName);
            Assert.Equal(newUnit, result.CourseUnit);
            Assert.Equal(newSeats, result.NoofSeats);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateCourseAsync_WithNonExistingCourse_ThrowsInvalidOperationException()
        {
            // Arrange
            var courseId = 999;
            _mockContext.Setup(c => c.Courses.FindAsync(courseId)).ReturnsAsync((Course)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _adminService.UpdateCourseAsync(courseId, "CS101", "Course", "3", 30));
        }

        [Theory]
        [InlineData(null, "Course", "3", 30)]
        [InlineData("", "Course", "3", 30)]
        [InlineData("   ", "Course", "3", 30)]
        public async Task UpdateCourseAsync_WithNullOrEmptyCourseCode_ThrowsArgumentException(string courseCode, string courseName, string courseUnit, int seatLimit)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _adminService.UpdateCourseAsync(1, courseCode, courseName, courseUnit, seatLimit));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        public async Task UpdateCourseAsync_WithNegativeSeatLimit_ThrowsArgumentException(int seatLimit)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _adminService.UpdateCourseAsync(1, "CS101", "Course", "3", seatLimit));
        }

        #endregion

        #region GetCourseByIdAsync Tests

        [Fact]
        public async Task GetCourseByIdAsync_WithExistingCourse_ReturnsModel()
        {
            // Arrange
            var courseId = 1;
            var course = new Course { Id = courseId, CourseCode = "CS101", CourseName = "Computer Science", CourseUnit = "3", NoofSeats = 30 };
            var courseList = new List<Course> { course }.AsQueryable();

            var mockSet = CreateMockDbSet(courseList);
            _mockContext.Setup(c => c.Courses).Returns(mockSet.Object);

            // Act
            var result = await _adminService.GetCourseByIdAsync(courseId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(courseId, result.Id);
            Assert.Equal("CS101", result.CourseCode);
        }

        [Fact]
        public async Task GetCourseByIdAsync_WithNonExistingCourse_ReturnsNull()
        {
            // Arrange
            var courseId = 999;
            var courseList = new List<Course>().AsQueryable();

            var mockSet = CreateMockDbSet(courseList);
            _mockContext.Setup(c => c.Courses).Returns(mockSet.Object);

            // Act
            var result = await _adminService.GetCourseByIdAsync(courseId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region UpdateStudentProfileAsync Tests

        [Fact]
        public async Task UpdateStudentProfileAsync_WithExistingStudent_UpdatesProfileAndReturnsTrue()
        {
            // Arrange
            var regid = "STU001";
            var student = new Student { StudentRegno = regid, StudentName = "Old Name", StudentPhoto = "old.jpg", Cgpa = 2.5m };
            var newName = "New Name";
            var newPhoto = "new.jpg";
            var newCgpa = 3.5m;

            _mockContext.Setup(c => c.Students.FindAsync(regid)).ReturnsAsync(student);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _adminService.UpdateStudentProfileAsync(regid, newName, newPhoto, newCgpa);

            // Assert
            Assert.True(result);
            Assert.Equal(newName, student.StudentName);
            Assert.Equal(newPhoto, student.StudentPhoto);
            Assert.Equal(newCgpa, student.Cgpa);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateStudentProfileAsync_WithNonExistingStudent_ReturnsFalse()
        {
            // Arrange
            var regid = "STU999";
            _mockContext.Setup(c => c.Students.FindAsync(regid)).ReturnsAsync((Student)null);

            // Act
            var result = await _adminService.UpdateStudentProfileAsync(regid, "Name", "photo.jpg", 3.0m);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetStudentByRegnoAsync Tests

        [Fact]
        public async Task GetStudentByRegnoAsync_WithExistingStudent_ReturnsModel()
        {
            // Arrange
            var regid = "STU001";
            var student = new Student { StudentRegno = regid, StudentName = "John Doe", Cgpa = 3.5m };
            var studentList = new List<Student> { student }.AsQueryable();

            var mockSet = CreateMockDbSet(studentList);
            _mockContext.Setup(c => c.Students).Returns(mockSet.Object);

            // Act
            var result = await _adminService.GetStudentByRegnoAsync(regid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(regid, result.StudentRegno);
            Assert.Equal("John Doe", result.StudentName);
        }

        [Fact]
        public async Task GetStudentByRegnoAsync_WithNonExistingStudent_ReturnsNull()
        {
            // Arrange
            var regid = "STU999";
            var studentList = new List<Student>().AsQueryable();

            var mockSet = CreateMockDbSet(studentList);
            _mockContext.Setup(c => c.Students).Returns(mockSet.Object);

            // Act
            var result = await _adminService.GetStudentByRegnoAsync(regid);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AuthenticateAdminAsync Tests

        [Fact]
        public async Task AuthenticateAdminAsync_WithValidCredentials_ReturnsModel()
        {
            // Arrange
            var username = "admin";
            var password = "password123";
            var admin = new Admin { Id = 1, Username = username, Password = password };
            var adminList = new List<Admin> { admin }.AsQueryable();

            var mockSet = CreateMockDbSet(adminList);
            _mockContext.Setup(c => c.Admins).Returns(mockSet.Object);

            // Act
            var result = await _adminService.AuthenticateAdminAsync(username, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
        }

        [Fact]
        public async Task AuthenticateAdminAsync_WithInvalidCredentials_ReturnsNull()
        {
            // Arrange
            var username = "admin";
            var password = "wrongpassword";
            var adminList = new List<Admin>().AsQueryable();

            var mockSet = CreateMockDbSet(adminList);
            _mockContext.Setup(c => c.Admins).Returns(mockSet.Object);

            // Act
            var result = await _adminService.AuthenticateAdminAsync(username, password);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AdminLogoutAsync Tests

        [Fact]
        public async Task AdminLogoutAsync_ReturnsTrue()
        {
            // Act
            var result = await _adminService.AdminLogoutAsync();

            // Assert
            Assert.True(result);
        }

        #endregion

        #region UpdateStudentPasswordAsync Tests

        [Fact]
        public async Task UpdateStudentPasswordAsync_WithExistingStudent_UpdatesPasswordAndReturnsTrue()
        {
            // Arrange
            var studentRegno = "STU001";
            var newPassword = "newPassword123";
            var student = new Student { StudentRegno = studentRegno, Password = "oldPassword" };

            _mockContext.Setup(c => c.Students.FindAsync(studentRegno)).ReturnsAsync(student);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _adminService.UpdateStudentPasswordAsync(studentRegno, newPassword);

            // Assert
            Assert.True(result);
            Assert.Equal(newPassword, student.Password);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateStudentPasswordAsync_WithNonExistingStudent_ReturnsFalse()
        {
            // Arrange
            var studentRegno = "STU999";
            _mockContext.Setup(c => c.Students.FindAsync(studentRegno)).ReturnsAsync((Student)null);

            // Act
            var result = await _adminService.UpdateStudentPasswordAsync(studentRegno, "newPassword");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null, "password")]
        [InlineData("", "password")]
        [InlineData("   ", "password")]
        public async Task UpdateStudentPasswordAsync_WithNullOrEmptyRegno_ThrowsArgumentException(string studentRegno, string newPassword)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _adminService.UpdateStudentPasswordAsync(studentRegno, newPassword));
        }

        [Theory]
        [InlineData("STU001", null)]
        [InlineData("STU001", "")]
        [InlineData("STU001", "   ")]
        public async Task UpdateStudentPasswordAsync_WithNullOrEmptyPassword_ThrowsArgumentException(string studentRegno, string newPassword)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _adminService.UpdateStudentPasswordAsync(studentRegno, newPassword));
        }

        #endregion

        #region GetAllStudentsAsync Tests

        [Fact]
        public async Task GetAllStudentsAsync_ReturnsAllStudents()
        {
            // Arrange
            var students = new List<Student>
            {
                new Student { StudentRegno = "STU001", StudentName = "John Doe" },
                new Student { StudentRegno = "STU002", StudentName = "Jane Smith" }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(students);
            _mockContext.Setup(c => c.Students).Returns(mockSet.Object);

            // Act
            var result = await _adminService.GetAllStudentsAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
        }

        #endregion

        #region CreateSemesterAsync Tests

        [Fact]
        public async Task CreateSemesterAsync_WithValidName_CreatesSemesterAndReturnsModel()
        {
            // Arrange
            var semesterName = "Fall 2024";

            _mockContext.Setup(c => c.Semesters.AddAsync(It.IsAny<Semester>(), default))
                .Callback<Semester, System.Threading.CancellationToken>((sem, token) => sem.Id = 1);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _adminService.CreateSemesterAsync(semesterName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(semesterName, result.SemesterName);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateSemesterAsync_WithNullOrEmptyName_ThrowsArgumentException(string semesterName)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _adminService.CreateSemesterAsync(semesterName));
        }

        #endregion

        #region DeleteSemesterAsync Tests

        [Fact]
        public async Task DeleteSemesterAsync_WithNoDependencies_DeletesSemesterAndReturnsTrue()
        {
            // Arrange
            var semesterId = 1;
            var semester = new Semester { Id = semesterId };
            var enrollmentList = new List<Courseenroll>().AsQueryable();

            _mockContext.Setup(c => c.Semesters.FindAsync(semesterId)).ReturnsAsync(semester);
            var mockEnrollSet = CreateMockDbSet(enrollmentList);
            _mockContext.Setup(c => c.Courseenrolls).Returns(mockEnrollSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _adminService.DeleteSemesterAsync(semesterId);

            // Assert
            Assert.True(result);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task DeleteSemesterAsync_WithDependencies_ReturnsFalse()
        {
            // Arrange
            var semesterId = 1;
            var semester = new Semester { Id = semesterId };
            var enrollment = new Courseenroll { Id = 1, Semester = semesterId };
            var enrollmentList = new List<Courseenroll> { enrollment }.AsQueryable();

            _mockContext.Setup(c => c.Semesters.FindAsync(semesterId)).ReturnsAsync(semester);
            var mockEnrollSet = CreateMockDbSet(enrollmentList);
            _mockContext.Setup(c => c.Courseenrolls).Returns(mockEnrollSet.Object);

            // Act
            var result = await _adminService.DeleteSemesterAsync(semesterId);

            // Assert
            Assert.False(result);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
        }

        [Fact]
        public async Task DeleteSemesterAsync_WithNonExistingSemester_ReturnsFalse()
        {
            // Arrange
            var semesterId = 999;
            _mockContext.Setup(c => c.Semesters.FindAsync(semesterId)).ReturnsAsync((Semester)null);

            // Act
            var result = await _adminService.DeleteSemesterAsync(semesterId);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetAllSemestersAsync Tests

        [Fact]
        public async Task GetAllSemestersAsync_ReturnsAllSemestersOrderedById()
        {
            // Arrange
            var semesters = new List<Semester>
            {
                new Semester { Id = 2, SemesterName = "Spring 2024" },
                new Semester { Id = 1, SemesterName = "Fall 2023" }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(semesters);
            _mockContext.Setup(c => c.Semesters).Returns(mockSet.Object);

            // Act
            var result = await _adminService.GetAllSemestersAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.Equal(1, resultList[0].Id);
            Assert.Equal(2, resultList[1].Id);
        }

        #endregion

        #region CreateSessionAsync Tests

        [Fact]
        public async Task CreateSessionAsync_WithValidName_CreatesSessionAndReturnsModel()
        {
            // Arrange
            var sessionName = "2023-2024";

            _mockContext.Setup(c => c.Sessions.AddAsync(It.IsAny<Session>(), default))
                .Callback<Session, System.Threading.CancellationToken>((sess, token) => sess.Id = 1);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _adminService.CreateSessionAsync(sessionName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(sessionName, result.SessionName);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateSessionAsync_WithNullOrEmptyName_ThrowsArgumentException(string sessionName)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _adminService.CreateSessionAsync(sessionName));
        }

        #endregion

        #region DeleteSessionAsync Tests

        [Fact]
        public async Task DeleteSessionAsync_WithNoDependencies_DeletesSessionAndReturnsTrue()
        {
            // Arrange
            var sessionId = 1;
            var session = new Session { Id = sessionId };
            var enrollmentList = new List<Courseenroll>().AsQueryable();

            _mockContext.Setup(c => c.Sessions.FindAsync(sessionId)).ReturnsAsync(session);
            var mockEnrollSet = CreateMockDbSet(enrollmentList);
            _mockContext.Setup(c => c.Courseenrolls).Returns(mockEnrollSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _adminService.DeleteSessionAsync(sessionId);

            // Assert
            Assert.True(result);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task DeleteSessionAsync_WithDependencies_ReturnsFalse()
        {
            // Arrange
            var sessionId = 1;
            var session = new Session { Id = sessionId };
            var enrollment = new Courseenroll { Id = 1, Session = sessionId };
            var enrollmentList = new List<Courseenroll> { enrollment }.AsQueryable();

            _mockContext.Setup(c => c.Sessions.FindAsync(sessionId)).ReturnsAsync(session);
            var mockEnrollSet = CreateMockDbSet(enrollmentList);
            _mockContext.Setup(c => c.Courseenrolls).Returns(mockEnrollSet.Object);

            // Act
            var result = await _adminService.DeleteSessionAsync(sessionId);

            // Assert
            Assert.False(result);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
        }

        [Fact]
        public async Task DeleteSessionAsync_WithNonExistingSession_ReturnsFalse()
        {
            // Arrange
            var sessionId = 999;
            _mockContext.Setup(c => c.Sessions.FindAsync(sessionId)).ReturnsAsync((Session)null);

            // Act
            var result = await _adminService.DeleteSessionAsync(sessionId);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetAllSessionsAsync Tests

        [Fact]
        public async Task GetAllSessionsAsync_ReturnsAllSessionsOrderedById()
        {
            // Arrange
            var sessions = new List<Session>
            {
                new Session { Id = 2, SessionName = "2024-2025" },
                new Session { Id = 1, SessionName = "2023-2024" }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(sessions);
            _mockContext.Setup(c => c.Sessions).Returns(mockSet.Object);

            // Act
            var result = await _adminService.GetAllSessionsAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.Equal(1, resultList[0].Id);
            Assert.Equal(2, resultList[1].Id);
        }

        #endregion

        #region CreateStudentAsync Tests

        [Fact]
        public async Task CreateStudentAsync_WithValidData_CreatesStudentAndReturnsModel()
        {
            // Arrange
            var studentName = "John Doe";
            var studentRegno = "STU001";
            var password = "password123";
            var pincode = "1234";

            _mockContext.Setup(c => c.Students.FindAsync(studentRegno)).ReturnsAsync((Student)null);
            _mockContext.Setup(c => c.Students.AddAsync(It.IsAny<Student>(), default));
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _adminService.CreateStudentAsync(studentName, studentRegno, password, pincode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(studentName, result.StudentName);
            Assert.Equal(studentRegno, result.StudentRegno);
            Assert.Equal(password, result.Password);
            Assert.Equal(pincode, result.Pincode);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task CreateStudentAsync_WithExistingRegno_ThrowsInvalidOperationException()
        {
            // Arrange
            var studentRegno = "STU001";
            var existingStudent = new Student { StudentRegno = studentRegno };

            _mockContext.Setup(c => c.Students.FindAsync(studentRegno)).ReturnsAsync(existingStudent);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _adminService.CreateStudentAsync("John Doe", studentRegno, "password", "1234"));
        }

        [Theory]
        [InlineData(null, "STU001", "password", "1234")]
        [InlineData("", "STU001", "password", "1234")]
        [InlineData("   ", "STU001", "password", "1234")]
        public async Task CreateStudentAsync_WithNullOrEmptyName_ThrowsArgumentException(string studentName, string studentRegno, string password, string pincode)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _adminService.CreateStudentAsync(studentName, studentRegno, password, pincode));
        }

        [Theory]
        [InlineData("John Doe", null, "password", "1234")]
        [InlineData("John Doe", "", "password", "1234")]
        [InlineData("John Doe", "   ", "password", "1234")]
        public async Task CreateStudentAsync_WithNullOrEmptyRegno_ThrowsArgumentException(string studentName, string studentRegno, string password, string pincode)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _adminService.CreateStudentAsync(studentName, studentRegno, password, pincode));
        }

        #endregion

        #region GetAllUserlogsAsync Tests

        [Fact]
        public async Task GetAllUserlogsAsync_ReturnsAllUserlogsOrderedByLoginTimeDescending()
        {
            // Arrange
            var userlogs = new List<Userlog>
            {
                new Userlog { Id = 1, StudentRegno = "STU001", LoginTime = DateTime.UtcNow.AddHours(-2) },
                new Userlog { Id = 2, StudentRegno = "STU002", LoginTime = DateTime.UtcNow.AddHours(-1) }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(userlogs);
            _mockContext.Setup(c => c.Userlogs).Returns(mockSet.Object);

            // Act
            var result = await _adminService.GetAllUserlogsAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.Equal(2, resultList[0].Id);
            Assert.Equal(1, resultList[1].Id);
        }

        [Fact]
        public async Task GetAllUserlogsAsync_WithNoLogs_ReturnsEmptyCollection()
        {
            // Arrange
            var userlogs = new List<Userlog>().AsQueryable();
            var mockSet = CreateMockDbSet(userlogs);
            _mockContext.Setup(c => c.Userlogs).Returns(mockSet.Object);

            // Act
            var result = await _adminService.GetAllUserlogsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region Helper Methods

        private Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(data.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(default))
                .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
            return mockSet;
        }

        #endregion
    }

    #region Test Async Query Provider

    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, System.Threading.CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethod(
                    name: nameof(IQueryProvider.Execute),
                    genericParameterCount: 1,
                    types: new[] { typeof(System.Linq.Expressions.Expression) })
                .MakeGenericMethod(resultType)
                .Invoke(this, new[] { expression });

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                .MakeGenericMethod(resultType)
                .Invoke(null, new[] { executionResult });
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(System.Linq.Expressions.Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(System.Threading.CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }
    }

    #endregion
}
