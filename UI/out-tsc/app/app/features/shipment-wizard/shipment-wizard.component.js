import { __decorate } from "tslib";
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { StoreService } from '../../core/services/store.service';
let ShipmentWizardComponent = class ShipmentWizardComponent {
    store = inject(StoreService);
};
ShipmentWizardComponent = __decorate([
    Component({
        selector: 'app-shipment-wizard',
        standalone: true,
        imports: [CommonModule, RouterLink],
        template: `
    <div class="flex flex-col min-h-screen bg-[#f0f9f6] text-slate-900 transition-colors duration-300">
      <header class="bg-white/80 backdrop-blur-md border-b border-slate-200 sticky top-0 z-50 px-10 py-4">
        <div class="max-w-7xl mx-auto flex items-center justify-between">
          <div class="flex items-center gap-3 cursor-pointer" routerLink="/dashboard">
            <div class="w-10 h-10 bg-accent-gold/10 rounded-xl flex items-center justify-center border border-accent-gold/30 shadow-neon">
              <span class="material-symbols-outlined text-accent-gold">local_shipping</span>
            </div>
            <span class="text-xl font-display font-bold text-slate-900 tracking-tight uppercase">SHIP<span class="text-accent-gold">24X7</span></span>
          </div>
          <button routerLink="/dashboard" class="text-[10px] font-bold text-slate-400 uppercase tracking-widest hover:text-accent-gold transition-colors flex items-center gap-2">
            <span class="material-symbols-outlined text-sm">arrow_back</span> Back to Dashboard
          </button>
        </div>
      </header>

      <main class="flex-1 p-10 max-w-4xl mx-auto w-full">
        <h1 class="text-5xl font-display font-bold text-slate-900 tracking-tighter leading-none mb-4 uppercase">
          NEW <span class="text-accent-gold">SHIPMENT</span>
        </h1>
        <p class="text-slate-500 font-medium mb-16">Create a new shipment order in a few simple steps.</p>

        <div class="glass-card tech-border p-12 text-center">
          <div class="w-20 h-20 rounded-2xl bg-accent-gold/10 flex items-center justify-center mx-auto mb-8 border border-accent-gold/20">
            <span class="material-symbols-outlined text-accent-gold text-4xl">package_2</span>
          </div>
          <h2 class="text-2xl font-display font-bold text-slate-900 mb-4 uppercase tracking-tight">Shipment Wizard</h2>
          <p class="text-slate-500 max-w-md mx-auto mb-8">
            This multi-step form will guide you through creating a new shipment. Enter sender and receiver details, package dimensions, and choose a shipping method.
          </p>
          <p class="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Coming Soon — Backend Integration Required</p>
        </div>
      </main>
    </div>
  `,
    })
], ShipmentWizardComponent);
export { ShipmentWizardComponent };
//# sourceMappingURL=shipment-wizard.component.js.map