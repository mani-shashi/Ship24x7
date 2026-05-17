import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HubService } from '../../../core/services/hub.service';
import { StoreService } from '../../../core/services/store.service';

@Component({
  selector: 'app-hub-management',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './hub-management.component.html',
  styleUrl: './hub-management.component.css'
})
export class HubManagementComponent implements OnInit {
  private hubService = inject(HubService);
  protected store = inject(StoreService);

  hubs = signal<any[]>([]);
  isLoading = signal(false);

  ngOnInit() {
    this.loadHubs();
  }

  loadHubs() {
    this.isLoading.set(true);
    this.hubService.getHubs().subscribe({
      next: (data) => {
        if (data && data.length > 0) {
          this.hubs.set(data);
        } else {
          // Fallback to mock if empty
          this.hubs.set([
            { id: '1', name: 'Mumbai Central Hub', code: 'BOM-01', city: 'Mumbai', capacity: 8500, currentLoad: 3825, status: 'Active', isActive: true },
            { id: '2', name: 'Delhi North Logistics', code: 'DEL-04', city: 'Delhi', capacity: 12000, currentLoad: 9840, status: 'High Load', isActive: true },
            { id: '3', name: 'Bangalore Tech Park Hub', code: 'BLR-02', city: 'Bangalore', capacity: 6000, currentLoad: 5760, status: 'Critical', isActive: true },
          ]);
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.store.addNotification('Failed to load hubs from server, showing local registry', 'info');
        this.hubs.set([
          { id: '1', name: 'Mumbai Central Hub', code: 'BOM-01', city: 'Mumbai', capacity: 8500, currentLoad: 3825, status: 'Active', isActive: true },
          { id: '2', name: 'Delhi North Logistics', code: 'DEL-04', city: 'Delhi', capacity: 12000, currentLoad: 9840, status: 'High Load', isActive: true },
          { id: '3', name: 'Bangalore Tech Park Hub', code: 'BLR-02', city: 'Bangalore', capacity: 6000, currentLoad: 5760, status: 'Critical', isActive: true },
        ]);
        this.isLoading.set(false);
      }
    });
  }

  provisionHub() {
    const newHub = {
      id: Math.random().toString(36).substring(7),
      name: 'New Logistics Node ' + (this.hubs().length + 1),
      code: 'NODE-' + (this.hubs().length + 1),
      location: 'Unassigned Location',
      capacity: 5000,
      currentLoad: 0,
      status: 'Active',
      isActive: true
    };
    this.hubs.update(hubs => [newHub, ...hubs]);
    this.store.addNotification('New hub node provisioned successfully', 'success');
  }

  toggleHubStatus(hubId: string) {
    this.hubs.update(hubs => hubs.map(h => {
      if (h.id === hubId) {
        const newState = !h.isActive;
        return { 
          ...h, 
          isActive: newState, 
          status: newState ? 'Active' : 'Maintenance' 
        };
      }
      return h;
    }));
    this.store.addNotification('Hub operational status updated', 'info');
  }

  getLoadColor(load: number, capacity: number): string {
    const percentage = (load / capacity) * 100;
    if (percentage > 90) return 'text-red-500';
    if (percentage > 70) return 'text-accent-gold';
    return 'text-emerald-500';
  }

  getLoadBg(load: number, capacity: number): string {
    const percentage = (load / capacity) * 100;
    if (percentage > 90) return 'bg-red-500';
    if (percentage > 70) return 'bg-accent-gold';
    return 'bg-emerald-500';
  }
}
