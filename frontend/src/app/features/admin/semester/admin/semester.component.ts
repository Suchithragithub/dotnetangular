import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { SemesterService } from '../../../../core/services/semester.service';
import { Semester } from '../../../../core/models/semester.model';

@Component({
  selector: 'app-admin-semester',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './semester.component.html',
  styleUrls: ['./semester.component.scss']
})
export class AdminSemesterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private semesterService = inject(SemesterService);
  private router = inject(Router);

  semesterForm: FormGroup;
  semesters = signal<Semester[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  constructor() {
    this.semesterForm = this.fb.group({
      semester: ['', [Validators.required, Validators.minLength(1)]]
    });
  }

  ngOnInit(): void {
    this.loadSemesters();
  }

  loadSemesters(): void {
    this.loading.set(true);
    this.error.set(null);

    this.semesterService.getAllSemesters().subscribe({
      next: (data: Semester[]) => {
        this.semesters.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load semesters. Please try again.');
        this.loading.set(false);
        console.error('Error loading semesters:', err);
      }
    });
  }

  onSubmit(): void {
    if (this.semesterForm.invalid) {
      this.semesterForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.successMessage.set(null);

    const semesterData: Semester = {
      semesterName: this.semesterForm.value.semester ?? ''
    };

    this.semesterService.createSemester(semesterData).subscribe({
      next: (response: Semester) => {
        this.loading.set(false);
        this.successMessage.set('Semester created successfully!');
        this.semesterForm.reset();
        this.loadSemesters();
        
        setTimeout(() => {
          this.successMessage.set(null);
        }, 3000);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Failed to create semester. Please try again.');
        console.error('Error creating semester:', err);
      }
    });
  }

  deleteSemester(id: number): void {
    if (!confirm('Are you sure you want to delete this semester? This action cannot be undone.')) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.successMessage.set(null);

    this.semesterService.deleteSemester(id).subscribe({
      next: () => {
        this.loading.set(false);
        this.successMessage.set('Semester deleted successfully!');
        this.loadSemesters();
        
        setTimeout(() => {
          this.successMessage.set(null);
        }, 3000);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Failed to delete semester. Please try again.');
        console.error('Error deleting semester:', err);
      }
    });
  }

  get semesterControl() {
    return this.semesterForm.get('semester');
  }
}

// function inject(arg0: any) {
//   throw new Error('Function not implemented.');
// }
