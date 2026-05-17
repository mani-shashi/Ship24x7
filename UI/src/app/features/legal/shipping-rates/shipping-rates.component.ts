import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { StoreService } from '../../../core/services/store.service';

import { NavbarComponent } from '../../../shared/components/navbar.component';

@Component({
  selector: 'app-shipping-rates',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  template: `
    <div class="min-h-screen bg-slate-50 dark:bg-primary-950 transition-colors duration-500 font-sans relative overflow-hidden">
      <!-- Animated Background Elements -->
      <div class="glass-blob -top-24 -left-24 w-96 h-96 bg-accent-gold/20"></div>
      <div class="glass-blob -bottom-24 -right-24 w-96 h-96 bg-emerald-500/20 animation-delay-2000"></div>
      <div class="scanline opacity-5 dark:opacity-10"></div>
      <div class="absolute inset-0 bg-[radial-gradient(circle_at_2px_2px,rgba(0,0,0,0.02)_1px,transparent_0)] bg-[size:40px_40px] pointer-events-none"></div>

      <app-navbar />

      <main class="max-w-4xl mx-auto pt-40 pb-20 px-6">
        <header class="mb-16">
          <p class="data-label text-accent-gold mb-4">Pricing Index 2026</p>
          <h1 class="text-5xl font-display font-black text-slate-900 dark:text-white uppercase tracking-tighter">Shipping <span class="text-accent-gold">Rates</span></h1>
          <p class="text-slate-500 mt-4">Transparent, enterprise-grade pricing based on weight, volume, and urgency.</p>
        </header>

        <div class="grid gap-8">
          <div class="glass-card tech-border p-8">
            <h3 class="text-xl font-bold text-slate-900 dark:text-white mb-6 uppercase tracking-tight flex items-center gap-3">
              <span class="material-symbols-outlined text-accent-gold">distance</span> Domestic Logistics
            </h3>
            <div class="overflow-x-auto">
              <table class="w-full text-left border-collapse">
                <thead>
                  <tr class="border-b border-slate-100 dark:border-white/5">
                    <th class="py-4 data-label">Weight Class</th>
                    <th class="py-4 data-label">Standard (3-5 Days)</th>
                    <th class="py-4 data-label">Express (Next Day)</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-50 dark:divide-white/5">
                  <tr>
                    <td class="py-4 font-mono text-sm">0 - 500g</td>
                    <td class="py-4 text-sm font-bold">₹99</td>
                    <td class="py-4 text-sm font-bold text-accent-gold">₹249</td>
                  </tr>
                  <tr>
                    <td class="py-4 font-mono text-sm">500g - 2kg</td>
                    <td class="py-4 text-sm font-bold">₹199</td>
                    <td class="py-4 text-sm font-bold text-accent-gold">₹399</td>
                  </tr>
                  <tr>
                    <td class="py-4 font-mono text-sm">2kg - 5kg</td>
                    <td class="py-4 text-sm font-bold">₹349</td>
                    <td class="py-4 text-sm font-bold text-accent-gold">₹649</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
          <div class="glass-card tech-border p-8">
            <h3 class="text-xl font-bold text-slate-900 dark:text-white mb-6 uppercase tracking-tight flex items-center gap-3">
              <span class="material-symbols-outlined text-accent-gold">public</span> Global Freight
            </h3>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div class="p-4 bg-slate-50 dark:bg-white/5 rounded-xl border border-slate-100 dark:border-white/5">
                <p class="text-[10px] font-black uppercase text-accent-gold mb-2">Zone 1: APAC & Middle East</p>
                <p class="text-sm font-medium">Starting from <span class="font-bold text-slate-900 dark:text-white">₹1,249</span> per KG</p>
              </div>
              <div class="p-4 bg-slate-50 dark:bg-white/5 rounded-xl border border-slate-100 dark:border-white/5">
                <p class="text-[10px] font-black uppercase text-accent-gold mb-2">Zone 2: Europe & Americas</p>
                <p class="text-sm font-medium">Starting from <span class="font-bold text-slate-900 dark:text-white">₹2,499</span> per KG</p>
              </div>
            </div>
          </div>
        </div>
        <footer class="mt-20 pt-10 border-t border-slate-100 dark:border-white/5">
          <p class="text-xs text-slate-400 italic leading-relaxed">* Rates are indicative and subject to dynamic fuel surcharges.</p>
          <a routerLink="/" class="mt-10 inline-flex items-center gap-2 text-accent-gold font-bold uppercase tracking-widest text-[10px] hover:translate-x-2 transition-transform">
            <span class="material-symbols-outlined text-sm">arrow_back</span> Back to Home
          </a>
        </footer>
      </main>
    </div>
  `
})
export class ShippingRatesComponent {
  store = inject(StoreService);
}
