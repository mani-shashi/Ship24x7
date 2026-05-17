import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { StoreService } from '../../../core/services/store.service';

import { NavbarComponent } from '../../../shared/components/navbar.component';

@Component({
  selector: 'app-safety-protocols',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  template: `
    <div class="min-h-screen bg-slate-50 dark:bg-primary-950 transition-colors duration-500 font-sans relative overflow-hidden">
      <!-- Animated Background Elements -->
      <div class="glass-blob -top-24 -left-24 w-96 h-96 bg-emerald-500/10"></div>
      <div class="glass-blob -bottom-24 -right-24 w-96 h-96 bg-accent-gold/10 animation-delay-2000"></div>
      <div class="scanline opacity-5 dark:opacity-10"></div>
      <div class="absolute inset-0 bg-[radial-gradient(circle_at_2px_2px,rgba(0,0,0,0.02)_1px,transparent_0)] bg-[size:40px_40px] pointer-events-none"></div>

      <app-navbar />

      <main class="max-w-4xl mx-auto pt-40 pb-20 px-6">
        <header class="mb-16">
          <p class="data-label text-emerald-500 mb-4">Operational Excellence</p>
          <h1 class="text-5xl font-display font-black text-slate-900 dark:text-white uppercase tracking-tighter">Safety <span class="text-emerald-500">Protocols</span></h1>
          <p class="text-slate-500 mt-4">Defining the standards for secure cargo handling and global transit safety.</p>
        </header>

        <div class="grid grid-cols-1 md:grid-cols-3 gap-8 mb-16">
          <div class="glass-card tech-border p-8 border-emerald-500/20">
            <span class="material-symbols-outlined text-emerald-500 text-3xl mb-6">verified_user</span>
            <h4 class="text-sm font-black uppercase tracking-widest text-slate-900 dark:text-white mb-4">Cargo Integrity</h4>
            <p class="text-[11px] text-slate-500 dark:text-primary-700 leading-relaxed">Multiple check-points and biometric verification for high-value assets during handover.</p>
          </div>
          <div class="glass-card tech-border p-8 border-emerald-500/20">
            <span class="material-symbols-outlined text-emerald-500 text-3xl mb-6">shield</span>
            <h4 class="text-sm font-black uppercase tracking-widest text-slate-900 dark:text-white mb-4">Safe Transit</h4>
            <p class="text-[11px] text-slate-500 dark:text-primary-700 leading-relaxed">Real-time G-force and tilt monitoring for fragile shipments via IoT telemetry.</p>
          </div>
          <div class="glass-card tech-border p-8 border-emerald-500/20">
            <span class="material-symbols-outlined text-emerald-500 text-3xl mb-6">eco</span>
            <h4 class="text-sm font-black uppercase tracking-widest text-slate-900 dark:text-white mb-4">Eco-Standards</h4>
            <p class="text-[11px] text-slate-500 dark:text-primary-700 leading-relaxed">Sustainability protocols focusing on carbon-neutral fleet operations and minimal packaging waste.</p>
          </div>
        </div>

        <section class="glass-card tech-border p-10 bg-emerald-500/5">
           <h3 class="text-xl font-bold text-slate-900 dark:text-white mb-6 uppercase tracking-tight">Emergency Response</h3>
           <p class="text-sm text-slate-600 dark:text-primary-700 leading-relaxed mb-6">
             In the event of transit disruption or safety breaches, our Global Control Center (GCC) initiates a Level-1 response protocol within 120 seconds. This includes local law enforcement coordination and asset recovery measures.
           </p>
           <div class="flex items-center gap-4">
              <div class="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></div>
              <span class="text-[10px] font-bold uppercase tracking-widest text-emerald-600">24/7 GCC Monitoring Active</span>
           </div>
        </section>

        <footer class="mt-20 pt-10 border-t border-slate-100 dark:border-white/5">
          <a routerLink="/" class="inline-flex items-center gap-2 text-accent-gold font-bold uppercase tracking-widest text-[10px] hover:translate-x-2 transition-transform">
            <span class="material-symbols-outlined text-sm">arrow_back</span> Back to Home
          </a>
        </footer>
      </main>
    </div>
  `
})
export class SafetyProtocolsComponent {
  store = inject(StoreService);
}
