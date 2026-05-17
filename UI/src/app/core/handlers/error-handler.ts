import { ErrorHandler, Injectable, NgZone, inject } from '@angular/core';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private zone = inject(NgZone);

  handleError(error: any): void {
    console.error('Ship24X7 Global Error:', error);

    this.zone.run(() => {
      // In a real app, you might show a toast or dialog here
      const message = error?.message || 'An unexpected error occurred.';
      alert(message);
    });
  }
}
