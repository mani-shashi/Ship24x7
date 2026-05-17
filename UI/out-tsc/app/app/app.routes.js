export const routes = [
    {
        path: '',
        loadComponent: () => import('./features/landing/landing.component').then(m => m.LandingComponent),
    },
    {
        path: 'login',
        loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent),
    },
    {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
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
        loadComponent: () => import('./features/shipments/shipments.component').then(m => m.ShipmentsComponent),
    },
    {
        path: 'new-shipment',
        loadComponent: () => import('./features/shipment-wizard/shipment-wizard.component').then(m => m.ShipmentWizardComponent),
    },
    { path: '**', redirectTo: '' }
];
//# sourceMappingURL=app.routes.js.map