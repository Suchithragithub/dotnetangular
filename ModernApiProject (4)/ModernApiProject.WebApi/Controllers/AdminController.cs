using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernApiProject.Application.Interfaces;
using ModernApiProject.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ModernApiProject.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")] 
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _configuration;

        public AdminController(IAdminService adminService, ILogger<AdminController> logger, IConfiguration configuration)
        {
            _adminService = adminService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("validate-password")]
        public async Task<IActionResult> ValidateAdminPassword([FromBody] AdminModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest("Password is required");
                }

                var isValid = await _adminService.ValidateAdminPasswordAsync(model.Password);
                return Ok(new { isValid });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error validating admin password");
                return StatusCode(500, "An error occurred while validating password");
            }
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> UpdateAdminPassword([FromBody] AdminModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.Username))
                {
                    return BadRequest("Old password and new password are required");
                }

                var result = await _adminService.UpdateAdminPasswordAsync(model.Username, model.Password);
                if (result)
                {
                    return Ok(new { message = "Password updated successfully" });
                }
                return BadRequest("Failed to update password");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error updating admin password");
                return StatusCode(500, "An error occurred while updating password");
            }
        }

        [HttpGet("check-student-availability/{regno}")]
        public async Task<IActionResult> CheckStudentRegnoAvailability(string regno)
        {
            try
            {
                var isAvailable = await _adminService.CheckStudentRegnoAvailabilityAsync(regno);
                return Ok(new { available = isAvailable });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error checking student regno availability");
                return StatusCode(500, "An error occurred while checking availability");
            }
        }

        [HttpPost("courses")]
        public async Task<IActionResult> CreateCourse([FromBody] CourseModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.CourseCode) || string.IsNullOrEmpty(model.CourseName))
                {
                    return BadRequest("Course code and name are required");
                }

                var course = await _adminService.CreateCourseAsync(
                    model.CourseCode,
                    model.CourseName,
                    model.CourseUnit,
                    model.NoofSeats);

                if (course != null)
                {
                    return Ok(course);
                }
                return BadRequest("Failed to create course");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                return StatusCode(500, "An error occurred while creating course");
            }
        }

        [HttpDelete("courses/{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                var result = await _adminService.DeleteCourseWithDependencyCheckAsync(id);
                if (result)
                {
                    return Ok(new { message = "Course deleted successfully" });
                }
                return BadRequest("Failed to delete course or course has dependencies");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error deleting course");
                return StatusCode(500, "An error occurred while deleting course");
            }
        }

        [HttpGet("courses")]
        public async Task<IActionResult> GetAllCourses()
        {
            try
            {
                var courses = await _adminService.GetAllCoursesAsync();
                return Ok(courses);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courses");
                return StatusCode(500, "An error occurred while retrieving courses");
            }
        }[HttpPost("departments")]
        public async Task<IActionResult> CreateDepartment([FromBody] DepartmentModel model)
        {
            try
            {
                // FIX: Changed model.Department to model.DepartmentName
                if (string.IsNullOrEmpty(model.DepartmentName))
                {
                    return BadRequest("Department name is required");
                }

                var department = await _adminService.CreateDepartmentAsync(model.DepartmentName);
                if (department != null)
                {
                    return Ok(department);
                }
                return BadRequest("Failed to create department");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error creating department");
                return StatusCode(500, "An error occurred while creating department");
            }
        }

        [HttpDelete("departments/{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var result = await _adminService.DeleteDepartmentAsync(id);
                if (result)
                {
                    return Ok(new { message = "Department deleted successfully" });
                }
                return BadRequest("Failed to delete department");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error deleting department");
                return StatusCode(500, "An error occurred while deleting department");
            }
        }[HttpGet("departments")]
        public async Task<IActionResult> GetAllDepartments()
        {
            try
            {
                var departments = await _adminService.GetAllDepartmentsAsync();
                return Ok(departments);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving departments");
                return StatusCode(500, "An error occurred while retrieving departments");
            }
        }

        [HttpPut("courses/{id}")]
        public async Task<IActionResult> UpdateCourse(int id,[FromBody] CourseModel model)
        {
            try
            {
                var course = await _adminService.UpdateCourseAsync(
                    id,
                    model.CourseCode,
                    model.CourseName,
                    model.CourseUnit,
                    model.NoofSeats);

                if (course != null)
                {
                    return Ok(course);
                }
                return NotFound("Course not found");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error updating course");
                return StatusCode(500, "An error occurred while updating course");
            }
        }

        [HttpGet("courses/{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            try
            {
                var course = await _adminService.GetCourseByIdAsync(id);
                if (course != null)
                {
                    return Ok(course);
                }
                return NotFound("Course not found");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course");
                return StatusCode(500, "An error occurred while retrieving course");
            }
        }[HttpPut("students/{regno}/profile")]
        public async Task<IActionResult> UpdateStudentProfile(string regno, [FromBody] StudentModel model)
        {
            try
            {
                var result = await _adminService.UpdateStudentProfileAsync(
                    regno,
                    model.StudentName,
                    model.StudentPhoto,
                    model.Cgpa);

                if (result)
                {
                    return Ok(new { message = "Student profile updated successfully" });
                }
                return BadRequest("Failed to update student profile");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error updating student profile");
                return StatusCode(500, "An error occurred while updating student profile");
            }
        }

        [HttpGet("students/{regno}")]
        public async Task<IActionResult> GetStudentByRegno(string regno)
        {
            try
            {
                var student = await _adminService.GetStudentByRegnoAsync(regno);
                if (student != null)
                {
                    return Ok(student);
                }
                return NotFound("Student not found");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student");
                return StatusCode(500, "An error occurred while retrieving student");
            }
        }[HttpGet("enrollment-history")]
        public async Task<IActionResult> GetEnrollmentHistory()
        {
            try
            {
                var history = await _adminService.GetEnrollmentHistoryAsync();
                return Ok(history);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving enrollment history");
                return StatusCode(500, "An error occurred while retrieving enrollment history");
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> AuthenticateAdmin([FromBody] AdminModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                    return BadRequest("Username and password are required");

                var admin = await _adminService.AuthenticateAdminAsync(model.Username, model.Password);
                if (admin == null)
                    return Unauthorized("Invalid credentials");

                // Generate JWT with "Admin" UserType claim
                var tokenHandler = new JwtSecurityTokenHandler();
                var secretKey = _configuration["Jwt:SecretKey"] ?? "your-super-secret-key-must-be-at-least-32-characters";
                var key = Encoding.ASCII.GetBytes(secretKey);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, admin.Username),
                        new Claim(ClaimTypes.Name, admin.Username),
                        new Claim("UserType", "Admin")   // <-- key differentiator
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(60),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _configuration["Jwt:Issuer"] ?? "ModernApiProject",
                    Audience = _configuration["Jwt:Audience"] ?? "ModernApiProject"
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Ok(new {
                    token = tokenString,
                    admin = admin
                });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error authenticating admin");
                return StatusCode(500, "An error occurred during authentication");
            }
        }

        [HttpPost("levels")]
        public async Task<IActionResult> CreateLevel([FromBody] LevelModel model)
        {
            try
            {
                // FIX: Changed model.Level to model.LevelName
                if (string.IsNullOrEmpty(model.LevelName))
                {
                    return BadRequest("Level name is required");
                }

                var level = await _adminService.CreateLevelAsync(model.LevelName);
                if (level != null)
                {
                    return Ok(level);
                }
                return BadRequest("Failed to create level");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error creating level");
                return StatusCode(500, "An error occurred while creating level");
            }
        }

        [HttpDelete("levels/{id}")]
        public async Task<IActionResult> DeleteLevel(int id)
        {
            try
            {
                var result = await _adminService.DeleteLevelAsync(id);
                if (result)
                {
                    return Ok(new { message = "Level deleted successfully" });
                }
                return BadRequest("Failed to delete level");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error deleting level");
                return StatusCode(500, "An error occurred while deleting level");
            }
        }[HttpGet("levels")]
        public async Task<IActionResult> GetAllLevels()
        {
            try
            {
                var levels = await _adminService.GetAllLevelsAsync();
                return Ok(levels);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving levels");
                return StatusCode(500, "An error occurred while retrieving levels");
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var result = await _adminService.AdminLogoutAsync();
                if (result)
                {
                    return Ok(new { message = "Logged out successfully" });
                }
                return BadRequest("Failed to logout");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, "An error occurred during logout");
            }
        }

        [HttpDelete("students/{regno}")]
        public async Task<IActionResult> DeleteStudent(string regno)
        {
            try
            {
                var result = await _adminService.DeleteStudentAsync(regno);
                if (result)
                {
                    return Ok(new { message = "Student deleted successfully" });
                }
                return BadRequest("Failed to delete student");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error deleting student");
                return StatusCode(500, "An error occurred while deleting student");
            }
        }

        [HttpPut("students/{regno}/password")]
        public async Task<IActionResult> UpdateStudentPassword(string regno, [FromBody] StudentModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest("New password is required");
                }

                var result = await _adminService.UpdateStudentPasswordAsync(regno, model.Password);
                if (result)
                {
                    return Ok(new { message = "Student password updated successfully" });
                }
                return BadRequest("Failed to update student password");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error updating student password");
                return StatusCode(500, "An error occurred while updating student password");
            }
        }[HttpGet("students")]
        public async Task<IActionResult> GetAllStudents()
        {
            try
            {
                var students = await _adminService.GetAllStudentsAsync();
                return Ok(students);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving students");
                return StatusCode(500, "An error occurred while retrieving students");
            }
        }

        [HttpGet("courses/{courseId}/print")]
        public async Task<IActionResult> GetPrintableCourseEnrollmentDetails(int courseId)
        {
            try
            {
                var details = await _adminService.GetPrintableCourseEnrollmentDetailsAsync(courseId);
                return Ok(details);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving printable course enrollment details");
                return StatusCode(500, "An error occurred while retrieving enrollment details");
            }
        }

        [HttpPost("semesters")]
        public async Task<IActionResult> CreateSemester([FromBody] SemesterModel model)
        {
            try
            {
                // FIX: Changed model.Semester to model.SemesterName
                if (string.IsNullOrEmpty(model.SemesterName))
                {
                    return BadRequest("Semester name is required");
                }

                var semester = await _adminService.CreateSemesterAsync(model.SemesterName);
                if (semester != null)
                {
                    return Ok(semester);
                }
                return BadRequest("Failed to create semester");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error creating semester");
                // return StatusCode(500, "An error occurred while creating semester");
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, $"An error occurred: {errorMessage}");
            }
        }[HttpDelete("semesters/{id}")]
        public async Task<IActionResult> DeleteSemester(int id)
        {
            try
            {
                var result = await _adminService.DeleteSemesterAsync(id);
                if (result)
                {
                    return Ok(new { message = "Semester deleted successfully" });
                }
                return BadRequest("Failed to delete semester");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error deleting semester");
                return StatusCode(500, "An error occurred while deleting semester");
            }
        }

        [HttpGet("semesters")]
        public async Task<IActionResult> GetAllSemesters()
        {
            try
            {
                var semesters = await _adminService.GetAllSemestersAsync();
                return Ok(semesters);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving semesters");
                return StatusCode(500, "An error occurred while retrieving semesters");
            }
        }[HttpPost("sessions")]
        public async Task<IActionResult> CreateSession([FromBody] SessionModel model)
        {
            try
            {
                // FIX: Changed model.Session to model.SessionName
                if (string.IsNullOrEmpty(model.SessionName))
                {
                    return BadRequest("Session name is required");
                }

                var session = await _adminService.CreateSessionAsync(model.SessionName);
                if (session != null)
                {
                    return Ok(session);
                }
                return BadRequest("Failed to create session");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error creating session");
                return StatusCode(500, "An error occurred while creating session");
            }
        }

        [HttpDelete("sessions/{id}")]
        public async Task<IActionResult> DeleteSession(int id)
        {
            try
            {
                var result = await _adminService.DeleteSessionAsync(id);
                if (result)
                {
                    return Ok(new { message = "Session deleted successfully" });
                }
                return BadRequest("Failed to delete session");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error deleting session");
                return StatusCode(500, "An error occurred while deleting session");
            }
        }[HttpGet("sessions")]
        public async Task<IActionResult> GetAllSessions()
        {
            try
            {
                var sessions = await _adminService.GetAllSessionsAsync();
                return Ok(sessions);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sessions");
                return StatusCode(500, "An error occurred while retrieving sessions");
            }
        }

        [HttpPost("students/register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterStudent([FromBody] StudentModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.StudentName) || string.IsNullOrEmpty(model.StudentRegno))
                {
                    return BadRequest("Student name and registration number are required");
                }

                var student = await _adminService.CreateStudentAsync(
                    model.StudentName,
                    model.StudentRegno,
                    model.Password,
                    model.Pincode);

                if (student != null)
                {
                    return Ok(student);
                }
                return BadRequest("Failed to register student");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error registering student");
                return StatusCode(500, "An error occurred while registering student");
            }
        }

        [HttpGet("userlogs")]
        public async Task<IActionResult> GetAllUserlogs()
        {
            try
            {
                var userlogs = await _adminService.GetAllUserlogsAsync();
                return Ok(userlogs);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user logs");
                return StatusCode(500, "An error occurred while retrieving user logs");
            }
        }
    }
}