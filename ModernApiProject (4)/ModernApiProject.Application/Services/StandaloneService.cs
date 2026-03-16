using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernApiProject.Application.Models;
using ModernApiProject.Application.Interfaces;
using ModernApiProject.Domain.Entities;
using ModernApiProject.Infrastructure.Data;

namespace ModernApiProject.Application.Services
{
    public class StandaloneService : IStandaloneService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StandaloneService> _logger;

        public StandaloneService(ApplicationDbContext context, ILogger<StandaloneService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
/// Validates if the provided password matches the student's current password.
/// </summary>
/// <param name="studentRegno">The student registration number.</param>
/// <param name="password">The password to validate.</param>
/// <returns>True if the password matches; otherwise, false.</returns>
public async Task<bool> ValidateStudentPasswordAsync(string studentRegno, string password)
{
    if (string.IsNullOrWhiteSpace(studentRegno))
    {
        throw new ArgumentException("Student registration number cannot be null or empty.", nameof(studentRegno));
    }

    if (string.IsNullOrWhiteSpace(password))
    {
        throw new ArgumentException("Password cannot be null or empty.", nameof(password));
    }

    try
    {
        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentRegno == studentRegno && s.Password == password);

        return student != null;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error validating student password for registration number: {StudentRegno}", studentRegno);
        throw;
    }
}

        /// <summary>
/// Updates the student's password.
/// </summary>
/// <param name="studentRegno">The student registration number.</param>
/// <param name="newPassword">The new password to set.</param>
/// <returns>True if the password was updated successfully; otherwise, false.</returns>
public async Task<bool> UpdateStudentPasswordAsync(string studentRegno, string newPassword)
{
    if (string.IsNullOrWhiteSpace(studentRegno))
    {
        throw new ArgumentException("Student registration number cannot be null or empty.", nameof(studentRegno));
    }

    if (string.IsNullOrWhiteSpace(newPassword))
    {
        throw new ArgumentException("New password cannot be null or empty.", nameof(newPassword));
    }

    try
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StudentRegno == studentRegno);

        if (student == null)
        {
            _logger.LogWarning("Student not found for registration number: {StudentRegno}", studentRegno);
            return false;
        }

        student.Password = newPassword;
        student.UpdationDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        _context.Students.Update(student);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Password updated successfully for student: {StudentRegno}", studentRegno);
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating password for student: {StudentRegno}", studentRegno);
        throw;
    }
}

        /// <summary>
/// Checks if a student is already enrolled in a specific course.
/// </summary>
/// <param name="studentRegno">The student registration number.</param>
/// <param name="courseId">The course identifier.</param>
/// <returns>True if the student is enrolled in the course; otherwise, false.</returns>
public async Task<bool> IsStudentEnrolledInCourseAsync(string studentRegno, int courseId)
{
    if (string.IsNullOrWhiteSpace(studentRegno))
    {
        throw new ArgumentException("Student registration number cannot be null or empty.", nameof(studentRegno));
    }

    try
    {
        var enrollment = await _context.Courseenrolls
            .AsNoTracking()
            .FirstOrDefaultAsync(ce => ce.Course == courseId && ce.StudentRegno == studentRegno);

        return enrollment != null;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error checking enrollment for student {StudentRegno} in course {CourseId}", studentRegno, courseId);
        throw;
    }
}

        /// <summary>
/// Gets the total number of students enrolled in a specific course.
/// </summary>
/// <param name="courseId">The course identifier.</param>
/// <returns>The count of enrolled students.</returns>
public async Task<int> GetCourseEnrollmentCountAsync(int courseId)
{
    try
    {
        var count = await _context.Courseenrolls
            .AsNoTracking()
            .CountAsync(ce => ce.Course == courseId);

        return count;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting enrollment count for course: {CourseId}", courseId);
        throw;
    }
}

        /// <summary>
