import { Component, OnInit, signal, inject, computed, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ShipmentService } from '../../../core/services/shipment.service';
import { HubService } from '../../../core/services/hub.service';
import { StoreService } from '../../../core/services/store.service';
import { TrackingService } from '../../../core/services/tracking.service';
import { Shipment, ShipmentStatus } from '../../../core/types';

@Component({
  selector: 'app-admin-shipment-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './admin-shipment-list.component.html',
})
export class AdminShipmentListComponent implements OnInit {
  private shipmentService = inject(ShipmentService);
  private hubService = inject(HubService);
  private trackingService = inject(TrackingService);
  protected store = inject(StoreService);

  shipments = signal<Shipment[]>([]);
  hubs = signal<any[]>([]);
  isLoading = signal(true);
  searchTerm = signal('');
  openDropdownId = signal<string | null>(null);
  
  // Computed filtered list
  filteredShipments = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.shipments();
    
    return this.shipments().filter(s => 
      s.trackingNumber.toLowerCase().includes(term) ||
      s.senderAddress.contactName.toLowerCase().includes(term) ||
      s.senderAddress.city.toLowerCase().includes(term) ||
      s.receiverAddress.city.toLowerCase().includes(term) ||
      s.status.toLowerCase().includes(term)
    );
  });
  
  // Simulation states
  deliveryCodes = new Map<string, string>();
  assignedHubs = new Map<string, any>();
  activePickups = new Map<string, string>(); // shipmentId -> pickupId

  ngOnInit() {
    this.loadAllShipments();
    this.loadHubs();
  }

  loadHubs() {
    this.hubService.getHubs().subscribe({
      next: (data) => {
        this.hubs.set(data);
        this.refreshAssignedHubs();
      },
      error: (err) => this.store.addNotification('Failed to load hubs: ' + (err.error?.error || err.message), 'error')
    });
  }

  loadAllShipments() {
    this.isLoading.set(true);
    this.shipmentService.getShipments().subscribe({
      next: (data) => {
        this.shipments.set(data);
        this.isLoading.set(false);
        this.refreshAssignedHubs();
      },
      error: (err) => {
        this.store.addNotification('Failed to load shipments: ' + (err.error?.error || 'Unknown error'), 'error');
        this.isLoading.set(false);
      }
    });
  }

  private refreshAssignedHubs() {
    this.shipments().forEach(s => {
      if (s.pickupId) {
        this.activePickups.set(s.id, s.pickupId);
      }
      if (s.currentHubId) {
        const hub = this.hubs().find(h => h.id === s.currentHubId);
        if (hub) this.assignedHubs.set(s.id, hub);
      }
    });
  }

  updateStatus(shipmentId: string, status: string, reason: string = 'Admin manual update') {
    const shipment = this.shipments().find(s => s.id === shipmentId);
    if (!shipment) return;

    this.isLoading.set(true);
    this.shipmentService.updateShipmentStatus(shipmentId, status as any, reason).subscribe({
      next: () => {
        // Record tracking event for production consistency
        this.trackingService.recordTrackingEvent({
          shipmentId: shipmentId,
          trackingNumber: shipment.trackingNumber,
          status: status,
          location: this.assignedHubs.get(shipmentId)?.city || 'Operational Center',
          description: reason,
          eventTimestamp: new Date().toISOString()
        }).subscribe();

        this.store.addNotification(`Shipment status updated to ${status}`, 'success');
        this.loadAllShipments();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.store.addNotification('Status update failed: ' + (err.error?.error || 'Unknown error'), 'error');
      }
    });
  }

  confirmShipment(shipmentId: string, customerId: string) {
    const shipment = this.shipments().find(s => s.id === shipmentId);
    this.isLoading.set(true);
    this.shipmentService.confirmShipment(shipmentId, customerId).subscribe({
      next: () => {
        // Record tracking event
        this.trackingService.recordTrackingEvent({
          shipmentId: shipmentId,
          trackingNumber: shipment?.trackingNumber,
          status: 'Booked',
          location: shipment?.senderAddress?.city || 'Origin',
          description: 'Shipment booking confirmed by administrator.',
          eventTimestamp: new Date().toISOString()
        }).subscribe();

        this.store.addNotification('Shipment confirmed successfully', 'success');
        this.loadAllShipments();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.store.addNotification('Confirmation failed: ' + (err.error?.error || 'Server error'), 'error');
      }
    });
  }

  // Enhanced Lifecycle Actions (Now using real Backend Commands)
  
  assignHub(shipmentId: string, hubId?: string) {
    const shipment = this.shipments().find(s => s.id === shipmentId);
    
    // Find a suitable hub if no specific hubId (Guid) is provided
    let targetHubId = hubId;
    
    // If hubId is not a Guid (looks like a city name or is missing), try to find by city
    if (!hubId || hubId.length < 20) {
      const cityToMatch = hubId || shipment?.senderAddress?.city;
      const hubInCity = this.hubs().find(h => h.city?.toLowerCase() === cityToMatch?.toLowerCase());
      targetHubId = hubInCity?.id || this.hubs()[0]?.id;
    }
    
    if (!targetHubId) {
      this.store.addNotification('No hub found for assignment', 'error');
      return;
    }

    this.isLoading.set(true);
    this.shipmentService.assignToHub(shipmentId, targetHubId).subscribe({
      next: () => {
        const hub = this.hubs().find(h => h.id === targetHubId);
        // Record tracking event
        this.trackingService.recordTrackingEvent({
          shipmentId: shipmentId,
          trackingNumber: shipment?.trackingNumber,
          status: shipment?.status || 'InTransit',
          location: hub?.city || hub?.name || 'Hub',
          description: `Shipment arrived at hub: ${hub?.name}`,
          eventTimestamp: new Date().toISOString()
        }).subscribe();

        this.store.addNotification('Shipment assigned to hub successfully', 'success');
        this.loadAllShipments();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.store.addNotification('Hub assignment failed: ' + (err.error?.error || 'Server error'), 'error');
      }
    });
  }

  initiateTransit(shipmentId: string) {
    const shipment = this.shipments().find(s => s.id === shipmentId);
    const hubId = shipment?.currentHubId || this.hubs()[0]?.id;
    const operatorId = this.store.currentUser()?.id || '00000000-0000-0000-0000-000000000001';

    if (!hubId) {
      this.store.addNotification('Cannot initiate transit: No hub assigned', 'error');
      return;
    }

    this.isLoading.set(true);
    this.shipmentService.initiateTransit(shipmentId, hubId, operatorId).subscribe({
      next: () => {
        const hub = this.hubs().find(h => h.id === hubId);
        // Record tracking event
        this.trackingService.recordTrackingEvent({
          shipmentId: shipmentId,
          trackingNumber: shipment?.trackingNumber,
          status: 'InTransit',
          location: hub?.city || hub?.name || 'Transit Center',
          description: `Shipment departed from hub: ${hub?.name}`,
          eventTimestamp: new Date().toISOString()
        }).subscribe();

        this.store.addNotification('Shipment is now in transit', 'success');
        this.loadAllShipments();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.store.addNotification('Transit failed: ' + (err.error?.error || 'Server error'), 'error');
      }
    });
  }

  dispatchForDelivery(shipmentId: string) {
    const shipment = this.shipments().find(s => s.id === shipmentId);
    if (!shipment) {
      this.store.addNotification('Shipment not found', 'error');
      return;
    }
    
    const hubId = shipment.currentHubId || this.hubs()[0]?.id;
    const operatorId = this.store.currentUser()?.id || '00000000-0000-0000-0000-000000000001';

    if (!hubId) {
      this.store.addNotification('Cannot dispatch: No hub assigned', 'error');
      return;
    }

    this.isLoading.set(true);
    this.shipmentService.markOutForDelivery(shipmentId, hubId, 'DEL-AGENT-001', operatorId).subscribe({
      next: (res) => {
        const otp = res.rawOtp || 'SENT-VIA-SMS';
        const hub = this.hubs().find(h => h.id === hubId);
        
        // Record tracking event
        this.trackingService.recordTrackingEvent({
          shipmentId: shipmentId,
          trackingNumber: shipment.trackingNumber,
          status: 'OutForDelivery',
          location: hub?.city || hub?.name || 'Local Hub',
          description: `Out for delivery. Agent assigned.`,
          eventTimestamp: new Date().toISOString()
        }).subscribe();

        this.store.addNotification('Dispatched for delivery!', 'success');
        
        this.loadAllShipments();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.store.addNotification('Dispatch failed: ' + (err.error?.error || 'Server error'), 'error');
      }
    });
  }

  verifyAndDeliver(shipmentId: string) {
    const otp = prompt('Enter 6-digit delivery OTP:');
    if (!otp) return;

    const operatorId = this.store.currentUser()?.id || '00000000-0000-0000-0000-000000000001';

    this.isLoading.set(true);
    this.shipmentService.completeDelivery(shipmentId, otp, operatorId).subscribe({
      next: () => {
        const shipment = this.shipments().find(s => s.id === shipmentId);
        // Record tracking event
        this.trackingService.recordTrackingEvent({
          shipmentId: shipmentId,
          trackingNumber: shipment?.trackingNumber,
          status: 'Delivered',
          location: shipment?.receiverAddress?.city || 'Destination',
          description: `Shipment delivered successfully. OTP verified.`,
          eventTimestamp: new Date().toISOString()
        }).subscribe();

        this.store.addNotification('Shipment delivered successfully', 'success');
        this.loadAllShipments();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.store.addNotification('Delivery failed: ' + (err.error?.error || 'Invalid OTP'), 'error');
      }
    });
  }

  viewHistory(shipmentId: string) {
    this.shipmentService.getShipmentHistory(shipmentId).subscribe({
      next: (history) => {
        // We'll show this in a simple alert for now, or a modal if we have one
        const timeline = history.map(h => `[${new Date(h.changedAt).toLocaleString()}] ${h.fromStatus} -> ${h.toStatus} (${h.reason})`).join('\n');
        alert(`Shipment History:\n\n${timeline}`);
      },
      error: () => this.store.addNotification('Failed to fetch history', 'error')
    });
  }

  schedulePickup(shipmentId: string) {
    const shipment = this.shipments().find(s => s.id === shipmentId);
    if (!shipment) return;

    if (shipment.status === 'Draft') {
      this.store.addNotification('Cannot schedule pickup for a Draft shipment. Please Confirm Booking first.', 'warning');
      return;
    }

    const pickupData = {
      customerId: shipment.customerId,
      pickupDate: new Date(Date.now() + 86400000).toISOString(),
      timeSlot: 'Morning',
      notes: 'Scheduled by Admin'
    };

    this.isLoading.set(true);
    this.shipmentService.schedulePickup(shipmentId, pickupData).subscribe({
      next: (res: any) => {
        const pickupId = res.pickupId || res.id;
        this.activePickups.set(shipmentId, pickupId); 
        this.store.addNotification('Pickup scheduled successfully', 'success');
        this.loadAllShipments();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.store.addNotification('Failed to schedule pickup: ' + (err.error?.error || 'Unknown error'), 'error');
      }
    });
  }

  completePickup(shipmentId: string) {
    const pickupId = this.activePickups.get(shipmentId);
    if (!pickupId) {
      this.updateStatus(shipmentId, 'PickedUp', 'Manual pickup confirmation');
      return;
    }

    this.isLoading.set(true);
    this.shipmentService.completePickup(pickupId).subscribe({
      next: () => {
        const shipment = this.shipments().find(s => s.id === shipmentId);
        // Record tracking event
        this.trackingService.recordTrackingEvent({
          shipmentId: shipmentId,
          trackingNumber: shipment?.trackingNumber,
          status: 'PickedUp',
          location: shipment?.senderAddress?.city || 'Origin',
          description: `Shipment picked up by driver.`,
          eventTimestamp: new Date().toISOString()
        }).subscribe();

        this.store.addNotification('Pickup completed successfully', 'success');
        this.activePickups.delete(shipmentId);
        this.loadAllShipments();
      },
      error: (err) => {
        const errorMessage = err.error?.error || err.message || '';
        if (errorMessage.toLowerCase().includes('not found')) {
          this.store.addNotification('Pickup record not found in backend. Performing manual override...', 'warning');
          this.activePickups.delete(shipmentId);
          this.updateStatus(shipmentId, 'PickedUp', 'Manual override (Missing pickup record)');
        } else {
          this.isLoading.set(false);
          this.store.addNotification('Pickup completion failed: ' + errorMessage, 'error');
        }
      }
    });
  }

  resendOtp(shipmentId: string) {
    const shipment = this.shipments().find(s => s.id === shipmentId);
    if (!shipment) return;

    const hubId = shipment.currentHubId || this.hubs()[0]?.id;
    if (!hubId) {
      this.store.addNotification('Cannot resend: No hub assigned', 'error');
      return;
    }

    this.isLoading.set(true);
    // Phase 1: Move to Delayed (to allow transition back to OutForDelivery)
    this.shipmentService.updateShipmentStatus(shipmentId, 'Delayed', 'Regenerating OTP for resend').subscribe({
      next: () => {
        const operatorId = this.store.currentUser()?.id || '00000000-0000-0000-0000-000000000001';
        // Phase 2: Move back to OutForDelivery (triggers new OTP generation and notification)
        this.shipmentService.markOutForDelivery(shipmentId, hubId, 'DEL-AGENT-001', operatorId).subscribe({
          next: (res) => {
            const otp = res.rawOtp || 'SENT-VIA-SMS';
            this.store.addNotification(`New OTP sent to customer! OTP: ${otp}`, 'success');
            this.loadAllShipments();
          },
          error: (err) => {
            this.isLoading.set(false);
            this.store.addNotification('Failed to mark out for delivery: ' + (err.error?.error || 'Unknown error'), 'error');
          }
        });
      },
      error: (err) => {
        this.isLoading.set(false);
        this.store.addNotification('Failed to cycle status: ' + (err.error?.error || 'Unknown error'), 'error');
      }
    });
  }

  toggleDropdown(id: string, event: MouseEvent) {
    event.stopPropagation();
    if (this.openDropdownId() === id) {
      this.openDropdownId.set(null);
    } else {
      this.openDropdownId.set(id);
    }
  }

  @HostListener('document:click')
  onDocumentClick() {
    this.openDropdownId.set(null);
  }

  getActions(item: Shipment) {
    return [
      { 
        label: 'Confirm Booking', 
        icon: 'check_circle',
        action: () => this.confirmShipment(item.id, item.customerId), 
        isEnabled: item.status === 'Draft' 
      },
      { 
        label: 'Verify Payment', 
        icon: 'payments',
        action: () => this.updateStatus(item.id, 'Paid', 'Manual payment verification'), 
        isEnabled: item.status === 'Booked' 
      },
      { 
        label: 'Deploy Pickup', 
        icon: 'local_shipping',
        action: () => this.schedulePickup(item.id), 
        isEnabled: (item.status === 'Paid' || item.status === 'Booked') && !this.activePickups.get(item.id) 
      },
      { 
        label: 'Confirm Pickup', 
        icon: 'hail',
        action: () => this.completePickup(item.id), 
        isEnabled: !!this.activePickups.get(item.id) && (item.status === 'Paid' || item.status === 'Booked') 
      },
      { 
        label: this.assignedHubs.get(item.id) ? 'Scan at Next Hub' : 'Assign Origin Hub', 
        icon: 'hub',
        action: () => this.assignHub(item.id), 
        isEnabled: item.status === 'PickedUp' || item.status === 'InTransit' 
      },
      { 
        label: 'Depart for Transit', 
        icon: 'departure_board',
        action: () => this.initiateTransit(item.id), 
        isEnabled: item.status === 'PickedUp' && !!this.assignedHubs.get(item.id) 
      },
      { 
        label: 'Dispatch for Delivery', 
        icon: 'delivery_dining',
        action: () => this.dispatchForDelivery(item.id), 
        isEnabled: item.status === 'InTransit' 
      },
      { 
        label: 'Complete Delivery', 
        icon: 'check_circle',
        action: () => this.updateStatus(item.id, 'Delivered', 'Delivery completed successfully'), 
        isEnabled: item.status === 'OutForDelivery' 
      }
    ];
  }
}
