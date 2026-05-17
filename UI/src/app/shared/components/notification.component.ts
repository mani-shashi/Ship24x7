import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StoreService } from '../../core/services/store.service';

@Component({
  selector: 'app-notification',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed top-8 right-8 z-[100] flex flex-col gap-4 w-96 tabular-nums">
      @for (n of store.notifications(); track n.id) {
        <div
          class="relative px-6 py-4 shadow-2xl flex items-start gap-4 backdrop-blur-xl border-l-[4px] bg-slate-900/90 dark:bg-primary-950/90 animate-fade-up overflow-hidden rounded-r-xl"
          [ngClass]="{
            'border-l-emerald-500/80': n.type === 'success',
            'border-l-rose-500/80': n.type === 'error',
            'border-l-accent-gold/80': n.type === 'warning',
            'border-l-blue-500/80': n.type === 'info'
          }"
        >
          <div class="absolute inset-0 bg-gradient-to-r from-white/[0.02] to-transparent pointer-events-none"></div>
          
          <div class="mt-0.5 shrink-0">
            @switch (n.type) {
              @case ('success') { <span class="material-symbols-outlined text-emerald-500 text-xl">check_circle</span> }
              @case ('error') { <span class="material-symbols-outlined text-rose-500 text-xl">error</span> }
              @case ('warning') { <span class="material-symbols-outlined text-accent-gold text-xl">warning</span> }
              @case ('info') { <span class="material-symbols-outlined text-blue-500 text-xl">info</span> }
            }
          </div>
          
          <div class="flex-1 space-y-1">
            <p class="text-[9px] opacity-50 uppercase tracking-[0.2em] font-black text-white">
              {{ n.type === 'success' ? 'Sync Complete' : (n.type === 'error' ? 'Critical Failure' : 'System Telemetry') }}
            </p>
            <p class="text-xs font-mono font-bold text-white tracking-wide leading-relaxed">
              {{ n.message }}
            </p>
          </div>
          
          <button 
            (click)="store.removeNotification(n.id)"
            class="mt-0.5 opacity-40 hover:opacity-100 transition-opacity hover:text-accent-gold text-white"
          >
            <span class="material-symbols-outlined text-sm">close</span>
          </button>
        </div>
      }
    </div>
  `
})
export class NotificationComponent {
  public store = inject(StoreService);
}
