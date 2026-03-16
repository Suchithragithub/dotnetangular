import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { UserlogService } from '../../core/services/userlog.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private userlogService = inject(UserlogService);
  private router = inject(Router);

  loginForm!: FormGroup;
  loading = signal(false);
  error = signal<string | null>(null);
  passwordVisible = signal(false);

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm(): void {
    this.loginForm = this.fb.group({
      regno: ['', [Validators.required]],
      password: ['', [Validators.required]]
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
      regno: this.loginForm.value.regno,
      password: this.loginForm.value.password
    };

    this.authService.studentLogin(credentials).subscribe({
      next: (response) => {
        this.loading.set(false);
        // Navigate to student dashboard on successful login
        this.router.navigate(['/student/dashboard']);
      },
      error: (err) => {
        this.loading.set(false);
        const errorMessage = err?.error?.message || err?.message || 'Login failed. Please check your credentials and try again.';
        this.error.set(errorMessage);
      }
    });
  }

  get regnoControl() {
    return this.loginForm.get('regno');
  }

  get passwordControl() {
    return this.loginForm.get('password');
  }
}
