import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-admin-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class AdminLoginComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  loading = signal(false);
  error = signal<string | null>(null);
  loginForm!: FormGroup;
  passwordVisible = signal(false);

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm(): void {
    this.loginForm = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  togglePasswordVisibility(): void {
    this.passwordVisible.set(!this.passwordVisible());
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const credentials = {
      username: this.loginForm.value.username,
      password: this.loginForm.value.password
    };

    this.authService.adminLogin(credentials).subscribe({
      next: (response) => {
        this.loading.set(false);
        // Store admin authentication token/session if provided in response
        if (response && response.token) {
          localStorage.setItem('token', response.token);
          localStorage.setItem('adminUser', JSON.stringify(response.admin));
        }

        // ✅ Clear any stale student session
        localStorage.removeItem('student');
        
        // Navigate to admin dashboard
        this.router.navigate(['/admin/dashboard']);
      },
      error: (err) => {
        this.loading.set(false);
        if (err.status === 401) {
          this.error.set('Invalid username or password. Please try again.');
        } else if (err.status === 403) {
          this.error.set('Access denied. You do not have admin privileges.');
        } else if (err.status === 0) {
          this.error.set('Unable to connect to server. Please check your internet connection.');
        } else {
          this.error.set(err.error?.message || 'Login failed. Please try again later.');
        }
        // Clear password field on error
        this.loginForm.patchValue({ password: '' });
      }
    });
  }

  get username() {
    return this.loginForm.get('username');
  }

  get password() {
    return this.loginForm.get('password');
  }
}
