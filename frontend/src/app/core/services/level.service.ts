import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Level } from '../models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class LevelService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  /**
   * Get all levels for enrollment dropdown
   * @returns Observable of Level array
   */
  getLevels(): Observable<Level[]> {
    return this.http.get<Level[]>(`${this.apiUrl}/api/Admin/levels`);
  }
  getLevelsForEnrollment(): Observable<Level[]> {
    return this.http.get<Level[]>(`${this.apiUrl}/api/Standalone/levels`);
  }

  /**
   * Create new academic level
   * @param level - Level object to create
   * @returns Observable of created Level
   */
  createLevel(level: Level): Observable<Level> {
    return this.http.post<Level>(`${this.apiUrl}/api/Admin/levels`, level);
  }

  /**
   * Delete level by ID
   * @param id - Level ID to delete
   * @returns Observable of void
   */
  deleteLevel(id: number | string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/Admin/levels/${id}`);
  }
}
