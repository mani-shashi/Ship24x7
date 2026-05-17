import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { StoreService } from '../services/store.service';

export const authGuard: CanActivateFn = (route, state) => {
  const store = inject(StoreService);
  const router = inject(Router);

  if (store.isAuthenticated()) {
    return true;
  }

  router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  return false;
};

export const noAuthGuard: CanActivateFn = (route, state) => {
  const store = inject(StoreService);
  const router = inject(Router);

  if (!store.isAuthenticated()) {
    return true;
  }

  router.navigate(['/dashboard']);
  return false;
};

export const roleGuard: (roles: string[]) => CanActivateFn = (roles) => {
  return (route, state) => {
    const store = inject(StoreService);
    const router = inject(Router);
    const user = store.currentUser();

    if (user && user.roles.some(r => roles.includes(r))) {
      return true;
    }

    router.navigate(['/dashboard']);
    return false;
  };
};
