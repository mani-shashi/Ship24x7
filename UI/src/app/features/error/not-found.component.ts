import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StoreService } from '../../core/services/store.service';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="min-h-screen bg-slate-50 dark:bg-primary-950 flex flex-col transition-colors duration-300 font-sans">
      <header class="p-10 flex justify-end">
        <button (click)="store.toggleTheme()" class="p-3 glass-card hover:border-accent-gold/40 flex items-center justify-center">
          @if (store.theme() === 'dark') { <span class="material-symbols-outlined text-accent-gold">light_mode</span> }
          @else { <span class="material-symbols-outlined text-slate-600">dark_mode</span> }
        </button>
      </header>
      
      <div class="flex-1 flex items-center justify-center p-6">
        <div class="max-w-md w-full text-center">
          <div class="relative mb-12">
            <div class="absolute inset-0 bg-accent-gold/10 blur-3xl rounded-full"></div>
            <span class="material-symbols-outlined text-[120px] relative mx-auto text-accent-gold animate-bounce-slow">sentiment_dissatisfied</span>
            <h1 class="text-9xl font-display font-black text-slate-900 dark:text-white opacity-10 absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 select-none">404</h1>
          </div>

          <h2 class="text-3xl font-display font-black text-slate-900 dark:text-white uppercase tracking-tighter mb-4">Page Not Found</h2>
          <p class="text-slate-500 dark:text-primary-700 text-sm font-bold uppercase tracking-widest leading-relaxed mb-10">
            The page you are looking for does not exist or has been moved.
          </p>

          <div class="flex flex-col sm:flex-row gap-4 items-center justify-center">
            <a routerLink="/dashboard" class="bg-slate-900 dark:bg-accent-gold text-white dark:text-primary-950 font-black uppercase tracking-widest text-[10px] px-8 h-12 rounded-2xl flex items-center justify-center gap-2 hover:scale-105 transition-all">
              <span class="material-symbols-outlined text-sm">home</span>
              Return to Core
            </a>
            <button (click)="goBack()" class="px-8 h-12 glass-card text-[10px] font-black uppercase tracking-widest text-slate-400 hover:text-accent-gold transition-colors flex items-center gap-2 justify-center">
              <span class="material-symbols-outlined text-sm">arrow_back</span>
              Reverse Track
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    @keyframes bounce-slow {
      0%, 100% { transform: translateY(0); }
      50% { transform: translateY(-20px); }
    }
    .animate-bounce-slow {
      animation: bounce-slow 4s infinite ease-in-out;
    }
    .glass-card {
      @apply bg-white/70 dark:bg-primary-900/40 backdrop-blur-xl transition-all duration-300 border border-slate-200 dark:border-white/5 rounded-2xl;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotFoundComponent {
  public store = inject(StoreService);
  
  goBack() {
    window.history.back();
  }
}

