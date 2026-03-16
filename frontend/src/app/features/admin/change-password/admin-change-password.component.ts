import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AdminService } from '../../../core/services/admin.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-admin-change-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './admin-change-password.component.html',
  styleUrls: ['./admin-change-password.component.scss']
})
export class AdminChangePasswordComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminService = inject(AdminService);
  private authService = inject(AuthService);
  private router = inject(Router);

  loading = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);
  showCurrentPassword = signal(false);
  showNewPassword = signal(false);

  changePasswordForm!: FormGroup;

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm(): void {
    this.changePasswordForm = this.fb.group({
      cpass: ['', [Validators.required, Validators.minLength(6)]],
      newpass: ['', [Validators.required, Validators.minLength(6)]],
      confirmNewPass: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  private passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const newPass = control.get('newpass')?.value;
    const confirmPass = control.get('confirmNewPass')?.value;

    if (newPass && confirmPass && newPass !== confirmPass) {
      control.get('confirmNewPass')?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }

    if (control.get('confirmNewPass')?.hasError('passwordMismatch')) {
      const errors = control.get('confirmNewPass')?.errors;
      if (errors) {
        delete errors['passwordMismatch'];
        if (Object.keys(errors).length === 0) {
          control.get('confirmNewPass')?.setErrors(null);
        }
      }
    }

    return null;
  }

  toggleCurrentPasswordVisibility(): void {
    this.showCurrentPassword.set(!this.showCurrentPassword());
  }

  toggleNewPasswordVisibility(): void {
    this.showNewPassword.set(!this.showNewPassword());
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

    this.adminService.changePassword(formData).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.success.set('Password changed successfully! You will be redirected to login.');
        this.changePasswordForm.reset();
        
        // Logout and redirect to login after 2 seconds
        setTimeout(() => {
          this.authService.logout();
          this.router.navigate(['/admin/login']);
        }, 2000);
      },
      error: (err) => {
        this.loading.set(false);
        if (err.error?.message) {
          this.error.set(err.error.message);
        } else if (err.status === 401) {
          this.error.set('Current password is incorrect. Please try again.');
        } else if (err.status === 400) {
          this.error.set('Invalid password format. Password must be at least 6 characters.');
        } else {
          this.error.set('Failed to change password. Please try again later.');
        }
      }
    });
  }

  getFieldError(fieldName: string): string {
    const field = this.changePasswordForm.get(fieldName);
    if (field?.hasError('required')) {
      return `${this.getFieldLabel(fieldName)} is required`;
    }
    if (field?.hasError('minlength')) {
      const minLength = field.errors?.['minlength'].requiredLength;
      return `${this.getFieldLabel(fieldName)} must be at least ${minLength} characters`;
    }
    if (fieldName === 'confirmNewPass' && field?.hasError('passwordMismatch')) {
      return 'Passwords do not match';
    }
    return '';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      cpass: 'Current password',
      newpass: 'New password',
      confirmNewPass: 'Confirm new password'
    };
    return labels[fieldName] || fieldName;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.changePasswordForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }
}