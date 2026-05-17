import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { StoreService } from '../../core/services/store.service';
import { PaymentService } from '../../core/services/payment.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.css']
})
export class CheckoutComponent {
  store = inject(StoreService);
  private payment = inject(PaymentService);
  private router = inject(Router);

  isLoading = signal(false);
  isProcessing = signal(false);
  paymentSuccess = signal(false);
  public Math = Math;

  // In a real app, these would come from a 'checkout' state in StoreService
  shipments = signal([
    { id: 'S1', serviceType: 'Express', totalAmount: 450, trackingNumber: 'SHIP-9910-A', weight: 2.5 },
    { id: 'S2', serviceType: 'Standard', totalAmount: 120, trackingNumber: 'SHIP-9910-B', weight: 1.2 }
  ]);

  totalAmount = computed(() => this.shipments().reduce((acc, s) => acc + s.totalAmount, 0));
  tax = computed(() => this.totalAmount() * 0.18);
  grandTotal = computed(() => this.totalAmount() + this.tax());

  initiatePayment() {
    const shipments = this.shipments();
    if (shipments.length === 0) return;

    // For now, process the first shipment. Multi-shipment batch payment
    // requires a backend endpoint that accepts multiple shipment IDs.
    const primary = shipments[0];
    this.isProcessing.set(true);

    this.payment.processPayment(primary.id, `CART_${primary.id.substring(0,8)}`, this.grandTotal()).subscribe({
      next: (res: any) => {
        this.isProcessing.set(false);
        this.paymentSuccess.set(true);
        this.store.addNotification('Payment processed successfully. Your shipments are now active.', 'success');
        setTimeout(() => this.router.navigate(['/dashboard']), 3000);
      },
      error: (err: Error) => {
        this.isProcessing.set(false);
        const msg = err?.message ?? 'Payment failed. Please try again.';
        this.store.addNotification(msg, 'error');
      }
    });
  }
}
