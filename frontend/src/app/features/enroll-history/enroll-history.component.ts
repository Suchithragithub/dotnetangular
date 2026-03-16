import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { EnrollmentService } from '../../services/enrollment.service';
import { AuthService } from '../../services/auth.service';
import { CourseEnroll } from '../../models/course-enroll.model';
import { Student } from '../../models/student.model';

interface EnrollmentHistoryItem {
  id: number;
  courseCode: string;
  courseName: string;
  courseUnits: number;
  sessionName: string;
  departmentName: string;
  levelName: string;
  semesterName: string;
  enrolledDate?: Date;
}

@Component({
  selector: 'app-enroll-history',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './enroll-history.component.html',
  styleUrls: ['./enroll-history.component.scss']
})
export class EnrollHistoryComponent implements OnInit {
  private enrollmentService = inject(EnrollmentService);
  private authService = inject(AuthService);

  loading = signal<boolean>(false);
  error = signal<string | null>(null);
  enrollments = signal<EnrollmentHistoryItem[]>([]);
  studentInfo = signal<Student | null>(null);
  groupedEnrollments = signal<Map<string, EnrollmentHistoryItem[]>>(new Map());

  ngOnInit(): void {
    this.loadEnrollmentHistory();
  }

  private loadEnrollmentHistory(): void {
    this.loading.set(true);
    this.error.set(null);

    const currentUser = this.authService.getCurrentUser();
    if (!currentUser || !currentUser.regno) {
      this.error.set('Unable to identify logged-in student. Please log in again.');
      this.loading.set(false);
      return;
    }

    const regno = currentUser.regno;

    this.enrollmentService.getEnrollmentsByStudent(regno).subscribe({
      next: (enrollments: CourseEnroll[]) => {
        const historyItems = this.mapEnrollmentsToHistory(enrollments);
        this.enrollments.set(historyItems);
        this.groupEnrollmentsBySession(historyItems);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading enrollment history:', err);
        this.error.set('Failed to load enrollment history. Please try again later.');
        this.loading.set(false);
      }
    });
  }

  private mapEnrollmentsToHistory(enrollments: CourseEnroll[]): EnrollmentHistoryItem[] {
    return enrollments.map(enrollment => ({
      id: enrollment.id || 0,
      courseCode: enrollment.course?.courseCode || 'N/A',
      courseName: enrollment.course?.courseName || 'Unknown Course',
      courseUnits: enrollment.course?.units || 0,
      sessionName: enrollment.session?.sessionName || 'N/A',
      departmentName: enrollment.department?.departmentName || 'N/A',
      levelName: enrollment.level?.levelName || 'N/A',
      semesterName: enrollment.semester?.semesterName || 'N/A',
      enrolledDate: enrollment.createdAt ? new Date(enrollment.createdAt) : undefined
    }));
  }

  private groupEnrollmentsBySession(enrollments: EnrollmentHistoryItem[]): void {
    const grouped = new Map<string, EnrollmentHistoryItem[]>();
    
    enrollments.forEach(enrollment => {
      const key = `${enrollment.sessionName} - ${enrollment.semesterName}`;
      if (!grouped.has(key)) {
        grouped.set(key, []);
      }
      grouped.get(key)!.push(enrollment);
    });

    // Sort groups by session/semester (most recent first)
    const sortedGroups = new Map(
      Array.from(grouped.entries()).sort((a, b) => b[0].localeCompare(a[0]))
    );

    this.groupedEnrollments.set(sortedGroups);
  }

  getTotalUnits(enrollments: EnrollmentHistoryItem[]): number {
    return enrollments.reduce((sum, e) => sum + e.courseUnits, 0);
  }

  getGroupKeys(): string[] {
    return Array.from(this.groupedEnrollments().keys());
  }

  getEnrollmentsByGroup(key: string): EnrollmentHistoryItem[] {
    return this.groupedEnrollments().get(key) || [];
  }

  printHistory(): void {
    window.print();
  }

  retry(): void {
    this.loadEnrollmentHistory();
  }
}