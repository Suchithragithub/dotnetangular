using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModernApiProject.Domain.Entities;
using ModernApiProject.Domain.Repositories;
using ModernApiProject.Infrastructure.Data;

namespace ModernApiProject.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Student entity operations.
    /// Provides asynchronous CRUD operations using Entity Framework Core.
    /// </summary>
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public async Task<Student?> GetByIdAsync(string studentRegno)
        {
            if (string.IsNullOrWhiteSpace(studentRegno))
                throw new ArgumentException("Student registration number cannot be null or empty.", nameof(studentRegno));

            return await _context.Students.FindAsync(studentRegno);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _context.Students.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Student> AddAsync(Student student)
        {
            if (student == null)
                throw new ArgumentNullException(nameof(student));

            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
            return student;
        }

        /// <inheritdoc />
        public async Task<Student> UpdateAsync(Student student)
        {
            if (student == null)
                throw new ArgumentNullException(nameof(student));

            _context.Students.Update(student);
            await _context.SaveChangesAsync();
            return student;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(string studentRegno)
        {
            if (string.IsNullOrWhiteSpace(studentRegno))
                throw new ArgumentException("Student registration number cannot be null or empty.", nameof(studentRegno));

            var student = await _context.Students.FindAsync(studentRegno);
            if (student == null)
                return false;

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}