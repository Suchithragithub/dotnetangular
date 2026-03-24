using ModernApiProject.Domain.Entities;
using ModernApiProject.Application.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace ModernApiProject.Application.Interfaces
{
    public interface IStandaloneService
    {
        Task<bool> ValidateStudentPasswordAsync(string studentRegno, string password);

        Task<bool> UpdateStudentPasswordAsync(string studentRegno, string currentPassword, string newPassword);

        Task<bool> IsStudentEnrolledInCourseAsync(string studentRegno, int courseId);

        Task<int> GetCourseEnrollmentCountAsync(int courseId);

        Task<int> GetCourseAvailableSeatsAsync(int courseId);

        Task<CourseenrollModel> CreateEnrollmentAsync(string studentRegno, string pincode, int sessionId, int departmentId, int levelId, int courseId, int semesterId);

        Task<StudentModel?> GetStudentByRegnoAsync(string studentRegno);

        Task<StudentModel?> AuthenticateStudentAsync(string regno, string password);

        Task<UserlogModel> CreateUserLoginLogAsync(string studentRegno, byte[] userIp, int status);

        Task<bool> UpdateUserlogLogoutAsync(string studentRegno, DateTime logoutDate);

        Task<bool> UpdateStudentProfileAsync(string studentRegno, string studentName, string? studentPhoto, decimal cgpa);

        Task<StudentModel?> GetStudentByPincodeAsync(string pincode);

        Task<CourseEnrollmentDetailModel?> GetPrintableCourseEnrollmentDetailsAsync(string studentRegno);

        Task<IEnumerable<CourseEnrollmentDetailModel>> GetEnrollmentHistoryByStudentAsync(string studentRegno);
        Task<IEnumerable<SessionModel>> GetAllSessionsAsync();
        Task<IEnumerable<DepartmentModel>> GetAllDepartmentsAsync();
        Task<IEnumerable<LevelModel>> GetAllLevelsForEnrollmentAsync();
        Task<IEnumerable<SemesterModel>> GetAllSemestersForEnrollmentAsync();
        Task<IEnumerable<CourseModel>> GetAllCoursesForEnrollmentAsync();
    }
}
