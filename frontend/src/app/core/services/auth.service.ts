import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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
    return this.http.post<StudentLoginResponse>(
      `${this.apiUrl}/api/auth/student/login`,
      credentials
    );
  }

  /**
   * Authenticate admin user with username and password
   * POST /api/auth/admin/login
   */
  adminLogin(credentials: AdminLoginRequest): Observable<AdminLoginResponse> {
    return this.http.post<AdminLoginResponse>(
      `${this.apiUrl}/api/auth/admin/login`,
      credentials
    );
  }

  isAdmin(): boolean {
    return !!localStorage.getItem('admin');
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  // ✅ ADD THIS METHOD
  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('admin');
    localStorage.removeItem('student');
  }
}
