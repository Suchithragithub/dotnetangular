import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Session } from '../models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SessionService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  /**
   * Get all available sessions for enrollment dropdown
   * @returns Observable of Session array
   */
  getSessions(): Observable<Session[]> {
    return this.http.get<Session[]>(`${this.apiUrl}/api/Admin/sessions`);
  }

  // Add new (used by student enrollment)
  getSessionsForEnrollment(): Observable<Session[]> {
    return this.http.get<Session[]>(`${this.apiUrl}/api/Standalone/sessions`);
  }
  /**
   * Create new academic session
   * @param session - Session object to create
   * @returns Observable of created Session
   */
  createSession(session: Session): Observable<Session> {
    return this.http.post<Session>(`${this.apiUrl}/api/Admin/sessions`, session);
  }

  /**
   * Delete session by ID
   * @param id - Session ID to delete
   * @returns Observable of void
   */
  deleteSession(id: number | string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/Admin/sessions/${id}`);
  }
}
