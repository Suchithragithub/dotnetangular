import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { EnrollmentService } from '../../core/services/enrollment.service';
import { Student } from '../../core/models/student.model';
import { Course } from '../../core/models/course.model';
import { Session } from '../../core/models/session.model';
import { Department } from '../../core/models/department.model';
import { Level } from '../../core/models/level.model';
import { Semester } from '../../core/models/semester.model';

interface EnrollmentPrintData {
  student: Student;
  course: Course;
  session: Session;
  department: Department;
  level: Level;
  semester: Semester;
  enrollmentDate?: string;
}

@Component({
  selector: 'app-print',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './print.component.html',
  styleUrls: ['./print.component.scss']
})
export class PrintComponent implements OnInit {
  private enrollmentService = inject(EnrollmentService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  loading = signal(false);
  error = signal<string | null>(null);
  enrollmentData = signal<EnrollmentPrintData | null>(null);
  regno = signal<string>('');

  ngOnInit(): void {
    const regnoParam = this.route.snapshot.paramMap.get('regno');
    
    if (!regnoParam) {
      this.error.set('Registration number is required');
      return;
    }

    this.regno.set(regnoParam);
    this.loadEnrollmentData(regnoParam);
  }

  // loadEnrollmentData(regno: string): void {
  //   this.loading.set(true);
  //   this.error.set(null);

  //   this.enrollmentService.getPrintableEnrollment(regno).subscribe({
  //     next: (data) => {
  //       this.enrollmentData.set(data);
  //       this.loading.set(false);
  //     },
  //     error: (err) => {
  //       console.error('Error loading enrollment data:', err);
  //       this.error.set(err.error?.message || 'Failed to load enrollment details. Please try again.');
  //       this.loading.set(false);
  //     }
  //   });
  // }

  loadEnrollmentData(regno: string): void {
    this.loading.set(true);
    this.error.set(null);

    this.enrollmentService.getPrintableEnrollment(regno).subscribe({
      next: (data) => {

        if (!data || data.length === 0) {
          this.error.set('No enrollment found');
          this.loading.set(false);
          return;
        }

        const enrollment = data[0];

        this.enrollmentData.set({
          student: enrollment.student,
          course: enrollment.course,
          session: enrollment.session,
          department: enrollment.department,
          level: enrollment.level,
          semester: enrollment.semester,
          enrollmentDate: enrollment.enrollment?.enrollDate
        });

        this.loading.set(false);
      },

      error: (err) => {
        console.error('Error loading enrollment data:', err);
        this.error.set(err.error?.message || 'Failed to load enrollment details.');
        this.loading.set(false);
      }
    });
  }

  printPage(): void {
    window.print();
  }

  goBack(): void {
    this.router.navigate(['/enrollments']);
  }

  getCurrentDate(): string {
    return new Date().toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
}