/// Gets the number of available seats for a specific course.
/// </summary>
/// <param name="courseId">The course identifier.</param>
/// <returns>The number of available seats, or -1 if the course is not found.</returns>
public async Task<int> GetCourseAvailableSeatsAsync(int courseId)
{
    try
    {
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
        {
            _logger.LogWarning("Course not found: {CourseId}", courseId);
            return -1;
        }

        var enrollmentCount = await _context.Courseenrolls
            .AsNoTracking()
            .CountAsync(ce => ce.Course == courseId);

        var availableSeats = course.NoofSeats - enrollmentCount;

        return availableSeats > 0 ? availableSeats : 0;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error calculating available seats for course: {CourseId}", courseId);
        throw;
    }
}

        public async Task<IEnumerable<CourseenrollModel>> GetEnrollmentHistoryByStudentAsync(string studentRegno)
{
    if (string.IsNullOrWhiteSpace(studentRegno))
    {
        throw new ArgumentException("Student registration number cannot be null or empty.", nameof(studentRegno));
    }

    var enrollments = await _context.Courseenrolls
        .Where(ce => ce.StudentRegno == studentRegno)
        .Join(_context.Courses,
            ce => ce.Course,
            c => c.Id,
            (ce, c) => new { ce, c })
        .Join(_context.Sessions,
            x => x.ce.Session,
            s => s.Id,
            (x, s) => new { x.ce, x.c, s })
        .Join(_context.Departments,
            x => x.ce.Department,
            d => d.Id,
            (x, d) => new { x.ce, x.c, x.s, d })
        .Join(_context.Levels,
            x => x.ce.Level,
            l => l.Id,
            (x, l) => new { x.ce, x.c, x.s, x.d, l })
        .Join(_context.Semesters,
            x => x.ce.Semester,
            sem => sem.Id,
            (x, sem) => new CourseenrollModel
            {
                Id = x.ce.Id,
                StudentRegno = x.ce.StudentRegno,
                Pincode = x.ce.Pincode,
                Session = x.ce.Session,
                Department = x.ce.Department,
                Level = x.ce.Level,
                Semester = x.ce.Semester,
                Course = x.ce.Course,
                EnrollDate = x.ce.EnrollDate
            })
        .ToListAsync();

    return enrollments;
}

        public async Task<CourseenrollModel> CreateEnrollmentAsync(string studentRegno, string pincode, int sessionId, int departmentId, int levelId, int courseId, int semesterId)
{
    if (string.IsNullOrWhiteSpace(studentRegno))
    {
        throw new ArgumentException("Student registration number cannot be null or empty.", nameof(studentRegno));
    }

    if (string.IsNullOrWhiteSpace(pincode))
    {
        throw new ArgumentException("Pincode cannot be null or empty.", nameof(pincode));
    }

    // Check if student is already enrolled in this course
    var existingEnrollment = await _context.Courseenrolls
        .AnyAsync(ce => ce.StudentRegno == studentRegno && ce.Course == courseId);

    if (existingEnrollment)
    {
        throw new InvalidOperationException("Student is already enrolled in this course.");
    }

    // Verify student exists and pincode matches
    var student = await _context.Students
        .FirstOrDefaultAsync(s => s.StudentRegno == studentRegno && s.Pincode == pincode);

    if (student == null)
    {
        throw new InvalidOperationException("Invalid student registration number or pincode.");
    }

    // Check course seat availability
    var course = await _context.Courses.FindAsync(courseId);
    if (course == null)
    {
        throw new InvalidOperationException("Course not found.");
    }

    var enrollmentCount = await _context.Courseenrolls.CountAsync(ce => ce.Course == courseId);
    if (enrollmentCount >= course.NoofSeats)
    {
        throw new InvalidOperationException("Course has reached maximum enrollment capacity.");
    }

    var enrollment = new Courseenroll
    {
        StudentRegno = studentRegno,
        Pincode = pincode,
        Session = sessionId,
        Department = departmentId,
        Level = levelId,
        Course = courseId,
        Semester = semesterId,
        EnrollDate = DateTime.UtcNow
    };

    await _context.Courseenrolls.AddAsync(enrollment);
    await _context.SaveChangesAsync();

    return new CourseenrollModel
    {
        Id = enrollment.Id,
        StudentRegno = enrollment.StudentRegno,
        Pincode = enrollment.Pincode,
        Session = enrollment.Session,
        Department = enrollment.Department,
        Level = enrollment.Level,
        Semester = enrollment.Semester,
        Course = enrollment.Course,
        EnrollDate = enrollment.EnrollDate
    };
}

        public async Task<StudentModel?> GetStudentByRegnoAsync(string studentRegno)
{
    if (string.IsNullOrWhiteSpace(studentRegno))
    {
        throw new ArgumentException("Student registration number cannot be null or empty.", nameof(studentRegno));
    }

    var student = await _context.Students
        .AsNoTracking()
        .FirstOrDefaultAsync(s => s.StudentRegno == studentRegno);

    if (student == null)
    {
        return null;
    }

    return new StudentModel
    {
        StudentRegno = student.StudentRegno,
        StudentPhoto = student.StudentPhoto,
        Password = student.Password,
        StudentName = student.StudentName,
        Pincode = student.Pincode,
        Session = student.Session,
        Department = student.Department,
        Semester = student.Semester,
        Cgpa = student.Cgpa,
        Creationdate = student.Creationdate,
        UpdationDate = student.UpdationDate
    };
}

        public async Task<IEnumerable<SessionModel>> GetAllSessionsAsync()
{
    var sessions = await _context.Sessions
        .AsNoTracking()
        .OrderBy(s => s.Id)
        .ToListAsync();

    return sessions.Select(s => new SessionModel
    {
        Id = s.Id,
        SessionName = s.SessionName,
        CreationDate = s.CreationDate
    });
}

        public async Task<IEnumerable<DepartmentModel>> GetAllDepartmentsAsync()
{
    var departments = await _context.Departments
        .AsNoTracking()
        .OrderBy(d => d.DepartmentName)
        .ToListAsync();

    return departments.Select(d => new DepartmentModel
    {
        Id = d.Id,
        DepartmentName = d.DepartmentName,
        CreationDate = d.CreationDate
    });
}

        /// <summary>
