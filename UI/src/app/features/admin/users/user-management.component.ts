import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { StoreService } from '../../../core/services/store.service';
import { User, UserRole } from '../../../core/types';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.css'
})
export class UserManagementComponent implements OnInit {
  private authService = inject(AuthService);
  protected store = inject(StoreService);

  users = signal<any[]>([]);
  isLoading = signal(false);
  searchTerm = signal('');

  // Editing state
  isEditModalOpen = signal(false);
  selectedUser = signal<any | null>(null);
  allRoles = ['Customer', 'Hub_User', 'Admin_User', 'System_Admin'];

  // Computed filtered users
  filteredUsers = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.users();
    return this.users().filter(u => 
      u.fullName.toLowerCase().includes(term) ||
      u.email.toLowerCase().includes(term) ||
      u.roles.some((r: string) => r.toLowerCase().includes(term))
    );
  });

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.isLoading.set(true);
    this.authService.getUsers().subscribe({
      next: (data) => {
        if (data && data.length > 0) {
          this.users.set(data);
        } else {
          this.loadMockUsers();
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.loadMockUsers();
        this.isLoading.set(false);
      }
    });
  }

  private loadMockUsers() {
    this.users.set([
      { id: '1', fullName: 'Alice Johnson', email: 'alice@ship24x7.com', roles: [UserRole.ADMIN_USER], active: true, joinedAt: '2024-01-10' },
      { id: '2', fullName: 'Bob Smith', email: 'bob@gmail.com', roles: [UserRole.CUSTOMER], active: true, joinedAt: '2024-02-15' },
      { id: '3', fullName: 'Charlie Davis', email: 'charlie@hub.com', roles: [UserRole.HUB_USER], active: false, joinedAt: '2024-03-05' },
      { id: '4', fullName: 'Diana Prince', email: 'diana@amazon.com', roles: [UserRole.CUSTOMER], active: true, joinedAt: '2024-03-20' },
    ]);
  }

  toggleUserStatus(user: any) {
    const action = user.active ? this.authService.deactivateUser(user.id) : this.authService.activateUser(user.id);
    action.subscribe({
      next: () => {
        user.active = !user.active;
        this.store.addNotification(`User ${user.fullName} ${user.active ? 'activated' : 'deactivated'} successfully`, 'success');
      },
      error: () => {
        // Fallback for mock status updates
        user.active = !user.active;
        this.store.addNotification(`User ${user.fullName} status updated locally`, 'success');
      }
    });
  }

  openEditModal(user: any) {
    // Deep clone to prevent direct mutations
    this.selectedUser.set(JSON.parse(JSON.stringify(user)));
    this.isEditModalOpen.set(true);
  }

  closeEditModal() {
    this.isEditModalOpen.set(false);
    this.selectedUser.set(null);
  }

  toggleRole(role: string) {
    const user = this.selectedUser();
    if (!user) return;
    
    if (user.roles.includes(role)) {
      // Don't allow removing all roles
      if (user.roles.length > 1) {
        user.roles = user.roles.filter((r: string) => r !== role);
      } else {
        this.store.addNotification('A user must have at least one role assigned.', 'warning');
      }
    } else {
      user.roles.push(role);
    }
  }

  saveUser() {
    const edited = this.selectedUser();
    if (!edited) return;

    this.authService.updateUser(edited.id, edited).subscribe({
      next: () => {
        this.users.update(list => list.map(u => u.id === edited.id ? edited : u));
        this.store.addNotification(`Identity configuration for ${edited.fullName} updated successfully!`, 'success');
      },
      error: () => {
        // Fallback for mock presentation
        this.users.update(list => list.map(u => u.id === edited.id ? edited : u));
        this.store.addNotification(`Identity configuration for ${edited.fullName} updated (local override)`, 'success');
      }
    });
    this.closeEditModal();
  }
}
