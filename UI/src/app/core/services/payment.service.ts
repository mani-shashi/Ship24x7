import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, switchMap, from } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RazorpayService } from './razorpay.service';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private http = inject(HttpClient);
  private razorpayService = inject(RazorpayService);
  private apiBase = environment.apiUrl;

  processPayment(shipmentId: string, trackingNumber: string, amount: number): Observable<any> {
    return this.createPaymentOrder(shipmentId, trackingNumber, amount).pipe(
      switchMap((order: any) => {
        return from(this.razorpayService.loadRazorpayScript()).pipe(
          switchMap((loaded) => {
            if (!loaded) throw new Error('Razorpay SDK failed to load');

            return new Observable((observer) => {
              const options = {
                key: environment.razorpayKey || 'rzp_test_5p7XJp2Xp2Xp2X', // Use env key
                amount: order.amount,
                currency: order.currency,
                name: 'Ship24x7 Logistics',
                description: `Payment for Shipment ${trackingNumber}`,
                order_id: order.razorpayOrderId,
                handler: (response: any) => {
                  this.verifyPayment(
                    response.razorpay_order_id,
                    response.razorpay_payment_id,
                    response.razorpay_signature
                  ).subscribe({
                    next: (res) => {
                      observer.next(res);
                      observer.complete();
                    },
                    error: (err) => observer.error(err)
                  });
                },
                modal: {
                  ondismiss: () => observer.error(new Error('Payment cancelled by user'))
                },
                prefill: {
                  name: 'Customer',
                  email: 'customer@example.com'
                },
                theme: {
                  color: '#d4af37'
                }
              };

              this.razorpayService.openCheckout(options);
            });
          })
        );
      })
    );
  }

  createPaymentOrder(shipmentId: string, trackingNumber: string, amount: number, currency: string = 'INR'): Observable<any> {
    const payload = {
      ShipmentId: shipmentId,
      TrackingNumber: trackingNumber,
      Amount: amount,
      Currency: currency,
      IdempotencyKey: crypto.randomUUID()
    };
    return this.http.post(`${this.apiBase}/payment/orders`, payload);
  }

  verifyPayment(razorpayOrderId: string, razorpayPaymentId: string, razorpaySignature: string): Observable<any> {
    const payload = {
      RazorpayOrderId: razorpayOrderId,
      RazorpayPaymentId: razorpayPaymentId,
      RazorpaySignature: razorpaySignature
    };
    return this.http.post(`${this.apiBase}/payment/verify`, payload);
  }

  getPaymentsByShipment(shipmentId: string): Observable<any> {
    return this.http.get(`${this.apiBase}/payment/shipments/${shipmentId}`);
  }
}
