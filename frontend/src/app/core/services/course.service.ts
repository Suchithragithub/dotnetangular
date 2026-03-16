import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Course } from '../models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CourseService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  /**
   * Get all courses for enrollment dropdown
   */
  getCourses(): Observable<Course[]> {
    return this.http.get<Course[]>(`${this.apiUrl}/api/courses`);
  }

  /**
   * Retrieve course details for editing
   */
  getCourseById(id: number | string): Observable<Course> {
    return this.http.get<Course>(`${this.apiUrl}/api/courses/${id}`);
  }

  /**
   * Create new course with code, name, unit, and seat limit
   */
  createCourse(course: Course): Observable<Course> {
    return this.http.post<Course>(`${this.apiUrl}/api/courses`, course);
  }

  /**
   * Update course details including code, name, unit, and seat limit
   */
  updateCourse(id: number | string, course: Course): Observable<Course> {
    return this.http.put<Course>(`${this.apiUrl}/api/courses/${id}`, course);
  }

  /**
   * Delete course by ID
   */
  deleteCourse(id: number | string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/courses/${id}`);
  }
}
