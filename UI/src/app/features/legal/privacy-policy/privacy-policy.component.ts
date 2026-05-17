import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { StoreService } from '../../../core/services/store.service';

import { NavbarComponent } from '../../../shared/components/navbar.component';

@Component({
  selector: 'app-privacy-policy',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  template: `
    <div class="min-h-screen bg-slate-50 dark:bg-primary-950 transition-colors duration-500 font-sans relative overflow-hidden">
      <!-- Animated Background Elements -->
      <div class="glass-blob -top-24 -left-24 w-96 h-96 bg-indigo-500/10"></div>
      <div class="glass-blob -bottom-24 -right-24 w-96 h-96 bg-emerald-500/10 animation-delay-2000"></div>
      <div class="scanline opacity-5 dark:opacity-10"></div>
      <div class="absolute inset-0 bg-[radial-gradient(circle_at_2px_2px,rgba(0,0,0,0.02)_1px,transparent_0)] bg-[size:40px_40px] pointer-events-none"></div>

      <app-navbar />

      <main class="max-w-3xl mx-auto pt-40 pb-20 px-6">
        <header class="mb-16">
          <p class="data-label text-emerald-500 mb-4">Data Governance</p>
          <h1 class="text-5xl font-display font-black text-slate-900 dark:text-white uppercase tracking-tighter">Privacy <span class="text-emerald-500">Policy</span></h1>
          <p class="text-slate-500 dark:text-primary-700 mt-4">Last Updated: May 2026. Your privacy is protected by our zero-trust data architecture.</p>
        </header>

        <div class="prose dark:prose-invert max-w-none space-y-12">
          <section>
            <h2 class="text-2xl font-display font-bold text-slate-900 dark:text-white uppercase tracking-tight mb-4">1. Data Collection</h2>
            <p class="text-slate-600 dark:text-primary-700 leading-relaxed">
              We collect essential metadata required for logistics orchestration, including sender/receiver identities, geospatial coordinates, and package specifications. This data is encrypted at rest and in transit.
            </p>
          </section>

          <section>
            <h2 class="text-2xl font-display font-bold text-slate-900 dark:text-white uppercase tracking-tight mb-4">2. Utilization Protocol</h2>
            <p class="text-slate-600 dark:text-primary-700 leading-relaxed">
              Data is utilized solely for route optimization, manifest generation, and regulatory compliance. We do not sell user data to third-party marketing entities.
            </p>
          </section>

          <section>
            <h2 class="text-2xl font-display font-bold text-slate-900 dark:text-white uppercase tracking-tight mb-4">3. Security Infrastructure</h2>
            <p class="text-slate-600 dark:text-primary-700 leading-relaxed">
              Our systems utilize AES-256 bit encryption and multi-factor authentication for all administrative access. Real-time telemetry data is scrubbed of PII (Personally Identifiable Information) before being stored in our analytics engine.
            </p>
          </section>
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
export class PrivacyPolicyComponent {
  store = inject(StoreService);
}
