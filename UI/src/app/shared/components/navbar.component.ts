import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { StoreService } from '../../core/services/store.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <nav class="nav-capsule flex items-center justify-between">
      <!-- Brand -->
      <div class="nav-logo group flex items-center gap-2" routerLink="/">
        <div class="nav-logo-mark flex items-center justify-center">
          <span class="material-symbols-outlined text-accent-gold">local_shipping</span>
        </div>
        <span class="nav-logo-text">Ship<span class="text-accent-gold">24<span class="text-xs lowercase opacity-70 mx-0.5">x</span>7</span><sup class="text-[8px] ml-0.5 text-accent-gold/60">TM</sup></span>
      </div>

      <!-- Navigation Links -->
      <div class="hidden md:flex items-center gap-10">
        <a routerLink="/" fragment="features" class="nav-link" routerLinkActive="active">Services</a>
        <a routerLink="/track" class="nav-link" routerLinkActive="active">Track</a>
        <a routerLink="/rates" class="nav-link" routerLinkActive="active">Rates</a>
        <a routerLink="/contact" class="nav-link" routerLinkActive="active">Support</a>
      </div>

      <!-- Action Center -->
      <div class="flex items-center gap-6">
        <!-- Status Node -->
        <div 
          class="hidden lg:flex items-center gap-3 px-4 py-2 border rounded-full transition-all duration-500"
          [ngClass]="isBackendActive() ? 'border-emerald-100 dark:border-emerald-500/20 bg-emerald-50/50 dark:bg-emerald-500/5' : 'border-rose-100 dark:border-rose-500/20 bg-rose-50/50 dark:bg-rose-500/5'"
        >
          <span 
            class="w-1.5 h-1.5 rounded-full transition-all duration-500"
            [ngClass]="isBackendActive() ? 'bg-emerald-500 animate-ping' : 'bg-rose-500 animate-pulse'"
          ></span>
          <span 
            class="text-[8px] font-black uppercase tracking-widest transition-colors duration-500"
            [ngClass]="isBackendActive() ? 'text-emerald-600 dark:text-emerald-500/60' : 'text-rose-600 dark:text-rose-500/60'"
          >
            {{ isBackendActive() ? 'Network Live' : 'Network Offline' }}
          </span>
        </div>
        
        <button 
          (click)="store.toggleTheme()"
          class="w-10 h-10 rounded-xl border border-emerald-100 dark:border-white/5 hover:bg-emerald-50 dark:hover:bg-white/5 transition-all text-slate-500 flex items-center justify-center"
        >
          @if (store.theme() === 'dark') {
            <span class="material-symbols-outlined text-accent-gold">light_mode</span>
          } @else {
            <span class="material-symbols-outlined text-slate-600">dark_mode</span>
          }
        </button>

        <div class="flex items-center gap-4">
          <a routerLink="/login" class="text-[10px] font-black uppercase tracking-widest text-slate-900 dark:text-white hover:text-accent-gold transition-colors">Login</a>
          <a routerLink="/register" class="btn-primary px-8 h-12 text-[10px] shadow-neon">Sign Up</a>
        </div>
      </div>
    </nav>
  `
})
export class NavbarComponent implements OnInit {
  store = inject(StoreService);
  private authService = inject(AuthService);

  isBackendActive = signal<boolean>(true);

  ngOnInit() {
    this.checkConnection();
    // Poll every 5 minutes to reduce API overhead
    setInterval(() => this.checkConnection(), 300000);
  }

  private checkConnection() {
    this.authService.checkHealth().subscribe({
      next: () => this.isBackendActive.set(true),
      error: () => this.isBackendActive.set(false)
    });
  }
}
