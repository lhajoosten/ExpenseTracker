import { ApplicationConfig } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { AuthService } from './services/auth.service';
import { AuthInterceptor } from './interceptors/auth.interceptor';
import { HttpErrorInterceptor } from './interceptors/http-error.interceptor';
import { routes } from '../app.routes';
import { LoggerService } from './services/logger.service';
import { HttpErrorService } from './services/http-error.service';

export const coreConfig: ApplicationConfig = {
    providers: [
        // Provide HttpClient with interceptors
        provideHttpClient(
            withInterceptors([AuthInterceptor, HttpErrorInterceptor]),
        ),
        // Provide routing
        provideRouter(routes),
        // Provide animations
        provideAnimations(),

        // Core services
        AuthService,
        LoggerService,
        HttpErrorService,
    ],
};
