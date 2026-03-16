import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { StudentService } from '../../services/student.service';
import { EnrollmentService } from '../../services/enrollment.service';
import { SessionService } from '../../services/session.service';
import { DepartmentService } from '../../services/department.service';
import { LevelService } from '../../services/level.service';
import { SemesterService } from '../../services/semester.service';
import { CourseService } from '../../services/course.service';
import { AuthService } from '../../services/auth.service';
import { Student } from '../../models/student.model';
import { Session } from '../../models/session.model';
import { Department } from '../../models/department.model';
import { Level } from '../../models/level.model';
import { Semester } from '../../models/semester.model';
import { Course } from '../../models/course.model';

interface AvailabilityCheckRequest {
  studentregno: string;
  courseId: number;
  sessionId: number;
  departmentId: number;
  levelId: number;
  semesterId: number;
}

interface AvailabilityCheckResponse {
  available: boolean;
  message?: string;
  alreadyEnrolled?: boolean;
  seatsAvailable?: boolean;
}

interface EnrollmentRequest {
  studentregno: string;
  sessionId: number;
  departmentId: number;
  levelId: number;
  semesterId: number;
  courseId: number;
}

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
      this.sessionService.getSessions().toPromise(),
      this.departmentService.getDepartments().toPromise(),
      this.levelService.getLevels().toPromise(),
      this.semesterService.getSemesters().toPromise(),
      this.courseService.getCourses().toPromise()
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
        if (student.Pincode === pincode) {
          this.studentData.set(student);
          this.pincodeVerified.set(true);
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

    const formValue = this.enrollmentForm.value;
    const studentregno = this.studentData()!.regno;

    const availabilityCheck: AvailabilityCheckRequest = {
      studentregno: studentregno,
      courseId: parseInt(formValue.course),
      sessionId: parseInt(formValue.session),
      departmentId: parseInt(formValue.department),
      levelId: parseInt(formValue.level),
      semesterId: parseInt(formValue.sem)
    };

    this.enrollmentService.checkAvailability(availabilityCheck).subscribe({
      next: (response: AvailabilityCheckResponse) => {
        if (response.available) {
          this.createEnrollment();
        } else {
          if (response.alreadyEnrolled) {
            this.error.set('You are already enrolled in this course for the selected session.');
          } else if (response.seatsAvailable === false) {
            this.error.set('No seats available for this course. Enrollment limit reached.');
          } else {
            this.error.set(response.message || 'Enrollment not available.');
          }
          this.loading.set(false);
        }
      },
      error: (err) => {
        this.error.set('Failed to check enrollment availability. Please try again.');
        this.loading.set(false);
        console.error('Error checking availability:', err);
      }
    });
  }

  private createEnrollment(): void {
    const formValue = this.enrollmentForm.value;
    const studentregno = this.studentData()!.regno;

    const enrollmentData: EnrollmentRequest = {
      studentregno: studentregno,
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
        this.error.set('Failed to create enrollment. Please try again.');
        this.loading.set(false);
        console.error('Error creating enrollment:', err);
      }
    });
  }

  resetForm(): void {
    this.pincodeForm.reset();
    this.enrollmentForm.reset();
    this.pincodeVerified.set(false);
    this.studentData.set(null);
    this.error.set(null);
    this.success.set(null);
  }
}