import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { StoreService } from '../../core/services/store.service';

@Component({
  selector: 'app-mobile-nav',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <nav class="md:hidden fixed bottom-0 left-0 right-0 bg-white/80 dark:bg-primary-950/80 backdrop-blur-lg border-t border-slate-200 dark:border-white/5 z-50 px-6 py-3 pb-8 flex items-center justify-between">
      <button routerLink="/dashboard" routerLinkActive="text-accent-gold" class="flex flex-col items-center gap-1 text-slate-400">
        <span class="material-symbols-outlined">dashboard</span>
        <span class="text-[10px] font-bold uppercase tracking-widest">Home</span>
      </button>
      
      <button routerLink="/track" routerLinkActive="text-accent-gold" class="flex flex-col items-center gap-1 text-slate-400">
        <span class="material-symbols-outlined">search</span>
        <span class="text-[10px] font-bold uppercase tracking-widest">Track</span>
      </button>

      <div class="relative -top-6">
        <button routerLink="/new-shipment" class="w-14 h-14 bg-accent-gold text-white rounded-full shadow-lg shadow-accent-gold/40 flex items-center justify-center border-4 border-white dark:border-primary-950">
          <span class="material-symbols-outlined text-3xl">add</span>
        </button>
      </div>

      <button routerLink="/shipments" routerLinkActive="text-accent-gold" class="flex flex-col items-center gap-1 text-slate-400">
        <span class="material-symbols-outlined">inventory_2</span>
        <span class="text-[10px] font-bold uppercase tracking-widest">Ships</span>
      </button>

      <button routerLink="/settings" routerLinkActive="text-accent-gold" class="flex flex-col items-center gap-1 text-slate-400">
        <span class="material-symbols-outlined">person</span>
        <span class="text-[10px] font-bold uppercase tracking-widest">Profile</span>
      </button>
    </nav>
  `,
  styles: [`
    :host { display: block; }
  `]
})
export class MobileNavComponent {
  store = inject(StoreService);
}
