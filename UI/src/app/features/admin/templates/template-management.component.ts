import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ShipmentService } from '../../../core/services/shipment.service';
import { StoreService } from '../../../core/services/store.service';

@Component({
  selector: 'app-template-management',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './template-management.component.html',
  styleUrl: './template-management.component.css'
})
export class TemplateManagementComponent implements OnInit {
  private shipmentService = inject(ShipmentService);
  protected store = inject(StoreService);

  templates = signal<any[]>([]);
  isLoading = signal(false);

  // Modal & Editor state
  isModalOpen = signal(false);
  isEditMode = signal(false);
  selectedTemplate = signal<any | null>(null);

  ngOnInit() {
    this.loadTemplates();
  }

  loadTemplates() {
    this.isLoading.set(true);
    this.shipmentService.getTemplates().subscribe({
      next: (data) => {
        if (data && data.length > 0) {
          this.templates.set(data);
        } else {
          this.loadMockTemplates();
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.loadMockTemplates();
        this.isLoading.set(false);
      }
    });
  }

  private loadMockTemplates() {
    this.templates.set([
      { id: '1', name: 'Shipment Confirmation', type: 'Email', subject: 'Your Shipment #{{trackingNumber}} is confirmed!', content: 'Hello {{customerName}},\n\nYour shipment with tracking number {{trackingNumber}} has been successfully booked.\n\nThank you for choosing Ship24x7!', lastModified: '2026-05-10' },
      { id: '2', name: 'Out for Delivery', type: 'SMS', subject: '', content: 'Hey {{customerName}}, your package (#{{trackingNumber}}) is out for delivery! Please be ready for receipt.', lastModified: '2026-05-12' },
      { id: '3', name: 'Payment Successful', type: 'Email', subject: 'Payment Received for Order #{{trackingNumber}}', content: 'Dear Customer,\n\nWe have successfully received payment for shipment #{{trackingNumber}}.\n\nRegards,\nShip24x7 Accounts', lastModified: '2026-05-15' },
      { id: '4', name: 'Hub Arrival', type: 'App', subject: 'Shipment Arrived at Hub', content: 'Shipment #{{trackingNumber}} has safely arrived at the next logistics node.', lastModified: '2026-05-17' },
    ]);
  }

  openCreateModal() {
    this.isEditMode.set(false);
    this.selectedTemplate.set({
      id: Math.random().toString(36).substring(7),
      name: '',
      type: 'Email',
      subject: '',
      content: '',
      lastModified: new Date().toISOString().split('T')[0]
    });
    this.isModalOpen.set(true);
  }

  openEditModal(template: any) {
    this.isEditMode.set(true);
    this.selectedTemplate.set(JSON.parse(JSON.stringify(template)));
    this.isModalOpen.set(true);
  }

  closeModal() {
    this.isModalOpen.set(false);
    this.selectedTemplate.set(null);
  }

  saveTemplate() {
    const tmpl = this.selectedTemplate();
    if (!tmpl || !tmpl.name || !tmpl.content) {
      this.store.addNotification('Template Name and Content are required.', 'warning');
      return;
    }

    tmpl.lastModified = new Date().toISOString().split('T')[0];

    if (this.isEditMode()) {
      // Update
      this.shipmentService.updateTemplate(tmpl.id, tmpl).subscribe({
        next: () => {
          this.templates.update(list => list.map(t => t.id === tmpl.id ? tmpl : t));
          this.store.addNotification(`Template '${tmpl.name}' updated successfully!`, 'success');
        },
        error: () => {
          // Graceful fallback for mock presentation
          this.templates.update(list => list.map(t => t.id === tmpl.id ? tmpl : t));
          this.store.addNotification(`Template '${tmpl.name}' updated (local override)`, 'success');
        }
      });
    } else {
      // Create
      this.shipmentService.createTemplate(tmpl).subscribe({
        next: () => {
          this.templates.update(list => [...list, tmpl]);
          this.store.addNotification(`New communication template '${tmpl.name}' created!`, 'success');
        },
        error: () => {
          // Graceful fallback for mock presentation
          this.templates.update(list => [...list, tmpl]);
          this.store.addNotification(`New communication template '${tmpl.name}' created (local override)`, 'success');
        }
      });
    }

    this.closeModal();
  }

  deleteTemplate(id: string, name: string) {
    if (confirm(`Are you sure you want to delete template '${name}'?`)) {
      this.shipmentService.deleteTemplate(id).subscribe({
        next: () => {
          this.templates.update(list => list.filter(t => t.id !== id));
          this.store.addNotification(`Template '${name}' deleted successfully.`, 'info');
        },
        error: () => {
          // Graceful fallback for mock presentation
          this.templates.update(list => list.filter(t => t.id !== id));
          this.store.addNotification(`Template '${name}' deleted (local override).`, 'info');
        }
      });
    }
  }

  getResolvedPreview(template: any): string {
    if (!template) return '';
    let text = template.type === 'Email' 
      ? `Subject: ${template.subject || ''}\n\n${template.content || ''}` 
      : template.content || '';

    // Simulate key placeholder replacements
    text = text.replace(/\{\{trackingNumber\}\}/g, 'SH-8492-IN')
               .replace(/\{\{customerName\}\}/g, 'Alice Johnson')
               .replace(/\{\{hubLocation\}\}/g, 'Mumbai Airport Hub');
               
    return text;
  }
}
