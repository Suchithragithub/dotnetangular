import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { StudentService } from '../../core/services/student.service';
import {
  AvailabilityResponse,
  CheckAvailabilityRequest,
  CreateEnrollmentRequest,
  EnrollmentService
} from '../../core/services/enrollment.service';
import { SessionService } from '../../core/services/session.service';
import { DepartmentService } from '../../core/services/department.service';
import { LevelService } from '../../core/services/level.service';
import { SemesterService } from '../../core/services/semester.service';
import { CourseService } from '../../core/services/course.service';
import { AuthService } from '../../core/services/auth.service';
import { Student } from '../../core/models/student.model';
import { Session } from '../../core/models/session.model';
import { Department } from '../../core/models/department.model';
import { Level } from '../../core/models/level.model';
import { Semester } from '../../core/models/semester.model';
import { Course } from '../../core/models/course.model';

@Component({
  selector: 'app-enroll',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './enroll.component.html',
  styleUrls: ['./enroll.component.scss']
})
export class EnrollComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private studentService = inject(StudentService);
  private enrollmentService = inject(EnrollmentService);
  private sessionService = inject(SessionService);
  private departmentService = inject(DepartmentService);
  private levelService = inject(LevelService);
  private semesterService = inject(SemesterService);
  private courseService = inject(CourseService);
  private authService = inject(AuthService);

  loading = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);
  pincodeVerified = signal(false);
  verifiedPincode = signal<string | null>(null);
  studentData = signal<Student | null>(null);

  sessions = signal<Session[]>([]);
  departments = signal<Department[]>([]);
  levels = signal<Level[]>([]);
  semesters = signal<Semester[]>([]);
  courses = signal<Course[]>([]);

  pincodeForm: FormGroup;
  enrollmentForm: FormGroup;

  constructor() {
    this.pincodeForm = this.fb.group({
      studentregno: ['', [Validators.required]],
      Pincode: ['', [Validators.required]]
    });

    this.enrollmentForm = this.fb.group({
      session: ['', [Validators.required]],
      department: ['', [Validators.required]],
      level: ['', [Validators.required]],
      sem: ['', [Validators.required]],
      course: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.loadDropdownData();
  }

  private loadDropdownData(): void {
    this.loading.set(true);
    this.error.set(null);

    Promise.all([
      this.sessionService.getSessionsForEnrollment().toPromise(),       // ← new method
      this.departmentService.getDepartmentsForEnrollment().toPromise(), // ← new method
      this.levelService.getLevelsForEnrollment().toPromise(),           // ← new method
      this.semesterService.getSemestersForEnrollment().toPromise(),     // ← new method
      this.courseService.getCoursesForEnrollment().toPromise()          // ← new method
    ])
      .then(([sessions, departments, levels, semesters, courses]) => {
        this.sessions.set(sessions || []);
        this.departments.set(departments || []);
        this.levels.set(levels || []);
        this.semesters.set(semesters || []);
        this.courses.set(courses || []);
        this.loading.set(false);
      })
      .catch((err) => {
        this.error.set('Failed to load enrollment options. Please try again.');
        this.loading.set(false);
        console.error('Error loading dropdown data:', err);
      });
  }

  onVerifyPincode(): void {
    if (this.pincodeForm.invalid) {
      Object.keys(this.pincodeForm.controls).forEach(key => {
        this.pincodeForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.success.set(null);

    const regno = this.pincodeForm.get('studentregno')?.value;
    const pincode = this.pincodeForm.get('Pincode')?.value;

    this.studentService.getStudentByRegno(regno).subscribe({
      next: (student: Student) => {
        if (student.pincode === pincode) {
          this.studentData.set(student);
          this.pincodeVerified.set(true);
          this.verifiedPincode.set(pincode);
          this.success.set('Pincode verified successfully. Please complete the enrollment form.');
          this.loading.set(false);
        } else {
          this.error.set('Invalid pincode. Please try again.');
          this.loading.set(false);
        }
      },
      error: (err) => {
        this.error.set('Student not found or invalid registration number.');
        this.loading.set(false);
        console.error('Error verifying pincode:', err);
      }
    });
  }

  onSubmitEnrollment(): void {
    if (this.enrollmentForm.invalid) {
      Object.keys(this.enrollmentForm.controls).forEach(key => {
        this.enrollmentForm.get(key)?.markAsTouched();
      });
      return;
    }

    if (!this.studentData()) {
      this.error.set('Student data not found. Please verify pincode first.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.success.set(null);

    this.createEnrollment();
  }

  private createEnrollment(): void {
    const formValue = this.enrollmentForm.value;
    const studentregno = this.studentData()!.studentRegno;
    const pincode = this.verifiedPincode();

    if (!pincode) {
      this.error.set('Pincode verification has expired. Please verify again.');
      this.loading.set(false);
      return;
    }

    const enrollmentData: CreateEnrollmentRequest = {
      studentregno: studentregno,
      pincode: pincode,
      sessionId: parseInt(formValue.session),
      departmentId: parseInt(formValue.department),
      levelId: parseInt(formValue.level),
      semesterId: parseInt(formValue.sem),
      courseId: parseInt(formValue.course)
    };

    this.enrollmentService.createEnrollment(enrollmentData).subscribe({
      next: (response) => {
        this.success.set('Enrollment successful! You have been registered for the course.');
        this.loading.set(false);
        this.enrollmentForm.reset();
        
        setTimeout(() => {
          this.router.navigate(['/student/enrollments']);
        }, 2000);
      },
      error: (err) => {
        this.loading.set(false);
        console.log('❌ Enrollment error:', err);
        if (err.status === 400) {
          this.error.set(err.error?.message || 'Enrollment failed. You may already be enrolled or seats are full.');
        } else if (err.status === 401) {
          this.error.set('Session expired. Please log in again.');
        } else {
          this.error.set('Failed to create enrollment. Please try again.');
        }
      }
    });
  }

  resetForm(): void {
    this.pincodeForm.reset();
    this.enrollmentForm.reset();
    this.pincodeVerified.set(false);
    this.verifiedPincode.set(null);
    this.studentData.set(null);
    this.error.set(null);
    this.success.set(null);
  }
}
