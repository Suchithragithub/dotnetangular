import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { DepartmentService } from '../../../../core/services/department.service';
import { Department } from '../../../../core/models/department.model';
import {HttpErrorResponse} from '@angular/common/http';


@Component({
  selector: 'app-admin-department',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './department.component.html',
  styleUrls: ['./department.component.scss']
})
export class AdminDepartmentComponent implements OnInit {
  private fb = inject(FormBuilder);
  private departmentService = inject(DepartmentService);
  private router = inject(Router);

  departmentForm: FormGroup;
  departments = signal<Department[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  deletingId = signal<number | null>(null);

  constructor() {
    this.departmentForm = this.fb.group({
      department: ['', [Validators.required, Validators.minLength(2)]]
    });
  }

  ngOnInit(): void {
    this.loadDepartments();
  }

  loadDepartments(): void {
    this.loading.set(true);
    this.error.set(null);

    this.departmentService.getDepartments().subscribe({
      next: (data: Department[]) => {
        this.departments.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load departments. Please try again.');
        this.loading.set(false);
        console.error('Error loading departments:', err);
      }
    });
  }

  onSubmit(): void {
    if (this.departmentForm.invalid) {
      this.departmentForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.successMessage.set(null);

    const departmentData: Department = {
      id:0,
      departmentName: this.departmentForm.value.department.trim()
    };

    this.departmentService.createDepartment(departmentData).subscribe({
      next: (response: Department) => {
        this.successMessage.set('Department created successfully!');
        this.departmentForm.reset();
        this.loadDepartments();
        this.loading.set(false);
        
        // Clear success message after 3 seconds
        setTimeout(() => {
          this.successMessage.set(null);
        }, 3000);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to create department. Please try again.');
        this.loading.set(false);
        console.error('Error creating department:', err);
      }
    });
  }

  deleteDepartment(id: number): void {
    if (!confirm('Are you sure you want to delete this department? This action cannot be undone.')) {
      return;
    }

    this.deletingId.set(id);
    this.error.set(null);
    this.successMessage.set(null);

    this.departmentService.deleteDepartment(id).subscribe({
      next: () => {
        this.successMessage.set('Department deleted successfully!');
        this.loadDepartments();
        this.deletingId.set(null);
        
        // Clear success message after 3 seconds
        setTimeout(() => {
          this.successMessage.set(null);
        }, 3000);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to delete department. It may be in use.');
        this.deletingId.set(null);
        console.error('Error deleting department:', err);
      }
    });
  }

  get departmentControl() {
    return this.departmentForm.get('department');
  }
}