/// Retrieves all academic levels available for course enrollment.
/// </summary>
/// <returns>A collection of level DTOs.</returns>
public async Task<IEnumerable<LevelModel>> GetAllLevelsForEnrollmentAsync()
{
    _logger.LogInformation("Retrieving all levels for enrollment");
    
    var levels = await _context.Levels
        .AsNoTracking()
        .OrderBy(l => l.Id)
        .ToListAsync();
    
    return levels.Select(l => new LevelModel
    {
        Id = l.Id,
        LevelName = l.LevelName,
        CreationDate = l.CreationDate
    });
}

        /// <summary>
/// Retrieves all semesters available for course enrollment.
/// </summary>
/// <returns>A collection of semester DTOs.</returns>
public async Task<IEnumerable<SemesterModel>> GetAllSemestersForEnrollmentAsync()
{
    _logger.LogInformation("Retrieving all semesters for enrollment");
    
    var semesters = await _context.Semesters
        .AsNoTracking()
        .OrderBy(s => s.Id)
        .ToListAsync();
    
    return semesters.Select(s => new SemesterModel
    {
        Id = s.Id,
        SemesterName = s.SemesterName,
        CreationDate = s.CreationDate,
        UpdationDate = s.UpdationDate
    });
}

        /// <summary>
/// Retrieves all courses available for enrollment.
/// </summary>
/// <returns>A collection of course DTOs.</returns>
public async Task<IEnumerable<CourseModel>> GetAllCoursesForEnrollmentAsync()
{
    _logger.LogInformation("Retrieving all courses for enrollment");
    
    var courses = await _context.Courses
        .AsNoTracking()
        .OrderBy(c => c.CourseName)
        .ToListAsync();
    
    return courses.Select(c => new CourseModel
    {
        Id = c.Id,
        CourseCode = c.CourseCode,
        CourseName = c.CourseName,
        CourseUnit = c.CourseUnit,
        NoofSeats = c.NoofSeats,
        CreationDate = c.CreationDate,
        UpdationDate = c.UpdationDate
    });
}

        /// <summary>
