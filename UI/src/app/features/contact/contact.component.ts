import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { StoreService } from '../../core/services/store.service';
import { NavbarComponent } from '../../shared/components/navbar.component';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NavbarComponent],
  template: `
    <div class="min-h-screen bg-slate-50 dark:bg-primary-950 transition-colors duration-500 font-sans relative overflow-hidden">
      <!-- Animated Background Elements -->
      <div class="glass-blob -top-24 -left-24 w-96 h-96 bg-emerald-500/10"></div>
      <div class="glass-blob top-1/2 -right-24 w-96 h-96 bg-accent-gold/10 animation-delay-2000"></div>
      
      <app-navbar />

      <main class="max-w-6xl mx-auto pt-40 pb-20 px-6">
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-20">
          <!-- Contact Narrative -->
          <div>
            <header class="mb-12">
              <p class="data-label text-accent-gold mb-4 uppercase tracking-[0.3em]">Communication Portal</p>
              <h1 class="text-6xl font-display font-black text-slate-900 dark:text-white uppercase tracking-tighter mb-6">Contact <span class="text-accent-gold">Us</span></h1>
              <p class="text-slate-500 dark:text-primary-700 text-lg leading-relaxed">
                Have an inquiry or operational concern? Use the transmission form to reach our global dispatch team. We prioritize every communication to ensure seamless logistics orchestration.
              </p>
            </header>

            <div class="space-y-8">
              <div class="flex items-center gap-6 p-6 glass-card tech-border border-emerald-500/20">
                 <div class="w-12 h-12 bg-emerald-500/10 rounded-xl flex items-center justify-center text-emerald-500">
                    <span class="material-symbols-outlined">headset_mic</span>
                 </div>
                 <div>
                    <h4 class="text-xs font-black uppercase text-slate-900 dark:text-white">24/7 Priority Support</h4>
                    <p class="text-[10px] text-slate-500 dark:text-primary-800 uppercase font-bold tracking-widest">Available via Secure Portal</p>
                 </div>
              </div>
              <div class="flex items-center gap-6 p-6 glass-card tech-border border-accent-gold/20">
                 <div class="w-12 h-12 bg-accent-gold/10 rounded-xl flex items-center justify-center text-accent-gold">
                    <span class="material-symbols-outlined">mail</span>
                 </div>
                 <div>
                    <h4 class="text-xs font-black uppercase text-slate-900 dark:text-white">Email Operations</h4>
                    <p class="text-[10px] text-slate-500 dark:text-primary-800 uppercase font-bold tracking-widest">Routed via Transmission Form</p>
                 </div>
              </div>
            </div>
          </div>

          <!-- Contact Form -->
          <div class="glass-card tech-border p-10 bg-white dark:bg-primary-900/40 relative overflow-hidden">
             @if (!submitted) {
               <form [formGroup]="contactForm" (ngSubmit)="onSubmit()" class="space-y-6 relative z-10">
                 <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div class="space-y-2">
                       <label class="data-label text-[9px]">Full Identity</label>
                       <input type="text" formControlName="name" placeholder="John Doe" class="input-field">
                    </div>
                    <div class="space-y-2">
                       <label class="data-label text-[9px]">Contact Email</label>
                       <input type="email" formControlName="email" placeholder="john&#64;enterprise.com" class="input-field">
                    </div>
                 </div>
                 <div class="space-y-2">
                    <label class="data-label text-[9px]">Inquiry Subject</label>
                    <select formControlName="subject" class="input-field appearance-none">
                       <option value="General Inquiry">General Inquiry</option>
                       <option value="Enterprise Solutions">Enterprise Solutions</option>
                       <option value="Technical Support">Technical Support</option>
                       <option value="Billing">Billing & Settlement</option>
                    </select>
                 </div>
                 <div class="space-y-2">
                    <label class="data-label text-[9px]">Your Concern</label>
                    <textarea formControlName="message" rows="5" placeholder="Describe your operational requirements..." class="input-field resize-none"></textarea>
                 </div>
                 <button 
                   type="submit" 
                   [disabled]="contactForm.invalid || loading"
                   class="btn-primary w-full h-16 text-sm group shadow-neon"
                 >
                   @if (!loading) {
                     <span>Transmit Message</span>
                     <span class="material-symbols-outlined group-hover:translate-x-1 transition-transform">send</span>
                   } @else {
                     <div class="w-5 h-5 border-2 border-primary-950 border-t-transparent rounded-full animate-spin"></div>
                   }
                 </button>
               </form>
             } @else {
               <div class="text-center py-20 animate-fade-up">
                  <div class="w-20 h-20 bg-emerald-500/10 rounded-full flex items-center justify-center text-emerald-500 mx-auto mb-8 shadow-neon">
                     <span class="material-symbols-outlined text-4xl">verified</span>
                  </div>
                  <h3 class="text-2xl font-display font-bold text-slate-900 dark:text-white uppercase tracking-tight mb-4">Transmission Successful</h3>
                  <p class="text-slate-500 text-sm leading-relaxed max-w-xs mx-auto">
                     Your message has been routed to our regional dispatch center. Expect a response within 120 minutes.
                  </p>
                  <button (click)="submitted = false; contactForm.reset()" class="mt-10 text-accent-gold font-bold uppercase tracking-widest text-[10px] hover:underline">Send another message</button>
               </div>
             }
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
export class ContactComponent {
  store = inject(StoreService);
  private fb = inject(FormBuilder);

  contactForm = this.fb.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    subject: ['General Inquiry', Validators.required],
    message: ['', [Validators.required, Validators.minLength(20)]]
  });

  loading = false;
  submitted = false;

  onSubmit() {
    if (this.contactForm.valid) {
      this.loading = true;
      // Mock transmission delay
      setTimeout(() => {
        this.loading = false;
        this.submitted = true;
      }, 1500);
    }
  }
}
