import { __decorate } from "tslib";
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StoreService } from '../../core/services/store.service';
let LandingComponent = class LandingComponent {
    store = inject(StoreService);
};
LandingComponent = __decorate([
    Component({
        selector: 'app-landing',
        standalone: true,
        imports: [RouterLink],
        templateUrl: './landing.component.html',
        styleUrl: './landing.component.css',
    })
], LandingComponent);
export { LandingComponent };
//# sourceMappingURL=landing.component.js.map