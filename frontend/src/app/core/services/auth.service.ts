import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Student, Admin, UserLog } from '../models';

export interface StudentLoginRequest {
  regno: string;
  password: string;
}

export interface AdminLoginRequest {
  username: string;
  password: string;
}

export interface StudentLoginResponse {
  student: Student;
  userLog: UserLog;
  token?: string;
}

export interface AdminLoginResponse {
  admin: Admin;
  token?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  /**
   * Authenticate student with regno and password, create userlog entry
   * POST /api/auth/student/login
   */
  studentLogin(credentials: StudentLoginRequest): Observable<StudentLoginResponse> {
    let params = new HttpParams()
      .set('regno', credentials.regno)
      .set('password', credentials.password);
    return this.http.post<StudentLoginResponse>(
      `${this.apiUrl}/api/Standalone/login`,
      null, { params }
    );
  }

  logActivity(regno: string, status: number) {
    return this.http.post(`${this.apiUrl}/api/Standalone/login-log?studentRegno=${regno}&status=${status}`, null);
  }

  getCurrentUser(): any {
    const studentStr = localStorage.getItem('student');
    return studentStr ? JSON.parse(studentStr) : null;
  }

  isStudent(): boolean {
  // Returns true ONLY if 'student' exists in localStorage
    return !!localStorage.getItem('student');
  }

  /**
   * Authenticate admin user with username and password
   * POST /api/auth/admin/login
   */
  adminLogin(credentials: AdminLoginRequest): Observable<AdminLoginResponse> {
    return this.http.post<AdminLoginResponse>(
      `${this.apiUrl}/api/Admin/login`,
      credentials
    );
  }

  isAdmin(): boolean {
    return !!localStorage.getItem('adminUser');
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  // ✅ ADD THIS METHOD
  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('adminUser');
    localStorage.removeItem('student');
  }
}
