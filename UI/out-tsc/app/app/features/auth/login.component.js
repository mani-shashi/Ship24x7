import { __decorate } from "tslib";
import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { StoreService } from '../../core/services/store.service';
import { UserRole } from '../../core/types';
let LoginComponent = class LoginComponent {
    store = inject(StoreService);
    router = inject(Router);
    isSignUp = signal(false);
    isForgot = signal(false);
    isLoading = signal(false);
    email = '';
    password = '';
    fullName = '';
    toggleMode() {
        this.isSignUp.update(v => !v);
        this.isForgot.set(false);
    }
    handleAuth(event) {
        event.preventDefault();
        this.isLoading.set(true);
        if (this.isForgot()) {
            setTimeout(() => {
                this.store.addNotification('Password reset link sent to ' + this.email, 'success');
                this.isLoading.set(false);
                this.isForgot.set(false);
            }, 1000);
            return;
        }
        // Mock login
        setTimeout(() => {
            this.store.setUser({
                id: '1',
                email: this.email,
                fullName: this.fullName || 'Demo User',
                roles: [UserRole.CUSTOMER],
                token: 'mock-jwt-token-' + Date.now(),
            });
            this.isLoading.set(false);
            this.router.navigate(['/dashboard']);
        }, 1000);
    }
};
LoginComponent = __decorate([
    Component({
        selector: 'app-login',
        standalone: true,
        imports: [CommonModule, FormsModule, RouterLink],
        templateUrl: './login.component.html',
    })
], LoginComponent);
export { LoginComponent };
//# sourceMappingURL=login.component.js.map