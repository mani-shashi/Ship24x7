import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { StoreService } from '../../../core/services/store.service';

import { NavbarComponent } from '../../../shared/components/navbar.component';

@Component({
  selector: 'app-logistics-tech',
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

      <main class="max-w-4xl mx-auto pt-40 pb-20 px-6">
        <header class="mb-16">
          <p class="data-label text-accent-gold mb-4 uppercase tracking-[0.3em]">Logistics Intelligence v4.0</p>
          <h1 class="text-5xl font-display font-black text-slate-900 dark:text-white uppercase tracking-tighter">Technology <span class="text-accent-gold">Stack</span></h1>
          <p class="text-slate-500 dark:text-primary-700 mt-6 text-lg leading-relaxed max-w-2xl">
            Powering global trade with AI-driven orchestration, real-time IoT telemetry, and zero-trust security architecture.
          </p>
        </header>

        <div class="grid gap-12">
           <div class="flex flex-col md:flex-row gap-12 items-center p-10 glass-card tech-border bg-white dark:bg-primary-900/20">
              <div class="w-24 h-24 bg-accent-gold/10 rounded-3xl flex items-center justify-center text-accent-gold shrink-0 shadow-neon">
                 <span class="material-symbols-outlined text-4xl">psychology</span>
              </div>
              <div>
                 <h4 class="text-lg font-black uppercase text-slate-900 dark:text-white mb-4">Neural Route Optimization</h4>
                 <p class="text-sm text-slate-600 dark:text-primary-700 leading-relaxed italic">
                    Our AI engine analyzes millions of data points—from weather patterns to local port congestion—to calculate the mathematically perfect route for every parcel.
                 </p>
              </div>
           </div>

           <div class="flex flex-col md:flex-row-reverse gap-12 items-center p-10 glass-card tech-border bg-white dark:bg-primary-900/20">
              <div class="w-24 h-24 bg-emerald-500/10 rounded-3xl flex items-center justify-center text-emerald-500 shrink-0 shadow-neon">
                 <span class="material-symbols-outlined text-4xl">sensors</span>
              </div>
              <div>
                 <h4 class="text-lg font-black uppercase text-slate-900 dark:text-white mb-4">IoT Telemetry Mesh</h4>
                 <p class="text-sm text-slate-600 dark:text-primary-700 leading-relaxed italic text-right md:text-left">
                    Real-time G-force, temperature, and humidity sensors integrated with our global satellite network ensure high-value cargo integrity throughout the journey.
                 </p>
              </div>
           </div>

           <div class="flex flex-col md:flex-row gap-12 items-center p-10 glass-card tech-border bg-white dark:bg-primary-900/20">
              <div class="w-24 h-24 bg-emerald-500/10 rounded-3xl flex items-center justify-center text-emerald-500 shrink-0 shadow-neon">
                 <span class="material-symbols-outlined text-4xl">shield</span>
              </div>
              <div>
                 <h4 class="text-lg font-black uppercase text-slate-900 dark:text-white mb-4">Zero-Trust Manifests</h4>
                 <p class="text-sm text-slate-600 dark:text-primary-700 leading-relaxed italic">
                    Immutable shipping manifests secured via encrypted ledger technology, preventing data tampering and ensuring 100% regulatory compliance.
                 </p>
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
export class LogisticsTechComponent {
  store = inject(StoreService);
}
