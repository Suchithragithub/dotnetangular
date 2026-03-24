using ModernApiProject.Domain.Repositories;
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
    public class AdminService : IAdminService
    {
        private readonly ILevelRepository _levelRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminService> _logger;

        public AdminService(ApplicationDbContext context, ILogger<AdminService> logger, ILevelRepository levelRepository, IStudentRepository studentRepository)
        {
            _context = context;
            _logger = logger;
            _levelRepository = levelRepository;
            _studentRepository = studentRepository;

        }

        public async Task<bool> ValidateAdminPasswordAsync(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            }

            var admin = await _context.Admins
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Password == password);

            return admin != null;
        }

            // AFTER (correct — looks up admin by username)
        public async Task<bool> UpdateAdminPasswordAsync(string username, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));
    
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("New password cannot be null or empty.", nameof(newPassword));
    
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Username == username);  // ← FIXED
    
            if (admin == null)
            {
                _logger.LogWarning("Admin with username {Username} not found during password update.", username);
                return false;
            }
    
            admin.Password = newPassword;
            admin.UpdationDate = DateTime.UtcNow;
    
            _context.Admins.Update(admin);
            await _context.SaveChangesAsync();
    
            _logger.LogInformation("Password updated successfully for admin {Username}.", username);
            return true;
        }

        public async Task<bool> CheckStudentRegnoAvailabilityAsync(string regno)
        {
            if (string.IsNullOrWhiteSpace(regno))
            {
                throw new ArgumentException("Registration number cannot be null or empty.", nameof(regno));
            }

            var exists = await _context.Students
                .AsNoTracking()
                .AnyAsync(s => s.StudentRegno == regno);

            return exists;
        }

        public async Task<CourseModel> CreateCourseAsync(string courseCode, string courseName, string courseUnit, int seatLimit)
        {
            if (string.IsNullOrWhiteSpace(courseCode))
            {
                throw new ArgumentException("Course code cannot be null or empty.", nameof(courseCode));
            }

            if (string.IsNullOrWhiteSpace(courseName))
            {
                throw new ArgumentException("Course name cannot be null or empty.", nameof(courseName));
            }

            if (string.IsNullOrWhiteSpace(courseUnit))
            {
                throw new ArgumentException("Course unit cannot be null or empty.", nameof(courseUnit));
            }

            if (seatLimit <= 0)
            {
                throw new ArgumentException("Seat limit must be greater than zero.", nameof(seatLimit));
            }

            var course = new Course
            {
                CourseCode = courseCode,
                CourseName = courseName,
                CourseUnit = courseUnit,
                NoofSeats = seatLimit,
                CreationDate = DateTime.UtcNow,
                UpdationDate = ""
            };

            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

            return new CourseModel
            {
                Id = course.Id,
                CourseCode = course.CourseCode,
                CourseName = course.CourseName,
                CourseUnit = course.CourseUnit,
                NoofSeats = course.NoofSeats,
                CreationDate = course.CreationDate,
                UpdationDate = course.UpdationDate
            };
        }

        public async Task<bool> DeleteCourseWithDependencyCheckAsync(int courseId)
        {
            if (courseId <= 0)
            {
                throw new ArgumentException("Course ID must be greater than zero.", nameof(courseId));
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return false;
            }

            var hasDependentEnrollments = await _context.Courseenrolls
                .AsNoTracking()
                .AnyAsync(ce => ce.Course == courseId);

            if (hasDependentEnrollments)
            {
                _logger.LogWarning("Cannot delete course {CourseId} because it has dependent enrollment records.", courseId);
                return false;
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<CourseModel>> GetAllCoursesAsync()
        {
            _logger.LogInformation("Retrieving all courses");
            
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

        public async Task<DepartmentModel> CreateDepartmentAsync(string departmentName)
        {
            if (string.IsNullOrWhiteSpace(departmentName))
            {
                throw new ArgumentException("Department name cannot be null or empty.", nameof(departmentName));
            }
            
            _logger.LogInformation("Creating new department: {DepartmentName}", departmentName);
            
            var department = new Department
            {
                DepartmentName = departmentName,
                CreationDate = DateTime.UtcNow
            };
            
            await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();
            
            return new DepartmentModel
            {
                Id = department.Id,
                DepartmentName = department.DepartmentName,
                CreationDate = department.CreationDate
            };
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
{
    _logger.LogInformation("Attempting to delete department with ID: {DepartmentId}", id);
    
    var department = await _context.Departments.FindAsync(id);
    if (department == null)
    {
        _logger.LogWarning("Department with ID {DepartmentId} not found", id);
        return false;
    }
    
    // Check for dependent records in courseenrolls table
    var hasDependentEnrollments = await _context.Courseenrolls
        .AnyAsync(ce => ce.Department == id);
    
    if (hasDependentEnrollments)
    {
        _logger.LogWarning("Cannot delete department {DepartmentId} - dependent course enrollments exist", id);
        return false;
    }
    
    // Check for dependent records in students table (stored as string)
    var hasDependentStudents = await _context.Students
        .AnyAsync(s => s.Department == id.ToString());
    
    if (hasDependentStudents)
    {
        _logger.LogWarning("Cannot delete department {DepartmentId} - dependent students exist", id);
        return false;
    }
    
    _context.Departments.Remove(department);
    await _context.SaveChangesAsync();
    
    _logger.LogInformation("Successfully deleted department with ID: {DepartmentId}", id);
    return true;
}

        public async Task<IEnumerable<DepartmentModel>> GetAllDepartmentsAsync()
{
    _logger.LogInformation("Retrieving all departments");
    
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

        public async Task<CourseModel> UpdateCourseAsync(int id, string courseCode, string courseName, string courseUnit, int seatLimit)
{
    if (string.IsNullOrWhiteSpace(courseCode))
    {
        throw new ArgumentException("Course code cannot be null or empty.", nameof(courseCode));
    }
    
    if (string.IsNullOrWhiteSpace(courseName))
    {
        throw new ArgumentException("Course name cannot be null or empty.", nameof(courseName));
    }
    
    if (string.IsNullOrWhiteSpace(courseUnit))
    {
        throw new ArgumentException("Course unit cannot be null or empty.", nameof(courseUnit));
    }
    
    if (seatLimit < 0)
    {
        throw new ArgumentException("Seat limit cannot be negative.", nameof(seatLimit));
    }
    
    _logger.LogInformation("Updating course with ID: {CourseId}", id);
    
    var course = await _context.Courses.FindAsync(id);
    if (course == null)
    {
        throw new InvalidOperationException($"Course with ID {id} not found.");
    }
    
    course.CourseCode = courseCode;
    course.CourseName = courseName;
    course.CourseUnit = courseUnit;
    course.NoofSeats = seatLimit;
    course.UpdationDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    
    _context.Courses.Update(course);
    await _context.SaveChangesAsync();
    
    _logger.LogInformation("Successfully updated course with ID: {CourseId}", id);
    
    return new CourseModel
    {
        Id = course.Id,
        CourseCode = course.CourseCode,
        CourseName = course.CourseName,
        CourseUnit = course.CourseUnit,
        NoofSeats = course.NoofSeats,
        CreationDate = course.CreationDate,
        UpdationDate = course.UpdationDate
    };
}

        public async Task<CourseModel?> GetCourseByIdAsync(int id)
{
    try
    {
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
        {
            return null;
        }

        return new CourseModel
        {
            Id = course.Id,
            CourseCode = course.CourseCode,
            CourseName = course.CourseName,
            CourseUnit = course.CourseUnit,
            NoofSeats = course.NoofSeats,
            CreationDate = course.CreationDate,
            UpdationDate = course.UpdationDate
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving course with ID {CourseId}", id);
        throw;
    }
}

        public async Task<bool> UpdateStudentProfileAsync(string regid, string studentname, string photo, decimal cgpa)
{
    try
    {
        var student = await _context.Students.FindAsync(regid);
        if (student == null)
        {
            _logger.LogWarning("Student with registration number {RegNo} not found", regid);
            return false;
        }

        student.StudentName = studentname;
        student.StudentPhoto = photo;
        student.Cgpa = cgpa;
        student.UpdationDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        _context.Students.Update(student);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Student profile updated for {RegNo}", regid);
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating student profile for {RegNo}", regid);
        throw;
    }
}

        public async Task<StudentModel?> GetStudentByRegnoAsync(string regid)
{
    try
    {
        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentRegno == regid);

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
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving student with registration number {RegNo}", regid);
        throw;
    }
}

        public async Task<IEnumerable<EnrollmentHistoryModel>> GetEnrollmentHistoryAsync()
{
    try
    {
        var enrollmentHistory = await _context.Courseenrolls
            .AsNoTracking()
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
            .Join(_context.Semesters,
                x => x.ce.Semester,
                sem => sem.Id,
                (x, sem) => new { x.ce, x.c, x.s, x.d, sem })
            .Join(_context.Students,
                x => x.ce.StudentRegno,
                st => st.StudentRegno,
                (x, st) => new EnrollmentHistoryModel
                {
                    CourseId = x.ce.Course,
                    CourseName = x.c.CourseName,
                    SessionName = x.s.SessionName,
                    DepartmentName = x.d.DepartmentName,
                    EnrollDate = x.ce.EnrollDate,
                    SemesterName = x.sem.SemesterName,
                    StudentName = st.StudentName,
                    StudentRegno = st.StudentRegno
                })
            .ToListAsync();

        return enrollmentHistory;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving enrollment history");
        throw;
    }
}

        public async Task<AdminModel?> AuthenticateAdminAsync(string username, string password)
{
    try
    {
        var admin = await _context.Admins
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Username == username && a.Password == password);

        if (admin == null)
        {
            _logger.LogWarning("Failed authentication attempt for username {Username}", username);
            return null;
        }

        return new AdminModel
        {
            Id = admin.Id,
            Username = admin.Username,
            Password = admin.Password,
            CreationDate = admin.CreationDate,
            UpdationDate = admin.UpdationDate
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error authenticating admin with username {Username}", username);
        throw;
    }
}

        public async Task<LevelModel> CreateLevelAsync(string levelName)
{
    if (string.IsNullOrWhiteSpace(levelName))
    {
        throw new ArgumentException("Level name cannot be null or empty.", nameof(levelName));
    }

    var level = new Level
    {
        LevelName = levelName,
        CreationDate = DateTime.UtcNow
    };

    var addedLevel = await _levelRepository.AddAsync(level);

    return new LevelModel
    {
        Id = addedLevel.Id,
        LevelName = addedLevel.LevelName,
        CreationDate = addedLevel.CreationDate
    };
}

        public async Task<bool> DeleteLevelAsync(int id)
{
    // Check for dependent records in courseenrolls table
    var dependentEnrollments = await _context.Courseenrolls
        .Where(ce => ce.Level == id)
        .AnyAsync();

    if (dependentEnrollments)
    {
        _logger.LogWarning("Cannot delete level with ID {LevelId} because dependent course enrollments exist.", id);
        return false;
    }

    var result = await _levelRepository.DeleteAsync(id);
    return result;
}

        public async Task<IEnumerable<LevelModel>> GetAllLevelsAsync()
{
    var levels = await _levelRepository.GetAllAsync();

    return levels.Select(l => new LevelModel
    {
        Id = l.Id,
        LevelName = l.LevelName,
        CreationDate = l.CreationDate
    }).ToList();
}

        public async Task<bool> AdminLogoutAsync()
{
    // In a modern API context, logout is typically handled by token invalidation
    // or session management at the controller/middleware level.
    // This method serves as a placeholder for any cleanup logic needed.
    // The actual session clearing would be done in the authentication layer.
    
    _logger.LogInformation("Admin logout requested.");
    
    // Return true to indicate successful logout request processing
    return await Task.FromResult(true);
}

        public async Task<bool> DeleteStudentAsync(string studentRegno)
{
    if (string.IsNullOrWhiteSpace(studentRegno))
    {
        throw new ArgumentException("Student registration number cannot be null or empty.", nameof(studentRegno));
    }

    // Check for dependent records in courseenrolls table
    var dependentEnrollments = await _context.Courseenrolls
        .Where(ce => ce.StudentRegno == studentRegno)
        .AnyAsync();

    if (dependentEnrollments)
    {
        _logger.LogWarning("Cannot delete student with registration number {StudentRegno} because dependent course enrollments exist.", studentRegno);
        return false;
    }

    // Check for dependent records in userlogs table
    var dependentLogs = await _context.Userlogs
        .Where(ul => ul.StudentRegno == studentRegno)
        .AnyAsync();

    if (dependentLogs)
    {
        _logger.LogWarning("Cannot delete student with registration number {StudentRegno} because dependent user logs exist.", studentRegno);
        return false;
    }

    var result = await _studentRepository.DeleteAsync(studentRegno);
    return result;
}

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

    var student = await _context.Students.FindAsync(studentRegno);
    if (student == null)
    {
        _logger.LogWarning("Student with registration number {StudentRegno} not found.", studentRegno);
        return false;
    }

    student.Password = newPassword;
    student.UpdationDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

    _context.Students.Update(student);
    await _context.SaveChangesAsync();

    _logger.LogInformation("Password updated for student {StudentRegno}.", studentRegno);
    return true;
}

        public async Task<IEnumerable<StudentModel>> GetAllStudentsAsync()
{
    var students = await _context.Students
        .AsNoTracking()
        .ToListAsync();

    return students.Select(s => new StudentModel
    {
        StudentRegno = s.StudentRegno,
        StudentPhoto = s.StudentPhoto,
        Password = s.Password,
        StudentName = s.StudentName,
        Pincode = s.Pincode,
        Session = s.Session,
        Department = s.Department,
        Semester = s.Semester,
        Cgpa = s.Cgpa,
        Creationdate = s.Creationdate,
        UpdationDate = s.UpdationDate
    });
}

        public async Task<IEnumerable<CourseEnrollmentPrintModel>> GetPrintableCourseEnrollmentDetailsAsync(int courseId)
{
    var enrollmentDetails = await _context.Courseenrolls
        .Where(ce => ce.Course == courseId)
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
        .Join(_context.Students,
            x => x.ce.StudentRegno,
            st => st.StudentRegno,
            (x, st) => new { x.ce, x.c, x.s, x.d, x.l, st })
        .Join(_context.Semesters,
            x => x.ce.Semester,
            sem => sem.Id,
            (x, sem) => new CourseEnrollmentPrintModel
            {
                CourseName = x.c.CourseName,
                CourseCode = x.c.CourseCode,
                CourseUnit = x.c.CourseUnit,
                SessionName = x.s.SessionName,
                DepartmentName = x.d.DepartmentName,
                LevelName = x.l.LevelName,
                EnrollDate = x.ce.EnrollDate,
                SemesterName = sem.SemesterName,
                StudentName = x.st.StudentName,
                StudentPhoto = x.st.StudentPhoto,
                StudentCgpa = x.st.Cgpa,
                StudentRegno = x.st.StudentRegno,
                StudentRegistrationDate = x.st.Creationdate
            })
        .AsNoTracking()
        .ToListAsync();

    return enrollmentDetails;
}

        public async Task<SemesterModel> CreateSemesterAsync(string semesterName)
{
    if (string.IsNullOrWhiteSpace(semesterName))
    {
        throw new ArgumentException("Semester name cannot be null or empty.", nameof(semesterName));
    }

    var semester = new Semester
    {
        SemesterName = semesterName,
        CreationDate = DateTime.UtcNow,
        UpdationDate = "" 
    };

    await _context.Semesters.AddAsync(semester);
    await _context.SaveChangesAsync();

    _logger.LogInformation("Semester {SemesterName} created with ID {SemesterId}.", semesterName, semester.Id);

    return new SemesterModel
    {
        Id = semester.Id,
        SemesterName = semester.SemesterName,
        CreationDate = semester.CreationDate,
        UpdationDate = semester.UpdationDate
    };
}

        public async Task<bool> DeleteSemesterAsync(int semesterId)
{
    var hasDependentEnrollments = await _context.Courseenrolls
        .AnyAsync(ce => ce.Semester == semesterId);

    if (hasDependentEnrollments)
    {
        _logger.LogWarning("Cannot delete semester {SemesterId} because it has dependent course enrollments.", semesterId);
        return false;
    }

    var semester = await _context.Semesters.FindAsync(semesterId);
    if (semester == null)
    {
        _logger.LogWarning("Semester with ID {SemesterId} not found.", semesterId);
        return false;
    }

    _context.Semesters.Remove(semester);
    await _context.SaveChangesAsync();

    _logger.LogInformation("Semester {SemesterId} deleted successfully.", semesterId);
    return true;
}

        public async Task<IEnumerable<SemesterModel>> GetAllSemestersAsync()
{
    _logger.LogInformation("Retrieving all semesters");
    
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

        public async Task<SessionModel> CreateSessionAsync(string sessionName)
{
    if (string.IsNullOrWhiteSpace(sessionName))
    {
        throw new ArgumentException("Session name cannot be null or empty.", nameof(sessionName));
    }
    
    _logger.LogInformation("Creating new session: {SessionName}", sessionName);
    
    var session = new Session
    {
        SessionName = sessionName,
        CreationDate = DateTime.UtcNow
    };
    
    await _context.Sessions.AddAsync(session);
    await _context.SaveChangesAsync();
    
    return new SessionModel
    {
        Id = session.Id,
        SessionName = session.SessionName,
        CreationDate = session.CreationDate
    };
}

        public async Task<bool> DeleteSessionAsync(int sessionId)
{
    _logger.LogInformation("Attempting to delete session with ID: {SessionId}", sessionId);
    
    var session = await _context.Sessions.FindAsync(sessionId);
    if (session == null)
    {
        _logger.LogWarning("Session with ID {SessionId} not found", sessionId);
        return false;
    }
    
    var hasDependentEnrollments = await _context.Courseenrolls
        .AnyAsync(ce => ce.Session == sessionId);
    
    if (hasDependentEnrollments)
    {
        _logger.LogWarning("Cannot delete session {SessionId} - dependent course enrollments exist", sessionId);
        return false;
    }
    
    _context.Sessions.Remove(session);
    await _context.SaveChangesAsync();
    
    _logger.LogInformation("Successfully deleted session with ID: {SessionId}", sessionId);
    return true;
}

        public async Task<IEnumerable<SessionModel>> GetAllSessionsAsync()
{
    _logger.LogInformation("Retrieving all sessions");
    
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

        public async Task<StudentModel> CreateStudentAsync(string studentName, string studentRegno, string password, string pincode)
{
    if (string.IsNullOrWhiteSpace(studentName))
    {
        throw new ArgumentException("Student name cannot be null or empty.", nameof(studentName));
    }
    if (string.IsNullOrWhiteSpace(studentRegno))
    {
        throw new ArgumentException("Student registration number cannot be null or empty.", nameof(studentRegno));
    }
    if (string.IsNullOrWhiteSpace(password))
    {
        throw new ArgumentException("Password cannot be null or empty.", nameof(password));
    }
    if (string.IsNullOrWhiteSpace(pincode))
    {
        throw new ArgumentException("Pincode cannot be null or empty.", nameof(pincode));
    }
    
    _logger.LogInformation("Creating new student with registration number: {StudentRegno}", studentRegno);
    
    var existingStudent = await _context.Students.FindAsync(studentRegno);
    if (existingStudent != null)
    {
        throw new InvalidOperationException($"Student with registration number {studentRegno} already exists.");
    }
    
    var student = new Student
    {
        StudentName = studentName,
        StudentRegno = studentRegno,
        Password = password,
        Pincode = pincode,
        Creationdate = DateTime.UtcNow,
        StudentPhoto = string.Empty,
        Session = string.Empty,
        Department = string.Empty,
        Semester = string.Empty,
        Cgpa = 0.0m,
        UpdationDate = null
    };
    
    await _context.Students.AddAsync(student);
    await _context.SaveChangesAsync();
    
    return new StudentModel
    {
        StudentRegno = student.StudentRegno,
        StudentName = student.StudentName,
        Password = student.Password,
        Pincode = student.Pincode,
        StudentPhoto = student.StudentPhoto,
        Session = student.Session,
        Department = student.Department,
        Semester = student.Semester,
        Cgpa = student.Cgpa,
        Creationdate = student.Creationdate,
        UpdationDate = student.UpdationDate
    };
}

        /// <summary>
/// Retrieves all user log entries from the system.
/// </summary>
/// <returns>A collection of all user log DTOs.</returns>
public async Task<IEnumerable<UserlogModel>> GetAllUserlogsAsync()
{
    _logger.LogInformation("Retrieving all user logs");
    
    try
    {
        var userlogs = await _context.Userlogs
            .AsNoTracking()
            .OrderByDescending(u => u.LoginTime)
            .ToListAsync();
        
        var userlogDtos = userlogs.Select(u => new UserlogModel
        {
            Id = u.Id,
            StudentRegno = u.StudentRegno,
            Userip = u.Userip,
            LoginTime = u.LoginTime,
            Logout = u.Logout,
            Status = u.Status
        }).ToList();
        
        _logger.LogInformation("Successfully retrieved {Count} user logs", userlogDtos.Count);
        return userlogDtos;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error occurred while retrieving all user logs");
        throw;
    }
}
    }
}
