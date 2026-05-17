import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { StoreService } from '../services/store.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const store = inject(StoreService);
  const token = store.currentUser()?.token;

  if (token) {
    const cloned = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    return next(cloned);
  }

  return next(req);
};
