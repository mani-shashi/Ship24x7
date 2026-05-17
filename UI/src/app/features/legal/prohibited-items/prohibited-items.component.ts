import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { StoreService } from '../../../core/services/store.service';

import { NavbarComponent } from '../../../shared/components/navbar.component';

@Component({
  selector: 'app-prohibited-items',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  template: `
    <div class="min-h-screen bg-slate-50 dark:bg-primary-950 transition-colors duration-500 font-sans relative overflow-hidden">
      <!-- Animated Background Elements -->
      <div class="glass-blob -top-24 -right-24 w-96 h-96 bg-rose-500/10"></div>
      <div class="glass-blob -bottom-24 -left-24 w-96 h-96 bg-indigo-500/10 animation-delay-2000"></div>
      <div class="scanline opacity-5 dark:opacity-10"></div>
      <div class="absolute inset-0 bg-[radial-gradient(circle_at_2px_2px,rgba(0,0,0,0.02)_1px,transparent_0)] bg-[size:40px_40px] pointer-events-none"></div>

      <app-navbar />

      <main class="max-w-4xl mx-auto pt-40 pb-20 px-6">
        <header class="mb-16">
          <p class="data-label text-rose-500 mb-4">Regulatory Compliance</p>
          <h1 class="text-5xl font-display font-black text-slate-900 dark:text-white uppercase tracking-tighter">Prohibited <span class="text-rose-500">Items</span></h1>
          <p class="text-slate-500 mt-4">Safety is our priority. Ensure your shipment does not contain any restricted materials.</p>
        </header>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
          <div class="glass-card tech-border p-8 border-rose-500/20">
            <h3 class="text-lg font-bold text-slate-900 dark:text-white mb-6 uppercase tracking-tight flex items-center gap-3">
              <span class="material-symbols-outlined text-rose-500">dangerous</span> Hazardous Materials
            </h3>
            <ul class="space-y-3">
              <li class="flex items-center gap-3 text-sm text-slate-600 dark:text-primary-700">
                 <div class="w-1.5 h-1.5 rounded-full bg-rose-500"></div> Explosives & Fireworks
              </li>
              <li class="flex items-center gap-3 text-sm text-slate-600 dark:text-primary-700">
                 <div class="w-1.5 h-1.5 rounded-full bg-rose-500"></div> Flammable Liquids & Gases
              </li>
              <li class="flex items-center gap-3 text-sm text-slate-600 dark:text-primary-700">
                 <div class="w-1.5 h-1.5 rounded-full bg-rose-500"></div> Toxic & Infectious Substances
              </li>
            </ul>
          </div>

          <div class="glass-card tech-border p-8 border-rose-500/20">
            <h3 class="text-lg font-bold text-slate-900 dark:text-white mb-6 uppercase tracking-tight flex items-center gap-3">
              <span class="material-symbols-outlined text-rose-500">block</span> Restricted Goods
            </h3>
            <ul class="space-y-3">
              <li class="flex items-center gap-3 text-sm text-slate-600 dark:text-primary-700">
                 <div class="w-1.5 h-1.5 rounded-full bg-rose-500"></div> Currency & Bearer Instruments
              </li>
              <li class="flex items-center gap-3 text-sm text-slate-600 dark:text-primary-700">
                 <div class="w-1.5 h-1.5 rounded-full bg-rose-500"></div> Perishable Foodstuffs (Standard)
              </li>
              <li class="flex items-center gap-3 text-sm text-slate-600 dark:text-primary-700">
                 <div class="w-1.5 h-1.5 rounded-full bg-rose-500"></div> Counterfeit Goods
              </li>
            </ul>
          </div>
        </div>

        <div class="mt-12 p-8 bg-rose-500/5 border border-rose-500/20 rounded-3xl">
           <p class="text-xs font-bold text-rose-500 uppercase tracking-widest mb-2">Legal Warning</p>
           <p class="text-sm text-slate-600 dark:text-primary-700 leading-relaxed italic">
             Attempting to ship prohibited items may result in immediate seizure by customs, permanent account suspension, and potential legal action by relevant authorities.
           </p>
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
export class ProhibitedItemsComponent {
  store = inject(StoreService);
}
