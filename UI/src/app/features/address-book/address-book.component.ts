import { Component, inject, signal, ChangeDetectionStrategy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { StoreService } from '../../core/services/store.service';
import { AuthService } from '../../core/services/auth.service';
import { SaveAddressRequest, SavedAddress } from '../../core/types';

@Component({
  selector: 'app-address-book',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './address-book.component.html',
  styleUrls: ['./address-book.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddressBookComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  public store = inject(StoreService);

  isAdding = signal(false);
  editingAddressId = signal<string | null>(null);
  
  addressForm = this.fb.group({
    label: ['', [Validators.required, Validators.minLength(2)]],
    contactName: ['', [Validators.required, Validators.minLength(2)]],
    contactPhone: ['', [Validators.required, Validators.pattern('^[0-9]{10,15}$')]],
    addressLine1: ['', [Validators.required, Validators.minLength(5)]],
    city: ['', Validators.required],
    state: ['', Validators.required],
    postalCode: ['', [Validators.required, Validators.pattern('^[0-9]{5,6}$')]],
    country: ['India', Validators.required],
    type: ['Other', Validators.required],
    isDefault: [false]
  });

  ngOnInit() {
    this.loadAddresses();
  }

  loadAddresses() {
    this.authService.getSavedAddresses().subscribe(list => this.store.setAddresses(list));
  }

  getIcon(type: string) {
    switch(type) {
      case 'Home': return 'home';
      case 'Office': return 'business';
      case 'Warehouse': return 'warehouse';
      default: return 'location_on';
    }
  }

  startEdit(addr: SavedAddress) {
    this.editingAddressId.set(addr.id);
    this.addressForm.patchValue({
      label: addr.label,
      contactName: addr.contactName,
      contactPhone: addr.contactPhone,
      addressLine1: addr.addressLine1,
      city: addr.city,
      state: addr.state,
      postalCode: addr.postalCode,
      country: addr.country,
      type: addr.type,
      isDefault: addr.isDefault
    });
    this.isAdding.set(true);
  }

  saveAddress() {
    if (this.addressForm.valid) {
      const payload = this.addressForm.value as SaveAddressRequest;
      
      if (this.editingAddressId()) {
        this.authService.updateAddress(this.editingAddressId()!, payload).subscribe({
          next: () => {
            this.store.addNotification('Address updated successfully', 'success');
            this.loadAddresses();
            this.cancelEdit();
          }
        });
      } else {
        this.authService.saveAddress(payload).subscribe({
          next: () => {
            this.store.addNotification('Address saved successfully', 'success');
            this.loadAddresses();
            this.cancelEdit();
          }
        });
      }
    }
  }

  cancelEdit() {
    this.addressForm.reset({ country: 'India', type: 'Other', isDefault: false });
    this.isAdding.set(false);
    this.editingAddressId.set(null);
  }

  deleteAddress(id: string) {
    if (confirm('Are you sure you want to delete this address?')) {
      this.authService.deleteAddress(id).subscribe({
        next: () => {
          this.store.addNotification('Address deleted', 'info');
          this.loadAddresses();
        }
      });
    }
  }

  copyToClipboard(text: string) {
    navigator.clipboard.writeText(text).then(() => {
      this.store.addNotification('Address copied to clipboard', 'success');
    });
  }
}
