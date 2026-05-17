import { __decorate } from "tslib";
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { StoreService } from '../../core/services/store.service';
let ShipmentsComponent = class ShipmentsComponent {
    store = inject(StoreService);
    shipments = [
        { id: '1', trackingNumber: 'SHP-100234', status: 'In Transit', origin: 'Mumbai, IN', destination: 'New York, US', date: '2026-05-01', amount: '$450.00' },
        { id: '2', trackingNumber: 'SHP-992134', status: 'Delivered', origin: 'Delhi, IN', destination: 'London, UK', date: '2026-04-28', amount: '$320.00' },
        { id: '3', trackingNumber: 'SHP-887421', status: 'PickedUp', origin: 'Bangalore, IN', destination: 'Singapore, SG', date: '2026-04-25', amount: '$180.00' },
        { id: '4', trackingNumber: 'SHP-776312', status: 'Delivered', origin: 'Chennai, IN', destination: 'Dubai, AE', date: '2026-04-20', amount: '$560.00' },
        { id: '5', trackingNumber: 'SHP-665203', status: 'Cancelled', origin: 'Kolkata, IN', destination: 'Tokyo, JP', date: '2026-04-18', amount: '$720.00' },
    ];
};
ShipmentsComponent = __decorate([
    Component({
        selector: 'app-shipments',
        standalone: true,
        imports: [CommonModule, RouterLink],
        templateUrl: './shipments.component.html',
    })
], ShipmentsComponent);
export { ShipmentsComponent };
//# sourceMappingURL=shipments.component.js.map