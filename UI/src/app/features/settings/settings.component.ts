import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { StoreService } from '../../core/services/store.service';
import { AuthService } from '../../core/services/auth.service';
import { UserRole, UpdateProfileRequest, ChangePasswordRequest, SavedAddress, SaveAddressRequest, UserPreferences } from '../../core/types';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SettingsComponent {
  public store = inject(StoreService);
  private authService = inject(AuthService);

  user = this.store.currentUser;
  activeTab = signal('profile'); // profile, addresses, security, preferences, admin
  
  // Profile Form
  profileForm = {
    fullName: this.store.currentUser()?.fullName || '',
    phoneNumber: this.store.currentUser()?.phoneNumber || '',
    profilePhotoUrl: ''
  };

  // Password Form
  passwordForm = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  // MFA Signals
  mfaEnabled = signal(false);
  mfaSetupStep = signal(0);
  mfaQrCode = signal('');
  mfaCode = '';

  // Email Stats
  emailVerified = signal(false);
  verificationSent = signal(false);

  // Addresses
  addresses = signal<SavedAddress[]>([]);
  isAddressesLoading = signal(false);
  showAddAddress = signal(false);
  addressForm: SaveAddressRequest = {
    label: '',
    contactName: '',
    contactPhone: '',
    addressLine1: '',
    city: '',
    state: '',
    postalCode: '',
    country: 'India',
    type: 'Home',
    isDefault: false
  };

  // Preferences
  preferences = signal<UserPreferences>({
    emailNotifications: true,
    smsNotifications: false,
    pushNotifications: true,
    marketingEmails: false,
    theme: 'light',
    language: 'en'
  });

  // Admin
  managedUsers = signal<any[]>([]);
  isAdmin = signal(false);

  constructor() {
    this.init();
  }

  async init() {
    this.checkStatus();
    this.loadPreferences();
    this.loadAddresses();
  }

  // --- Profile Actions ---
  updateProfile() {
    this.authService.updateProfile(this.profileForm).subscribe({
      next: (updatedUser) => {
        this.store.setUser(updatedUser);
        alert('Profile updated successfully!');
      },
      error: (err) => alert(err.error?.error || 'Failed to update profile')
    });
  }

  changePassword() {
    if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) {
      alert('Passwords do not match');
      return;
    }

    this.authService.changePassword({
      currentPassword: this.passwordForm.currentPassword,
      newPassword: this.passwordForm.newPassword
    }).subscribe({
      next: () => {
        alert('Password changed successfully');
        this.passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' };
      },
      error: (err) => alert(err.error?.error || 'Failed to change password')
    });
  }

  // --- Address Actions ---
  loadAddresses() {
    this.isAddressesLoading.set(true);
    this.authService.getSavedAddresses().subscribe({
      next: (list) => {
        this.addresses.set(list);
        this.isAddressesLoading.set(false);
      },
      error: () => this.isAddressesLoading.set(false)
    });
  }

  deleteAddress(id: string) {
    if (!confirm('Are you sure you want to delete this address?')) return;
    this.authService.deleteAddress(id).subscribe(() => {
      this.addresses.update(list => list.filter(a => a.id !== id));
    });
  }

  saveAddress() {
    this.authService.saveAddress(this.addressForm).subscribe({
      next: (newAddr) => {
        this.addresses.update(list => [...list, newAddr]);
        this.showAddAddress.set(false);
        this.resetAddressForm();
      },
      error: (err) => alert(err.error?.error || 'Failed to save address')
    });
  }

  private resetAddressForm() {
    this.addressForm = {
      label: '', contactName: '', contactPhone: '', addressLine1: '',
      city: '', state: '', postalCode: '', country: 'India',
      type: 'Home', isDefault: false
    };
  }

  // --- Preferences Actions ---
  loadPreferences() {
    this.authService.getUserPreferences().subscribe({
      next: (prefs) => this.preferences.set(prefs),
      error: () => {}
    });
  }

  togglePreference(key: string) {
    const updated = { ...this.preferences() };
    updated[key] = !updated[key];
    this.preferences.set(updated);
    
    this.authService.updateUserPreferences(updated).subscribe();
  }

  // --- Helpers & Existing Logic ---
  checkStatus() {
    const currentUser = this.store.currentUser();
    if (currentUser) {
      this.emailVerified.set(currentUser.emailVerified);
      this.mfaEnabled.set(currentUser.mfaEnabled);
      this.isAdmin.set(currentUser.roles.includes(UserRole.ADMIN_USER) || currentUser.roles.includes(UserRole.SYSTEM_ADMIN));
      if (this.isAdmin()) this.loadUsers();
    }
  }

  setupMfa() {
    this.authService.mfaSetup().subscribe({
      next: (res: any) => {
        this.mfaQrCode.set(res.qrCodeUrl);
        this.mfaSetupStep.set(1);
      },
      error: () => this.mfaSetupStep.set(1)
    });
  }

  verifyMfa() {
    this.authService.mfaVerify(this.mfaCode).subscribe({
      next: () => {
        this.mfaEnabled.set(true);
        this.mfaSetupStep.set(0);
      }
    });
  }

  disableMfa() {
    this.authService.mfaDisable().subscribe(() => this.mfaEnabled.set(false));
  }

  sendVerification() {
    this.authService.verifyEmail('dummy-token').subscribe(() => this.verificationSent.set(true));
  }

  loadUsers() {
    this.authService.getUsers().subscribe(users => this.managedUsers.set(users));
  }
}
