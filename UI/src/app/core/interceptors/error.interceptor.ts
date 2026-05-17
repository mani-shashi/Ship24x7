import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { StoreService } from '../services/store.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const store = inject(StoreService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'System Error';

      if (error.error instanceof ErrorEvent) {
        errorMessage = `Client Error: ${error.error.message}`;
      } else {
        switch (error.status) {
          case 401:
            // Only redirect if the user was previously authenticated
            if (store.isAuthenticated()) {
              errorMessage = 'Session Expired: Please Login Again';
              store.logout();
              router.navigate(['/login']);
            }
            break;
          case 403:
            errorMessage = 'Access Denied: Insufficient Permissions';
            break;
          case 404:
            errorMessage = 'Resource Not Found';
            break;
          case 500:
            errorMessage = 'Internal Server Error';
            break;
          case 0:
            errorMessage = 'Network Error: Gateway Unreachable';
            break;
          default:
            errorMessage = `Unexpected Error [${error.status}]`;
        }
      }

      console.error(errorMessage, error);
      return throwError(() => error);
    })
  );
};
