import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { CourseService } from '../../../../core/services/course.service';
import { Course } from '../../../../core/models/course.model';

interface EditCourseForm {
  coursecode: FormControl<string>;
  coursename: FormControl<string>;
  courseunit: FormControl<number>;
  seatlimit: FormControl<number>;
}

@Component({
  selector: 'app-edit-course',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './edit-course.component.html',
  styleUrls: ['./edit-course.component.scss']
})
export class EditCourseComponent implements OnInit {
  private courseService = inject(CourseService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  loading = signal(false);
  error = signal<string | null>(null);
  courseId: number | null = null;
  course = signal<Course | null>(null);

  form: FormGroup<EditCourseForm> = new FormGroup({
    coursecode: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    coursename: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    courseunit: new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(1)] }),
    seatlimit: new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(1)] })
  });

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.courseId = parseInt(id, 10);
        this.loadCourse();
      } else {
        this.error.set('Course ID not provided');
      }
    });
  }

  loadCourse(): void {
    if (!this.courseId) return;

    this.loading.set(true);
    this.error.set(null);

    this.courseService.getCourseById(this.courseId).subscribe({
      next: (course: Course) => {
        this.course.set(course);
        this.form.patchValue({
          coursecode: course.courseCode || '',
          coursename: course.courseName || '',
          courseunit: Number(course.courseUnit) || 0,
          seatlimit: course.noofSeats || 0
        });
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err?.error?.message || 'Failed to load course details');
        this.loading.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (!this.courseId) {
      this.error.set('Course ID is missing');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const formValue = this.form.getRawValue();
    const updatedCourse: Course = {
      id: this.courseId,
      courseCode: formValue.coursecode,
      courseName: formValue.coursename,
      courseUnit: String(formValue.courseunit),
      noofSeats: formValue.seatlimit
    };

    this.courseService.updateCourse(this.courseId, updatedCourse).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/admin/courses']);
      },
      error: (err) => {
        this.error.set(err?.error?.message || 'Failed to update course');
        this.loading.set(false);
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/admin/courses']);
  }
}
