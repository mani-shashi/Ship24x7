import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { NotificationService, NotificationLog } from '../../core/services/notification.service';
import { StoreService } from '../../core/services/store.service';

@Component({
  selector: 'app-notification-center',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen bg-slate-50 dark:bg-[#0a0a0c] pt-24 pb-12">
      <div class="max-w-4xl mx-auto px-4">
        <!-- Header -->
        <div class="flex items-center justify-between mb-8">
          <div>
            <h1 class="text-2xl font-bold text-slate-900 dark:text-white flex items-center gap-3">
              <span class="material-symbols-outlined text-accent-gold">notifications_active</span>
              Notification Center
            </h1>
            <p class="text-slate-500 text-sm mt-1 uppercase tracking-widest font-medium">History of all alerts and verification codes</p>
          </div>
          <button (click)="loadNotifications()" class="flex items-center gap-2 px-4 py-2 bg-white dark:bg-white/5 border border-slate-200 dark:border-white/10 rounded-xl hover:bg-slate-50 transition-all">
            <span class="material-symbols-outlined text-sm">refresh</span>
            <span class="text-xs font-bold uppercase tracking-widest">Refresh</span>
          </button>
        </div>

        <!-- Notification List -->
        <div class="space-y-4">
          @if (isLoading()) {
            <div class="flex flex-col items-center justify-center py-24 gap-4">
              <div class="w-8 h-8 border-2 border-accent-gold border-t-transparent rounded-full animate-spin"></div>
              <p class="text-xs font-bold text-accent-gold uppercase tracking-[0.2em]">Retrieving Secure Logs...</p>
            </div>
          } @else if (notifications().length === 0) {
            <div class="bg-white dark:bg-white/5 border border-slate-200 dark:border-white/10 rounded-2xl p-12 text-center">
              <span class="material-symbols-outlined text-4xl text-slate-300 dark:text-white/10 mb-4">notifications_off</span>
              <h3 class="text-slate-900 dark:text-white font-bold">No Notifications Yet</h3>
              <p class="text-slate-500 text-sm mt-2">Your shipment updates and OTPs will appear here.</p>
            </div>
          } @else {
            @for (note of notifications(); track note.id) {
              <div class="bg-white dark:bg-[#121215] border border-slate-200 dark:border-white/5 rounded-2xl p-6 hover:border-accent-gold/30 transition-all group">
                <div class="flex items-start justify-between mb-4">
                  <div class="flex items-center gap-3">
                    <div [ngClass]="{
                      'bg-emerald-500/10 text-emerald-500': note.status === 'Sent',
                      'bg-amber-500/10 text-amber-500': note.status === 'Pending',
                      'bg-rose-500/10 text-rose-500': note.status === 'Failed'
                    }" class="w-10 h-10 rounded-xl flex items-center justify-center">
                      <span class="material-symbols-outlined">{{ getIcon(note.eventType) }}</span>
                    </div>
                    <div>
                      <h4 class="font-bold text-slate-900 dark:text-white">{{ note.subject || note.eventType }}</h4>
                      <p class="text-[10px] text-slate-500 font-bold uppercase tracking-widest mt-0.5">
                        {{ note.createdAt | date:'medium' }} • {{ note.channel }}
                      </p>
                    </div>
                  </div>
                  @if (hasOTP(note.body)) {
                    <span class="px-2 py-1 bg-accent-gold text-black text-[9px] font-bold rounded uppercase tracking-tighter">Verification Required</span>
                  }
                </div>

                <div class="bg-slate-50 dark:bg-white/5 rounded-xl p-4 border border-slate-100 dark:border-white/5">
                  <p class="text-sm text-slate-600 dark:text-slate-300 leading-relaxed">{{ note.body }}</p>
                  
                  @if (extractOTP(note.body); as otp) {
                    <div class="mt-4 pt-4 border-t border-slate-200 dark:border-white/10 flex items-center justify-between">
                      <div class="flex items-center gap-3">
                        <span class="text-[10px] font-bold text-slate-500 uppercase tracking-widest">Delivery OTP:</span>
                        <span class="text-lg font-mono font-bold text-accent-gold tracking-[0.2em]">{{ otp }}</span>
                      </div>
                      <button (click)="store.copyToClipboard(otp)" class="flex items-center gap-2 px-3 py-1.5 bg-black dark:bg-white text-white dark:text-black rounded-lg hover:opacity-80 transition-all">
                        <span class="material-symbols-outlined text-sm">content_copy</span>
                        <span class="text-[10px] font-bold uppercase tracking-widest">Copy</span>
                      </button>
                    </div>
                  }
                </div>
              </div>
            }
          }
        </div>
      </div>
    </div>
  `
})
export class NotificationCenterComponent implements OnInit {
  private notificationService = inject(NotificationService);
  protected store = inject(StoreService);

  notifications = signal<NotificationLog[]>([]);
  isLoading = signal(true);

  ngOnInit() {
    this.loadNotifications();
  }

  loadNotifications() {
    this.isLoading.set(true);
    this.notificationService.getHistory(1, 20).subscribe({
      next: (logs) => {
        this.notifications.set(logs);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.store.addNotification('Failed to load notification history', 'error');
      }
    });
  }

  getIcon(eventType: string): string {
    if (eventType.includes('Booked')) return 'receipt_long';
    if (eventType.includes('Paid')) return 'payments';
    if (eventType.includes('Picked')) return 'box';
    if (eventType.includes('Transit')) return 'local_shipping';
    if (eventType.includes('Delivery')) return 'hail';
    if (eventType.includes('Delivered')) return 'verified';
    return 'notifications';
  }

  hasOTP(body: string): boolean {
    return /OTP[:\s]+(\d{6})/i.test(body);
  }

  extractOTP(body: string): string | null {
    const match = body.match(/OTP[:\s]+(\d{6})/i);
    return match ? match[1] : null;
  }
}
