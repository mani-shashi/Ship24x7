import { __decorate } from "tslib";
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { StoreService } from '../../core/services/store.service';
let DashboardComponent = class DashboardComponent {
    store = inject(StoreService);
    stats = [
        { label: 'Total Shipments', value: '1,284', icon: 'inventory_2', color: 'text-accent-gold', change: '+12.4%' },
        { label: 'In Transit', value: '42', icon: 'local_shipping', color: 'text-indigo-500', change: '+5.2%' },
        { label: 'Pending Action', value: '3', icon: 'schedule', color: 'text-amber-500', change: '-2.1%' },
        { label: 'Delivered', value: '1,239', icon: 'check_circle', color: 'text-emerald-500', change: '+18.4%' },
    ];
    recentShipments = [
        { id: '1', trackingNumber: 'SHP-100234', status: 'In Transit', origin: 'Mumbai', destination: 'New York', date: 'Today', type: 'Express' },
        { id: '2', trackingNumber: 'SHP-992134', status: 'Delivered', origin: 'Delhi', destination: 'London', date: 'Yesterday', type: 'Standard' },
        { id: '3', trackingNumber: 'SHP-887421', status: 'PickedUp', origin: 'Bangalore', destination: 'Singapore', date: 'May 1', type: 'Express' },
    ];
};
DashboardComponent = __decorate([
    Component({
        selector: 'app-dashboard',
        standalone: true,
        imports: [CommonModule, RouterLink],
        templateUrl: './dashboard.component.html',
    })
], DashboardComponent);
export { DashboardComponent };
//# sourceMappingURL=dashboard.component.js.map