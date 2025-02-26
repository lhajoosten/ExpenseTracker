import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

import { coreConfig } from './core/core.config';
import { sharedConfig } from './shared/shared.config';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes), provideAnimationsAsync(),

    // Merge the providers from coreConfig and sharedConfig
    ...coreConfig.providers,
    ...sharedConfig.providers
  ],
};
