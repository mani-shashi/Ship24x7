import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Shipment, ShipmentStatus } from '../types';
import { StoreService } from './store.service';

@Injectable({
  providedIn: 'root'
})
export class ShipmentService {
  private http = inject(HttpClient);
  private store = inject(StoreService);
  private apiBase = environment.apiUrl;

  getShipments(): Observable<Shipment[]> {
    const user = this.store.currentUser();
    const isAdmin = this.store.isAdmin();
    
    let url = `${this.apiBase}/shipments`;
    if (user && !isAdmin) {
      url += `?customerId=${user.id}`;
    }
    
    return this.http.get<Shipment[]>(url).pipe(
      map((shipments: any[]) => shipments.map((s: any) => ({
        ...s,
        totalAmount: s.totalAmount || s.totalCost,
        bookedAt: s.bookedAt || s.createdAt
      })))
    );
  }

  getShipmentById(id: string): Observable<Shipment> {
    return this.http.get<Shipment>(`${this.apiBase}/shipments/${id}`);
  }

  getShipmentByTrackingNumber(trackingNumber: string, customerId?: string): Observable<any> {
    const url = customerId 
      ? `${this.apiBase}/shipments/tracking/${trackingNumber}?customerId=${customerId}`
      : `${this.apiBase}/shipments/tracking/${trackingNumber}`;
    return this.http.get<any>(url);
  }

  createShipment(shipmentData: any): Observable<Shipment> {
    return this.http.post<Shipment>(`${this.apiBase}/shipments`, shipmentData);
  }

  confirmShipment(shipmentId: string, customerId: string): Observable<any> {
    return this.http.put(`${this.apiBase}/shipments/${shipmentId}/confirm`, { shipmentId, customerId });
  }

  archiveShipments(ids: string[]): Observable<any> {
    return this.http.post(`${this.apiBase}/shipments/bulk-archive`, { ids });
  }

  getShipmentHistory(shipmentId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiBase}/shipments/${shipmentId}/history`);
  }

  updateShipmentStatus(shipmentId: string, newStatus: ShipmentStatus | string, reason: string = 'Admin override'): Observable<any> {
    return this.http.put(`${this.apiBase}/shipments/${shipmentId}/status`, { newStatus, reason });
  }

  // --- Specialized Lifecycle Endpoints ---
  
  assignToHub(shipmentId: string, hubId: string): Observable<any> {
    return this.http.put(`${this.apiBase}/shipments/${shipmentId}/hub`, { hubId });
  }

  initiateTransit(shipmentId: string, hubId: string, operatorId: string): Observable<any> {
    return this.http.put(`${this.apiBase}/shipments/${shipmentId}/transit`, { shipmentId, hubId, operatorId });
  }

  markOutForDelivery(shipmentId: string, hubId: string, agentId: string, operatorId: string): Observable<any> {
    return this.http.put(`${this.apiBase}/shipments/${shipmentId}/out-for-delivery`, { 
      shipmentId, 
      hubId, 
      deliveryAgentId: agentId, 
      operatorId 
    });
  }

  completeDelivery(shipmentId: string, otp: string, operatorId: string, override: boolean = false): Observable<any> {
    return this.http.put(`${this.apiBase}/shipments/${shipmentId}/delivered`, { 
      shipmentId, 
      otp, 
      operatorId, 
      supervisorOverride: override 
    });
  }

  // --- Pickup Operations ---
  schedulePickup(shipmentId: string, pickupData: any): Observable<any> {
    const payload = {
      shipmentId: shipmentId,
      ...pickupData
    };
    return this.http.post(`${this.apiBase}/pickups`, payload);
  }

  completePickup(pickupId: string, driverId: string | null = null): Observable<any> {
    return this.http.put(`${this.apiBase}/pickups/${pickupId}/complete`, { driverId });
  }

  // --- Dashboard & Analytics ---
  getDashboardStats(): Observable<any> {
    const user = this.store.currentUser();
    const url = user ? `${this.apiBase}/dashboard/summary?customerId=${user.id}` : `${this.apiBase}/dashboard/summary`;
    return this.http.get<any>(url);
  }

  // --- Templates ---
  getTemplates(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiBase}/notifications/templates`);
  }

  createTemplate(templateData: any): Observable<any> {
    return this.http.post<any>(`${this.apiBase}/notifications/templates`, templateData);
  }

  updateTemplate(id: string, templateData: any): Observable<any> {
    return this.http.put(`${this.apiBase}/notifications/templates/${id}`, templateData);
  }

  deleteTemplate(id: string): Observable<any> {
    return this.http.delete(`${this.apiBase}/notifications/templates/${id}`);
  }
}
