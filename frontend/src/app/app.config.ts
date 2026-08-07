import { registerLocaleData } from '@angular/common';
import localeEsCo from '@angular/common/locales/es-CO';
import {
  provideHttpClient,
  withInterceptors,
} from '@angular/common/http';
import {
  ApplicationConfig,
  LOCALE_ID,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideRouter } from '@angular/router';

import { problemDetailsInterceptor } from './core/interceptors/problem-details.interceptor';
import { authCredentialsInterceptor } from './core/interceptors/auth-credentials.interceptor';
import { appRoutes } from './app.routes';

registerLocaleData(localeEsCo);

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(appRoutes),
    provideHttpClient(
      withInterceptors([
        authCredentialsInterceptor,
        problemDetailsInterceptor,
      ]),
    ),
    {
      provide: LOCALE_ID,
      useValue: 'es-CO',
    },
  ],
};
