// file: src/app/core/services/admin.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
 
export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}
 
export interface ChangePasswordResponse {
  success: boolean;
  message: string;
}
 
@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;
 
  changePassword(request: ChangePasswordRequest): Observable<ChangePasswordResponse> {
    // token must be stored on login (see note below)
    const token = localStorage.getItem('token');
 
    const headers = token
      ? new HttpHeaders({ 'Authorization': `Bearer ${token}` })
      : new HttpHeaders(); // still send request but backend will return 401
 
    return this.http.post<ChangePasswordResponse>(
      `${this.apiUrl}/api/Admin/change-password`,
      request,
      { headers }
    ).pipe(
      // The catchError block — replace the existing one:
catchError(err => {
  let errMsg = 'Unknown error';
  if (err?.status === 401) {
    errMsg = 'Unauthorized — login required or session expired.';
  } else if (err?.error?.message) {
    errMsg = err.error.message;          // ← backend returns { message: "..." }
  } else if (typeof err?.error === 'string') {
    errMsg = err.error;
  } else if (err?.message) {
    errMsg = err.message;
  }
  return throwError(() => ({ status: err.status, message: errMsg }));
})
    );
  }
}
 