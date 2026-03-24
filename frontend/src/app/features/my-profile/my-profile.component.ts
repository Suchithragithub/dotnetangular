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
 
  readonly defaultAvatar =
    'data:image/svg+xml;utf8,' +
    encodeURIComponent(`
      <svg xmlns="http://www.w3.org/2000/svg" width="200" height="200" viewBox="0 0 200 200">
        <rect width="200" height="200" fill="#e9ecef"/>
        <circle cx="100" cy="75" r="35" fill="#adb5bd"/>
        <path d="M40 180c10-35 35-55 60-55s50 20 60 55" fill="#adb5bd"/>
      </svg>
    `);
 
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
    console.log("DEBUG user:", currentUser);
 
    const regno = currentUser?.studentRegno; // FIXED
 
    if (!currentUser || !regno) {
      this.error.set('User not authenticated. Please login again.');
      this.loading.set(false);
      this.router.navigate(['/login']);
      return;
    }
 
    this.studentService.getStudentByRegno(regno).subscribe({
      next: (student: any) => {
        console.log("DEBUG student API:", student);
 
        this.currentStudent.set({
          studentRegno: student.studentRegno ?? student.StudentRegno,
          studentName: student.studentName ?? student.StudentName,
          cgpa: student.cgpa ?? student.Cgpa,
          studentPhoto: student.studentPhoto ?? student.StudentPhoto,
          department: student.department ?? student.Department,
          session: student.session ?? student.Session,
          semester: student.semester ?? student.Semester
        } as any);
 
        this.profileForm.patchValue({
          studentname: student.studentName ?? student.StudentName ?? '',
          cgpa: student.cgpa ?? student.Cgpa ?? ''
        });
 
        this.photoPreview.set(student.studentPhoto ?? student.StudentPhoto ?? null);
        this.loading.set(false);
      },
      error: (err) => {
        console.error("DEBUG error:", err);
        this.error.set('Failed to load profile data. Please try again.');
        this.loading.set(false);
      }
    });
  }
 
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      return;
    }
 
    const file = input.files[0];
    this.selectedFile.set(file);
 
    const reader = new FileReader();
    reader.onload = (e: ProgressEvent<FileReader>) => {
      if (e.target?.result) {
        this.photoPreview.set(e.target.result as string);
      }
    };
    reader.readAsDataURL(file);
  }
 
  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (img.src !== this.defaultAvatar) {
      img.src = this.defaultAvatar;
    }
  }
 
  onSubmit(): void {
    if (this.profileForm.invalid) {
      Object.keys(this.profileForm.controls).forEach(key => {
        this.profileForm.get(key)?.markAsTouched();
      });
      return;
    }
 
    const currentUser = this.authService.getCurrentUser();
    if (!currentUser?.studentRegno) {
      this.error.set('User not authenticated.');
      return;
    }
 
    this.loading.set(true);
    this.error.set(null);
    this.success.set(null);
 
    const request = {
      name: this.profileForm.get('studentname')?.value,
      cgpa: Number(this.profileForm.get('cgpa')?.value),
      photo: this.photoPreview() ?? this.currentStudent()?.studentPhoto ?? ''
    };
 
    this.studentService.updateProfile(currentUser.studentRegno, request).subscribe({
      next: () => {
        this.success.set('Profile updated successfully!');
        this.loading.set(false);
        this.selectedFile.set(null);
 
        setTimeout(() => {
          this.loadStudentProfile();
          this.success.set(null);
        }, 1500);
      },
      error: (err) => {
        this.error.set(err?.error?.message || 'Failed to update profile. Please try again.');
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
    const labels: Record<string, string> = {
      studentname: 'Student Name',
      cgpa: 'CGPA',
      photo: 'Photo'
    };
    return labels[fieldName] || fieldName;
  }
}
 