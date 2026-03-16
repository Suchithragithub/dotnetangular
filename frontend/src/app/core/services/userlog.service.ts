import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserLog } from '../models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class UserlogService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * Retrieve all user login/logout activity logs
   * @returns Observable of UserLog array
   */
  getUserLogs(): Observable<UserLog[]> {
    return this.http.get<UserLog[]>(`${this.apiUrl}/api/userlogs`);
  }
}
