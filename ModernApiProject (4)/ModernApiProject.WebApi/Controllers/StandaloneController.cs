using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernApiProject.Application.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ModernApiProject.WebApi.Controllers
{
    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "StudentOnly")]
    public class StandaloneController : ControllerBase
    {
        private readonly IStandaloneService _standaloneService;
        private readonly ILogger<StandaloneController> _logger;
        private readonly IConfiguration _configuration; // <-- ADD THIS

        public StandaloneController(IStandaloneService standaloneService, ILogger<StandaloneController> logger, IConfiguration configuration)
        {
            _standaloneService = standaloneService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("validate-password")]
        public async Task<IActionResult> ValidateStudentPassword([FromQuery] string studentRegno, [FromQuery] string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentRegno) || string.IsNullOrWhiteSpace(password))
                {
                    return BadRequest("Student registration number and password are required.");
                }

                var isValid = await _standaloneService.ValidateStudentPasswordAsync(studentRegno, password);
                return Ok(new { isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating student password for {StudentRegno}", studentRegno);
                return StatusCode(500, "An error occurred while validating the password.");
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> UpdateStudentPassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (request == null ||
                    string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                    string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return BadRequest(new { message = "Current password and new password are required." });
                }
 
                if (request.NewPassword.Length < 6)
                {
                    return BadRequest(new { message = "New password must be at least 6 characters." });
                }
 
                var studentRegno =
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    User.FindFirst(ClaimTypes.Name)?.Value;
 
                if (string.IsNullOrWhiteSpace(studentRegno))
                {
                    return Unauthorized(new { message = "Unable to identify the student from token." });
                }
 
                var result = await _standaloneService.UpdateStudentPasswordAsync(
                    studentRegno,
                    request.CurrentPassword,
                    request.NewPassword);
 
                if (!result)
                {
                    return Unauthorized(new { message = "Current password is incorrect." });
                }
 
                return Ok(new { success = true, message = "Password updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password for student");
                return StatusCode(500, new { message = "An error occurred while updating password." });
            }
        }

        [HttpGet("check-enrollment")]
        public async Task<IActionResult> CheckStudentEnrollment([FromQuery] string studentRegno, [FromQuery] int courseId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentRegno) || courseId <= 0)
                {
                    return BadRequest("Valid student registration number and course ID are required.");
                }

                var isEnrolled = await _standaloneService.IsStudentEnrolledInCourseAsync(studentRegno, courseId);
                return Ok(new { isEnrolled });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking enrollment for student {StudentRegno} in course {CourseId}", studentRegno, courseId);
                return StatusCode(500, "An error occurred while checking enrollment.");
            }
        }

        [HttpGet("course-enrollment-count")]
        public async Task<IActionResult> GetCourseEnrollmentCount([FromQuery] int courseId)
        {
            try
            {
                if (courseId <= 0)
                {
                    return BadRequest("Valid course ID is required.");
                }

                var count = await _standaloneService.GetCourseEnrollmentCountAsync(courseId);
                return Ok(new { courseId, enrollmentCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enrollment count for course {CourseId}", courseId);
                return StatusCode(500, "An error occurred while retrieving enrollment count.");
            }
        }

        [HttpGet("course-available-seats")]
        public async Task<IActionResult> GetCourseAvailableSeats([FromQuery] int courseId)
        {
            try
            {
                if (courseId <= 0)
                {
                    return BadRequest("Valid course ID is required.");
                }

                var availableSeats = await _standaloneService.GetCourseAvailableSeatsAsync(courseId);
                return Ok(new { courseId, availableSeats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available seats for course {CourseId}", courseId);
                return StatusCode(500, "An error occurred while retrieving available seats.");
            }
        }

        [HttpGet("enroll-history")]
        public async Task<IActionResult> GetEnrollmentHistory([FromQuery] string studentRegno)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentRegno))
                {
                    return BadRequest("Student registration number is required.");
                }

                var history = await _standaloneService.GetEnrollmentHistoryByStudentAsync(studentRegno);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enrollment history for student {StudentRegno}", studentRegno);
                return StatusCode(500, "An error occurred while retrieving enrollment history.");
            }
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> CreateEnrollment([FromQuery] string studentRegno, [FromQuery] string pincode, [FromQuery] int sessionId, [FromQuery] int departmentId, [FromQuery] int levelId, [FromQuery] int courseId, [FromQuery] int semesterId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentRegno) || string.IsNullOrWhiteSpace(pincode) || sessionId <= 0 || departmentId <= 0 || levelId <= 0 || courseId <= 0 || semesterId <= 0)
                {
                    return BadRequest("All enrollment parameters are required and must be valid.");
                }

                var enrollment = await _standaloneService.CreateEnrollmentAsync(studentRegno, pincode, sessionId, departmentId, levelId, courseId, semesterId);
                if (enrollment == null)
                {
                    return BadRequest("Enrollment failed. Please check the provided information.");
                }

                return Ok(enrollment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating enrollment for student {StudentRegno}", studentRegno);
                return StatusCode(500, "An error occurred while creating the enrollment.");
            }
        }

        [HttpGet("student")]
        public async Task<IActionResult> GetStudentByRegno([FromQuery] string studentRegno)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentRegno))
                {
                    return BadRequest("Student registration number is required.");
                }

                var student = await _standaloneService.GetStudentByRegnoAsync(studentRegno);
                if (student == null)
                {
                    return NotFound("Student not found.");
                }

                return Ok(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student by regno {StudentRegno}", studentRegno);
                return StatusCode(500, "An error occurred while retrieving student information.");
            }
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetAllSessions()
        {
            try
            {
                var sessions = await _standaloneService.GetAllSessionsAsync();
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all sessions");
                return StatusCode(500, "An error occurred while retrieving sessions.");
            }
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetAllDepartments()
        {
            try
            {
                var departments = await _standaloneService.GetAllDepartmentsAsync();
                return Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all departments");
                return StatusCode(500, "An error occurred while retrieving departments.");
            }
        }

        [HttpGet("levels")]
        public async Task<IActionResult> GetAllLevels()
        {
            try
            {
                var levels = await _standaloneService.GetAllLevelsForEnrollmentAsync();
                return Ok(levels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all levels");
                return StatusCode(500, "An error occurred while retrieving levels.");
            }
        }

        [HttpGet("semesters")]
        public async Task<IActionResult> GetAllSemesters()
        {
            try
            {
                var semesters = await _standaloneService.GetAllSemestersForEnrollmentAsync();
                return Ok(semesters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all semesters");
                return StatusCode(500, "An error occurred while retrieving semesters.");
            }
        }

        [HttpGet("courses")]
        public async Task<IActionResult> GetAllCourses()
        {
            try
            {
                var courses = await _standaloneService.GetAllCoursesForEnrollmentAsync();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all courses");
                return StatusCode(500, "An error occurred while retrieving courses.");
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> AuthenticateStudent([FromQuery] string regno, [FromQuery] string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(regno) || string.IsNullOrWhiteSpace(password))
                {
                    return BadRequest("Registration number and password are required.");
                }

                var student = await _standaloneService.AuthenticateStudentAsync(regno, password);
                if (student == null)
                {
                    return Unauthorized("Invalid credentials.");
                }

                // --- JWT TOKEN GENERATION LOGIC ---
                var tokenHandler = new JwtSecurityTokenHandler();
                var secretKey = _configuration["Jwt:SecretKey"] ?? "your-super-secret-key-must-be-at-least-32-characters";
                var key = Encoding.ASCII.GetBytes(secretKey);
                
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, student.StudentRegno),
                        new Claim(ClaimTypes.Name, student.StudentName ?? ""),
                        new Claim("UserType", "Student")
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(60), // Token valid for 1 hour
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _configuration["Jwt:Issuer"] ?? "ModernApiProject",
                    Audience = _configuration["Jwt:Audience"] ?? "ModernApiProject"
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // Return both the token and the student data
                return Ok(new { 
                    token = tokenString,
                    student = student 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating student {Regno}", regno);
                return StatusCode(500, "An error occurred during authentication.");
            }
        }

        [HttpPost("login-log")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUserLoginLog([FromQuery] string studentRegno, [FromQuery] int status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentRegno))
                {
                    return BadRequest("Student registration number is required.");
                }

                var userIp = HttpContext.Connection.RemoteIpAddress?.GetAddressBytes() ?? new byte[0];
                var log = await _standaloneService.CreateUserLoginLogAsync(studentRegno, userIp, status);
                return Ok(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating login log for student {StudentRegno}", studentRegno);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("logout")]
        public async Task<IActionResult> UpdateUserlogLogout([FromQuery] string studentRegno)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentRegno))
                {
                    return BadRequest("Student registration number is required.");
                }

                var logoutDate = DateTime.UtcNow;
                var result = await _standaloneService.UpdateUserlogLogoutAsync(studentRegno, logoutDate);
                if (!result)
                {
                    return NotFound("User log not found or logout update failed.");
                }

                return Ok(new { message = "Logout recorded successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating logout for student {StudentRegno}", studentRegno);
                return StatusCode(500, "An error occurred while recording logout.");
            }
        }

        [HttpPut("my-profile")]
        public async Task<IActionResult> UpdateStudentProfile([FromQuery] string studentRegno, [FromQuery] string studentName, [FromQuery] string? studentPhoto, [FromQuery] decimal cgpa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentRegno) || string.IsNullOrWhiteSpace(studentName))
                {
                    return BadRequest("Student registration number and name are required.");
                }

                var result = await _standaloneService.UpdateStudentProfileAsync(studentRegno, studentName, studentPhoto?? "", cgpa);
                if (!result)
                {
                    return NotFound("Student not found or profile update failed.");
                }

                return Ok(new { message = "Profile updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for student {StudentRegno}", studentRegno);
                return StatusCode(500, "An error occurred while updating the profile.");
            }
        }

        [HttpGet("pincode-verification")]
        public async Task<IActionResult> GetStudentByPincode([FromQuery] string pincode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pincode))
                {
                    return BadRequest("Pincode is required.");
                }

                var student = await _standaloneService.GetStudentByPincodeAsync(pincode);
                if (student == null)
                {
                    return NotFound("Student not found with the provided pincode.");
                }

                return Ok(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student by pincode");
                return StatusCode(500, "An error occurred while verifying the pincode.");
            }
        }

        [HttpGet("print")]
        public async Task<IActionResult> GetPrintableCourseEnrollmentDetails([FromQuery] string studentRegno)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentRegno))
                {
                    return BadRequest("Student registration number is required.");
                }

                var details = await _standaloneService.GetPrintableCourseEnrollmentDetailsAsync(studentRegno);
                if (details == null)
                {
                    return NotFound("Enrollment details not found for the student.");
                }

                return Ok(details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting printable enrollment details for student {StudentRegno}", studentRegno);
                return StatusCode(500, "An error occurred while retrieving enrollment details.");
            }
        }
    }
}
