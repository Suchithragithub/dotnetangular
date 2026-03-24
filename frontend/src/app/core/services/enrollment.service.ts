import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CourseEnroll,
  Student,
  Course,
  Session,
  Department,
  Level,
  Semester
} from '../models';

/**
 * Interface for enrollment creation request
 */
export interface CreateEnrollmentRequest {
  studentregno: string; // Changed from studentId?: number;
  pincode: string;      // Added pincode (required by backend)
  courseId: number;
  sessionId: number;
  departmentId: number;
  levelId: number;
  semesterId: number;
}

/**
 * Interface for checking enrollment availability
 */
export interface CheckAvailabilityRequest {
  studentregno: string;
  courseId: number;
  sessionId: number;
  departmentId: number;
  levelId: number;
  semesterId: number;
}

/**
 * Interface for availability check response
 */
export interface AvailabilityResponse {
  available: boolean;
  alreadyEnrolled: boolean;
  seatsAvailable: number | boolean;
  message?: string;
}

/**
 * Interface for enrollment with full details
 */
// export interface EnrollmentDetails extends Omit<CourseEnroll, 'course' | 'session' | 'department' | 'level' | 'semester'> {
//   student?: Student;
//   course?: Course;
//   session?: Session;
//   department?: Department;
//   level?: Level;
//   semester?: Semester;
// }
export interface EnrollmentDetails {
  studentName: string;
  studentRegno: string;
  courseId: number;
  courseName: string;
  sessionName: string;
  departmentName: string;
  semesterName: string;
  enrollDate: string;
}

/**
 * Interface for printable enrollment details
 */
export interface PrintableEnrollment {
  enrollment: EnrollmentDetails;
  student: Student;
  course: Course;
  session: Session;
  department: Department;
  level: Level;
  semester: Semester;
}

/**
 * Service for managing course enrollments
 * Handles enrollment creation, availability checks, and retrieval of enrollment records
 */
@Injectable({ providedIn: 'root' })
export class EnrollmentService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  /**
   * Create a new course enrollment record
   * @param enrollment - Enrollment data including session, department, level, semester, and course
   * @returns Observable of the created enrollment record
   */
  // createEnrollment(enrollment: CreateEnrollmentRequest): Observable<CourseEnroll> {
  //   return this.http.post<CourseEnroll>(
  //     `${this.apiUrl}/api/enrollments`,
  //     enrollment
  //   );
  // }

  createEnrollment(enrollment: CreateEnrollmentRequest): Observable<CourseEnroll> {
    let params = new HttpParams()
      .set('studentRegno', enrollment.studentregno)
      .set('pincode', enrollment.pincode)
      .set('sessionId', enrollment.sessionId.toString())
      .set('departmentId', enrollment.departmentId.toString())
      .set('levelId', enrollment.levelId.toString())
      .set('courseId', enrollment.courseId.toString())
      .set('semesterId', enrollment.semesterId.toString());

    return this.http.post<CourseEnroll>(
      `${this.apiUrl}/api/Standalone/enroll`,
      null,
      { params }
    );
  }

  /**
   * Check if student is already enrolled in a course and verify seat availability
   * @param request - Request containing student and course information
   * @returns Observable of availability check result
   */
  checkAvailability(request: CheckAvailabilityRequest): Observable<AvailabilityResponse> {
    return this.http.post<AvailabilityResponse>(
      `${this.apiUrl}/api/enrollments/check-availability`,
      request
    );
  }

  /**
   * Retrieve enrollment history for a specific student
   * Includes course, session, department, level, and semester details
   * @param regno - Student registration number
   * @returns Observable of enrollment history with full details
   */
  getStudentEnrollments(regno: string | number): Observable<EnrollmentDetails[]> {
    return this.http.get<EnrollmentDetails[]>(
      `${this.apiUrl}/api/Standalone/enroll-history`,
      { params: { studentRegno: regno.toString() } }  // pass as query param
    );
  }

  /**
   * Retrieve printable enrollment details for a specific student
   * Includes student, course, session, department, level, and semester information
   * @param regno - Student registration number
   * @returns Observable of printable enrollment details
   */
  getPrintableEnrollment(regno: string | number): Observable<PrintableEnrollment[]> {
    return this.http.get<PrintableEnrollment[]>(
      `${this.apiUrl}/api/enrollments/print/${regno}`
    );
  }

  /**
   * Retrieve all enrollment records
   * Includes student, course, session, department, level, and semester details
   * @returns Observable of all enrollment records with full details
   */
  getAllEnrollments(): Observable<EnrollmentDetails[]> {
    return this.http.get<EnrollmentDetails[]>(
      `${this.apiUrl}/api/Admin/enrollment-history`
    );
  }

  /**
   * Retrieve printable enrollment details for a specific course
   * Includes all student and academic information for students enrolled in the course
   * @param cid - Course ID
   * @returns Observable of printable enrollment details for the course
   */
  getPrintableCourseEnrollments(cid: string | number): Observable<PrintableEnrollment[]> {
    return this.http.get<PrintableEnrollment[]>(
      `${this.apiUrl}/api/enrollments/print/course/${cid}`
    );
  }
}
