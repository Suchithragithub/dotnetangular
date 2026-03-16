import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { EnrollmentService } from '../../../../core/services/enrollment.service';
import { CourseService } from '../../../../core/services/course.service';
import { Course } from '../../../../core/models/course.model';
import { CourseEnroll } from '../../../../core/models/course-enroll.model';
import { Student } from '../../../../core/models/student.model';
import { Session } from '../../../../core/models/session.model';
import { Department } from '../../../../core/models/department.model';
import { Level } from '../../../../core/models/level.model';
import { Semester } from '../../../../core/models/semester.model';

interface EnrollmentPrintData {
  course: Course;
  enrollments: Array<{
    enrollment: CourseEnroll;
    student: Student;
    session: Session;
    department: Department;
    level: Level;
    semester: Semester;
  }>;
}

@Component({
  selector: 'app-admin-print',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './print.component.html',
  styleUrls: ['./print.component.scss']
})
export class AdminPrintComponent implements OnInit {
  private enrollmentService = inject(EnrollmentService);
  private courseService = inject(CourseService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  loading = signal<boolean>(false);
  error = signal<string | null>(null);
  printData = signal<EnrollmentPrintData | null>(null);
  courseId = signal<number | null>(null);
  currentDate = signal<Date>(new Date());

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const cid = params['cid'];
      if (cid) {
        this.courseId.set(+cid);
        this.loadPrintData(+cid);
      } else {
        this.error.set('Course ID is required');
      }
    });
  }

  loadPrintData(courseId: number): void {
    this.loading.set(true);
    this.error.set(null);

    this.enrollmentService.getPrintableCourseEnrollments(courseId).subscribe({
      next: (data: any) => {
        this.printData.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading print data:', err);
        this.error.set('Failed to load enrollment data for printing. Please try again.');
        this.loading.set(false);
      }
    });
  }

  print(): void {
    window.print();
  }

  goBack(): void {
    this.router.navigate(['/admin/enrollments']);
  }

  getTotalEnrollments(): number {
    return this.printData()?.enrollments?.length || 0;
  }

  getAvailableSeats(): number {
    const data = this.printData();
    if (!data) return 0;
    return (data.course.noofSeats  || 0) - this.getTotalEnrollments();
  }
}
