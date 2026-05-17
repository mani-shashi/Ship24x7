import { Injectable, signal, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { User, UserRole } from '../types';

export interface AppNotification {
  id: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
}

@Injectable({
  providedIn: 'root'
})
export class StoreService {
  private router = inject(Router);
  private _user = signal<User | null>(null);
  private _notifications = signal<AppNotification[]>([]);
  private _theme = signal<'light' | 'dark'>('light');
  private _addresses = signal<any[]>([]);
  private _carbonSaved = signal<number>(0);

  currentUser = computed(() => this._user());
  isAuthenticated = computed(() => !!this._user());
  userRoles = computed(() => this._user()?.roles || []);
  isAdmin = computed(() => {
    const roles = this.userRoles();
    return roles.includes('Admin_User') || roles.includes('System_Admin');
  });
  notifications = computed(() => this._notifications());
  theme = computed(() => this._theme());
  isDarkMode = computed(() => this._theme() === 'dark');
  savedAddresses = computed(() => this._addresses());
  carbonSaved = computed(() => this._carbonSaved());

  constructor() {
    this.setTheme(this._theme());
    this.loadSession();
  }

  private loadSession() {
    const token = localStorage.getItem('ship24x7_token');
    const userJson = localStorage.getItem('ship24x7_user');
    if (token && userJson) {
      try {
        const user = JSON.parse(userJson);
        this._user.set({ ...user, token });
      } catch (e) {
        localStorage.removeItem('ship24x7_token');
        localStorage.removeItem('ship24x7_user');
      }
    }
  }

  setTheme(theme: 'light' | 'dark') {
    this._theme.set(theme);
    if (theme === 'dark') {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }

  toggleTheme() {
    this.setTheme(this._theme() === 'dark' ? 'light' : 'dark');
  }

  setUser(user: User | null) {
    this._user.set(user);
    if (user?.token) {
      localStorage.setItem('ship24x7_token', user.token);
      localStorage.setItem('ship24x7_user', JSON.stringify(user));
    } else {
      localStorage.removeItem('ship24x7_token');
      localStorage.removeItem('ship24x7_user');
    }
  }

  setAddresses(addresses: any[]) {
    this._addresses.set(addresses);
  }

  updateCarbonMetric(amount: number) {
    this._carbonSaved.update(v => v + amount);
  }

  addNotification(message: string, type: AppNotification['type'] = 'info') {
    const id = Math.random().toString(36).substring(7);
    this._notifications.update(n => [...n, { id, message, type }]);
    setTimeout(() => this.removeNotification(id), 5000);
  }

  removeNotification(id: string) {
    this._notifications.update(n => n.filter(item => item.id !== id));
  }

  logout() {
    this.setUser(null);
    this.router.navigate(['/login']);
  }

  showSuccess(message: string) {
    this.addNotification(message, 'success');
  }

  copyToClipboard(text: string) {
    navigator.clipboard.writeText(text).then(() => {
      this.addNotification('Copied to clipboard', 'success');
    });
  }
}