/// Authenticates a student using registration number and password.
/// </summary>
/// <param name="regno">The student registration number.</param>
/// <param name="password">The student password.</param>
/// <returns>The authenticated student DTO if credentials are valid; otherwise, null.</returns>
public async Task<StudentModel?> AuthenticateStudentAsync(string regno, string password)
{
    if (string.IsNullOrWhiteSpace(regno))
    {
        _logger.LogWarning("Authentication attempted with empty registration number");
        return null;
    }
    
    if (string.IsNullOrWhiteSpace(password))
    {
        _logger.LogWarning("Authentication attempted with empty password for regno: {Regno}", regno);
        return null;
    }
    
    _logger.LogInformation("Authenticating student with regno: {Regno}", regno);
    
    var student = await _context.Students
        .AsNoTracking()
        .FirstOrDefaultAsync(s => s.StudentRegno == regno && s.Password == password);
    
    if (student == null)
    {
        _logger.LogWarning("Authentication failed for regno: {Regno}", regno);
        return null;
    }
    
    _logger.LogInformation("Authentication successful for regno: {Regno}", regno);
    
    return new StudentModel
    {
        StudentRegno = student.StudentRegno,
        StudentPhoto = student.StudentPhoto,
        Password = student.Password,
        StudentName = student.StudentName,
        Pincode = student.Pincode,
        Session = student.Session,
        Department = student.Department,
        Semester = student.Semester,
        Cgpa = student.Cgpa,
        Creationdate = student.Creationdate,
        UpdationDate = student.UpdationDate
    };
}

        /// <summary>
