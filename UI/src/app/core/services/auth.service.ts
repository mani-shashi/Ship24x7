import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  User, LoginCredentials, AuthResponse, UpdateProfileRequest, 
  ChangePasswordRequest, SavedAddress, SaveAddressRequest, 
  UserPreferences, UpdatePreferencesRequest 
} from '../types';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private apiBase = environment.apiUrl;

  checkHealth(): Observable<any> {
    return this.http.get(`${this.apiBase}/auth/health`);
  }

  login(credentials: LoginCredentials): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiBase}/auth/login`, credentials);
  }

  register(userData: any): Observable<User> {
    return this.http.post<User>(`${this.apiBase}/auth/register`, userData);
  }

  refreshToken(): Observable<any> {
    return this.http.post(`${this.apiBase}/auth/refresh`, {});
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.apiBase}/auth/logout`, {});
  }

  getCurrentUser(): Observable<User> {
    return this.http.get<User>(`${this.apiBase}/auth/me`);
  }

  // --- Profile & Settings ---
  updateProfile(data: UpdateProfileRequest): Observable<User> {
    return this.http.put<User>(`${this.apiBase}/auth/me`, data);
  }

  changePassword(data: ChangePasswordRequest): Observable<any> {
    return this.http.put(`${this.apiBase}/auth/me/password`, data);
  }

  verifyEmail(token: string): Observable<any> {
    return this.http.get(`${this.apiBase}/auth/verify-email?token=${token}`);
  }

  // --- Address Book ---
  getSavedAddresses(): Observable<SavedAddress[]> {
    return this.http.get<SavedAddress[]>(`${this.apiBase}/auth/addresses`);
  }

  saveAddress(data: SaveAddressRequest): Observable<SavedAddress> {
    return this.http.post<SavedAddress>(`${this.apiBase}/auth/addresses`, data);
  }

  updateAddress(id: string, data: SaveAddressRequest): Observable<SavedAddress> {
    return this.http.put<SavedAddress>(`${this.apiBase}/auth/addresses/${id}`, data);
  }

  deleteAddress(id: string): Observable<any> {
    return this.http.delete(`${this.apiBase}/auth/addresses/${id}`);
  }

  // --- User Preferences ---
  getUserPreferences(): Observable<UserPreferences> {
    return this.http.get<UserPreferences>(`${this.apiBase}/auth/preferences`);
  }

  updateUserPreferences(data: UpdatePreferencesRequest): Observable<UserPreferences> {
    return this.http.put<UserPreferences>(`${this.apiBase}/auth/preferences`, data);
  }

  // --- Notification Preferences ---
  getNotificationPreferences(): Observable<any> {
    return this.http.get<any>(`${this.apiBase}/notifications/preferences`.replace('/v1/v1', '/v1'));
  }

  getNotificationHistory(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiBase}/notifications/history`.replace('/v1/v1', '/v1'));
  }

  updateNotificationPreferences(prefs: any): Observable<any> {
    return this.http.put(`${this.apiBase}/auth/notifications/preferences`, prefs);
  }

  // --- MFA ---
  mfaSetup(): Observable<any> {
    return this.http.post(`${this.apiBase}/auth/mfa/setup`, {});
  }

  mfaVerify(code: string): Observable<any> {
    return this.http.post(`${this.apiBase}/auth/mfa/verify`, { code });
  }

  mfaDisable(): Observable<any> {
    return this.http.post(`${this.apiBase}/auth/mfa/disable`, {});
  }

  // --- User Management (Admin) ---
  getUsers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiBase}/auth/users`);
  }

  activateUser(id: string): Observable<any> {
    return this.http.post(`${this.apiBase}/auth/users/${id}/activate`, {});
  }

  deactivateUser(id: string): Observable<any> {
    return this.http.post(`${this.apiBase}/auth/users/${id}/deactivate`, {});
  }

  updateUser(id: string, data: any): Observable<any> {
    return this.http.put(`${this.apiBase}/auth/users/${id}`, data);
  }
}
