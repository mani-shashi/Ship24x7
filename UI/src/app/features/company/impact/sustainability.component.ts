import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { StoreService } from '../../../core/services/store.service';

import { NavbarComponent } from '../../../shared/components/navbar.component';

@Component({
  selector: 'app-sustainability',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  template: `
    <div class="min-h-screen bg-slate-50 dark:bg-primary-950 transition-colors duration-500 font-sans relative overflow-hidden">
      <!-- Animated Background Elements -->
      <div class="glass-blob -top-24 -left-24 w-96 h-96 bg-emerald-500/10"></div>
      <div class="glass-blob -bottom-24 -right-24 w-96 h-96 bg-accent-gold/10 animation-delay-2000"></div>
      <div class="scanline opacity-5 dark:opacity-10"></div>
      <div class="absolute inset-0 bg-[radial-gradient(circle_at_2px_2px,rgba(16,185,129,0.03)_1px,transparent_0)] bg-[size:40px_40px] pointer-events-none"></div>

      <app-navbar />

      <main class="max-w-4xl mx-auto pt-40 pb-20 px-6">
        <header class="mb-16">
          <p class="data-label text-emerald-500 mb-4 uppercase tracking-[0.3em]">Carbon Neutrality 2026</p>
          <h1 class="text-5xl font-display font-black text-slate-900 dark:text-white uppercase tracking-tighter">Impact & <span class="text-emerald-500">Sustainability</span></h1>
          <p class="text-slate-500 dark:text-primary-700 mt-6 text-lg leading-relaxed">
            Our commitment to a zero-carbon future through intelligent routing and carbon-offset integration.
          </p>
        </header>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div class="glass-card tech-border p-8 md:p-10 bg-emerald-500/5 border-emerald-500/20 flex flex-col items-start overflow-hidden">
               <div class="w-14 h-14 bg-emerald-500/10 rounded-xl flex items-center justify-center mb-6 shrink-0 overflow-hidden">
                  <span class="material-symbols-outlined text-emerald-500 text-2xl leading-none">eco</span>
               </div>
               <h3 class="text-xl font-bold text-slate-900 dark:text-white mb-4 uppercase tracking-tight">Eco-Check Enabled</h3>
               <p class="text-sm text-slate-600 dark:text-primary-700 leading-relaxed italic">
                  Every shipment through Ship247 includes automated carbon-offset calculations, contributing to global reforestation projects.
               </p>
            </div>
            
            <div class="glass-card tech-border p-8 md:p-10 bg-emerald-500/5 border-emerald-500/20 flex flex-col items-start overflow-hidden">
               <div class="w-14 h-14 bg-emerald-500/10 rounded-xl flex items-center justify-center mb-6 shrink-0 overflow-hidden">
                  <span class="material-symbols-outlined text-emerald-500 text-2xl leading-none">local_shipping</span>
               </div>
               <h3 class="text-xl font-bold text-slate-900 dark:text-white mb-4 uppercase tracking-tight">Green Fleet</h3>
               <p class="text-sm text-slate-600 dark:text-primary-700 leading-relaxed italic">
                  We are transitioning our last-mile delivery network to 100% electric vehicles in 40+ major metropolitan zones by the end of 2026.
               </p>
            </div>
          </div>

        <div class="mt-20 p-10 glass-card tech-border bg-white dark:bg-primary-900/40 text-center">
           <p class="data-label text-emerald-500 mb-6">Current Offset Performance</p>
           <p class="text-7xl font-display font-black text-slate-900 dark:text-white tracking-tighter mb-4">12.4K <span class="text-2xl text-slate-400">TONS</span></p>
           <p class="text-xs font-bold text-slate-500 dark:text-primary-700 uppercase tracking-widest">CO2 Removed from atmosphere through our global initiatives.</p>
        </div>

        <footer class="mt-20 pt-10 border-t border-slate-100 dark:border-white/5">
          <a routerLink="/" class="inline-flex items-center gap-2 text-accent-gold font-bold uppercase tracking-widest text-[10px] hover:translate-x-2 transition-transform">
            <span class="material-symbols-outlined text-sm">arrow_back</span> Back to Home
          </a>
        </footer>
      </main>
    </div>
  `
})
export class SustainabilityComponent {
  store = inject(StoreService);
}
