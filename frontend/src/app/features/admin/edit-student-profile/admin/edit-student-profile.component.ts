import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { StudentService, UpdateProfileRequest } from '../../../../core/services/student.service';
import { Student } from '../../../../core/models/student.model';

@Component({
  selector: 'app-admin-edit-student-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './edit-student-profile.component.html',
  styleUrls: ['./edit-student-profile.component.scss']
})
export class AdminEditStudentProfileComponent implements OnInit {
  private fb = inject(FormBuilder);
  private studentService = inject(StudentService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  loading = signal(false);
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  studentRegNo = signal<string>('');
  currentPhotoUrl = signal<string | null>(null);
  selectedFile = signal<File | null>(null);
  previewUrl = signal<string | null>(null);

  profileForm: FormGroup;

  constructor() {
    this.profileForm = this.fb.group({
      studentname: ['', [Validators.required, Validators.minLength(2)]],
      photo: [null],
      cgpa: ['', [Validators.required, Validators.min(0), Validators.max(4.0)]]
    });
  }

  ngOnInit(): void {
    const regno = this.route.snapshot.queryParamMap.get('regno');
    if (!regno) {
      this.error.set('Student registration number is required');
      return;
    }
    this.studentRegNo.set(regno);
    this.loadStudentProfile(regno);
  }

  loadStudentProfile(regno: string): void {
    this.loading.set(true);
    this.error.set(null);

    this.studentService.getStudentByRegno(regno).subscribe({
      next: (student: Student) => {
        this.profileForm.patchValue({
          studentname: student.studentName,
          cgpa: student.cgpa
        });
        if (student.studentPhoto) {
          this.currentPhotoUrl.set(student.studentPhoto);
        }
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err?.error?.message || 'Failed to load student profile');
        this.loading.set(false);
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      // Validate file type
      const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
      if (!allowedTypes.includes(file.type)) {
        this.error.set('Please select a valid image file (JPEG, PNG, or GIF)');
        return;
      }

      // Validate file size (max 5MB)
      const maxSize = 5 * 1024 * 1024;
      if (file.size > maxSize) {
        this.error.set('File size must not exceed 5MB');
        return;
      }

      this.selectedFile.set(file);
      this.error.set(null);

      // Create preview
      const reader = new FileReader();
      reader.onload = (e: ProgressEvent<FileReader>) => {
        if (e.target?.result) {
          this.previewUrl.set(e.target.result as string);
        }
      };
      reader.readAsDataURL(file);
    }
  }

  removePhoto(): void {
    this.selectedFile.set(null);
    this.previewUrl.set(null);
    const fileInput = document.getElementById('photoInput') as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
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
    this.successMessage.set(null);

    const request: UpdateProfileRequest = {
      name: this.profileForm.get('studentname')?.value,
      cgpa: parseFloat(this.profileForm.get('cgpa')?.value)
    };

    // Assuming your backend parses base64 data for the 'photo' string property
    if (this.previewUrl()) {
       request.photo = this.previewUrl() as string;
    }

    this.studentService.updateStudentProfileByAdmin(this.studentRegNo(), request).subscribe({
      next: (response: Student) => {
        this.loading.set(false);
        this.successMessage.set('Student profile updated successfully');
        setTimeout(() => {
          this.router.navigate(['/admin/manage-students']);
        }, 2000);
      },
      error: (err: HttpErrorResponse) => {
        this.error.set(err?.error?.message || 'Failed to update student profile');
        this.loading.set(false);
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/admin/students']);
  }

  get studentnameControl() {
    return this.profileForm.get('studentname');
  }

  get cgpaControl() {
    return this.profileForm.get('cgpa');
  }
}