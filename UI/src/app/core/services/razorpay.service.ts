import { Injectable } from '@angular/core';

declare var Razorpay: any;

@Injectable({
  providedIn: 'root'
})
export class RazorpayService {
  private scriptLoaded = false;

  loadRazorpayScript(): Promise<boolean> {
    return new Promise((resolve) => {
      if (this.scriptLoaded) {
        resolve(true);
        return;
      }

      const script = document.createElement('script');
      script.src = 'https://checkout.razorpay.com/v1/checkout.js';
      script.onload = () => {
        this.scriptLoaded = true;
        resolve(true);
      };
      script.onerror = () => resolve(false);
      document.body.appendChild(script);
    });
  }

  openCheckout(options: any): void {
    const rzp = new Razorpay(options);
    rzp.open();
  }
}
