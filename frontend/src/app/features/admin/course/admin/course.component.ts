import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CourseService } from '../../../../core/services/course.service';
import { Course } from '../../../../core/models/course.model';

@Component({
  selector: 'app-admin-course',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './course.component.html',
  styleUrls: ['./course.component.scss']
})
export class AdminCourseComponent implements OnInit {
  private courseService = inject(CourseService);
  private fb = inject(FormBuilder);

  courseForm: FormGroup;
  courses = signal<Course[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  deletingCourseId = signal<number | null>(null);

  constructor() {
    this.courseForm = this.fb.group({
      coursecode: ['', [Validators.required]],
      coursename: ['', [Validators.required]],
      courseunit: ['', [Validators.required, Validators.min(1)]],
      seatlimit: ['', [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit(): void {
    this.loadCourses();
  }

  loadCourses(): void {
    this.loading.set(true);
    this.error.set(null);

    this.courseService.getCourses().subscribe({
      next: (data: Course[]) => {
        this.courses.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load courses. Please try again.');
        this.loading.set(false);
        console.error('Error loading courses:', err);
      }
    });
  }

  onSubmit(): void {
    if (this.courseForm.invalid) {
      Object.keys(this.courseForm.controls).forEach(key => {
        this.courseForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.successMessage.set(null);

    const courseData: Course = {
      id:0,
      courseCode: this.courseForm.value.coursecode,
      courseName: this.courseForm.value.coursename,
      courseUnit: this.courseForm.value.courseunit.toString(),
      noofSeats: Number(this.courseForm.value.seatlimit)
    };

    this.courseService.createCourse(courseData).subscribe({
      next: (response) => {
        this.successMessage.set('Course created successfully!');
        this.courseForm.reset();
        this.loadCourses();
        this.loading.set(false);
        
        // Clear success message after 3 seconds
        setTimeout(() => {
          this.successMessage.set(null);
        }, 3000);
      },
      error: (err) => {
        this.error.set('Failed to create course. Please check if the course code already exists.');
        this.loading.set(false);
        console.error('Error creating course:', err);
      }
    });
  }

  deleteCourse(courseId: number): void {
    if (!confirm('Are you sure you want to delete this course? This action cannot be undone.')) {
      return;
    }

    this.deletingCourseId.set(courseId);
    this.error.set(null);
    this.successMessage.set(null);

    this.courseService.deleteCourse(courseId).subscribe({
      next: () => {
        this.successMessage.set('Course deleted successfully!');
        this.loadCourses();
        this.deletingCourseId.set(null);
        
        // Clear success message after 3 seconds
        setTimeout(() => {
          this.successMessage.set(null);
        }, 3000);
      },
      error: (err) => {
        this.error.set('Failed to delete course. It may be associated with existing enrollments.');
        this.deletingCourseId.set(null);
        console.error('Error deleting course:', err);
      }
    });
  }

  getFieldError(fieldName: string): string {
    const field = this.courseForm.get(fieldName);
    if (field?.hasError('required')) {
      return `${this.getFieldLabel(fieldName)} is required`;
    }
    if (field?.hasError('min')) {
      return `${this.getFieldLabel(fieldName)} must be at least 1`;
    }
    return '';
  }

  getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      coursecode: 'Course Code',
      coursename: 'Course Name',
      courseunit: 'Course Unit',
      seatlimit: 'Seat Limit'
    };
    return labels[fieldName] || fieldName;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.courseForm.get(fieldName);
    return !!(field?.invalid && field?.touched);
  }
  trackByCourse(index: number, course: Course) {
  return course.id;
}
}

