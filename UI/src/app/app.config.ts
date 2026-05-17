import { ApplicationConfig, provideExperimentalZonelessChangeDetection, ErrorHandler } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { correlationIdInterceptor } from './core/interceptors/correlation-id.interceptor';
import { GlobalErrorHandler } from './core/handlers/error-handler';

import { provideAnimations } from '@angular/platform-browser/animations';

export const appConfig: ApplicationConfig = {
  providers: [
    provideExperimentalZonelessChangeDetection(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor, correlationIdInterceptor])),
    provideAnimations(),
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
  ],
};

