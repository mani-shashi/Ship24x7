export enum UserRole {
  CUSTOMER = 'Customer',
  HUB_USER = 'Hub_User',
  ADMIN_USER = 'Admin_User',
  SYSTEM_ADMIN = 'System_Admin',
}

export enum ShipmentStatus {
  DRAFT = 'Draft',
  BOOKED = 'Booked',
  PAYMENT_PENDING = 'PaymentPending',
  PAID = 'Paid',
  PICKED_UP = 'PickedUp',
  IN_TRANSIT = 'InTransit',
  OUT_FOR_DELIVERY = 'OutForDelivery',
  DELIVERED = 'Delivered',
  FAILED = 'Failed',
  RETURNED = 'Returned',
  CANCELLED = 'Cancelled',
  DELAYED = 'Delayed',
  PAYMENT_FAILED = 'PaymentFailed',
  CONFIRMED = 'Confirmed', // Keeping for backward compatibility
}

export interface LoginCredentials {
  email: string;
  password?: string;
  mfaCode?: string;
}

export interface User {
  id: string;
  email: string;
  fullName: string;
  phoneNumber: string;
  roles: string[];
  emailVerified: boolean;
  isActive: boolean;
  mfaEnabled: boolean;
  token?: string; // Optional token for internal use
}

export interface Address {
  contactName: string;
  contactPhone: string;
  addressLine1: string;
  city: string;
  state: string;
  postalCode: string;
}

export interface ShipmentItem {
  description: string;
  quantity: number;
  weight: number;
  length: number;
  width: number;
  height: number;
}

export interface Shipment {
  id: string;
  trackingNumber: string;
  status: ShipmentStatus;
  senderAddress: Address;
  receiverAddress: Address;
  totalAmount?: number;
  totalCost?: number; // Backend field
  bookedAt?: string;
  createdAt?: string; // Backend field
  serviceType?: string;
  weight?: number;
  customerId: string;
  currentHubId?: string;
  pickupId?: string;
}

export interface TrackingEvent {
  id: string;
  status: ShipmentStatus;
  description: string;
  location: string;
  recordedAt: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  requiresMfa: boolean;
  user?: User;
}

export interface SavedAddress extends Address {
  id: string;
  label: string;
  addressLine2?: string;
  country: string;
  type: string; // Home, Office, etc.
  isDefault: boolean;
}

export interface UserPreferences {
  [key: string]: any; // Index signature for template access
  emailNotifications: boolean;
  smsNotifications: boolean;
  pushNotifications: boolean;
  marketingEmails: boolean;
  theme: string;
  language: string;
}

export interface UpdateProfileRequest {
  fullName: string;
  phoneNumber: string;
  profilePhotoUrl?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface SaveAddressRequest {
  label: string;
  contactName: string;
  contactPhone: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  type: string;
  isDefault: boolean;
}

export interface UpdatePreferencesRequest extends UserPreferences {}
