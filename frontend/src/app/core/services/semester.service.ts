import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Semester } from '../models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SemesterService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  /**
   * Get all semesters for enrollment dropdown
   * @returns Observable of Semester array
   */
  getAllSemesters(): Observable<Semester[]> {
    return this.http.get<Semester[]>(`${this.apiUrl}/api/semesters`);
  }

  /**
   * Create new semester
   * @param semester - The semester data to create
   * @returns Observable of created Semester
   */
  createSemester(semester: Semester): Observable<Semester> {
    return this.http.post<Semester>(`${this.apiUrl}/api/semesters`, semester);
  }

  /**
   * Delete semester by ID
   * @param id - The semester ID to delete
   * @returns Observable of void
   */
  deleteSemester(id: number | string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/semesters/${id}`);
  }
}
