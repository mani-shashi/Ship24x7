import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { StoreService } from '../../../core/services/store.service';

import { NavbarComponent } from '../../../shared/components/navbar.component';

@Component({
  selector: 'app-global-network',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  template: `
    <div class="min-h-screen bg-slate-50 dark:bg-primary-950 transition-colors duration-500 font-sans relative overflow-hidden">
      <!-- Animated Background Elements -->
      <div class="glass-blob -top-24 -left-24 w-96 h-96 bg-accent-gold/10"></div>
      <div class="glass-blob -bottom-24 -right-24 w-96 h-96 bg-indigo-500/10 animation-delay-2000"></div>
      <div class="scanline opacity-5 dark:opacity-10"></div>
      <div class="absolute inset-0 bg-[radial-gradient(circle_at_2px_2px,rgba(0,0,0,0.02)_1px,transparent_0)] bg-[size:40px_40px] pointer-events-none"></div>

      <app-navbar />

      <main class="max-w-6xl mx-auto pt-40 pb-20 px-6">
        <header class="mb-16 text-center">
          <p class="data-label text-accent-gold mb-4 uppercase tracking-[0.3em]">Geospatial reach</p>
          <h1 class="text-6xl font-display font-black text-slate-900 dark:text-white uppercase tracking-tighter">Global <span class="text-accent-gold">Network</span></h1>
          <p class="text-slate-500 dark:text-primary-700 mt-6 max-w-2xl mx-auto text-lg leading-relaxed">
            Connected 120+ strategic logistics hubs across 6 continents, delivering 99.9% uptime for your supply chain.
          </p>
        </header>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
           <div class="lg:col-span-2 glass-card tech-border p-10 bg-white/40 dark:bg-primary-900/40 min-h-[400px] flex items-center justify-center relative overflow-hidden">
              <div class="absolute inset-0 opacity-20 bg-[url('https://www.transparenttextures.com/patterns/carbon-fibre.png')]"></div>
              <span class="material-symbols-outlined text-[240px] text-accent-gold opacity-10 animate-pulse">public</span>
              <div class="absolute inset-0 flex items-center justify-center">
                 <div class="w-full h-px bg-gradient-to-r from-transparent via-accent-gold/20 to-transparent absolute"></div>
                 <div class="h-full w-px bg-gradient-to-b from-transparent via-accent-gold/20 to-transparent absolute"></div>
              </div>
              <p class="relative z-10 font-mono text-[10px] text-accent-gold uppercase tracking-[0.5em]">Real-time Visualization active</p>
           </div>
           
           <div class="space-y-6">
              <div class="glass-card tech-border p-6 border-accent-gold/20">
                 <h4 class="text-xs font-black uppercase text-slate-900 dark:text-white mb-2">APAC Regional Hubs</h4>
                 <p class="text-[10px] text-slate-500 dark:text-primary-700 leading-relaxed italic">Singapore, Mumbai, Tokyo, Shanghai Terminal B-04.</p>
              </div>
              <div class="glass-card tech-border p-6 border-emerald-500/20">
                 <h4 class="text-xs font-black uppercase text-slate-900 dark:text-white mb-2">MENA Gateway</h4>
                 <p class="text-[10px] text-slate-500 dark:text-primary-700 leading-relaxed italic">Dubai Logistics City, Cairo West, Riyadh Central Hub.</p>
              </div>
              <div class="glass-card tech-border p-6 border-emerald-500/20">
                 <h4 class="text-xs font-black uppercase text-slate-900 dark:text-white mb-2">Trans-Atlantic Nodes</h4>
                 <p class="text-[10px] text-slate-500 dark:text-primary-700 leading-relaxed italic">London Gateway, Rotterdam Port, New York JFK-01 Terminal.</p>
              </div>
           </div>
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
export class GlobalNetworkComponent {
  store = inject(StoreService);
}
