// file: src/app/core/services/userlog.service.ts

import { Injectable } from '@angular/core';

import { HttpClient, HttpHeaders } from '@angular/common/http';

import { Observable } from 'rxjs';

import { UserLog } from '../models';

import { environment } from '../../../environments/environment';
 
@Injectable({ providedIn: 'root' })

export class UserlogService {

  private readonly apiUrl = environment.apiUrl;
 
  constructor(private http: HttpClient) {}
 
  /**

   * Retrieve all user login/logout activity logs.

   * Requires an admin JWT token stored in localStorage under key 'token'.

   */

  getUserLogs(): Observable<UserLog[]> {

    const token = localStorage.getItem('token');

    const headers = token

      ? new HttpHeaders({ Authorization: `Bearer ${token}` })

      : new HttpHeaders();
 
    // ✅ Corrected: was '/api/userlogs', must be '/api/admin/userlogs'

    return this.http.get<UserLog[]>(`${this.apiUrl}/api/Admin/userlogs`, { headers });

  }

}
 