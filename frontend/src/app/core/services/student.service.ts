import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
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
  studentName: string;   // was: name
  studentRegno: string;  // was: regno
  password: string;
  pincode: string;       // was: missing
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
  // changePassword(request: ChangePasswordRequest): Observable<{ message: string }> {
  //   return this.http.put<{ message: string }>(
  //     `${this.apiUrl}/api/Standalone/change-password`,
  //     request
  //   );
  // }

  changePassword(request: ChangePasswordRequest): Observable<{ message: string }> {
    const token = localStorage.getItem('token');
    const headers = token
      ? new HttpHeaders({ Authorization: `Bearer ${token}` })
      : new HttpHeaders();
 
    return this.http.post<{ message: string }>(
      `${this.apiUrl}/api/Standalone/change-password`,
      request,
      { headers }
    );
  }

  /**
   * Verify student pincode before allowing course enrollment
   */
  // In student.service.ts
  verifyPincode(payload: { regno: string; pincode: string }): Observable<any> {
    return this.http.get(  // ← try GET instead of POST
      `${this.apiUrl}/api/Standalone/pincode-verification`,
      { params: { pincode: payload.pincode } }
    );
  }

  /**
   * Retrieve student details for enrollment form
   */
  // getStudentByRegno(regno: string): Observable<Student> {
  //   return this.http.get<Student>(
  //     `${this.apiUrl}/api/Standalone/students/${regno}`
  //   );
  // }

  getStudentByRegno(regno: string | number): Observable<Student> {
    return this.http.get<Student>(
      `${this.apiUrl}/api/Standalone/student`,
      { params: { studentRegno: regno.toString() } }  // ← query param, not path
    );
  }

  /**
   * Update student name, photo, and CGPA
   */
  // updateProfile(request: UpdateProfileRequest): Observable<Student> {
  //   return this.http.put<Student>(
  //     `${this.apiUrl}/api/Standalone/students/profile`,
  //     request
  //   );
  // }

  updateProfile(
    studentRegno: string,
    request: UpdateProfileRequest
  ): Observable<{ message: string }> {
    const token = localStorage.getItem('token');
    const headers = token
      ? new HttpHeaders({ Authorization: `Bearer ${token}` })
      : new HttpHeaders();
 
    const params = new HttpParams()
      .set('studentRegno', studentRegno)
      .set('studentName', request.name ?? '')
      .set('studentPhoto', request.photo ?? '')
      .set('cgpa', request.cgpa != null ? String(request.cgpa) : '0');
 
    return this.http.put<{ message: string }>(
      `${this.apiUrl}/api/Standalone/my-profile`,
      null,
      { headers, params }
    );
  }

  /**
   * Register new student with name, regno, password, and auto-generated pincode
   */
  registerStudent(request: RegisterStudentRequest): Observable<RegisterStudentResponse> {
    return this.http.post<RegisterStudentResponse>(
      `${this.apiUrl}/api/Admin/students/register`,
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
      `${this.apiUrl}/api/Standalone/students/${regno}/reset-password`,
      {}
    );
  }

  /**
   * Retrieve all student records
   */
  getAllStudents(): Observable<Student[]> {
    return this.http.get<Student[]>(
      `${this.apiUrl}/api/Admin/students`
    );
  }

  updatePassword(studentRegno: string, newPassword: string): Observable<{ message: string }> {
    let params = new HttpParams().set('studentRegno', studentRegno).set('newPassword', newPassword);
    return this.http.put<{ message: string }>(`${this.apiUrl}/api/standalone/change-password`, null, { params });
  }

  /**
   * Update student name, photo, and CGPA by admin
   */
  updateStudentProfileByAdmin(regno: string, request: UpdateProfileRequest): Observable<Student> {
    return this.http.put<Student>(
      `${this.apiUrl}/api/Standalone/students/${regno}/profile`,
      request
    );
  }
}
