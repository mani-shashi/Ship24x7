import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { StoreService } from '../../core/services/store.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
})
export class LoginComponent {
  store = inject(StoreService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  isForgot = signal(false);
  isLoading = signal(false);
  requiresMfa = signal(false);

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
    mfaCode: ['']
  });

  toggleMode() {
    this.router.navigate(['/register']);
  }

  handleAuth(event?: Event) {
    if (event) event.preventDefault();
    
    if (this.loginForm.invalid && !this.requiresMfa()) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);

    if (this.isForgot()) {
      this.store.addNotification('If your email is registered, you will receive a reset link.', 'info');
      this.isLoading.set(false);
      this.isForgot.set(false);
      return;
    }

    const credentials = {
      email: this.loginForm.value.email!,
      password: this.loginForm.value.password!,
      mfaCode: this.requiresMfa() ? this.loginForm.value.mfaCode : undefined
    };

    this.authService.login(credentials as any).subscribe({
      next: (res) => {
        if (res.requiresMfa) {
          this.requiresMfa.set(true);
          this.isLoading.set(false);
          this.store.addNotification('MFA code required to proceed.', 'info');
        } else if (res.user) {
          const userWithToken = { ...res.user, token: res.accessToken };
          this.store.setUser(userWithToken);
          this.isLoading.set(false);
          this.store.addNotification('Login successful!', 'success');
          
          // Redirect based on role
          const isAdmin = res.user.roles.some((r: string) => 
            r === 'Admin_User' || r === 'System_Admin'
          );
          
          if (isAdmin) {
            this.router.navigate(['/admin']);
          } else {
            this.router.navigate(['/dashboard']);
          }
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        const errorMsg = err.error?.error || 'Invalid credentials. Please try again.';
        this.store.addNotification(errorMsg, 'error');
      }
    });
  }

  verifyMfa() {
    this.handleAuth();
  }
}
