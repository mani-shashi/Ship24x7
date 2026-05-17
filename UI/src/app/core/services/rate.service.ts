import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class RateService {
  private http = inject(HttpClient);
  private apiBase = environment.apiUrl;

  getRates(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiBase}/rates`);
  }

  createRate(rateData: any): Observable<any> {
    return this.http.post<any>(`${this.apiBase}/rates`, rateData);
  }

  updateRate(id: string, rateData: any): Observable<any> {
    return this.http.put<any>(`${this.apiBase}/rates/${id}`, rateData);
  }

  deleteRate(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiBase}/rates/${id}`);
  }
}
