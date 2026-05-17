import { Routes } from '@angular/router';
import { authGuard, noAuthGuard, roleGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/landing/landing.component').then(m => m.LandingComponent),
  },
  {
    path: 'login',
    canActivate: [noAuthGuard],
    loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'register',
    canActivate: [noAuthGuard],
    loadComponent: () => import('./features/auth/register.component').then(m => m.RegisterComponent),
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
  },
  {
    path: 'notifications',
    canActivate: [authGuard],
    loadComponent: () => import('./features/notifications/notification-center.component').then(m => m.NotificationCenterComponent),
  },
  {
    path: 'track',
    loadComponent: () => import('./features/tracking/tracking.component').then(m => m.TrackingComponent),
  },
  {
    path: 'track/:trackingNumber',
    loadComponent: () => import('./features/tracking/tracking.component').then(m => m.TrackingComponent),
  },
  {
    path: 'shipments',
    canActivate: [authGuard],
    loadComponent: () => import('./features/shipments/shipments.component').then(m => m.ShipmentsComponent),
  },
  { 
    path: 'admin', 
    canActivate: [authGuard, roleGuard(['Admin_User', 'System_Admin'])],
    loadComponent: () => import('./features/admin/dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent) 
  },
  { 
    path: 'admin/shipments', 
    canActivate: [authGuard, roleGuard(['Admin_User', 'System_Admin'])],
    loadComponent: () => import('./features/admin/shipments/admin-shipment-list.component').then(m => m.AdminShipmentListComponent) 
  },
  { 
    path: 'admin/users', 
    canActivate: [authGuard, roleGuard(['Admin_User', 'System_Admin'])],
    loadComponent: () => import('./features/admin/users/user-management.component').then(m => m.UserManagementComponent) 
  },
  { 
    path: 'admin/hubs', 
    canActivate: [authGuard, roleGuard(['Admin_User', 'System_Admin'])],
    loadComponent: () => import('./features/admin/hubs/hub-management.component').then(m => m.HubManagementComponent) 
  },
  { 
    path: 'admin/rates', 
    canActivate: [authGuard, roleGuard(['Admin_User', 'System_Admin'])],
    loadComponent: () => import('./features/admin/rates/rate-management.component').then(m => m.RateManagementComponent) 
  },
  { 
    path: 'admin/templates', 
    canActivate: [authGuard, roleGuard(['Admin_User', 'System_Admin'])],
    loadComponent: () => import('./features/admin/templates/template-management.component').then(m => m.TemplateManagementComponent) 
  },
  { 
    path: 'checkout', 
    canActivate: [authGuard],
    loadComponent: () => import('./features/checkout/checkout.component').then(m => m.CheckoutComponent) 
  },
  {
    path: 'new-shipment',
    canActivate: [authGuard],
    loadComponent: () => import('./features/shipment-wizard/shipment-wizard.component').then(m => m.ShipmentWizardComponent),
  },
  {
    path: 'address-book',
    canActivate: [authGuard],
    loadComponent: () => import('./features/address-book/address-book.component').then(m => m.AddressBookComponent),
  },
  {
    path: 'settings',
    canActivate: [authGuard],
    loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent),
  },
  {
    path: 'rates',
    loadComponent: () => import('./features/legal/shipping-rates/shipping-rates.component').then(m => m.ShippingRatesComponent),
  },
  {
    path: 'prohibited',
    loadComponent: () => import('./features/legal/prohibited-items/prohibited-items.component').then(m => m.ProhibitedItemsComponent),
  },
  {
    path: 'privacy',
    loadComponent: () => import('./features/legal/privacy-policy/privacy-policy.component').then(m => m.PrivacyPolicyComponent),
  },
  {
    path: 'safety',
    loadComponent: () => import('./features/legal/safety-protocols/safety-protocols.component').then(m => m.SafetyProtocolsComponent),
  },
  {
    path: 'network',
    loadComponent: () => import('./features/company/network/global-network.component').then(m => m.GlobalNetworkComponent),
  },
  {
    path: 'impact',
    loadComponent: () => import('./features/company/impact/sustainability.component').then(m => m.SustainabilityComponent),
  },
  {
    path: 'tech',
    loadComponent: () => import('./features/company/tech/logistics-tech.component').then(m => m.LogisticsTechComponent),
  },
  {
    path: 'contact',
    loadComponent: () => import('./features/contact/contact.component').then(m => m.ContactComponent),
  },
  {
    path: '404',
    loadComponent: () => import('./features/error/not-found.component').then(m => m.NotFoundComponent),
  },
  { path: '**', redirectTo: '404' }
];
