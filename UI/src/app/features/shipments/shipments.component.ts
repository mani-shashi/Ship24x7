import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { StoreService } from '../../core/services/store.service';
import { ShipmentService } from '../../core/services/shipment.service';
import { NotificationService } from '../../core/services/notification.service';
import { SkeletonComponent } from './skeleton.component';
import { listAnimation } from '../../shared/animations';
import { Shipment } from '../../core/types';

@Component({
  selector: 'app-shipments',
  standalone: true,
  imports: [CommonModule, RouterLink, SkeletonComponent],
  templateUrl: './shipments.component.html', 
  animations: [listAnimation]
})
export class ShipmentsComponent implements OnInit {
  private shipmentService = inject(ShipmentService);
  private notificationService = inject(NotificationService);
  protected store = inject(StoreService);

  shipments = signal<Shipment[]>([]);
  isLoading = signal(true);
  selectedIds = signal<Set<string>>(new Set());
  notifications = signal<any[]>([]);

  ngOnInit() {
    this.loadShipments();
    this.loadNotifications();
  }

  loadNotifications() {
    this.notificationService.getHistory(1, 20).subscribe({
      next: (logs) => this.notifications.set(logs)
    });
  }

  getOTPForShipment(trackingNumber: string): string | null {
    const note = this.notifications().find(n => 
      n.eventType === 'ShipmentOutForDelivery' && 
      (n.body.includes(trackingNumber) || n.subject.includes(trackingNumber))
    );
    if (!note) return null;
    const match = note.body.match(/OTP[:\s]+(\d{6})/i);
    return match ? match[1] : null;
  }

  loadShipments() {
    this.isLoading.set(true);
    this.shipmentService.getShipments().subscribe({
      next: (data) => {
        this.shipments.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.shipments.set([]);
        if (err.status !== 404) {
          this.store.addNotification('Failed to load shipments. Please try again later.', 'error');
        }
      }
    });
  }

  toggleSelection(id: string) {
    const current = new Set(this.selectedIds());
    if (current.has(id)) current.delete(id);
    else current.add(id);
    this.selectedIds.set(current);
  }

  toggleAll() {
    if (this.selectedIds().size === this.shipments().length) {
      this.selectedIds.set(new Set());
    } else {
      this.selectedIds.set(new Set(this.shipments().map(s => s.id)));
    }
  }

  bulkAction(action: string) {
    const ids = Array.from(this.selectedIds());
    if (ids.length === 0) return;

    if (action === 'Archive') {
      this.isLoading.set(true);
      this.shipmentService.archiveShipments(ids).subscribe({
        next: () => {
          this.store.addNotification(`${ids.length} shipments archived successfully`, 'success');
          this.selectedIds.set(new Set());
          this.loadShipments();
        },
        error: (err) => {
          this.isLoading.set(false);
          this.store.addNotification('Failed to archive shipments: ' + (err.error?.error || 'Unknown error'), 'error');
        }
      });
    } else {
      this.store.addNotification(`${action} initiated for ${ids.length} shipments`, 'info');
      this.selectedIds.set(new Set());
    }
  }
}
