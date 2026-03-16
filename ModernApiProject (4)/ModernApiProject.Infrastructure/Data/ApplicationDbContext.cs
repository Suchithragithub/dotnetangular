using Microsoft.EntityFrameworkCore;
using ModernApiProject.Domain.Entities;
using System;

namespace ModernApiProject.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Courseenroll> Courseenrolls { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Userlog> Userlogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table name mappings
            modelBuilder.Entity<Admin>().ToTable("admin");
            modelBuilder.Entity<Course>().ToTable("course");
            modelBuilder.Entity<Courseenroll>().ToTable("courseenrolls");
            modelBuilder.Entity<Department>().ToTable("department");
            modelBuilder.Entity<Level>().ToTable("level");
            modelBuilder.Entity<News>().ToTable("news");
            modelBuilder.Entity<Semester>().ToTable("semester");
            modelBuilder.Entity<Session>().ToTable("session");
            modelBuilder.Entity<Student>().ToTable("students");
            modelBuilder.Entity<Userlog>().ToTable("userlog");

            // Primary key configurations
            modelBuilder.Entity<Admin>().HasKey(a => a.Id);
            modelBuilder.Entity<Course>().HasKey(c => c.Id);
            modelBuilder.Entity<Courseenroll>().HasKey(ce => ce.Id);
            modelBuilder.Entity<Department>().HasKey(d => d.Id);
            modelBuilder.Entity<Level>().HasKey(l => l.Id);
            modelBuilder.Entity<News>().HasKey(n => n.Id);
            modelBuilder.Entity<Semester>().HasKey(s => s.Id);
            modelBuilder.Entity<Session>().HasKey(s => s.Id);
            modelBuilder.Entity<Student>().HasKey(s => s.StudentRegno);
            modelBuilder.Entity<Userlog>().HasKey(u => u.Id);

            // Column type configurations
            modelBuilder.Entity<Student>()
                .Property(s => s.Cgpa)
                .HasColumnType("decimal(10,2)");

            // Relationships
            modelBuilder.Entity<Courseenroll>()
                .HasOne<Student>()
                .WithMany()
                .HasForeignKey(ce => ce.StudentRegno)
                .HasPrincipalKey(s => s.StudentRegno);

            modelBuilder.Entity<Courseenroll>()
                .HasOne<Session>()
                .WithMany()
                .HasForeignKey(ce => ce.Session)
                .HasPrincipalKey(s => s.Id);

            modelBuilder.Entity<Courseenroll>()
                .HasOne<Department>()
                .WithMany()
                .HasForeignKey(ce => ce.Department)
                .HasPrincipalKey(d => d.Id);

            modelBuilder.Entity<Courseenroll>()
                .HasOne<Level>()
                .WithMany()
                .HasForeignKey(ce => ce.Level)
                .HasPrincipalKey(l => l.Id);

            modelBuilder.Entity<Courseenroll>()
                .HasOne<Semester>()
                .WithMany()
                .HasForeignKey(ce => ce.Semester)
                .HasPrincipalKey(s => s.Id);

            modelBuilder.Entity<Courseenroll>()
                .HasOne<Course>()
                .WithMany()
                .HasForeignKey(ce => ce.Course)
                .HasPrincipalKey(c => c.Id);
        }
    }
}
