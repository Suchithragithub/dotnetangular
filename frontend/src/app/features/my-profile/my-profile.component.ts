import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { StudentService } from '../../core/services/student.service';
import { Student } from '../../core/models/student.model';

@Component({
  selector: 'app-my-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './my-profile.component.html',
  styleUrls: ['./my-profile.component.scss']
})
export class MyProfileComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private studentService = inject(StudentService);
  private router = inject(Router);

  profileForm!: FormGroup;
  loading = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);
  currentStudent = signal<Student | null>(null);
  selectedFile = signal<File | null>(null);
  photoPreview = signal<string | null>(null);

  ngOnInit(): void {
    this.initializeForm();
    this.loadStudentProfile();
  }

  private initializeForm(): void {
    this.profileForm = this.fb.group({
      studentname: ['', [Validators.required, Validators.minLength(2)]],
      photo: [''],
      cgpa: ['', [Validators.required, Validators.min(0), Validators.max(4.0)]]
    });
  }

  private loadStudentProfile(): void {
    this.loading.set(true);
    this.error.set(null);

    const currentUser = this.authService.getCurrentUser();
    if (!currentUser || !currentUser.regno) {
      this.error.set('User not authenticated. Please login again.');
      this.loading.set(false);
      this.router.navigate(['/login']);
      return;
    }

    const regno = currentUser.regno;

    this.studentService.getStudentByRegno(regno).subscribe({
      next: (student: Student) => {
        this.currentStudent.set(student);
        this.profileForm.patchValue({
          studentname: student.studentname || '',
          cgpa: student.cgpa || ''
        });
        if (student.photo) {
          this.photoPreview.set(student.photo);
        }
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading student profile:', err);
        this.error.set('Failed to load profile data. Please try again.');
        this.loading.set(false);
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.selectedFile.set(file);

      // Create preview
      const reader = new FileReader();
      reader.onload = (e: ProgressEvent<FileReader>) => {
        if (e.target?.result) {
          this.photoPreview.set(e.target.result as string);
        }
      };
      reader.readAsDataURL(file);
    }
  }

  onSubmit(): void {
    if (this.profileForm.invalid) {
      Object.keys(this.profileForm.controls).forEach(key => {
        this.profileForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.success.set(null);

    const currentUser = this.authService.getCurrentUser();
    if (!currentUser || !currentUser.regno) {
      this.error.set('User not authenticated.');
      this.loading.set(false);
      return;
    }

    const formData = new FormData();
    formData.append('regno', currentUser.regno);
    formData.append('studentname', this.profileForm.get('studentname')?.value);
    formData.append('cgpa', this.profileForm.get('cgpa')?.value);

    if (this.selectedFile()) {
      formData.append('photo', this.selectedFile()!);
    }

    this.studentService.updateProfile(formData).subscribe({
      next: (response) => {
        this.success.set('Profile updated successfully!');
        this.loading.set(false);
        this.selectedFile.set(null);
        
        // Reload profile to get updated data
        setTimeout(() => {
          this.loadStudentProfile();
          this.success.set(null);
        }, 2000);
      },
      error: (err) => {
        console.error('Error updating profile:', err);
        this.error.set(err.error?.message || 'Failed to update profile. Please try again.');
        this.loading.set(false);
      }
    });
  }

  getFieldError(fieldName: string): string {
    const control = this.profileForm.get(fieldName);
    if (control?.hasError('required')) {
      return `${this.getFieldLabel(fieldName)} is required`;
    }
    if (control?.hasError('minlength')) {
      return `${this.getFieldLabel(fieldName)} must be at least ${control.errors?.['minlength'].requiredLength} characters`;
    }
    if (control?.hasError('min')) {
      return `${this.getFieldLabel(fieldName)} must be at least ${control.errors?.['min'].min}`;
    }
    if (control?.hasError('max')) {
      return `${this.getFieldLabel(fieldName)} cannot exceed ${control.errors?.['max'].max}`;
    }
    return '';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      studentname: 'Student Name',
      cgpa: 'CGPA',
      photo: 'Photo'
    };
    return labels[fieldName] || fieldName;
  }
}
