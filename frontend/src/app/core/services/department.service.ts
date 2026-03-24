import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Department } from '../models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class DepartmentService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  /**
   * Get all departments for enrollment dropdown
   * @returns Observable of Department array
   */
  getDepartments(): Observable<Department[]> {
    return this.http.get<Department[]>(`${this.apiUrl}/api/Admin/departments`);
  }
  getDepartmentsForEnrollment(): Observable<Department[]> {
    return this.http.get<Department[]>(`${this.apiUrl}/api/Standalone/departments`);
  }

  /**
   * Create new department
   * @param department - Department object to create
   * @returns Observable of created Department
   */
  createDepartment(department: Department): Observable<Department> {
    return this.http.post<Department>(`${this.apiUrl}/api/Admin/departments`, department);
  }

  /**
   * Delete department by ID
   * @param id - Department ID to delete
   * @returns Observable of void
   */
  deleteDepartment(id: number | string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/Admin/departments/${id}`);
  }
}
