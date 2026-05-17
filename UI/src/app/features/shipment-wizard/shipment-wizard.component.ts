import { Component, inject, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
// Triggering recompile after StoreService fix
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { StoreService } from '../../core/services/store.service';
import { ShipmentService } from '../../core/services/shipment.service';
import { RateService } from '../../core/services/rate.service';
import { AuthService } from '../../core/services/auth.service';
import { PaymentService } from '../../core/services/payment.service';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs/operators';
import { SaveAddressRequest } from '../../core/types';

@Component({
  selector: 'app-shipment-wizard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './shipment-wizard.component.html',
  styleUrls: ['./shipment-wizard.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ShipmentWizardComponent {
  private fb = inject(FormBuilder);
  public store = inject(StoreService);
  private shipmentService = inject(ShipmentService);
  private authService = inject(AuthService);
  private rateService = inject(RateService);
  private router = inject(Router);
  private payment = inject(PaymentService);

  private stableIdempotencyKey = crypto.randomUUID();

  isFieldInvalid(form: any, field: string): boolean {
    const control = form.get(field);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  countries = ["IN", "US", "RU", "CN"];
  

  showSavedOrigin = signal(false);
  showSavedDest = signal(false);

  currentStep = signal(1);
  totalSteps = 7;
  isPaying = signal(false);
  paymentSuccess = signal(false);
  errorMessage = signal<string | null>(null);
  labelData = signal<any>(null);

  availableTemplates = signal<any[]>([]);
  showLabel = signal(false);

  originForm = this.fb.group({
    senderName: ['', [Validators.required, Validators.minLength(3)]],
    senderEmail: ['', [Validators.required, Validators.email]],
    senderPhone: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
    senderAddress: ['', [Validators.required, Validators.minLength(10)]],
    senderCity: ['Mumbai', Validators.required],
    senderState: ['Maharashtra', Validators.required],
    senderPostalCode: ['400001', [Validators.required, Validators.pattern('^[0-9]{6}$')]],
    saveToAddressBook: [true]
  });

  destForm = this.fb.group({
    receiverName: ['', [Validators.required, Validators.minLength(3)]],
    receiverEmail: ['', [Validators.required, Validators.email]],
    receiverPhone: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
    receiverAddress: ['', [Validators.required, Validators.minLength(10)]],
    receiverCity: ['Delhi', Validators.required],
    receiverState: ['Delhi', Validators.required],
    receiverPostalCode: ['110001', [Validators.required, Validators.pattern('^[0-9]{6}$')]],
    saveToAddressBook: [true]
  });

  packageForm = this.fb.group({
    weight: [null as number | null, [Validators.required, Validators.min(0.1), Validators.max(1000)]],
    length: [null as number | null, [Validators.required, Validators.min(1), Validators.max(500)]],
    width: [null as number | null, [Validators.required, Validators.min(1), Validators.max(500)]],
    height: [null as number | null, [Validators.required, Validators.min(1), Validators.max(500)]],
    type: ['Standard', Validators.required]
  });

  serviceForm = this.fb.group({
    serviceId: ['', Validators.required]
  });

  addonsForm = this.fb.group({
    insurance: [false],
    carbonOffset: [true],
    fragile: [false],
    signature: [false]
  });

  billingForm = this.fb.group({
    paymentMethod: ['card', Validators.required],
    promoCode: ['']
  });

  taxRate = 0.18;
  
  toggleTheme() {
    this.store.setTheme(this.store.isDarkMode() ? 'light' : 'dark');
  }

  syncAddressBook() {
    const user = this.store.currentUser();
    if (!user) return;

    // Handle Origin (Sender)
    if (this.currentStep() === 1) {
      const origin = this.originForm.value as any;
      if (this.originForm.get('saveToAddressBook')?.value && origin.senderName && origin.senderAddress) {
        const payload: SaveAddressRequest = {
          label: origin.senderName,
          contactName: origin.senderName,
          contactPhone: origin.senderPhone,
          addressLine1: origin.senderAddress,
          city: origin.senderCity,
          state: origin.senderState,
          postalCode: origin.senderPostalCode,
          country: 'India',
          type: 'Other',
          isDefault: false
        };
        this.authService.saveAddress(payload).subscribe({
          next: () => {
            this.store.addNotification('Sender address saved to book.', 'success');
            this.authService.getSavedAddresses().subscribe(list => this.store.setAddresses(list));
          },
          error: (err) => {
            console.error('Failed to save sender address:', err);
            // Don't show error to user if it's just a duplicate or minor issue
          }
        });
      }
    }

    // Handle Destination (Receiver)
    if (this.currentStep() === 2) {
      const dest = this.destForm.value as any;
      if (this.destForm.get('saveToAddressBook')?.value && dest.receiverName && dest.receiverAddress) {
        const payload: SaveAddressRequest = {
          label: dest.receiverName,
          contactName: dest.receiverName,
          contactPhone: dest.receiverPhone,
          addressLine1: dest.receiverAddress,
          city: dest.receiverCity,
          state: dest.receiverState,
          postalCode: dest.receiverPostalCode,
          country: 'India',
          type: 'Other',
          isDefault: false
        };
        this.authService.saveAddress(payload).subscribe({
          next: () => {
            this.store.addNotification('Receiver address saved to book.', 'success');
            this.authService.getSavedAddresses().subscribe(list => this.store.setAddresses(list));
          },
          error: (err) => {
            console.error('Failed to save receiver address:', err);
          }
        });
      }
    }
  }
  
  // Reactive Signals for Form Values
  packageValue = toSignal(this.packageForm.valueChanges.pipe(map(() => this.packageForm.value)), { initialValue: this.packageForm.value });
  addonsValue = toSignal(this.addonsForm.valueChanges.pipe(map(() => this.addonsForm.value)), { initialValue: this.addonsForm.value });

  selectedServiceId = signal<string>('');
  availableServices = signal<any[]>([]);

  getStepTitle(): string {
    switch(this.currentStep()) {
      case 1: return 'Sender Details';
      case 2: return 'Receiver Details';
      case 3: return 'Package Specs';
      case 4: return 'Service Selection';
      case 5: return 'Logistics Add-ons';
      case 6: return 'Billing & Review';
      case 7: return 'Shipment Confirmed';
      default: return 'Shipment Wizard';
    }
  }

  loadTemplates() {
    this.shipmentService.getTemplates().subscribe({
      next: (templates: any[]) => this.availableTemplates.set(templates),
      error: () => this.availableTemplates.set([])
    });
    this.loadRates();
  }

  loadRates() {
    this.rateService.getRates().subscribe({
      next: (rates: any[]) => {
        if (!rates || rates.length === 0) {
          this.setFallbackRates();
          return;
        }

        const pkgWeight = this.packageForm.get('weight')?.value || 0;
        const isInternational = this.originForm.get('senderCountry')?.value !== this.destForm.get('receiverCountry')?.value;

        // Filter services based on shipment context
        let filteredRates = rates.filter(r => {
          const type = (r.serviceType || r.ServiceType || "").toLowerCase();
          
          // Hide international if shipping domestically (both are India by default)
          if (!isInternational && type === 'international') return false;
          
          // Hide freight if weight is less than 50kg (standard threshold)
          if (pkgWeight < 50 && type === 'freight') return false;
          
          return true;
        });

        this.availableServices.set(filteredRates.map(r => ({
          id: r.id || r.Id,
          name: r.serviceName || r.ServiceName || r.serviceType || r.ServiceType,
          price: r.minimumCharge || r.MinimumCharge || 100,
          perKg: r.baseRatePerKg || r.BaseRatePerKg || 50,
          days: r.estimatedDeliveryDays || r.EstimatedDeliveryDays || 3,
          carbonSavedImg: Math.random() * 5
        })));
      },
      error: (err) => {
        console.error('Failed to load rates, using fallbacks', err);
        this.setFallbackRates();
      }
    });
  }

  private setFallbackRates() {
    this.availableServices.set([
      { id: '11111111-1111-1111-1111-111111111111', name: 'Standard Delivery', price: 150, perKg: 40, days: 5, carbonSavedImg: 1.2 },
      { id: '22222222-2222-2222-2222-222222222222', name: 'Express Priority', price: 350, perKg: 80, days: 2, carbonSavedImg: 2.5 },
      { id: '33333333-3333-3333-3333-333333333333', name: 'Overnight Air', price: 750, perKg: 150, days: 1, carbonSavedImg: 0.8 }
    ]);
  }



  aiInsights = signal({
    optimized: true,
    savedTime: '4h',
    routeEfficiency: '98%',
    recommendation: 'Sustainable Express Routing suggested for this path.'
  });

  estimatedRate = computed(() => {
    const serviceId = this.selectedServiceId();
    const service = this.availableServices().find(s => s.id === serviceId);
    if (!service) return null;

    try {
      const basePrice = service.price || 0;
      const weight = this.packageValue()?.weight || 0;
      const perKgRate = service.perKg || 0;
      const weightPrice = weight * perKgRate;
      
      const insurancePrice = this.addonsValue()?.insurance ? 50 : 0;
      const fragilePrice = this.addonsValue()?.fragile ? 100 : 0;
      const signaturePrice = this.addonsValue()?.signature ? 30 : 0;
      
      const subtotal = basePrice + weightPrice + insurancePrice + fragilePrice + signaturePrice;
      const tax = subtotal * this.taxRate;

      return {
        subtotal,
        tax,
        total: subtotal + tax,
        days: service.days,
        carbon: service.carbonSavedImg
      };
    } catch (e) {
      console.error('Rate calculation error:', e);
      return null;
    }
  });

  constructor() {
    this.loadTemplates();
    this.restoreSession();
    this.authService.getSavedAddresses().subscribe(list => this.store.setAddresses(list));
    
    // Auto-save on any change
    this.originForm.valueChanges.subscribe(() => this.saveSession());
    this.destForm.valueChanges.subscribe(() => this.saveSession());
    this.packageForm.valueChanges.subscribe(() => this.saveSession());
    this.serviceForm.valueChanges.subscribe(() => this.saveSession());
    this.addonsForm.valueChanges.subscribe(() => this.saveSession());
    this.billingForm.valueChanges.subscribe(() => this.saveSession());
  }

  saveSession() {
    const data = {
      step: this.currentStep(),
      origin: this.originForm.value,
      dest: this.destForm.value,
      package: this.packageForm.value,
      service: this.serviceForm.value,
      addons: this.addonsForm.value,
      billing: this.billingForm.value,
      idempotencyKey: this.stableIdempotencyKey
    };
    sessionStorage.setItem('shipment_wizard_data', JSON.stringify(data));
  }

  restoreSession() {
    const saved = sessionStorage.getItem('shipment_wizard_data');
    if (saved) {
      try {
        const data = JSON.parse(saved);
        this.currentStep.set(data.step || 1);
        if (data.origin) this.originForm.patchValue(data.origin);
        if (data.dest) this.destForm.patchValue(data.dest);
        if (data.package) this.packageForm.patchValue(data.package);
        if (data.service) {
          this.serviceForm.patchValue(data.service);
          this.selectedServiceId.set(data.service.serviceId || '');
        }
        if (data.addons) this.addonsForm.patchValue(data.addons);
        if (data.billing) this.billingForm.patchValue(data.billing);
        if (data.idempotencyKey) this.stableIdempotencyKey = data.idempotencyKey;
      } catch (e) {
        console.error('Failed to restore session', e);
      }
    }
  }

  selectService(id: string) {
    this.serviceForm.patchValue({ serviceId: id });
    this.selectedServiceId.set(id);
    this.saveSession();
  }

  resetWizard() {
    if (confirm('Are you sure you want to clear all data and start over?')) {
      this.originForm.reset({
        senderCity: 'Mumbai',
        senderState: 'Maharashtra',
        senderPostalCode: '400001'
      });
      this.destForm.reset({
        receiverCity: 'Delhi',
        receiverState: 'Delhi',
        receiverPostalCode: '110001',
        saveToAddressBook: true
      });
      this.packageForm.reset({ type: 'Standard' });
      this.serviceForm.reset();
      this.addonsForm.reset({ carbonOffset: true });
      this.billingForm.reset({ paymentMethod: 'card' });
      
      this.currentStep.set(1);
      this.selectedServiceId.set('');
      sessionStorage.removeItem('shipment_wizard_data');
      this.store.addNotification('Wizard has been reset.', 'info');
    }
  }

  injectTemplate(template: any) {
    this.packageForm.patchValue({
      weight: template.weight,
      length: template.length,
      width: template.width,
      height: template.height,
      type: template.type || 'Standard'
    });
  }

  nextStep() {
    if (this.currentStep() === 1 || this.currentStep() === 2) {
      this.syncAddressBook();
    }

    if (this.currentStep() === 6) {
      this.confirmAndPay();
      return;
    }

    if (this.currentStep() === 3) {
      this.loadRates(); // Refresh rates based on weight/dest before showing step 4
    }

    if (this.currentStep() < this.totalSteps) {
      this.currentStep.update(s => s + 1);
      this.saveSession();
    }
  }

  public confirmAndPay() {
    if (this.isProcessing()) return;
    this.isProcessing.set(true);
    
    // 1. Create Shipment (Draft)
    const payload = this.preparePayload();
    if (!payload) {
      this.isProcessing.set(false);
      this.store.addNotification('Missing required shipment information.', 'error');
      return;
    }
    
    this.shipmentService.createShipment(payload).subscribe({
      next: (shipment: any) => {
        console.log('Shipment created successfully:', shipment);
        const shipmentId = shipment.id || shipment.Id;
        const trackingNumber = shipment.trackingNumber || shipment.TrackingNumber;
        
        // 2. Confirm Shipment (Move from Draft to Booked)
        this.shipmentService.confirmShipment(shipmentId, payload.customerId).subscribe({
          next: () => {
            // 3. Initiate Payment
            const amount = this.estimatedRate()?.total || 0;
            this.payment.processPayment(shipmentId, trackingNumber, amount).subscribe({
              next: (res: any) => {
                console.log('Payment process complete:', res);
                this.isProcessing.set(false);
                this.paymentSuccess.set(true);
                this.errorMessage.set(null);
                this.labelData.set({
                  trackingId: trackingNumber,
                  origin: shipment.senderCity || 'Mumbai',
                  dest: shipment.receiverCity || 'Delhi',
                  date: new Date().toLocaleDateString(),
                  senderName: (this.originForm.value as any).senderName,
                  receiverName: (this.destForm.value as any).receiverName
                });
                this.currentStep.set(7);
                this.saveSession();
                this.store.addNotification('Shipment booked successfully!', 'success');
              },
              error: (err: any) => {
                console.error('Payment Error:', err);
                this.isProcessing.set(false);
                this.paymentSuccess.set(false);
                this.errorMessage.set(`Shipment confirmed but payment failed: ${err.error?.error || 'Unknown error'}`);
                this.currentStep.set(7);
              }
            });
          },
          error: (err) => {
            console.error('Confirmation Error:', err);
            this.isProcessing.set(false);
            this.store.addNotification('Failed to confirm shipment: ' + (err.error?.error || 'Unknown error'), 'error');
          }
        });
      },
      error: (err) => {
        console.error('Shipment Creation Error:', err);
        this.isProcessing.set(false);
        this.store.addNotification('Failed to create shipment: ' + (err.error?.error || 'Unknown error'), 'error');
      }
    });
  }

  private preparePayload() {
    const user = this.store.currentUser();
    if (!user) return null;

    const origin = this.originForm.value;
    const dest = this.destForm.value;
    const pkg = this.packageValue();
    
    return {
      customerId: user.id,
      idempotencyKey: this.stableIdempotencyKey,
      senderContactName: origin.senderName,
      senderContactPhone: origin.senderPhone, 
      senderContactEmail: origin.senderEmail,
      senderAddressLine1: origin.senderAddress,
      senderCity: origin.senderCity,
      senderState: origin.senderState,
      senderPostalCode: origin.senderPostalCode,
      senderCountry: 'India',
      
      receiverContactName: dest.receiverName,
      receiverContactPhone: dest.receiverPhone,
      receiverContactEmail: dest.receiverEmail,
      receiverAddressLine1: dest.receiverAddress,
      receiverCity: dest.receiverCity,
      receiverState: dest.receiverState,
      receiverPostalCode: dest.receiverPostalCode,
      receiverCountry: 'India',
      
      serviceRateId: this.selectedServiceId(),
      isFragile: this.addonsValue()?.fragile,
      declaredValue: 1000,
      items: [
        {
          description: pkg?.type || 'Standard Package',
          quantity: 1,
          weight: pkg?.weight || 0.5,
          length: pkg?.length || 10,
          width: pkg?.width || 10,
          height: pkg?.height || 10,
          packageType: 'Box'
        }
      ]
    };
  }

  isProcessing = signal(false);

  prevStep() {
    if (this.currentStep() > 1) {
      this.currentStep.update(s => s - 1);
      this.saveSession();
    }
  }

  canProceed(): boolean {
    switch(this.currentStep()) {
      case 1: return this.originForm.valid;
      case 2: return this.destForm.valid;
      case 3: return this.packageForm.valid;
      case 4: return this.serviceForm.valid;
      case 5: return this.addonsForm.valid;
      case 6: return this.billingForm.valid;
      default: return true;
    }
  }

  submitShipment() {
    sessionStorage.removeItem('shipment_wizard_data');
    this.router.navigate(['/dashboard']);
  }

  useAddress(type: 'origin' | 'destination', addr: any) {
    if (type === 'origin') {
      this.originForm.patchValue({
        senderName: addr.contactName || addr.name,
        senderPhone: addr.contactPhone || '',
        senderAddress: addr.addressLine1 || addr.address,
        senderCity: addr.city || 'Mumbai',
        senderState: addr.state || 'Maharashtra',
        senderPostalCode: addr.postalCode || '400001'
      });
    } else {
      this.destForm.patchValue({
        receiverName: addr.contactName || addr.name,
        receiverPhone: addr.contactPhone || '',
        receiverAddress: addr.addressLine1 || addr.address,
        receiverCity: addr.city || 'Delhi',
        receiverState: addr.state || 'Delhi',
        receiverPostalCode: addr.postalCode || '110001'
      });
    }
    this.store.addNotification('Address applied successfully!', 'success');
  }

  printLabel() {
    const printContent = document.querySelector('.print-label-content');
    const printWindow = window.open('', '_blank', 'width=800,height=900');

    if (printWindow && printContent) {
      printWindow.document.write(`
        <html>
          <head>
            <title>Shipping Label - ${this.labelData()?.trackingId}</title>
            <link href="https://fonts.googleapis.com/css2?family=Outfit:wght@500;600;700&family=JetBrains+Mono:wght@400;500&display=swap" rel="stylesheet">
            <script src="https://cdn.tailwindcss.com"></script>
            <script>
              tailwind.config = {
                theme: {
                  extend: {
                    colors: {
                      'accent-gold': '#d4af37',
                      'primary-950': '#011612',
                    },
                    fontFamily: {
                      display: ['Outfit', 'sans-serif'],
                      mono: ['JetBrains Mono', 'monospace'],
                    }
                  }
                }
              }
            </script>
            <style>
              body { 
                background: #f8fafc; 
                display: flex; 
                justify-content: center; 
                align-items: center; 
                min-height: 100vh;
                margin: 0;
              }
              .label-outer {
                width: 100mm; /* Standard 4-inch width */
                margin: 0 auto;
                background: white;
                padding: 10mm;
              }
              @media print {
                body { background: white; margin: 0; }
                .label-outer { 
                  width: 100mm !important; 
                  padding: 0 !important;
                  margin: 0 !important;
                }
                @page { 
                  size: 100mm 150mm; /* Standard 4x6 inch label size */
                  margin: 0; 
                }
              }
            </style>
          </head>
          <body>
            <div class="label-outer">
              ${printContent.outerHTML}
            </div>
            <script>
              window.onload = () => {
                setTimeout(() => {
                  window.print();
                  window.close();
                }, 1000);
              };
            </script>
          </body>
        </html>
      `);
      printWindow.document.close();
    }
  }

  downloadPdf() {
    // Mocking PDF download
    alert('Preparing your high-resolution shipping label PDF...');
    setTimeout(() => {
      const link = document.createElement('a');
      link.href = 'data:application/pdf;base64,JVBERi0xLjcK...'; // Mock data
      link.download = `Label-${this.labelData()?.trackingId}.pdf`;
      // In a real app, we would use a library like jsPDF to generate this
      alert('Label download started.');
    }, 1000);
  }
}
