import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TrackingService {
  private http = inject(HttpClient);
  private apiBase = environment.apiUrl;

  trackShipment(trackingNumber: string): Observable<any> {
    return this.http.get<any>(`${this.apiBase}/tracking/${trackingNumber}`);
  }

  recordTrackingEvent(eventData: any): Observable<any> {
    return this.http.post<any>(`${this.apiBase}/tracking/events`, eventData);
  }
}
