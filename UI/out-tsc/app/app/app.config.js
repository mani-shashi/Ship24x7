import { provideExperimentalZonelessChangeDetection } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { routes } from './app.routes';
export const appConfig = {
    providers: [
        provideExperimentalZonelessChangeDetection(),
        provideRouter(routes, withComponentInputBinding()),
        provideHttpClient(),
    ],
};
//# sourceMappingURL=app.config.js.map