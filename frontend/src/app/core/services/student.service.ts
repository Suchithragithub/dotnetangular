import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Student } from '../models';
import { environment } from '../../../environments/environment';

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface VerifyPincodeRequest {
  regno: string;
  pincode: string;
}

export interface CheckRegnoRequest {
  regno: string;
}

export interface CheckRegnoResponse {
  exists: boolean;
}

export interface RegisterStudentRequest {
  name: string;
  regno: string;
  password: string;
}

export interface RegisterStudentResponse {
  student: Student;
  pincode: string;
}

export interface UpdateProfileRequest {
  name?: string;
  photo?: string;
  cgpa?: number;
}

export interface ResetPasswordResponse {
  message: string;
  defaultPassword: string;
}

@Injectable({ providedIn: 'root' })
export class StudentService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  /**
   * Verify current password and update to new password for logged-in student
   */
  changePassword(request: ChangePasswordRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(
      `${this.apiUrl}/api/students/change-password`,
      request
    );
  }

  /**
   * Verify student pincode before allowing course enrollment
   */
  verifyPincode(request: VerifyPincodeRequest): Observable<{ valid: boolean }> {
    return this.http.post<{ valid: boolean }>(
      `${this.apiUrl}/api/students/verify-pincode`,
      request
    );
  }

  /**
   * Retrieve student details for enrollment form
   */
  getStudentByRegno(regno: string): Observable<Student> {
    return this.http.get<Student>(
      `${this.apiUrl}/api/students/${regno}`
    );
  }

  /**
   * Update student name, photo, and CGPA
   */
  updateProfile(request: UpdateProfileRequest): Observable<Student> {
    return this.http.put<Student>(
      `${this.apiUrl}/api/students/profile`,
      request
    );
  }

  /**
   * Register new student with name, regno, password, and auto-generated pincode
   */
  registerStudent(request: RegisterStudentRequest): Observable<RegisterStudentResponse> {
    return this.http.post<RegisterStudentResponse>(
      `${this.apiUrl}/api/students`,
      request
    );
  }

  /**
   * Check if student regno already exists
   */
  checkRegno(request: CheckRegnoRequest): Observable<CheckRegnoResponse> {
    return this.http.post<CheckRegnoResponse>(
      `${this.apiUrl}/api/students/check-regno`,
      request
    );
  }

  /**
   * Delete student record by registration number
   */
  deleteStudent(regno: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(
      `${this.apiUrl}/api/students/${regno}`
    );
  }

  /**
   * Reset student password to default Test@123
   */
  resetPassword(regno: string): Observable<ResetPasswordResponse> {
    return this.http.put<ResetPasswordResponse>(
      `${this.apiUrl}/api/students/${regno}/reset-password`,
      {}
    );
  }

  /**
   * Retrieve all student records
   */
  getAllStudents(): Observable<Student[]> {
    return this.http.get<Student[]>(
      `${this.apiUrl}/api/students`
    );
  }

  /**
   * Update student name, photo, and CGPA by admin
   */
  updateStudentProfileByAdmin(regno: string, request: UpdateProfileRequest): Observable<Student> {
    return this.http.put<Student>(
      `${this.apiUrl}/api/students/${regno}/profile`,
      request
    );
  }
}
