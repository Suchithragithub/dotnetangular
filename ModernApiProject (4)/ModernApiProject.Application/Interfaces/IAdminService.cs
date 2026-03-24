using ModernApiProject.Domain.Entities;
using ModernApiProject.Application.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModernApiProject.Application.Interfaces
{
    public interface IAdminService
    {
        Task<bool> ValidateAdminPasswordAsync(string password);
        Task<bool> UpdateAdminPasswordAsync(string username, string newPassword);
        Task<bool> CheckStudentRegnoAvailabilityAsync(string regno);
        Task<CourseModel> CreateCourseAsync(string courseCode, string courseName, string courseUnit, int seatLimit);
        Task<bool> DeleteCourseWithDependencyCheckAsync(int courseId);
        Task<DepartmentModel> CreateDepartmentAsync(string departmentName);
        Task<bool> DeleteDepartmentAsync(int id);
        Task<CourseModel> UpdateCourseAsync(int id, string courseCode, string courseName, string courseUnit, int seatLimit);
        Task<CourseModel?> GetCourseByIdAsync(int id);
        Task<bool> UpdateStudentProfileAsync(string regid, string studentname, string photo, decimal cgpa);
        Task<StudentModel?> GetStudentByRegnoAsync(string regid);
        Task<AdminModel?> AuthenticateAdminAsync(string username, string password);
        Task<LevelModel> CreateLevelAsync(string levelName);
        Task<bool> DeleteLevelAsync(int id);
        Task<bool> AdminLogoutAsync();
        Task<bool> DeleteStudentAsync(string studentRegno);
        Task<bool> UpdateStudentPasswordAsync(string studentRegno, string newPassword);
        Task<SemesterModel> CreateSemesterAsync(string semesterName);
        Task<bool> DeleteSemesterAsync(int semesterId);
        Task<SessionModel> CreateSessionAsync(string sessionName);
        Task<bool> DeleteSessionAsync(int sessionId);
        Task<StudentModel> CreateStudentAsync(string studentName, string studentRegno, string password, string pincode);

        // --- ADDED MISSING METHODS HERE ---
        Task<IEnumerable<CourseModel>> GetAllCoursesAsync();
        Task<IEnumerable<DepartmentModel>> GetAllDepartmentsAsync();
        Task<IEnumerable<EnrollmentHistoryModel>> GetEnrollmentHistoryAsync();
        Task<IEnumerable<LevelModel>> GetAllLevelsAsync();
        Task<IEnumerable<StudentModel>> GetAllStudentsAsync();
        Task<IEnumerable<CourseEnrollmentPrintModel>> GetPrintableCourseEnrollmentDetailsAsync(int courseId);
        Task<IEnumerable<SemesterModel>> GetAllSemestersAsync();
        Task<IEnumerable<SessionModel>> GetAllSessionsAsync();
        Task<IEnumerable<UserlogModel>> GetAllUserlogsAsync();
        
    }
}