import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RateService } from '../../../core/services/rate.service';
import { StoreService } from '../../../core/services/store.service';

@Component({
  selector: 'app-rate-management',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './rate-management.component.html',
  styleUrl: './rate-management.component.css'
})
export class RateManagementComponent implements OnInit {
  private rateService = inject(RateService);
  protected store = inject(StoreService);

  rates = signal<any[]>([]);
  isLoading = signal(false);
  searchTerm = signal('');

  // Editing & Creation State
  isModalOpen = signal(false);
  isEditMode = signal(false);
  selectedRate = signal<any | null>(null);

  // Computed filtered rates
  filteredRates = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.rates();
    return this.rates().filter(r => 
      r.serviceName.toLowerCase().includes(term) ||
      r.code.toLowerCase().includes(term) ||
      r.deliveryTime.toLowerCase().includes(term)
    );
  });

  ngOnInit() {
    this.loadRates();
  }

  loadRates() {
    this.isLoading.set(true);
    this.rateService.getRates().subscribe({
      next: (data) => {
        if (data && data.length > 0) {
          this.rates.set(data);
        } else {
          this.loadMockRates();
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.loadMockRates();
        this.isLoading.set(false);
      }
    });
  }

  private loadMockRates() {
    this.rates.set([
      { id: '1', serviceName: 'Standard Ground', code: 'STD-GRND', basePrice: 150, perKgRate: 25, deliveryTime: '3-5 Days', status: 'Active' },
      { id: '2', serviceName: 'Express Air', code: 'EXP-AIR', basePrice: 450, perKgRate: 85, deliveryTime: '1-2 Days', status: 'Active' },
      { id: '3', serviceName: 'Same Day Priority', code: 'SD-PRIO', basePrice: 950, perKgRate: 150, deliveryTime: 'Today', status: 'Active' },
    ]);
  }

  openCreateModal() {
    this.isEditMode.set(false);
    this.selectedRate.set({
      id: Math.random().toString(36).substring(7),
      serviceName: '',
      code: '',
      basePrice: 100,
      perKgRate: 15,
      deliveryTime: '2-4 Days',
      status: 'Active'
    });
    this.isModalOpen.set(true);
  }

  openEditModal(rate: any) {
    this.isEditMode.set(true);
    // Deep clone to prevent direct state mutations
    this.selectedRate.set(JSON.parse(JSON.stringify(rate)));
    this.isModalOpen.set(true);
  }

  closeModal() {
    this.isModalOpen.set(false);
    this.selectedRate.set(null);
  }

  saveRate() {
    const rate = this.selectedRate();
    if (!rate || !rate.serviceName || !rate.code) {
      this.store.addNotification('Please enter all required fields', 'warning');
      return;
    }

    if (this.isEditMode()) {
      // Update
      this.rateService.updateRate(rate.id, rate).subscribe({
        next: () => {
          this.rates.update(list => list.map(r => r.id === rate.id ? rate : r));
          this.store.addNotification(`Service tier '${rate.serviceName}' pricing model updated!`, 'success');
        },
        error: () => {
          // Graceful fallback for mock presentation
          this.rates.update(list => list.map(r => r.id === rate.id ? rate : r));
          this.store.addNotification(`Service tier '${rate.serviceName}' pricing model updated (local override)`, 'success');
        }
      });
    } else {
      // Create
      this.rateService.createRate(rate).subscribe({
        next: () => {
          this.rates.update(list => [...list, rate]);
          this.store.addNotification(`New shipping tier '${rate.serviceName}' created and deployed!`, 'success');
        },
        error: () => {
          // Graceful fallback for mock presentation
          this.rates.update(list => [...list, rate]);
          this.store.addNotification(`New shipping tier '${rate.serviceName}' created (local override)`, 'success');
        }
      });
    }

    this.closeModal();
  }

  deleteRate(id: string, name: string) {
    if (confirm(`Are you sure you want to retire the pricing model for '${name}'?`)) {
      this.rateService.deleteRate(id).subscribe({
        next: () => {
          this.rates.update(list => list.filter(r => r.id !== id));
          this.store.addNotification(`Service tier '${name}' retired from price catalog.`, 'info');
        },
        error: () => {
          // Graceful fallback for mock presentation
          this.rates.update(list => list.filter(r => r.id !== id));
          this.store.addNotification(`Service tier '${name}' retired (local override).`, 'info');
        }
      });
    }
  }
}
