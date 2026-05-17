import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { StoreService } from '../../core/services/store.service';
import { ShipmentService } from '../../core/services/shipment.service';
import { NotificationService, NotificationLog } from '../../core/services/notification.service';
import { Shipment } from '../../core/types';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent implements OnInit {
  store = inject(StoreService);
  private shipmentService = inject(ShipmentService);
  private notificationService = inject(NotificationService);

  stats = signal<any[]>([]);
  recentShipments = signal<Shipment[]>([]);
  notifications = signal<NotificationLog[]>([]);
  isLoading = signal(true);

  ngOnInit() {
    this.fetchDashboardData();
    this.loadNotifications();
  }

  loadNotifications() {
    this.notificationService.getHistory(1, 5).subscribe({
      next: (logs) => {
        this.notifications.set(logs);
      },
      error: () => {
        // Fallback to empty if service fails or not routed yet
        this.notifications.set([]);
      }
    });
  }

  fetchDashboardData() {
    this.isLoading.set(true);
    
    // Fetch aggregated stats & recent shipments
    this.shipmentService.getShipments().subscribe({
      next: (shipments: Shipment[]) => {
        this.recentShipments.set(shipments.slice(0, 5));
        
        // Synthesize stats from the full list for total accuracy
        const total = shipments.length;
        const inTransit = shipments.filter(s => ['InTransit', 'OutForDelivery', 'PickedUp'].includes(s.status)).length;
        const pending = shipments.filter(s => ['Draft', 'Booked', 'PaymentPending', 'PaymentFailed'].includes(s.status)).length;
        const delivered = shipments.filter(s => s.status === 'Delivered').length;

        this.stats.set([
          { 
            label: 'Total Shipments', 
            value: total.toLocaleString(), 
            icon: 'inventory_2', 
            color: 'text-accent-gold', 
            change: '+Live',
            changeColor: 'text-emerald-500'
          },
          { 
            label: 'In Transit', 
            value: inTransit.toLocaleString(), 
            icon: 'local_shipping', 
            color: 'text-indigo-500', 
            change: 'Active',
            changeColor: 'text-indigo-500'
          },
          { 
            label: 'Pending Action', 
            value: pending.toLocaleString(), 
            icon: 'schedule', 
            color: 'text-amber-500', 
            change: 'Needs attention',
            changeColor: 'text-amber-500'
          },
          { 
            label: 'Delivered', 
            value: delivered.toLocaleString(), 
            icon: 'check_circle', 
            color: 'text-emerald-500', 
            change: 'Completed',
            changeColor: 'text-emerald-500'
          },
        ]);
        this.isLoading.set(false);
      },
      error: () => {
        this.setDefaultStats();
        this.isLoading.set(false);
      }
    });
  }

  private setDefaultStats() {
    this.stats.set([
      { label: 'Total Shipments', value: '0', icon: 'inventory_2', color: 'text-accent-gold', change: '0%' },
      { label: 'In Transit', value: '0', icon: 'local_shipping', color: 'text-indigo-500', change: '0%' },
      { label: 'Pending Action', value: '0', icon: 'schedule', color: 'text-amber-500', change: '0%' },
      { label: 'Delivered', value: '0', icon: 'check_circle', color: 'text-emerald-500', change: '0%' },
    ]);
  }

  extractOTP(body: string): string | null {
    if (!body) return null;
    const match = body.match(/OTP[:\s]+(\d{6})/i);
    return match ? match[1] : null;
  }
}
