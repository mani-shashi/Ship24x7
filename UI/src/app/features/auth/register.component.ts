import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { StoreService } from '../../core/services/store.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="min-h-screen bg-slate-50 dark:bg-primary-950 flex items-center justify-center p-6 relative overflow-hidden transition-colors duration-500">
      <!-- Background elements -->
      <div class="absolute top-0 right-0 w-[600px] h-[600px] bg-accent-gold/5 blur-[120px] rounded-full pointer-events-none opacity-50"></div>
      <div class="absolute bottom-0 left-0 w-[600px] h-[600px] bg-accent-gold/5 blur-[120px] rounded-full pointer-events-none opacity-50"></div>

      <div class="max-w-md w-full relative z-10 animate-fade-up">
        <header class="text-center mb-10">
          <div class="flex items-center justify-center gap-3 mb-6 cursor-pointer" routerLink="/">
            <div class="w-12 h-12 bg-accent-gold/10 rounded-2xl flex items-center justify-center border border-accent-gold/30 shadow-neon">
              <span class="material-symbols-outlined text-accent-gold text-3xl">local_shipping</span>
            </div>
            <span class="text-3xl font-display font-bold text-slate-900 dark:text-white tracking-tighter uppercase">Ship<span class="text-accent-gold">24<span class="text-sm lowercase mx-0.5 opacity-70">x</span>7</span></span>
          </div>
          <h2 class="text-xl font-display font-bold text-slate-900 dark:text-white uppercase tracking-widest">Create Your Account</h2>
          <p class="data-label mt-2">Join the world's most advanced logistics network.</p>
        </header>

        <div class="glass-card tech-border p-10 rounded-[2.5rem]">
          <form [formGroup]="registerForm" (submit)="onSubmit()" class="space-y-6">
            <div class="space-y-2">
              <label class="data-label ml-1">Full Name</label>
              <div class="relative group">
                <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-accent-gold transition-colors">person</span>
                <input 
                  formControlName="fullName"
                  type="text" 
                  class="input-field pl-12"
                  placeholder="John Doe"
                  [class.border-red-500]="registerForm.get('fullName')?.invalid && registerForm.get('fullName')?.touched"
                >
              </div>
              @if (registerForm.get('fullName')?.invalid && registerForm.get('fullName')?.touched) {
                <p class="text-[10px] text-red-500 font-bold uppercase tracking-widest pl-1">Name must be at least 3 characters</p>
              }
            </div>

            <div class="space-y-2">
              <label class="data-label ml-1">Email Address</label>
              <div class="relative group">
                <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-accent-gold transition-colors">mail</span>
                <input 
                  formControlName="email"
                  type="email" 
                  class="input-field pl-12"
                  placeholder="name@company.com"
                  [class.border-red-500]="registerForm.get('email')?.invalid && registerForm.get('email')?.touched"
                >
              </div>
              @if (registerForm.get('email')?.invalid && registerForm.get('email')?.touched) {
                <p class="text-[10px] text-red-500 font-bold uppercase tracking-widest pl-1">Please enter a valid email address</p>
              }
            </div>

            <div class="space-y-2">
              <label class="data-label ml-1">Phone Number</label>
              <div class="relative group">
                <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-accent-gold transition-colors">phone</span>
                <input 
                  formControlName="phoneNumber"
                  type="tel" 
                  class="input-field pl-12"
                  placeholder="+91 98765 43210"
                  [class.border-red-500]="registerForm.get('phoneNumber')?.invalid && registerForm.get('phoneNumber')?.touched"
                >
              </div>
              @if (registerForm.get('phoneNumber')?.invalid && registerForm.get('phoneNumber')?.touched) {
                <p class="text-[10px] text-red-500 font-bold uppercase tracking-widest pl-1">Valid phone number required (min 10 digits)</p>
              }
            </div>

            <div class="space-y-2">
              <label class="data-label ml-1">Password</label>
              <div class="relative group">
                <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-accent-gold transition-colors">lock</span>
                <input 
                  formControlName="password"
                  [type]="showPassword() ? 'text' : 'password'" 
                  class="input-field pl-12 pr-12"
                  placeholder="••••••••"
                  [class.border-red-500]="registerForm.get('password')?.invalid && registerForm.get('password')?.touched"
                >
                <button 
                  type="button"
                  (click)="showPassword.set(!showPassword())"
                  class="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600 dark:hover:text-white transition-colors"
                >
                  <span class="material-symbols-outlined text-sm">{{ showPassword() ? 'visibility_off' : 'visibility' }}</span>
                </button>
              </div>
              @if (registerForm.get('password')?.invalid && registerForm.get('password')?.touched) {
                <p class="text-[10px] text-red-500 font-bold uppercase tracking-widest pl-1">Password must be 8+ chars with upper, lower, digit & special char</p>
              } @else {
                <p class="text-[9px] text-slate-400 font-mono uppercase tracking-widest pl-1">Security: Strong encryption required</p>
              }
            </div>

            <div class="pt-4">
              <button 
                type="submit" 
                [disabled]="registerForm.invalid || isLoading()"
                class="btn-primary w-full h-14 text-sm font-black uppercase tracking-[0.2em] group relative overflow-hidden"
              >
                @if (isLoading()) {
                  <div class="w-6 h-6 border-2 border-primary-950 border-t-transparent rounded-full animate-spin"></div>
                } @else {
                  <span>Create Account</span>
                  <span class="material-symbols-outlined ml-2 group-hover:translate-x-1 transition-transform">arrow_forward</span>
                }
              </button>
            </div>
          </form>

          <div class="mt-10 pt-10 border-t border-slate-100 dark:border-white/5 text-center">
            <p class="text-[10px] text-slate-400 font-bold uppercase tracking-widest">
              Already have an account? 
              <a routerLink="/login" class="text-accent-gold hover:underline underline-offset-4 ml-1">Log In Here</a>
            </p>
          </div>
        </div>

        <footer class="mt-12 text-center">
           <p class="text-[9px] font-mono text-slate-400 dark:text-primary-800 uppercase tracking-widest">© 2026 Ship24x7™ CORE. SECURE TRANSMISSION ACTIVE.</p>
        </footer>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; }
  `]
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private store = inject(StoreService);
  private router = inject(Router);

  registerForm = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: ['', [Validators.required, Validators.pattern(/^[0-9\+\-\s]{10,15}$/)]],
    password: ['', [
      Validators.required, 
      Validators.minLength(8),
      Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/)
    ]]
  });

  isLoading = signal(false);
  showPassword = signal(false);

  onSubmit() {
    if (this.registerForm.valid) {
      this.isLoading.set(true);
      this.authService.register(this.registerForm.value).subscribe({
        next: (response: any) => {
          this.isLoading.set(false);
          this.store.addNotification(response.message || 'Registration successful! Please check your email.', 'success');
          this.router.navigate(['/login']);
        },
        error: (err) => {
          this.isLoading.set(false);
          const errorMsg = err.error?.error || 'Registration failed. Please try again.';
          this.store.addNotification(errorMsg, 'error');
        }
      });
    }
  }
}
