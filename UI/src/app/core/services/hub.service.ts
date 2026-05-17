import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class HubService {
  private http = inject(HttpClient);
  private apiBase = environment.apiUrl;

  getHubs(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiBase}/hubs`);
  }
}