/// Creates a user login log entry for tracking student authentication activity.
/// </summary>
/// <param name="studentRegno">The student registration number.</param>
/// <param name="userIp">The user's IP address as byte array.</param>
/// <param name="status">The login status (1 for success, 0 for failure).</param>
/// <returns>The created userlog DTO.</returns>
public async Task<UserlogModel> CreateUserLoginLogAsync(string studentRegno, byte[] userIp, int status)
{
    if (string.IsNullOrWhiteSpace(studentRegno))
    {
        throw new ArgumentException("Student registration number cannot be null or empty.", nameof(studentRegno));
    }
    
    if (userIp == null || userIp.Length == 0)
    {
        throw new ArgumentException("User IP address cannot be null or empty.", nameof(userIp));
    }
    
    _logger.LogInformation("Creating user login log for student: {StudentRegno}, Status: {Status}", studentRegno, status);
    
    var userlog = new Userlog
    {
        StudentRegno = studentRegno,
        Userip = userIp,
        LoginTime = DateTime.UtcNow,
        Status = status,
        Logout = null
    };
    
    await _context.Userlogs.AddAsync(userlog);
    await _context.SaveChangesAsync();
    
    _logger.LogInformation("User login log created with ID: {Id}", userlog.Id);
    
    return new UserlogModel
    {
        Id = userlog.Id,
        StudentRegno = userlog.StudentRegno,
        Userip = userlog.Userip,
        LoginTime = userlog.LoginTime,
        Logout = userlog.Logout,
        Status = userlog.Status
    };
}

        public async Task<bool> UpdateUserlogLogoutAsync(string studentRegno, DateTime logoutDate)
{
    if (string.IsNullOrWhiteSpace(studentRegno))
    {
        _logger.LogWarning("UpdateUserlogLogoutAsync called with null or empty studentRegno");
        return false;
    }

    try
    {
        var userlog = await _context.Userlogs
            .Where(u => u.StudentRegno == studentRegno && string.IsNullOrEmpty(u.Logout))
            .OrderByDescending(u => u.LoginTime)
            .FirstOrDefaultAsync();

        if (userlog == null)
        {
            _logger.LogWarning($"No active userlog found for student {studentRegno}");
            return false;
        }

        userlog.Logout = logoutDate.ToString("yyyy-MM-dd HH:mm:ss");
        _context.Userlogs.Update(userlog);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Updated logout time for student {studentRegno}");
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error updating userlog logout for student {studentRegno}");
        return false;
    }
}

        public async Task<bool> UpdateStudentProfileAsync(string studentRegno, string studentName, string studentPhoto, decimal cgpa)
{
    if (string.IsNullOrWhiteSpace(studentRegno))
    {
        _logger.LogWarning("UpdateStudentProfileAsync called with null or empty studentRegno");
        return false;
    }

    try
    {
        var student = await _context.Students.FindAsync(studentRegno);
        if (student == null)
        {
            _logger.LogWarning($"Student {studentRegno} not found for profile update");
            return false;
        }

        student.StudentName = studentName;
        student.StudentPhoto = studentPhoto;
        student.Cgpa = cgpa;
        student.UpdationDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        _context.Students.Update(student);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Updated profile for student {studentRegno}");
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error updating student profile for {studentRegno}");
        return false;
    }
}


        public async Task<StudentModel?> GetStudentByPincodeAsync(string pincode)
{
    if (string.IsNullOrWhiteSpace(pincode))
    {
        _logger.LogWarning("GetStudentByPincodeAsync called with null or empty pincode");
        return null;
    }

    try
    {
        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Pincode == pincode);

        if (student == null)
        {
            _logger.LogWarning($"Student with pincode {pincode} not found");
            return null;
        }

        return new StudentModel
        {
            StudentRegno = student.StudentRegno,
            StudentPhoto = student.StudentPhoto,
            Password = student.Password,
            StudentName = student.StudentName,
            Pincode = student.Pincode,
            Session = student.Session,
            Department = student.Department,
            Semester = student.Semester,
            Cgpa = student.Cgpa,
            Creationdate = student.Creationdate,
            UpdationDate = student.UpdationDate
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error retrieving student by pincode {pincode}");
        return null;
    }
}

        public async Task<CourseEnrollmentDetailModel?> GetPrintableCourseEnrollmentDetailsAsync(string studentRegno)
{
    if (string.IsNullOrWhiteSpace(studentRegno))
    {
        _logger.LogWarning("GetPrintableCourseEnrollmentDetailsAsync called with null or empty studentRegno");
        return null;
    }

    try
    {
        var enrollmentDetails = await (from ce in _context.Courseenrolls
                                       join c in _context.Courses on ce.Course equals c.Id
                                       join sess in _context.Sessions on ce.Session equals sess.Id
                                       join dept in _context.Departments on ce.Department equals dept.Id
                                       join lvl in _context.Levels on ce.Level equals lvl.Id
                                       join stud in _context.Students on ce.StudentRegno equals stud.StudentRegno
                                       join sem in _context.Semesters on ce.Semester equals sem.Id
                                       where ce.StudentRegno == studentRegno
                                       select new CourseEnrollmentDetailModel
                                       {
                                           CourseName = c.CourseName,
                                           CourseCode = c.CourseCode,
                                           CourseUnit = c.CourseUnit,
                                           SessionName = sess.SessionName,
                                           DepartmentName = dept.DepartmentName,
                                           LevelName = lvl.LevelName,
                                           EnrollDate = ce.EnrollDate,
                                           SemesterName = sem.SemesterName,
                                           StudentName = stud.StudentName,
                                           StudentPhoto = stud.StudentPhoto,
                                           Cgpa = stud.Cgpa,
                                           StudentRegistrationDate = stud.Creationdate
                                       }).FirstOrDefaultAsync();

        if (enrollmentDetails == null)
        {
            _logger.LogWarning($"No enrollment details found for student {studentRegno}");
            return null;
        }

        return enrollmentDetails;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error retrieving printable enrollment details for student {studentRegno}");
        return null;
    }
}

    }
}
