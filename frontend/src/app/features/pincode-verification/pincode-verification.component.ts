import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { StudentService } from '../../services/student.service';
import { AuthService } from '../../services/auth.service';

interface PincodeVerificationForm {
  pincode: FormControl<string | null>;
}

interface VerifyPincodeResponse {
  success: boolean;
  message: string;
  verified?: boolean;
}

@Component({
  selector: 'app-pincode-verification',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './pincode-verification.component.html',
  styleUrl: './pincode-verification.component.scss'
})
export class PincodeVerificationComponent implements OnInit {
  private readonly studentService = inject(StudentService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  loading = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);

  form: FormGroup<PincodeVerificationForm>;

  private returnUrl: string = '/student/enrollment';

  constructor() {
    this.form = new FormGroup<PincodeVerificationForm>({
      pincode: new FormControl('', {
        validators: [Validators.required, Validators.minLength(4)],
        nonNullable: true
      })
    });
  }

  ngOnInit(): void {
    // Check if user is authenticated
    const currentUser = this.authService.currentUser;
    if (!currentUser) {
      this.router.navigate(['/login']);
      return;
    }

    // Get return URL from query params or default to enrollment page
    this.route.queryParams.subscribe(params => {
      this.returnUrl = params['returnUrl'] || '/student/enrollment';
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.success.set(null);

    const pincodeValue = this.form.value.pincode;

    if (!pincodeValue) {
      this.error.set('Pincode is required');
      this.loading.set(false);
      return;
    }

    const payload = {
      pincode: pincodeValue
    };

    this.studentService.verifyPincode(payload).subscribe({
      next: (response: VerifyPincodeResponse) => {
        this.loading.set(false);
        if (response.success && response.verified) {
          this.success.set('Pincode verified successfully! Redirecting to enrollment...');
          // Store verification status in session/local storage if needed
          sessionStorage.setItem('pincodeVerified', 'true');
          sessionStorage.setItem('pincodeVerifiedAt', new Date().toISOString());
          
          // Redirect after short delay to show success message
          setTimeout(() => {
            this.router.navigate([this.returnUrl]);
          }, 1500);
        } else {
          this.error.set(response.message || 'Invalid pincode. Please try again.');
          this.form.reset();
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(
          err.error?.message || 
          'Failed to verify pincode. Please check your pincode and try again.'
        );
        this.form.reset();
      }
    });
  }

  get pincodeControl(): FormControl {
    return this.form.get('pincode') as FormControl;
  }

  onCancel(): void {
    this.router.navigate(['/student/dashboard']);
  }
}
