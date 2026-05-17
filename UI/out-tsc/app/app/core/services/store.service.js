import { __decorate } from "tslib";
import { Injectable, signal, computed } from '@angular/core';
let StoreService = class StoreService {
    _user = signal(null);
    _notifications = signal([]);
    _theme = signal('light');
    currentUser = computed(() => this._user());
    isAuthenticated = computed(() => !!this._user());
    userRoles = computed(() => this._user()?.roles || []);
    notifications = computed(() => this._notifications());
    theme = computed(() => this._theme());
    constructor() {
        this.setTheme(this._theme());
    }
    setTheme(theme) {
        this._theme.set(theme);
        if (theme === 'dark') {
            document.documentElement.classList.add('dark');
        }
        else {
            document.documentElement.classList.remove('dark');
        }
    }
    toggleTheme() {
        this.setTheme(this._theme() === 'dark' ? 'light' : 'dark');
    }
    setUser(user) {
        this._user.set(user);
        if (user?.token) {
            localStorage.setItem('ship24x7_token', user.token);
        }
        else {
            localStorage.removeItem('ship24x7_token');
        }
    }
    addNotification(message, type = 'info') {
        const id = Math.random().toString(36).substring(7);
        this._notifications.update(n => [...n, { id, message, type }]);
        setTimeout(() => this.removeNotification(id), 5000);
    }
    removeNotification(id) {
        this._notifications.update(n => n.filter(item => item.id !== id));
    }
    logout() {
        this.setUser(null);
    }
};
StoreService = __decorate([
    Injectable({
        providedIn: 'root'
    })
], StoreService);
export { StoreService };
//# sourceMappingURL=store.service.js.map