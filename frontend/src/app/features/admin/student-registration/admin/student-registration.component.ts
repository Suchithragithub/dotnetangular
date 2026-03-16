import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { StudentService } from '../../../../core/services/student.service';
import { SessionService } from '../../../../core/services/session.service';
import { DepartmentService } from '../../../../core/services/department.service';
import { LevelService } from '../../../../core/services/level.service';
import { inject } from '@angular/core';
import { Session } from '../../../../core/models/session.model';
import { Department } from '../../../../core/models/department.model';
import { Level } from '../../../../core/models/level.model';

interface StudentRegistrationRequest {
  studentname: string;
  studentregno: string;
  password: string;
  sessionId?: number;
  departmentId?: number;
  levelId?: number;
}

interface StudentRegistrationResponse {
  id: number;
  studentname: string;
  studentregno: string;
  pincode: string;
  message?: string;
}

@Component({
  selector: 'app-student-registration',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './student-registration.component.html',
  styleUrls: ['./student-registration.component.scss']
})
export class StudentRegistrationComponent implements OnInit {
  private fb = inject(FormBuilder);
  private studentService = inject(StudentService);
  private sessionService = inject(SessionService);
  private departmentService = inject(DepartmentService);
  private levelService = inject(LevelService);
  private router = inject(Router);

  registrationForm!: FormGroup;
  loading = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);
  generatedPincode = signal<string | null>(null);
  checkingRegno = signal(false);
  regnoExists = signal(false);

  sessions = signal<Session[]>([]);
  departments = signal<Department[]>([]);
  levels = signal<Level[]>([]);

  ngOnInit(): void {
    this.initializeForm();
    this.loadReferenceData();
    this.setupRegnoValidation();
  }

  private initializeForm(): void {
    this.registrationForm = this.fb.group({
      studentname: ['', [Validators.required, Validators.minLength(2)]],
      studentregno: ['', [Validators.required, Validators.pattern(/^[A-Za-z0-9]+$/)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
      sessionId: [''],
      departmentId: [''],
      levelId: ['']
    }, { validators: this.passwordMatchValidator });
  }

  private passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  private loadReferenceData(): void {
    this.sessionService.getSessions().subscribe({
      next: (sessions) => this.sessions.set(sessions),
      error: (err) => console.error('Error loading sessions:', err)
    });

    this.departmentService.getDepartments().subscribe({
      next: (departments) => this.departments.set(departments),
      error: (err) => console.error('Error loading departments:', err)
    });

    this.levelService.getLevels().subscribe({
      next: (levels) => this.levels.set(levels),
      error: (err) => console.error('Error loading levels:', err)
    });
  }

  private setupRegnoValidation(): void {
    const regnoControl = this.registrationForm.get('studentregno');
    if (regnoControl) {
      regnoControl.valueChanges.subscribe((value: string) => {
        if (value && value.length >= 3) {
          this.checkRegnoExists(value);
        } else {
          this.regnoExists.set(false);
        }
      });
    }
  }

  private checkRegnoExists(regno: string): void {
    this.checkingRegno.set(true);
    this.studentService.checkRegnoExists(regno).subscribe({
      next: (response: { exists: boolean }) => {
        this.regnoExists.set(response.exists);
        this.checkingRegno.set(false);
      },
      error: (err) => {
        console.error('Error checking regno:', err);
        this.checkingRegno.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.registrationForm.invalid) {
      this.markFormGroupTouched(this.registrationForm);
      this.error.set('Please fill in all required fields correctly.');
      return;
    }

    if (this.regnoExists()) {
      this.error.set('Registration number already exists. Please use a different registration number.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.success.set(null);
    this.generatedPincode.set(null);

    const formValue = this.registrationForm.value;
    const registrationData: StudentRegistrationRequest = {
      studentname: formValue.studentname,
      studentregno: formValue.studentregno,
      password: formValue.password
    };

    if (formValue.sessionId) {
      registrationData.sessionId = parseInt(formValue.sessionId, 10);
    }
    if (formValue.departmentId) {
      registrationData.departmentId = parseInt(formValue.departmentId, 10);
    }
    if (formValue.levelId) {
      registrationData.levelId = parseInt(formValue.levelId, 10);
    }

    this.studentService.registerStudent(registrationData).subscribe({
      next: (response: StudentRegistrationResponse) => {
        this.loading.set(false);
        this.generatedPincode.set(response.pincode);
        this.success.set(`Student registered successfully! Registration Number: ${response.studentregno}`);
        this.registrationForm.reset();
        this.regnoExists.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Failed to register student. Please try again.');
      }
    });
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  resetForm(): void {
    this.registrationForm.reset();
    this.error.set(null);
    this.success.set(null);
    this.generatedPincode.set(null);
    this.regnoExists.set(false);
  }

  get studentname() {
    return this.registrationForm.get('studentname');
  }

  get studentregno() {
    return this.registrationForm.get('studentregno');
  }

  get password() {
    return this.registrationForm.get('password');
  }

  get confirmPassword() {
    return this.registrationForm.get('confirmPassword');
  }
}