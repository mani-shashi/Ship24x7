import { __decorate } from "tslib";
import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { StoreService } from '../../core/services/store.service';
let TrackingComponent = class TrackingComponent {
    store = inject(StoreService);
    route = inject(ActivatedRoute);
    router = inject(Router);
    trackingNumber = '';
    isSearching = signal(false);
    errorMsg = '';
    result = signal(null);
    ngOnInit() {
        this.route.paramMap.subscribe(params => {
            const id = params.get('trackingNumber');
            if (id) {
                this.trackingNumber = id;
                this.fetchTracking(id);
            }
        });
    }
    handleTrack() {
        if (!this.trackingNumber.trim())
            return;
        const num = this.trackingNumber.trim().toUpperCase();
        this.router.navigate(['/track', num]);
    }
    fetchTracking(num) {
        this.isSearching.set(true);
        this.errorMsg = '';
        this.result.set(null);
        setTimeout(() => {
            if (num === 'NOTFOUND') {
                this.errorMsg = 'No shipment found with tracking number: ' + num;
                this.isSearching.set(false);
                return;
            }
            this.result.set({
                number: num,
                status: 'In Transit',
                events: [
                    { status: 'In Transit', location: 'Indian Ocean', time: new Date(), desc: 'Vessel is currently en route to destination port. GPS fix confirmed.' },
                    { status: 'Departed Port', location: 'Shanghai Port, CN', time: new Date(Date.now() - 86400000), desc: 'Container loaded onto vessel MSC Isabella. Manifest sealed.' },
                    { status: 'Customs Cleared', location: 'Shanghai, CN', time: new Date(Date.now() - 86400000 * 2), desc: 'All documentation verified. Customs duty processed successfully.' },
                    { status: 'Shipment Created', location: 'System', time: new Date(Date.now() - 86400000 * 3), desc: 'Shipment registered in Ship24X7 logistics network.' }
                ]
            });
            this.isSearching.set(false);
        }, 800);
    }
};
TrackingComponent = __decorate([
    Component({
        selector: 'app-tracking',
        standalone: true,
        imports: [CommonModule, FormsModule, RouterLink],
        templateUrl: './tracking.component.html',
    })
], TrackingComponent);
export { TrackingComponent };
//# sourceMappingURL=tracking.component.js.map