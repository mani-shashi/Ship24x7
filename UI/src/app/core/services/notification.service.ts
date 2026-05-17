import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface NotificationLog {
  id: string;
  userId: string;
  recipientEmail: string;
  recipientPhone: string;
  channel: string;
  subject: string;
  body: string; // We'll expect Claude to add this
  status: string;
  eventType: string;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/notifications`.replace('/v1/v1', '/v1');

  getHistory(pageNumber: number = 1, pageSize: number = 50): Observable<NotificationLog[]> {
    return this.http.get<NotificationLog[]>(`${this.apiUrl}/history`, {
      params: { pageNumber, pageSize }
    });
  }

  getPreferences(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/preferences`);
  }

  updatePreferences(preferences: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/preferences`, preferences);
  }
}
