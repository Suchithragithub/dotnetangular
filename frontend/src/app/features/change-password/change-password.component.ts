import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { StudentService } from '../../services/student.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.scss']
})
export class ChangePasswordComponent implements OnInit {
  private fb = inject(FormBuilder);
  private studentService = inject(StudentService);
  private authService = inject(AuthService);
  private router = inject(Router);

  loading = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);
  showCurrentPassword = signal(false);
  showNewPassword = signal(false);
  showConfirmPassword = signal(false);

  changePasswordForm!: FormGroup;

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm(): void {
    this.changePasswordForm = this.fb.group({
      cpass: ['', [Validators.required, Validators.minLength(6)]],
      newpass: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(50)]],
      confirmpass: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  private passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const newpass = control.get('newpass')?.value;
    const confirmpass = control.get('confirmpass')?.value;

    if (!newpass || !confirmpass) {
      return null;
    }

    return newpass === confirmpass ? null : { passwordMismatch: true };
  }

  togglePasswordVisibility(field: 'current' | 'new' | 'confirm'): void {
    if (field === 'current') {
      this.showCurrentPassword.set(!this.showCurrentPassword());
    } else if (field === 'new') {
      this.showNewPassword.set(!this.showNewPassword());
    } else {
      this.showConfirmPassword.set(!this.showConfirmPassword());
    }
  }

  onSubmit(): void {
    if (this.changePasswordForm.invalid) {
      this.changePasswordForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.success.set(null);

    const formData = {
      currentPassword: this.changePasswordForm.value.cpass,
      newPassword: this.changePasswordForm.value.newpass
    };

    this.studentService.changePassword(formData).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.success.set('Password changed successfully! You will be redirected to login.');
        this.changePasswordForm.reset();
        
        // Logout and redirect to login after 2 seconds
        setTimeout(() => {
          this.authService.logout();
          this.router.navigate(['/login']);
        }, 2000);
      },
      error: (err) => {
        this.loading.set(false);
        if (err.status === 401) {
          this.error.set('Current password is incorrect. Please try again.');
        } else if (err.status === 400) {
          this.error.set(err.error?.message || 'Invalid password format. Please check your input.');
        } else if (err.status === 403) {
          this.error.set('Session expired. Please login again.');
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 2000);
        } else {
          this.error.set(err.error?.message || 'Failed to change password. Please try again later.');
        }
      }
    });
  }

  getFieldError(fieldName: string): string | null {
    const field = this.changePasswordForm.get(fieldName);
    if (field && field.invalid && field.touched) {
      if (field.hasError('required')) {
        return 'This field is required';
      }
      if (field.hasError('minlength')) {
        const minLength = field.getError('minlength').requiredLength;
        return `Password must be at least ${minLength} characters long`;
      }
      if (field.hasError('maxlength')) {
        const maxLength = field.getError('maxlength').requiredLength;
        return `Password must not exceed ${maxLength} characters`;
      }
    }
    return null;
  }

  getFormError(): string | null {
    if (this.changePasswordForm.hasError('passwordMismatch') && 
        this.changePasswordForm.get('confirmpass')?.touched) {
      return 'New password and confirm password do not match';
    }
    return null;
  }

  onCancel(): void {
    this.router.navigate(['/student/dashboard']);
  }
}